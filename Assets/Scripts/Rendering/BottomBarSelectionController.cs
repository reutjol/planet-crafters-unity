using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BottomBarSelectionController : MonoBehaviour
{
    private const string SelectedBackgroundName = "SelectedBG";

    [SerializeField] private Transform navContainer;
    [SerializeField] private Sprite selectedBackgroundSprite;
    [SerializeField] private Vector2 selectedBackgroundSize = new Vector2(176f, 176f);
    [SerializeField] private Vector2 selectedBackgroundOffset = Vector2.zero;
    [SerializeField] private Color selectedIconColor = Color.white;
    [SerializeField] private Color unselectedIconColor = new Color(0.8f, 0.73f, 1f, 0.88f);
    [SerializeField] private Color selectedLabelColor = Color.white;
    [SerializeField] private Color unselectedLabelColor = new Color(0.84f, 0.78f, 1f, 0.9f);

    private void Awake()
    {
        AutoAssignReferences();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        RefreshSelection();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnValidate()
    {
        AutoAssignReferences();
    }

    public void RefreshSelection()
    {
        AutoAssignReferences();

        if (navContainer == null)
            return;

        int activeBuildIndex = SceneManager.GetActiveScene().buildIndex;
        SetButtonSelected("WheelButton", activeBuildIndex == (int)GameSceneId.Wheel);
        SetButtonSelected("ShopButton", activeBuildIndex == (int)GameSceneId.Shop);
        SetButtonSelected("PlanetButton", activeBuildIndex == (int)GameSceneId.PlanetHub);
        SetButtonSelected("AchievementsButton", activeBuildIndex == (int)GameSceneId.Achievements);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSelection();
    }

    private void AutoAssignReferences()
    {
        if (navContainer != null)
            return;

        Transform found = transform.Find("NavContainer");
        if (found != null)
            navContainer = found;
    }

    private void SetButtonSelected(string buttonName, bool isSelected)
    {
        Transform button = navContainer.Find(buttonName);
        if (button == null)
            return;

        Transform selectedBackground = button.Find(SelectedBackgroundName);
        if (selectedBackground != null)
        {
            RectTransform selectedRect = selectedBackground as RectTransform;
            if (selectedRect != null)
            {
                selectedRect.anchorMin = new Vector2(0.5f, 0.5f);
                selectedRect.anchorMax = new Vector2(0.5f, 0.5f);
                selectedRect.pivot = new Vector2(0.5f, 0.5f);
                selectedRect.anchoredPosition = selectedBackgroundOffset;
                selectedRect.sizeDelta = selectedBackgroundSize;
                selectedRect.localScale = Vector3.one;
                selectedRect.localRotation = Quaternion.identity;
            }

            Image selectedImage = selectedBackground.GetComponent<Image>();
            if (selectedImage != null)
            {
                if (selectedBackgroundSprite != null)
                    selectedImage.sprite = selectedBackgroundSprite;

                selectedImage.color = Color.white;
                selectedImage.preserveAspect = false;
                selectedImage.raycastTarget = false;
            }

            selectedBackground.gameObject.SetActive(isSelected);
        }

        Image[] images = button.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image == null || image.transform == button || image.transform == selectedBackground)
                continue;

            image.color = isSelected ? selectedIconColor : unselectedIconColor;
        }

        TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            TMP_Text label = labels[i];
            if (label != null)
                label.color = isSelected ? selectedLabelColor : unselectedLabelColor;
        }
    }
}
