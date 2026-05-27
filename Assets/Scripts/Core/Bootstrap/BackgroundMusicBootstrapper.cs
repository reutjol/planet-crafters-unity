using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-1100)]
public sealed class BackgroundMusicBootstrapper : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 0.5f)] private float volume = 0.35f;

    private void Awake()
    {
        BackgroundMusicService.EnsureInstance(musicClip, volume);
    }
}
