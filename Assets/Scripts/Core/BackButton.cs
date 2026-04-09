using UnityEngine;

public class BackButton : MonoBehaviour
{
    public void OnBackClicked()
    {
        SceneLoader.Instance?.GoBack();
    }
}
