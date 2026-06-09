using System;
using System.Collections;

public class ShopApiClient : BaseApiClient
{
    public static ShopApiClient Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static ShopApiClient GetOrCreate()
    {
        if (Instance != null) return Instance;
        var go = new UnityEngine.GameObject("ShopApiClient");
        return go.AddComponent<ShopApiClient>();
    }

    public IEnumerator GetProfile(string token, Action<ShopProfileDto> onSuccess, Action<string> onError)
    {
        yield return GetRequest<ShopProfileDto>("/api/shop/profile", token, onSuccess, onError);
    }

    public IEnumerator BuyAvatar(string token, string avatarId, Action<ShopPurchaseResultDto> onSuccess, Action<string> onError)
    {
        yield return PostRequest<object, ShopPurchaseResultDto>(
            $"/api/shop/buy/avatar/{avatarId}", new object(), token, onSuccess, onError);
    }

    public IEnumerator BuyBooster(string token, string boosterType, Action<ShopPurchaseResultDto> onSuccess, Action<string> onError)
    {
        yield return PostRequest<object, ShopPurchaseResultDto>(
            $"/api/shop/buy/booster/{boosterType}", new object(), token, onSuccess, onError);
    }

    public IEnumerator SetAvatar(string token, string avatarId, Action<ShopSetAvatarResultDto> onSuccess, Action<string> onError)
    {
        yield return PutRequest<ShopSetAvatarRequestDto, ShopSetAvatarResultDto>(
            "/api/shop/avatar", new ShopSetAvatarRequestDto { avatarId = avatarId }, token, onSuccess, onError);
    }

    public IEnumerator AddCoins(string token, int amount, Action<ShopAddCoinsResultDto> onSuccess, Action<string> onError)
    {
        yield return PostRequest<ShopAddCoinsRequestDto, ShopAddCoinsResultDto>(
            "/api/shop/add-coins", new ShopAddCoinsRequestDto { amount = amount }, token, onSuccess, onError);
    }
}
