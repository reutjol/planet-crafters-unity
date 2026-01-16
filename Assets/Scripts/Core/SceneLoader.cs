using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("Loading Screen Scene Index")]
    [SerializeField] int loadingSceneIndex = 1;

    [Header("Optional Boot Behavior")]
    [SerializeField] bool loadOnStart = false;
    [SerializeField] int startSceneIndex = 2;

    int targetSceneIndex;

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
        targetSceneIndex = sceneIndex;
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
            LoadingSceneController.Instance.OnSceneReady(() => async.allowSceneActivation = true);
        else
            async.allowSceneActivation = true;
    }
}
