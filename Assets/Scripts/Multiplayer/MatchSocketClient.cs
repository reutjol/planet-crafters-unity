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
    public event Action<long>    OnMatchStarted;  // startTime ms — timer begins
    public event Action<MatchDto> OnMatchFinished; // final result for all end cases
    public event Action<string>  OnMatchError;    // server-side error (e.g. playerReady failed)
    public event Action OnChallenged;
    public event Action<System.Collections.Generic.List<UnlockedAchievementDto>> OnAchievementRewards;

    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;
    private readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();
    private readonly System.Threading.SemaphoreSlim _sendLock = new System.Threading.SemaphoreSlim(1, 1);
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
        _pendingMatchId      = matchId;
        _pendingUserId       = userId;
        _onConnectedCallback = onConnected;
        _ = ConnectAsync(serverUrl);
    }

    public void EmitScore(string matchId, string userId, int score)
    {
        if (!_isConnected) return;
        _ = SendEventAsync("vsScore", new { matchId, userId, score });
    }

    public void EmitPlayerReady(string matchId, string userId)
    {
        if (!_isConnected || string.IsNullOrEmpty(matchId)) return;
        _ = SendEventAsync("playerReady", new { matchId, userId });
    }

    public void EmitSubmitScore(string matchId, string userId, int finalScore)
    {
        if (!_isConnected || string.IsNullOrEmpty(matchId)) return;
        _ = SendEventAsync("submitScore", new { matchId, userId, finalScore });
    }

    public void JoinLobby(string userId, string username, string avatarId, string planetId, string stageId)
    {
        _ = SendEventAsync("joinLobby", new { userId, username, avatarId, planetId, stageId });
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
        _isConnected = false;
        try
        {
            if (_ws?.State == WebSocketState.Open)
                _ = _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", System.Threading.CancellationToken.None);
            else
                _ws?.Abort();
        }
        catch { }
        // Cancel AFTER initiating the close frame so the server receives the graceful disconnect
        _cts?.Cancel();
    }

    // ── Connection ──────────────────────────────────────────────

    private async Task ConnectAsync(string serverUrl)
    {
        try
        {
            _cts?.Cancel();
            _ws?.Dispose();
            _cts = new CancellationTokenSource();
            _ws  = new ClientWebSocket();

            _namespaceSent = false;
            _handshakeDone = false;

            var wsUrl = serverUrl.Replace("https://", "wss://").Replace("http://", "ws://");
            var uri   = new Uri($"{wsUrl}/socket.io/?EIO=4&transport=websocket");

            await _ws.ConnectAsync(uri, _cts.Token);

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
        var accum  = new System.IO.MemoryStream();

        while (_ws?.State == WebSocketState.Open && !(_cts?.IsCancellationRequested ?? true))
        {
            try
            {
                accum.SetLength(0);
                WebSocketReceiveResult result;

                do
                {
                    result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close) { _isConnected = false; return; }
                    accum.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                var msg = Encoding.UTF8.GetString(accum.GetBuffer(), 0, (int)accum.Length);
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
    }

    private async Task HandleMessageAsync(string msg)
    {
        // Engine.IO open packet
        if (msg.StartsWith("0") && !_namespaceSent)
        {
            _namespaceSent = true;
            await SendRawAsync("40");
            return;
        }

        // Engine.IO ping → pong
        if (msg == "2") { await SendRawAsync("3"); return; }

        // Socket.IO namespace ACK
        if (msg.StartsWith("40") && !_handshakeDone)
        {
            _handshakeDone = true;
            _isConnected   = true;
            if (!string.IsNullOrEmpty(_pendingMatchId))
                await SendEventAsync("joinVsMatch", new { matchId = _pendingMatchId, userId = _pendingUserId });
            var cb = _onConnectedCallback;
            _onConnectedCallback = null;
            _mainThreadQueue.Enqueue(() => cb?.Invoke());
            return;
        }

        // Socket.IO event
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
        await _sendLock.WaitAsync(_cts.Token);
        try
        {
            if (_ws?.State != WebSocketState.Open) return;
            await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
        }
        finally
        {
            _sendLock.Release();
        }
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
                    var matchReady = data?.ToObject<MatchDto>();
                    OnMatchReady?.Invoke(matchReady);
                    break;
                case "challengeError":
                    OnChallengeError?.Invoke(data?.ToString());
                    break;
                case "matchStarted":
                    var startTime = data?["startTime"]?.Value<long>() ?? 0L;
                    OnMatchStarted?.Invoke(startTime);
                    break;
                case "matchFinished":
                    var matchFinished = data?.ToObject<MatchDto>();
                    OnMatchFinished?.Invoke(matchFinished);
                    break;
                case "matchError":
                    var errMsg = data?["message"]?.ToString() ?? "Unknown match error";
                    OnMatchError?.Invoke(errMsg);
                    break;
                case "challenged":
                    OnChallenged?.Invoke();
                    break;
                case "achievementRewards":
                    var achRewards = data?["rewards"]?.ToObject<System.Collections.Generic.List<UnlockedAchievementDto>>();
                    if (achRewards != null && achRewards.Count > 0)
                        OnAchievementRewards?.Invoke(achRewards);
                    break;
            }
        }
        catch (Exception e) { Debug.LogWarning($"[MatchSocket] Parse error: {e.Message}"); }
    }

    private void OnDestroy() => Disconnect();
}
