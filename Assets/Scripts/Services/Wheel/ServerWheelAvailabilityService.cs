using System;
using UnityEngine;

public class ServerWheelAvailabilityService : ISpinAvailabilityService
{
    private readonly WheelApiClient apiClient;

    private bool statusLoaded;
    private bool canSpin;
    private DateTime nextSpinUtc;
    private bool refreshInProgress;
    private bool consumeInProgress;

    public ServerWheelAvailabilityService(WheelApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public bool CanSpin()
    {
        if (!statusLoaded)
            return false;

        if (canSpin)
            return true;

        if (DateTime.UtcNow >= nextSpinUtc)
        {
            canSpin = true;
            return true;
        }

        return false;
    }

    public TimeSpan GetRemainingCooldown()
    {
        if (!statusLoaded || canSpin)
            return TimeSpan.Zero;

        TimeSpan remaining = nextSpinUtc - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    public void ConsumeSpin()
    {
        ConsumeSpin(null);
    }

    public void ConsumeSpin(Action<WheelSpinConsumeResult> onCompleted)
    {
        if (consumeInProgress)
            return;

        if (apiClient == null || AppSession.Instance == null || !AppSession.Instance.HasAccess())
        {
            onCompleted?.Invoke(Blocked("Missing server session."));
            return;
        }

        consumeInProgress = true;
        apiClient.StartCoroutine(apiClient.ConsumeSpin(
            AppSession.Instance.AccessToken,
            status =>
            {
                consumeInProgress = false;
                ApplyStatus(status);

                onCompleted?.Invoke(new WheelSpinConsumeResult
                {
                    success = status != null && status.success,
                    remainingCooldown = GetRemainingCooldown(),
                    message = status?.msg
                });
            },
            error =>
            {
                consumeInProgress = false;
                Debug.LogError($"[ServerWheelAvailabilityService] Consume spin failed: {error}");
                onCompleted?.Invoke(Blocked(error));
            }
        ));
    }

    public void Refresh(Action onCompleted = null)
    {
        if (refreshInProgress)
        {
            onCompleted?.Invoke();
            return;
        }

        if (apiClient == null || AppSession.Instance == null || !AppSession.Instance.HasAccess())
        {
            onCompleted?.Invoke();
            return;
        }

        refreshInProgress = true;
        apiClient.StartCoroutine(apiClient.GetStatus(
            AppSession.Instance.AccessToken,
            status =>
            {
                refreshInProgress = false;
                ApplyStatus(status);
                onCompleted?.Invoke();
            },
            error =>
            {
                refreshInProgress = false;
                Debug.LogError($"[ServerWheelAvailabilityService] Refresh failed: {error}");
                onCompleted?.Invoke();
            }
        ));
    }

    private void ApplyStatus(WheelStatusDto status)
    {
        if (status == null)
            return;

        statusLoaded = true;
        canSpin = status.canSpin;

        int remainingSeconds = Math.Max(0, status.remainingSeconds);
        nextSpinUtc = DateTime.UtcNow.AddSeconds(remainingSeconds);

        if (remainingSeconds <= 0)
            canSpin = true;
    }

    private static WheelSpinConsumeResult Blocked(string message)
    {
        return new WheelSpinConsumeResult
        {
            success = false,
            remainingCooldown = TimeSpan.Zero,
            message = message
        };
    }
}
