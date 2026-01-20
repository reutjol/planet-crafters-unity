using System;
using System.Collections;
using UnityEngine;

public class AuthApiClient : BaseApiClient
{
    [Serializable]
    private class LoginBody { public string email; public string password; }

    [Serializable]
    private class RegisterBody { public string name; public string email; public string userName; public string password; }

    [Serializable]
    private class RefreshBody { public string refreshToken; }

    [Serializable]
    private class AccessTokenWrapper { public string accessToken; }

    public IEnumerator Login(string email, string password,
        Action<AuthResponseDto> onSuccess,
        Action<string> onError)
    {
        var body = new LoginBody { email = email, password = password };
        yield return PostRequest<LoginBody, AuthResponseDto>(
            "/api/auth",
            body,
            null,
            resp =>
            {
                if (string.IsNullOrEmpty(resp.accessToken))
                {
                    onError?.Invoke("Auth failed: missing accessToken");
                    return;
                }
                onSuccess?.Invoke(resp);
            },
            onError
        );
    }

    public IEnumerator Register(string name, string email, string userName, string password,
        Action<AuthResponseDto> onSuccess,
        Action<string> onError)
    {
        var body = new RegisterBody { name = name, email = email, userName = userName, password = password };
        yield return PostRequest<RegisterBody, AuthResponseDto>(
            "/api/users",
            body,
            null,
            resp =>
            {
                if (string.IsNullOrEmpty(resp.accessToken))
                {
                    onError?.Invoke("Auth failed: missing accessToken");
                    return;
                }
                onSuccess?.Invoke(resp);
            },
            onError
        );
    }

    public IEnumerator Refresh(string refreshToken,
        Action<string> onSuccess,
        Action<string> onError)
    {
        var body = new RefreshBody { refreshToken = refreshToken };
        yield return PostRequest<RefreshBody, AccessTokenWrapper>(
            "/api/auth/refresh",
            body,
            null,
            wrapper =>
            {
                if (string.IsNullOrEmpty(wrapper.accessToken))
                {
                    onError?.Invoke("Refresh failed: missing accessToken");
                    return;
                }
                onSuccess?.Invoke(wrapper.accessToken);
            },
            onError
        );
    }
}
