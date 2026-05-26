using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Button handler for restarting the stage from the beginning
/// Resets server state, pre-fetches fresh state while the loading screen is
/// visible, then activates the gameplay scene once data is ready.
/// </summary>
public class RestartStageButton : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private GameObject panelToHide;

    private void Awake()
    {
        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("GameConfig");
    }

    public void OnRestartClicked()
    {
        if (panelToHide != null)
            panelToHide.SetActive(false);

        GameManager.Instance.StartCoroutine(ResetAndReload());
    }

    private IEnumerator ResetAndReload()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[Restart] GameManager.Instance is null!");
            yield break;
        }

        // ── 1. Reset server state (fast) ──────────────────────────────────
        bool resetDone = false;
        bool resetSuccess = false;

        GameManager.Instance.ResetCurrentStage(
            onSuccess: () => { resetDone = true; resetSuccess = true; },
            onError: (err) =>
            {
                resetDone = true;
                Debug.LogError($"[Restart] Reset failed: {err}");
            }
        );

        float elapsed = 0f;
        while (!resetDone && elapsed < 15f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!resetSuccess)
        {
            Debug.LogError("[Restart] Failed to reset stage");
            yield break;
        }

        // ── 2. Show loading screen and start pre-fetching state ───────────
        // HoldActivation keeps the loading screen up until we release it.
        SceneLoader.HoldActivation = true;

        if (SceneLoader.Instance != null && gameConfig != null)
            SceneLoader.Instance.LoadScene(gameConfig.gameplaySceneIndex);

        // ── 3. Pre-fetch fresh state while loading screen is visible ──────
        bool stateDone = false;
        Action<PlanetStageStateDto> stateHandler = _ => stateDone = true;
        Action<string>              errHandler   = _ => stateDone = true;
        Action                      unauthHandler = () => stateDone = true;

        GameManager.Instance.OnPlanetStageStateLoaded += stateHandler;
        GameManager.Instance.OnError                  += errHandler;
        GameManager.Instance.OnUnauthorized           += unauthHandler;

        GameManager.Instance.RequestPlanetStageState(forceRefresh: true);

        elapsed = 0f;
        while (!stateDone && elapsed < 90f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        GameManager.Instance.OnPlanetStageStateLoaded -= stateHandler;
        GameManager.Instance.OnError                  -= errHandler;
        GameManager.Instance.OnUnauthorized           -= unauthHandler;

        // ── 4. Release hold — loading screen will activate gameplay scene ─
        SceneLoader.HoldActivation = false;
    }
}