using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Keeps the EventSystem alive across scenes and ensures only one instance exists.
/// Attach to the EventSystem GameObject in the BootScene.
/// </summary>
public class PersistentEventSystem : MonoBehaviour
{
    private static PersistentEventSystem instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
