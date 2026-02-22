using UnityEngine;

public class PersistentUIRoot : MonoBehaviour
{
    static PersistentUIRoot instance;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
