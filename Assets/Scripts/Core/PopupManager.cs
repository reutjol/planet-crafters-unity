using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks whether any popup is currently open.
/// Use this to block 3D input when UI popups are active.
/// </summary>
public static class PopupManager
{
    private static int openPopupCount = 0;

    public static bool IsAnyPopupOpen => openPopupCount > 0;

    public static void OnPopupOpened() => openPopupCount++;
    public static void OnPopupClosed() => openPopupCount = Mathf.Max(0, openPopupCount - 1);
    public static void Reset() => openPopupCount = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneReset()
    {
        SceneManager.sceneLoaded += (scene, mode) => Reset();
    }
}
