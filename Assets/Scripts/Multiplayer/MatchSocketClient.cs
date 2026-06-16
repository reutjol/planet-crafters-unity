using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class MatchSocketClient : MonoBehaviour
{
    public static MatchSocketClient Instance { get; private set; }
    public bool IsConnected => _isConnected;

    public event Action<JArray> OnMatchStateReceived;
    public event Action<string> OnReactionReceived;
    public event Action<System.Collections.Generic.List<LobbyPlayerDto>> OnLobbyUpdated;
    public event Action<MatchDto> OnMatchReady;
    public event Action<string> OnChallengeError;
    public event Action OnOpponentLeft;
    public event Action OnOpponentFinished;
    public event Action OnChallenged;

    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;
    private readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();
    private bool _isConnected;

    // Handshake state
    private bool _namespaceSent;
    private bool _handshakeDone;
    private string _pendingMatchId;
    private string _pendingUserId;
    private System.Action _onConnectedCallback;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        while (_mainThreadQueue.TryDequeue(out var action))
            action?.Invoke();
    }

    // ── Public API ──────────────────────────────────────────────

    public void Connect(string serverUrl, string matchId, string userId, System.Action onConnected = null)
    {
        if (_isConnected)
        {
            if (!string.IsNullOrEmpty(matchId))
                _ = SendEventAsync("joinVsMatch", new { matchId, userId });
            onConnected?.Invoke();
            return;
        }
        _pendingMatchId       = matchId;
        _pendingUserId        = userId;
        _onConnectedCallback  = onConnected;
        _ = ConnectAsync(serverUrl);
    }

    public void EmitScore(string matchId, string userId, int score)
    {
        if (!_isConnected) return;
        _ = SendEventAsync("vsScore", new { matchId, userId, score });
    }

    public void JoinLobby(string userId, string username, string planetId, string stageId)
    {
        _ = SendEventAsync("joinLobby", new { userId, username, planetId, stageId });
    }

    public void LeaveLobby()
    {
        _ = SendEventAsync("leaveLobby", new { });
    }

    public void ChallengePlayer(string targetUserId)
    {
        _ = SendEventAsync("challengePlayer", new { targetUserId });
    }

    public void ChallengeRandom()
    {
        _ = SendEventAsync("challengePlayer", new { targetUserId = (string)null });
    }

    public void Disconnect()
    {
        _cts?.Cancel();
        _isConnected = false;
        try
        {
            if (_ws?.State == WebSocketState.Open)
                _ = _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", System.Threading.CancellationToken.None);
            else
                _ws?.Abort();
        }
        catch { }
    }

    // ── Connection ──────────────────────────────────────────────

    private async Task ConnectAsync(string serverUrl)
    {
        try
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _ws  = new ClientWebSocket();

            _namespaceSent = false;
            _handshakeDone = false;

            var wsUrl = serverUrl.Replace("https://", "wss://").Replace("http://", "ws://");
            var uri   = new Uri($"{wsUrl}/socket.io/?EIO=4&transport=websocket");

            Debug.Log($"[MatchSocket] Connecting to {uri}");
            await _ws.ConnectAsync(uri, _cts.Token);
            Debug.Log("[MatchSocket] WebSocket open — starting receive loop");

            _ = ReceiveLoopAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MatchSocket] Connect failed: {e.Message}");
        }
    }

    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[8192];
        while (_ws?.State == WebSocketState.Open && !(_cts?.IsCancellationRequested ?? true))
        {
            try
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                if (result.MessageType == WebSocketMessageType.Close) break;

                var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log($"[MatchSocket] RX: {msg.Substring(0, Math.Min(60, msg.Length))}");
                await HandleMessageAsync(msg);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception e)
            {
                Debug.LogWarning($"[MatchSocket] Receive error: {e.Message}");
                break;
            }
        }
        _isConnected = false;
        Debug.Log("[MatchSocket] Receive loop ended");
    }

    private async Task HandleMessageAsync(string msg)
    {
        // Engine.IO open packet
        if (msg.StartsWith("0") && !_namespaceSent)
        {
            _namespaceSent = true;
            Debug.Log("[MatchSocket] Got open packet — sending 40");
            await SendRawAsync("40");
            return;
        }

        // Engine.IO ping → pong
        if (msg == "2") { await SendRawAsync("3"); return; }

        // socket.io namespace ACK
        if (msg.StartsWith("40") && !_handshakeDone)
        {
            _handshakeDone = true;
            _isConnected   = true;
            Debug.Log("[MatchSocket] Namespace ACK — socket ready");
            if (!string.IsNullOrEmpty(_pendingMatchId))
                await SendEventAsync("joinVsMatch", new { matchId = _pendingMatchId, userId = _pendingUserId });
            var cb = _onConnectedCallback;
            _onConnectedCallback = null;
            _mainThreadQueue.Enqueue(() => cb?.Invoke());
            return;
        }

        // socket.io event
        if (msg.StartsWith("42"))
        {
            _mainThreadQueue.Enqueue(() => DispatchEvent(msg.Substring(2)));
        }
    }

    // ── Helpers ─────────────────────────────────────────────────

    private async Task SendEventAsync(string eventName, object data)
    {
        if (_ws?.State != WebSocketState.Open) return;
        var payload = JsonConvert.SerializeObject(new object[] { eventName, data });
        await SendRawAsync($"42{payload}");
    }

    private async Task SendRawAsync(string msg)
    {
        if (_ws?.State != WebSocketState.Open) return;
        var bytes = Encoding.UTF8.GetBytes(msg);
        await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
    }

    private void DispatchEvent(string json)
    {
        try
        {
            var arr       = JArray.Parse(json);
            var eventName = arr[0]?.ToString();
            var data      = arr.Count > 1 ? arr[1] : null;

            switch (eventName)
            {
                case "vsMatchState":
                    OnMatchStateReceived?.Invoke(data?["players"] as JArray);
                    break;
                case "vsReaction":
                    OnReactionReceived?.Invoke(data?["message"]?.ToString());
                    break;
                case "lobbyUpdate":
                    var players = data?["players"]?.ToObject<System.Collections.Generic.List<LobbyPlayerDto>>();
                    OnLobbyUpdated?.Invoke(players);
                    break;
                case "matchReady":
                    var match = data?.ToObject<MatchDto>();
                    OnMatchReady?.Invoke(match);
                    break;
                case "challengeError":
                    OnChallengeError?.Invoke(data?.ToString());
                    break;
                case "opponentLeft":
                    OnOpponentLeft?.Invoke();
                    break;
                case "opponentFinished":
                    OnOpponentFinished?.Invoke();
                    break;
                case "challenged":
                    OnChallenged?.Invoke();
                    break;
            }
        }
        catch (Exception e) { Debug.LogWarning($"[MatchSocket] Parse error: {e.Message}"); }
    }

    private void OnDestroy() => Disconnect();
}
