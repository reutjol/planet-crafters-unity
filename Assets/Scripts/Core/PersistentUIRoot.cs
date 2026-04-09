using UnityEngine;
<<<<<<< HEAD
using UnityEngine.SceneManagement;
=======
>>>>>>> origin/main

public class PersistentUIRoot : MonoBehaviour
{
    static PersistentUIRoot instance;

<<<<<<< HEAD
    [Tooltip("Scene indices where this UI should be visible")]
    [SerializeField] private int[] visibleInScenes;

=======
>>>>>>> origin/main
    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
<<<<<<< HEAD
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
=======
>>>>>>> origin/main
    }
}
