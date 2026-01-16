using TMPro;
using UnityEngine;

public class SignInController : MonoBehaviour
{
    public AuthApiClient api;

    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public void OnClickConnect()
    {
        string email = emailInput.text.Trim();
        string pass = passwordInput.text;
        Debug.Log($"email: {email}, pass: {pass}");

        StartCoroutine(api.Login(email, pass,
            onOk: (resp) =>
            {
                AppSession.Instance.SetTokens(resp.accessToken, resp.refreshToken);
                SceneLoader.Instance.LoadScene(5);
            },
            onErr: (err) => Debug.LogError(err)
        ));
    }
}
