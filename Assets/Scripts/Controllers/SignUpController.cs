using UnityEngine;
using TMPro;

public class SignUpController : MonoBehaviour
{
    public AuthApiClient api;

    public TMP_InputField nameInput;
    public TMP_InputField userNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public void OnClickSignUp()
    {
        var name = nameInput.text.Trim();
        var userName = userNameInput.text.Trim();
        var email = emailInput.text.Trim();
        var pass = passwordInput.text;

        StartCoroutine(api.Register(name, email, userName, pass,
            onOk: (resp) =>
            {
                SceneLoader.Instance.LoadScene(3);
            },
            onErr: (err) => Debug.LogError(err)
        ));
    }
}
