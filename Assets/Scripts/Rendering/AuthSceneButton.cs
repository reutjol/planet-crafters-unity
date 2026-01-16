using UnityEngine;

public class AuthSceneButton : MonoBehaviour
{
    public int targetSceneIndex;

    public void ChangeScene()
    {
        if (SceneLoader.Instance == null)
        {
            Debug.LogError("SceneLoader.Instance is null. Make sure SceneLoader exists in the Boot scene.");
            return;
        }

        SceneLoader.Instance.LoadScene(targetSceneIndex);
    }
}
