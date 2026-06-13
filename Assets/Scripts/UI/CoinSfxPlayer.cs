using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CoinSfxPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private bool hasLastCoins;
    private int lastCoins;

    private void Reset()
    {
        CacheAudioSource();
    }

    private void Awake()
    {
        CacheAudioSource();
    }

    private void OnEnable()
    {
        UserCoinsDisplay.OnCoinsChanged += HandleCoinsChanged;
    }

    private void OnDisable()
    {
        UserCoinsDisplay.OnCoinsChanged -= HandleCoinsChanged;
    }

    private void HandleCoinsChanged(int coins)
    {
        if (hasLastCoins && coins > lastCoins)
            PlaySfx();

        lastCoins = coins;
        hasLastCoins = true;
    }

    private void PlaySfx()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        audioSource.Stop();
        audioSource.Play();
    }

    private void CacheAudioSource()
    {
        if (audioSource != null)
            return;

        TryGetComponent(out audioSource);
    }
}
