using UnityEngine;

public class BackButton : MonoBehaviour
{
    public void OnBackClicked()
    {
        MatchManager.Instance?.CancelMatch();
        SceneLoader.Instance?.GoBack();
    }
}
