using UnityEngine;

/// <summary>
/// Initial authentication menu controller.
/// Provides navigation to Sign In and Sign Up screens.
/// </summary>
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
