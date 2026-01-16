using UnityEngine;

public class RegistrationMenuController : MonoBehaviour
{
    public void OnClickSignIn()
    {
        SceneLoader.Instance.LoadScene(3);
    }

    public void OnClickSignUp()
    {
        SceneLoader.Instance.LoadScene(4);
    }
}
