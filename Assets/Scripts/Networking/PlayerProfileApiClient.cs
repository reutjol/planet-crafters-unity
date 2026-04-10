using System;
using System.Collections;
using UnityEngine;

public class PlayerProfileApiClient : BaseApiClient
{
    public static PlayerProfileApiClient Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator GetMyProfile(
        string accessToken,
        Action<UserDto> onSuccess,
        Action<string> onError)
    {
        yield return GetRequest<UserDto>("/api/users/me", accessToken, onSuccess, onError);
    }

    public IEnumerator UpdateMyProfile(
        string accessToken,
        UpdateProfileRequestDto request,
        Action<UserDto> onSuccess,
        Action<string> onError)
    {
        yield return PutRequest<UpdateProfileRequestDto, UserDto>(
            "/api/users/me",
            request,
            accessToken,
            onSuccess,
            onError
        );
    }
}
