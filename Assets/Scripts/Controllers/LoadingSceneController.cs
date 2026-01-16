using System;
using System.Collections;
using UnityEngine;

public class LoadingSceneController : MonoBehaviour
{
    public static LoadingSceneController Instance;

    public float exitDelay = 0.3f;

    Action onFadeOutComplete;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        SceneLoader.Instance.StartAsyncLoad();
    }

    public void OnSceneReady(Action callback)
    {
        onFadeOutComplete = callback;
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        yield return new WaitForSeconds(exitDelay);

        onFadeOutComplete?.Invoke();
    }
}
