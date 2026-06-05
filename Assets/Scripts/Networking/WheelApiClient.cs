using System;
using System.Collections;
using UnityEngine;

public class WheelApiClient : BaseApiClient
{
    public static WheelApiClient Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static WheelApiClient GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject gameObject = new GameObject("WheelApiClient");
        return gameObject.AddComponent<WheelApiClient>();
    }

    public IEnumerator GetStatus(
        string accessToken,
        Action<WheelStatusDto> onSuccess,
        Action<string> onError)
    {
        yield return GetRequest<WheelStatusDto>(
            "/api/wheel/status",
            accessToken,
            onSuccess,
            onError
        );
    }

    public IEnumerator ConsumeSpin(
        string accessToken,
        Action<WheelStatusDto> onSuccess,
        Action<string> onError)
    {
        yield return PostRequest<WheelSpinRequestDto, WheelStatusDto>(
            "/api/wheel/spin",
            new WheelSpinRequestDto(),
            accessToken,
            onSuccess,
            onError
        );
    }
}
