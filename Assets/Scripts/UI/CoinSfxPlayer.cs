using UnityEngine;

public class CoinSfxPlayer : MonoBehaviour
{
    public static CoinSfxPlayer Instance { get; private set; }

    [SerializeField] private AudioClip coinClip;

    private int _lastCoins = int.MaxValue;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (UserCoinsDisplay.LastKnownCoins >= 0)
            _lastCoins = UserCoinsDisplay.LastKnownCoins;
        UserCoinsDisplay.OnCoinsChanged += HandleCoinsChanged;
    }
    private void OnDisable() => UserCoinsDisplay.OnCoinsChanged -= HandleCoinsChanged;

    private void HandleCoinsChanged(int coins)
    {
        if (coins > _lastCoins)
            SoundEffectService.Instance?.Play(coinClip, 1f);

        _lastCoins = coins;
    }
}
