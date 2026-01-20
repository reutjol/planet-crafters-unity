using UnityEngine;

/// <summary>
/// Game configuration settings - holds all centralized configuration like URLs, scene indices, and gameplay constants.
/// Create an instance via: Assets > Create > Game > Game Config
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Server Settings")]
    [Tooltip("Base URL for the backend server API")]
    public string serverBaseUrl = "https://planet-crafters-server.onrender.com";

    [Header("Scene Indices")]
    [Tooltip("Scene index for the Boot scene")]
    public int bootSceneIndex = 0;

    [Tooltip("Scene index for the Loading scene")]
    public int loadingSceneIndex = 1;

    [Tooltip("Scene index for the Registration Menu scene")]
    public int registrationMenuSceneIndex = 2;

    [Tooltip("Scene index for the Sign In scene")]
    public int signInSceneIndex = 3;

    [Tooltip("Scene index for the Sign Up scene")]
    public int signUpSceneIndex = 4;

    [Tooltip("Scene index for the Planet Selection scene")]
    public int planetSceneIndex = 5;

    [Tooltip("Scene index for the Stages Map scene")]
    public int stagesMapSceneIndex = 6;

    [Tooltip("Scene index for the Gameplay scene")]
    public int gameplaySceneIndex = 7;

    [Header("Gameplay Settings")]
    [Tooltip("Default deck size for new stages")]
    public int defaultDeckSize = 30;

    [Tooltip("Maximum hand size (number of tiles in hand)")]
    public int maxHandSize = 3;

    [Tooltip("Auto-save debounce delay in seconds")]
    public float autoSaveDebounceDelay = 0.25f;

    [Header("UI Settings")]
    [Tooltip("Loading scene minimum display time in seconds")]
    public float loadingSceneMinDuration = 1.0f;
}
