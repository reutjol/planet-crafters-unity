using UnityEngine;

public class BackToStageMapButton : MonoBehaviour
{
    [SerializeField] private int stageMapSceneIndex = 6;

    public void OnBackClicked()
    {
        SceneLoader.Instance.LoadScene(stageMapSceneIndex);
    }
}
