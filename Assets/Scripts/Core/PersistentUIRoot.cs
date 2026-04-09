using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentUIRoot : MonoBehaviour
{
    static PersistentUIRoot instance;

    [Tooltip("Scene indices where this UI should be visible")]
    [SerializeField] private int[] visibleInScenes;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool show = System.Array.IndexOf(visibleInScenes, scene.buildIndex) >= 0;
        gameObject.SetActive(show);
    }
}
