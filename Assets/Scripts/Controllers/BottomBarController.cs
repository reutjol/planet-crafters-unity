using UnityEngine;
using UnityEngine.SceneManagement;

public class BottomBarController : MonoBehaviour
{
    public void GoWheel() => GoTo(GameSceneId.Wheel);
    public void GoShop() => GoTo(GameSceneId.Shop);
    public void GoPlanet() => GoTo(GameSceneId.PlanetHub);
    public void GoAchievements() => GoTo(GameSceneId.Achievements);

    void GoTo(GameSceneId target)
    {
        if (PopupManager.IsAnyPopupOpen) return;

        int sceneIndex = (int)target;

        if (SceneManager.GetActiveScene().buildIndex == sceneIndex)
            return;

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(sceneIndex);
        else
            SceneManager.LoadScene(sceneIndex);
    }
}
