using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles asynchronous scene loading with loading screen transitions.
/// Singleton that persists across scenes. Maintains navigation history stack.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("Loading Screen Scene Index")]
    [SerializeField] int loadingSceneIndex = 1;

    [Header("Optional Boot Behavior")]
    [SerializeField] bool loadOnStart = false;
    [SerializeField] int startSceneIndex = 2;

    int targetSceneIndex;
    private bool isLoading = false;
    private readonly Stack<int> history = new();
    public int PreviousSceneIndex => history.Count > 0 ? history.Peek() : -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        if (loadOnStart) LoadScene(startSceneIndex);
    }

    public void LoadScene(int sceneIndex)
    {
        if (isLoading) return;

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene != loadingSceneIndex)
            history.Push(currentScene);

        targetSceneIndex = sceneIndex;
        isLoading = true;
        SceneManager.LoadScene(loadingSceneIndex);
    }

    public void GoBack()
    {
        if (isLoading || history.Count == 0) return;
        targetSceneIndex = history.Pop();
        isLoading = true;
        SceneManager.LoadScene(loadingSceneIndex);
    }

    public void StartAsyncLoad()
    {
        StartCoroutine(AsyncLoadRoutine());
    }

    IEnumerator AsyncLoadRoutine()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(targetSceneIndex);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f) yield return null;

        if (LoadingSceneController.Instance != null)
            LoadingSceneController.Instance.OnSceneReady(() => { async.allowSceneActivation = true; isLoading = false; });
        else
        {
            async.allowSceneActivation = true;
            isLoading = false;
        }
    }
}
