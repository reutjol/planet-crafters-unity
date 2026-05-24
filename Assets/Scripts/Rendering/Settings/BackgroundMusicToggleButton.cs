using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class BackgroundMusicToggleButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color enabledIconColor = Color.white;
    [SerializeField] private Color disabledIconColor = new Color(1f, 1f, 1f, 0.38f);
    [SerializeField] private Color enabledBackgroundColor = Color.white;
    [SerializeField] private Color disabledBackgroundColor = new Color(0.55f, 0.55f, 0.65f, 0.82f);

    private IBackgroundMusicService service;
    private bool originalInteractable = true;
    private bool hasCapturedOriginalInteractable;

    private void Reset()
    {
        button = GetComponent<Button>();
        backgroundImage = GetComponent<Image>();
        Transform imageChild = transform.Find("Image");
        iconImage = imageChild != null ? imageChild.GetComponent<Image>() : null;
    }

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (iconImage == null)
        {
            Transform imageChild = transform.Find("Image");
            iconImage = imageChild != null ? imageChild.GetComponent<Image>() : null;
        }

        CaptureOriginalInteractable();
    }

    private void OnEnable()
    {
        CaptureOriginalInteractable();
        service = BackgroundMusicService.Instance;

        if (service == null)
        {
            SetInteractable(false);
            RefreshVisualState(false);
            return;
        }

        service.MusicEnabledChanged += RefreshVisualState;
        SetInteractable(originalInteractable);

        if (button != null)
        {
            button.onClick.RemoveListener(ToggleMusic);
            button.onClick.AddListener(ToggleMusic);
        }

        RefreshVisualState(service.IsMusicEnabled);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(ToggleMusic);

        if (service != null)
            service.MusicEnabledChanged -= RefreshVisualState;
    }

    private void ToggleMusic()
    {
        if (service == null)
            return;

        service.ToggleMusic();
    }

    private void SetInteractable(bool isInteractable)
    {
        if (button != null)
            button.interactable = isInteractable;
    }

    private void CaptureOriginalInteractable()
    {
        if (hasCapturedOriginalInteractable)
            return;

        originalInteractable = button == null || button.interactable;
        hasCapturedOriginalInteractable = true;
    }

    private void RefreshVisualState(bool isEnabled)
    {
        if (iconImage != null)
            iconImage.color = isEnabled ? enabledIconColor : disabledIconColor;

        if (backgroundImage != null)
            backgroundImage.color = isEnabled ? enabledBackgroundColor : disabledBackgroundColor;
    }
}
