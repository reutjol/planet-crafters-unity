using System.Collections;
<<<<<<< HEAD
using System.Collections.Generic;
=======
>>>>>>> origin/main
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles asynchronous scene loading with loading screen transitions.
<<<<<<< HEAD
/// Singleton that persists across scenes. Maintains navigation history stack.
=======
/// Singleton that persists across scenes.
>>>>>>> origin/main
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
<<<<<<< HEAD
    private readonly Stack<int> history = new();
    public int PreviousSceneIndex => history.Count > 0 ? history.Peek() : -1;
=======
>>>>>>> origin/main

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
<<<<<<< HEAD
        history.Push(SceneManager.GetActiveScene().buildIndex);
=======
>>>>>>> origin/main
        targetSceneIndex = sceneIndex;
        SceneManager.LoadScene(loadingSceneIndex);
    }

<<<<<<< HEAD
    public void GoBack()
    {
        if (history.Count == 0) return;
        targetSceneIndex = history.Pop();
        SceneManager.LoadScene(loadingSceneIndex);
    }

=======
>>>>>>> origin/main
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
            LoadingSceneController.Instance.OnSceneReady(() => async.allowSceneActivation = true);
        else
            async.allowSceneActivation = true;
    }
}
