using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SettingsPopupStyleApplier
{
    private const string GeneratedFolder = "Assets/Generated/SettingsPopupStyle";
    private const string SettingsPopupPrefabPath = "Assets/Resources/Prefabs/SettingsPopup.prefab";

    [MenuItem("Tools/Planet/Apply Settings Popup Style")]
    public static void ApplyFromMenu()
    {
        Debug.Log(Apply());
    }

    public static string Apply()
    {
        if (SceneManager.GetActiveScene().name != "PlanetScene")
            return "PlanetScene is not active; no changes applied.";

        SettingsPopupView view = FindSceneObject<SettingsPopupView>();
        if (view == null)
            return "SettingsPopupView was not found in PlanetScene.";

        System.IO.Directory.CreateDirectory(GeneratedFolder);
        PopupSprites sprites = GenerateSprites();

        ApplyToPopup(view.transform, sprites);
        string prefabResult = ApplyToPrefab(sprites);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        return "Settings popup sci-fi visual style applied. Existing popup content and script references were preserved. " + prefabResult;
    }

    private static void ApplyToPopup(Transform popup, PopupSprites sprites)
    {
        ConfigurePopupRoot(popup);
        RemoveLatestRedesignObjects(popup);
        RestorePopupCanvas(popup);
        ConfigureBackground(popup, sprites.outerPanel);
        ConfigureContainer(popup, sprites.sectionPanel);
        ConfigureHeader(popup, sprites.closeFrame, sprites.closeIcon);
        ConfigureProfileSection(popup, sprites.sectionPanel, sprites.profileFrame, sprites.buttonFrame);
        ConfigureBottomButtons(popup, sprites.squareFrame);
        ConfigureBottomLinks(popup);
    }

    private static string ApplyToPrefab(PopupSprites sprites)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(SettingsPopupPrefabPath);
        if (prefabRoot == null)
            return "Settings popup prefab was not found.";

        try
        {
            SettingsPopupView prefabView = prefabRoot.GetComponentInChildren<SettingsPopupView>(true);
            Transform popup = prefabView != null ? prefabView.transform : prefabRoot.transform;
            ApplyToPopup(popup, sprites);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, SettingsPopupPrefabPath);
            return prefabView != null ? "Settings popup prefab updated." : "Settings popup prefab visuals updated from root hierarchy.";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static void RemoveLatestRedesignObjects(Transform popup)
    {
        DestroyChild(popup, "SettingsPopupScreenDim");

        Transform container = popup.Find("PopupContainer");
        if (container == null)
            return;

        DestroyChild(container, "SettingsPopupTopAccent");
        DestroyChild(container, "SettingsPopupBottomAccent");
    }

    private static void RestorePopupCanvas(Transform popup)
    {
        Canvas canvas = popup.GetComponentInParent<Canvas>(true);
        if (canvas == null)
            return;

        canvas.overrideSorting = false;
        canvas.sortingOrder = 0;
    }

    private static void DestroyChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
            Object.DestroyImmediate(child.gameObject);
    }

    private static void ConfigurePopupRoot(Transform popup)
    {
        RectTransform rect = popup as RectTransform;
        if (rect == null)
            return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void ConfigureBackground(Transform popup, Sprite outerPanel)
    {
        Transform background = popup.Find("Background");
        if (background == null)
            return;

        RectTransform rect = background as RectTransform;
        SetCenter(rect, Vector2.zero, new Vector2(980f, 1240f));

        Image image = background.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = outerPanel;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = false;
        }
    }

    private static void ConfigureContainer(Transform popup, Sprite sectionPanel)
    {
        Transform container = popup.Find("PopupContainer");
        if (container == null)
            return;

        DisableLayoutComponents(container);
        SetCenter(container as RectTransform, Vector2.zero, new Vector2(860f, 980f));

        Image image = container.GetComponent<Image>();
        if (image == null)
            image = container.gameObject.AddComponent<Image>();

        image.sprite = null;
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = false;

        GameObject profileSection = EnsureChild(container, "ProfileSectionPanel", typeof(Image));
        profileSection.transform.SetSiblingIndex(1);
        SetCenter(profileSection.GetComponent<RectTransform>(), new Vector2(0f, 145f), new Vector2(800f, 360f));
        Image profileImage = profileSection.GetComponent<Image>();
        profileImage.sprite = sectionPanel;
        profileImage.type = Image.Type.Sliced;
        profileImage.color = Color.white;
        profileImage.raycastTarget = false;

        GameObject bottomSection = EnsureChild(container, "ControlsSectionPanel", typeof(Image));
        bottomSection.transform.SetSiblingIndex(2);
        SetCenter(bottomSection.GetComponent<RectTransform>(), new Vector2(0f, -270f), new Vector2(800f, 380f));
        Image bottomImage = bottomSection.GetComponent<Image>();
        bottomImage.sprite = sectionPanel;
        bottomImage.type = Image.Type.Sliced;
        bottomImage.color = Color.white;
        bottomImage.raycastTarget = false;
    }

    private static void ConfigureHeader(Transform popup, Sprite closeFrame, Sprite closeIcon)
    {
        Transform header = popup.Find("PopupContainer/HeaderSection");
        if (header == null)
            return;

        DisableLayoutComponents(header);
        SetCenter(header as RectTransform, new Vector2(0f, 405f), new Vector2(800f, 120f));

        Transform title = header.Find("Text");
        if (title != null)
        {
            RectTransform titleRect = title as RectTransform;
            SetCenter(titleRect, Vector2.zero, new Vector2(620f, 100f));
            TMP_Text titleText = title.GetComponent<TMP_Text>();
            if (titleText != null)
            {
                titleText.text = titleText.text.ToUpperInvariant();
                titleText.fontSize = 78f;
                titleText.fontStyle = FontStyles.Bold;
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.color = Color.white;
                titleText.enableAutoSizing = true;
                titleText.fontSizeMin = 42f;
                titleText.fontSizeMax = 78f;
                titleText.outlineColor = Hex("#6D67FF", 0.85f);
                titleText.outlineWidth = 0.22f;
            }
        }

        Transform spacer = header.Find("Spacer");
        if (spacer != null)
            spacer.gameObject.SetActive(false);

        Transform closeButton = header.Find("CloseButton");
        if (closeButton == null)
            return;

        SetCenter(closeButton as RectTransform, new Vector2(335f, 0f), new Vector2(88f, 88f));
        Image closeRoot = closeButton.GetComponent<Image>();
        if (closeIcon == null && closeRoot != null)
            closeIcon = closeRoot.sprite;
        if (closeRoot != null)
        {
            closeRoot.sprite = closeFrame;
            closeRoot.type = Image.Type.Simple;
            closeRoot.color = Color.white;
            closeRoot.raycastTarget = true;
        }

        Button button = closeButton.GetComponent<Button>();
        if (button != null && closeRoot != null)
            button.targetGraphic = closeRoot;

        Image icon = ConfigureChildImage(closeButton, "CloseIcon", closeIcon, Vector2.zero, new Vector2(54f, 54f), true);
        icon.color = Color.white;
    }

    private static void ConfigureProfileSection(Transform popup, Sprite sectionPanel, Sprite profileFrame, Sprite buttonFrame)
    {
        Transform profileInfo = popup.Find("PopupContainer/ProfileInfo");
        if (profileInfo == null)
            return;

        DisableLayoutComponents(profileInfo);
        profileInfo.SetAsLastSibling();
        SetCenter(profileInfo as RectTransform, new Vector2(0f, 145f), new Vector2(800f, 360f));
        Image sectionImage = profileInfo.GetComponent<Image>();
        if (sectionImage != null)
        {
            sectionImage.sprite = null;
            sectionImage.color = new Color(1f, 1f, 1f, 0f);
            sectionImage.raycastTarget = false;
        }

        Transform left = profileInfo.Find("Left");
        if (left != null)
        {
            SetCenter(left as RectTransform, new Vector2(-240f, -6f), new Vector2(280f, 280f));
            Transform profileBackground = left.Find("ProfileBackground");
            if (profileBackground != null)
            {
                SetCenter(profileBackground as RectTransform, Vector2.zero, new Vector2(270f, 270f));
                Image bgImage = profileBackground.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.sprite = profileFrame;
                    bgImage.type = Image.Type.Simple;
                    bgImage.color = Color.white;
                    bgImage.raycastTarget = false;
                }
            }

            Transform profileImage = left.Find("ProfileImage");
            if (profileImage != null)
            {
                SetCenter(profileImage as RectTransform, Vector2.zero, new Vector2(205f, 205f));
                Image image = profileImage.GetComponent<Image>();
                if (image != null)
                {
                    image.preserveAspect = true;
                    image.color = Color.white;
                    image.raycastTarget = false;
                }
            }
        }

        Transform right = profileInfo.Find("Right");
        if (right != null)
        {
            DisableLayoutComponents(right);
            SetCenter(right as RectTransform, new Vector2(210f, -6f), new Vector2(410f, 250f));
            ConfigureTextButton(right.Find("logout"), buttonFrame, new Vector2(0f, 65f), new Vector2(390f, 88f));
            ConfigureTextButton(right.Find("LanguageButton"), buttonFrame, new Vector2(0f, -65f), new Vector2(390f, 88f));
        }
    }

    private static void ConfigureTextButton(Transform buttonTransform, Sprite buttonFrame, Vector2 position, Vector2 size)
    {
        if (buttonTransform == null)
            return;

        SetCenter(buttonTransform as RectTransform, position, size);
        Image rootImage = buttonTransform.GetComponent<Image>();
        if (rootImage == null)
            rootImage = buttonTransform.gameObject.AddComponent<Image>();

        rootImage.sprite = buttonFrame;
        rootImage.type = Image.Type.Simple;
        rootImage.color = Color.white;
        rootImage.raycastTarget = true;

        Button button = buttonTransform.GetComponent<Button>();
        if (button != null)
            button.targetGraphic = rootImage;

        Transform childImage = buttonTransform.Find("Image");
        if (childImage != null)
        {
            RectTransform childRect = childImage as RectTransform;
            if (childRect != null)
            {
                childRect.anchorMin = Vector2.zero;
                childRect.anchorMax = Vector2.one;
                childRect.offsetMin = Vector2.zero;
                childRect.offsetMax = Vector2.zero;
            }

            Image image = childImage.GetComponent<Image>();
            if (image != null)
                image.color = new Color(1f, 1f, 1f, 0f);
        }

        TMP_Text text = buttonTransform.GetComponentInChildren<TMP_Text>(true);
        if (text == null)
            text = EnsureButtonLabel(buttonTransform);

        if (text != null)
            StyleButtonText(text, 34f);
    }

    private static TMP_Text EnsureButtonLabel(Transform buttonTransform)
    {
        string label = buttonTransform.name == "logout" ? "LOGOUT" : "LANGUAGE";
        GameObject labelObject = EnsureChild(buttonTransform, "SciFiLabel", typeof(TextMeshProUGUI));
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        SetCenter(rect, Vector2.zero, new Vector2(330f, 64f));

        TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.raycastTarget = false;
        return text;
    }

    private static void ConfigureBottomButtons(Transform popup, Sprite squareFrame)
    {
        Transform bottomButtons = popup.Find("PopupContainer/BottomButtons");
        if (bottomButtons == null)
            return;

        DisableLayoutComponents(bottomButtons);
        bottomButtons.SetAsLastSibling();
        SetCenter(bottomButtons as RectTransform, new Vector2(0f, -220f), new Vector2(760f, 190f));

        ConfigureSquareIconButton(bottomButtons.Find("MusicButton"), squareFrame, LoadSprite("Assets/Resources/Sprites/Buttons&Icons/music.png"), new Vector2(-285f, 0f));
        ConfigureSquareIconButton(bottomButtons.Find("SFXButton"), squareFrame, LoadSprite("Assets/Resources/Sprites/Buttons&Icons/sfx.png"), new Vector2(-95f, 0f));
        ConfigureSquareIconButton(bottomButtons.Find("VibrationButton"), squareFrame, LoadSprite("Assets/Resources/Sprites/Buttons&Icons/vibration.png"), new Vector2(95f, 0f));
        ConfigureSquareIconButton(bottomButtons.Find("NotificationButton"), squareFrame, LoadSprite("Assets/Resources/Sprites/Buttons&Icons/notification.png"), new Vector2(285f, 0f));
    }

    private static void ConfigureSquareIconButton(Transform buttonTransform, Sprite squareFrame, Sprite iconSprite, Vector2 position)
    {
        if (buttonTransform == null)
            return;

        SetCenter(buttonTransform as RectTransform, position, new Vector2(150f, 150f));
        Image rootImage = buttonTransform.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.sprite = squareFrame;
            rootImage.type = Image.Type.Simple;
            rootImage.color = Color.white;
            rootImage.raycastTarget = true;
        }

        Button button = buttonTransform.GetComponent<Button>();
        if (button != null && rootImage != null)
            button.targetGraphic = rootImage;

        Transform icon = buttonTransform.Find("Image");
        if (icon == null)
            return;

        SetCenter(icon as RectTransform, Vector2.zero, new Vector2(88f, 88f));
        Image iconImage = icon.GetComponent<Image>();
        if (iconImage != null)
        {
            if (iconSprite != null)
                iconImage.sprite = iconSprite;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }
    }

    private static void ConfigureBottomLinks(Transform popup)
    {
        Transform bottomText = popup.Find("PopupContainer/BottomText");
        if (bottomText == null)
            return;

        DisableLayoutComponents(bottomText);
        bottomText.SetAsLastSibling();
        SetCenter(bottomText as RectTransform, new Vector2(0f, -400f), new Vector2(760f, 70f));

        ConfigureLinkButton(bottomText.Find("PolicyButton"), new Vector2(-220f, 0f));
        ConfigureLinkButton(bottomText.Find("TermsButton"), new Vector2(220f, 0f));
    }

    private static void ConfigureLinkButton(Transform buttonTransform, Vector2 position)
    {
        if (buttonTransform == null)
            return;

        SetCenter(buttonTransform as RectTransform, position, new Vector2(280f, 58f));
        Image image = buttonTransform.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = null;
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
        }

        TMP_Text text = buttonTransform.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.fontSize = 27f;
            text.fontStyle |= FontStyles.Underline;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Hex("#8DD0FF", 1f);
            text.enableAutoSizing = true;
            text.fontSizeMin = 18f;
            text.fontSizeMax = 27f;
        }
    }

    private static Image ConfigureChildImage(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, bool preserveAspect)
    {
        GameObject child = EnsureChild(parent, name, typeof(Image));
        RectTransform rect = child.GetComponent<RectTransform>();
        SetCenter(rect, position, size);

        Image image = child.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = false;
        return image;
    }

    private static void StyleButtonText(TMP_Text text, float fontSize)
    {
        text.text = text.text.ToUpperInvariant();
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.enableAutoSizing = true;
        text.fontSizeMin = 22f;
        text.fontSizeMax = fontSize;
        text.outlineColor = Hex("#16003E", 0.85f);
        text.outlineWidth = 0.14f;
    }

    private static void DisableLayoutComponents(Transform target)
    {
        LayoutGroup[] layouts = target.GetComponents<LayoutGroup>();
        for (int i = 0; i < layouts.Length; i++)
            layouts[i].enabled = false;

        ContentSizeFitter[] fitters = target.GetComponents<ContentSizeFitter>();
        for (int i = 0; i < fitters.Length; i++)
            fitters[i].enabled = false;
    }

    private static void SetCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static GameObject EnsureChild(Transform parent, string name, params System.Type[] components)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            for (int i = 0; i < components.Length; i++)
            {
                if (existing.GetComponent(components[i]) == null)
                    existing.gameObject.AddComponent(components[i]);
            }
            return existing.gameObject;
        }

        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        for (int i = 0; i < components.Length; i++)
        {
            if (child.GetComponent(components[i]) == null)
                child.AddComponent(components[i]);
        }
        return child;
    }

    private static T FindSceneObject<T>() where T : Object
    {
        T[] objects = Resources.FindObjectsOfTypeAll<T>();
        for (int i = 0; i < objects.Length; i++)
        {
            Component component = objects[i] as Component;
            if (component != null && component.gameObject.scene.IsValid())
                return objects[i];
        }
        return null;
    }

    private static PopupSprites GenerateSprites()
    {
        string outerPath = GeneratedFolder + "/settings_outer_panel.png";
        string sectionPath = GeneratedFolder + "/settings_section_panel.png";
        string buttonPath = GeneratedFolder + "/settings_sci_fi_long_button.png";
        string squarePath = GeneratedFolder + "/settings_sci_fi_square_button.png";
        string profilePath = GeneratedFolder + "/settings_sci_fi_profile_frame.png";
        string closePath = GeneratedFolder + "/settings_sci_fi_close_hex.png";
        string closeIconPath = GeneratedFolder + "/settings_sci_fi_close_x.png";

        SaveSciFiPopupBackground(outerPath, 1000, 1120);
        SaveSciFiSection(sectionPath, 900, 420);
        SaveSciFiLongButton(buttonPath, 600, 140);
        SaveSciFiSquareButton(squarePath, 280, 280);
        SaveSciFiProfileFrame(profilePath, 330, 330);
        SaveSciFiCloseHex(closePath, 220, 220);
        SaveSciFiCloseIcon(closeIconPath, 140, 140);

        return new PopupSprites
        {
            outerPanel = LoadSprite(outerPath),
            sectionPanel = LoadSprite(sectionPath),
            squareFrame = LoadSprite(squarePath),
            buttonFrame = LoadSprite(buttonPath),
            profileFrame = LoadSprite(profilePath),
            closeFrame = LoadSprite(closePath),
            closeIcon = LoadSprite(closeIconPath)
        };
    }

    private static void SaveSciFiPopupBackground(string path, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Vector2[] panel =
        {
            new Vector2(76f, 16f),
            new Vector2(width * 0.40f, 16f),
            new Vector2(width * 0.43f, 30f),
            new Vector2(width * 0.57f, 30f),
            new Vector2(width * 0.60f, 16f),
            new Vector2(width - 76f, 16f),
            new Vector2(width - 16f, 76f),
            new Vector2(width - 16f, height - 76f),
            new Vector2(width - 76f, height - 16f),
            new Vector2(width * 0.60f, height - 16f),
            new Vector2(width * 0.57f, height - 30f),
            new Vector2(width * 0.43f, height - 30f),
            new Vector2(width * 0.40f, height - 16f),
            new Vector2(76f, height - 16f),
            new Vector2(16f, height - 76f),
            new Vector2(16f, 76f)
        };

        Vector2 planetCenter = new Vector2(width * 0.50f, height * 0.20f);
        float planetRadius = width * 0.43f;

        for (int y = 0; y < height; y++)
        {
            float vertical = y / (float)(height - 1);
            for (int x = 0; x < width; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                bool inside = IsPointInPolygon(point, panel);
                float edgeDistance = DistanceToPolygonEdge(point, panel);

                if (!inside)
                {
                    if (edgeDistance <= 42f)
                    {
                        float glowAlpha = Mathf.Pow(1f - edgeDistance / 42f, 2f) * 0.88f;
                        texture.SetPixel(x, y, WithAlpha(Hex("#3A6CFF", 1f), glowAlpha));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                    continue;
                }

                Color fill = ThreeStopGradient(
                    vertical,
                    Hex("#07112E", 0.98f),
                    Hex("#12236B", 0.98f),
                    Hex("#162B86", 0.98f));

                float center = 1f - Mathf.Clamp01(Mathf.Abs(x / (float)width - 0.5f) * 2f);
                fill = Color.Lerp(fill, Hex("#3653CF", fill.a), center * 0.16f);

                bool upperBand = IsRoundedBox(point, width * 0.50f, height * 0.61f, width * 0.82f, height * 0.31f, 12f);
                bool lowerBand = IsRoundedBox(point, width * 0.50f, height * 0.29f, width * 0.82f, height * 0.33f, 12f);
                if (upperBand || lowerBand)
                {
                    Color band = Color.Lerp(Hex("#0A123B", 0.92f), Hex("#152065", 0.92f), vertical);
                    fill = BlendOver(fill, band);
                }

                float planetDistance = Vector2.Distance(point, planetCenter);
                if (planetDistance < planetRadius)
                {
                    float planetMask = 1f - Mathf.Clamp01(planetDistance / planetRadius);
                    Color planet = Color.Lerp(Hex("#2D1D70", 0.62f), Hex("#CA5ED3", 0.72f), planetMask);
                    fill = BlendOver(fill, planet);
                }

                float ring = Mathf.Abs(planetDistance - planetRadius * 0.64f);
                if (ring < 3.5f && point.y < planetCenter.y + planetRadius * 0.24f && point.x > width * 0.18f)
                    fill = Color.Lerp(fill, Hex("#688AFF", fill.a), 0.35f);

                float grain = Hash01(x, y) - 0.5f;
                fill.r = Mathf.Clamp01(fill.r + grain * 0.04f);
                fill.g = Mathf.Clamp01(fill.g + grain * 0.035f);
                fill.b = Mathf.Clamp01(fill.b + grain * 0.07f);

                bool star = Hash01(x * 11, y * 17) > 0.9925f;
                if (star)
                    fill = Color.Lerp(fill, Color.white, 0.5f);

                Color pixel = fill;
                if (edgeDistance <= 10f)
                {
                    float edge = Mathf.Clamp01(1f - edgeDistance / 10f);
                    pixel = Color.Lerp(Hex("#5D73FF", 1f), Color.white, edge * 0.42f);
                }
                else if (edgeDistance <= 22f)
                {
                    float edge = 1f - (edgeDistance - 10f) / 12f;
                    pixel = Color.Lerp(pixel, Hex("#25AAFF", 1f), edge * 0.46f);
                }

                if (IsTopAccentPixel(point, width, height))
                {
                    pixel = Color.Lerp(pixel, Hex("#D044FF", 1f), 0.75f);
                    if (Hash01(x * 3, y * 5) > 0.82f)
                        pixel = Color.Lerp(pixel, Color.white, 0.08f);
                }

                if (IsCornerTechLine(point, width, height))
                {
                    pixel = Color.Lerp(pixel, Hex("#92A5FF", 1f), 0.78f);
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        ImportSprite(path, Vector4.zero);
    }

    private static void SaveSciFiSection(string path, int width, int height)
    {
        Vector2[] polygon =
        {
            new Vector2(34f, 0f),
            new Vector2(width - 34f, 0f),
            new Vector2(width, 34f),
            new Vector2(width, height - 34f),
            new Vector2(width - 34f, height),
            new Vector2(34f, height),
            new Vector2(0f, height - 34f),
            new Vector2(0f, 34f)
        };

        SavePolygonPanel(path, width, height, polygon, Hex("#0E1B5A", 0.68f), Hex("#050B24", 0.84f), Hex("#2C45A4", 0.78f), Hex("#222A8F", 0.55f), 4f, 12f, true);
    }

    private static void SaveSciFiLongButton(string path, int width, int height)
    {
        Vector2[] polygon =
        {
            new Vector2(42f, 0f),
            new Vector2(width - 42f, 0f),
            new Vector2(width, 34f),
            new Vector2(width, height - 34f),
            new Vector2(width - 42f, height),
            new Vector2(42f, height),
            new Vector2(0f, height - 34f),
            new Vector2(0f, 34f)
        };

        SavePolygonPanel(path, width, height, polygon, Hex("#B244FF", 0.98f), Hex("#290078", 0.98f), Hex("#F1D7FF", 1f), Hex("#783BFF", 0.95f), 10f, 22f, true);
    }

    private static void SaveSciFiSquareButton(string path, int width, int height)
    {
        Vector2[] polygon =
        {
            new Vector2(34f, 0f),
            new Vector2(width - 34f, 0f),
            new Vector2(width, 34f),
            new Vector2(width, height - 34f),
            new Vector2(width - 34f, height),
            new Vector2(34f, height),
            new Vector2(0f, height - 34f),
            new Vector2(0f, 34f)
        };

        SavePolygonPanel(path, width, height, polygon, Hex("#9A36FF", 0.98f), Hex("#1D0663", 0.98f), Hex("#F0D7FF", 1f), Hex("#3158FF", 0.92f), 10f, 24f, true);
    }

    private static void SaveSciFiProfileFrame(string path, int width, int height)
    {
        Vector2[] polygon =
        {
            new Vector2(44f, 0f),
            new Vector2(width - 44f, 0f),
            new Vector2(width, 44f),
            new Vector2(width, height - 44f),
            new Vector2(width - 44f, height),
            new Vector2(44f, height),
            new Vector2(0f, height - 44f),
            new Vector2(0f, 44f)
        };

        SavePolygonPanel(path, width, height, polygon, Hex("#5322AF", 0.52f), Hex("#110730", 0.80f), Hex("#F3DDFF", 1f), Hex("#B943FF", 1f), 14f, 28f, true);
    }

    private static void SaveSciFiCloseHex(string path, int width, int height)
    {
        float margin = 14f;
        float half = height * 0.5f;
        Vector2[] polygon =
        {
            new Vector2(width * 0.28f, margin),
            new Vector2(width * 0.72f, margin),
            new Vector2(width - margin, half),
            new Vector2(width * 0.72f, height - margin),
            new Vector2(width * 0.28f, height - margin),
            new Vector2(margin, half)
        };

        SavePolygonPanel(path, width, height, polygon, Hex("#2835A6", 0.96f), Hex("#18005E", 0.98f), Hex("#A9B5FF", 1f), Hex("#5438FF", 0.9f), 9f, 20f, true);
    }

    private static void SaveSciFiCloseIcon(string path, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Vector2 a = new Vector2(width * 0.28f, height * 0.28f);
        Vector2 b = new Vector2(width * 0.72f, height * 0.72f);
        Vector2 c = new Vector2(width * 0.72f, height * 0.28f);
        Vector2 d = new Vector2(width * 0.28f, height * 0.72f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                float distance = Mathf.Min(DistanceToSegment(point, a, b), DistanceToSegment(point, c, d));
                if (distance > 16f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float alpha = distance <= 8f ? 1f : 1f - (distance - 8f) / 8f;
                Color color = Color.Lerp(Hex("#E9F0FF", alpha), Color.white, 0.45f);
                color.a = alpha;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        ImportSprite(path, Vector4.zero);
    }

    private static void SavePolygonPanel(string path, int width, int height, Vector2[] polygon, Color fillTop, Color fillBottom, Color border, Color glow, float borderWidth, float glowWidth, bool textureNoise)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            float vertical = y / (float)(height - 1);
            for (int x = 0; x < width; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                bool inside = IsPointInPolygon(point, polygon);
                float edgeDistance = DistanceToPolygonEdge(point, polygon);

                if (!inside)
                {
                    if (edgeDistance <= glowWidth)
                    {
                        float alpha = Mathf.Pow(1f - edgeDistance / glowWidth, 2.2f) * glow.a;
                        texture.SetPixel(x, y, WithAlpha(glow, alpha));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                    continue;
                }

                Color fill = Color.Lerp(fillBottom, fillTop, vertical);
                float centerGlow = 1f - Mathf.Clamp01(Mathf.Abs(x / (float)width - 0.5f) * 2f);
                fill = Color.Lerp(fill, Hex("#8A35FF", fill.a), centerGlow * 0.2f);

                if (textureNoise)
                {
                    float grain = Hash01(x, y) - 0.5f;
                    fill.r = Mathf.Clamp01(fill.r + grain * 0.035f);
                    fill.g = Mathf.Clamp01(fill.g + grain * 0.025f);
                    fill.b = Mathf.Clamp01(fill.b + grain * 0.055f);
                }

                Color pixel = fill;
                if (edgeDistance <= borderWidth)
                {
                    float edge = Mathf.Clamp01(1f - edgeDistance / borderWidth);
                    pixel = Color.Lerp(border, Color.white, edge * 0.32f);
                }
                else if (edgeDistance <= borderWidth + 16f)
                {
                    float edge = 1f - (edgeDistance - borderWidth) / 16f;
                    pixel = Color.Lerp(fill, glow, edge * 0.42f);
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        ImportSprite(path, Vector4.zero);
    }

    private static Color ThreeStopGradient(float t, Color bottom, Color middle, Color top)
    {
        if (t < 0.55f)
            return Color.Lerp(bottom, middle, t / 0.55f);

        return Color.Lerp(middle, top, (t - 0.55f) / 0.45f);
    }

    private static float RoundedRectSignedDistance(Vector2 point, float width, float height, float radius, float margin)
    {
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        Vector2 half = new Vector2(width * 0.5f - margin, height * 0.5f - margin);
        Vector2 local = new Vector2(Mathf.Abs(point.x - center.x), Mathf.Abs(point.y - center.y));
        Vector2 q = local - (half - new Vector2(radius, radius));
        Vector2 outside = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f));
        return outside.magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
    }

    private static bool IsRoundedBox(Vector2 point, float centerX, float centerY, float boxWidth, float boxHeight, float radius)
    {
        Vector2 center = new Vector2(centerX, centerY);
        Vector2 half = new Vector2(boxWidth * 0.5f, boxHeight * 0.5f);
        Vector2 local = new Vector2(Mathf.Abs(point.x - center.x), Mathf.Abs(point.y - center.y));
        Vector2 q = local - (half - new Vector2(radius, radius));
        Vector2 outside = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f));
        float distance = outside.magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
        return distance <= 0f;
    }

    private static bool IsTopAccentPixel(Vector2 point, int width, int height)
    {
        bool top = point.y > height * 0.925f && point.y < height * 0.955f && point.x > width * 0.38f && point.x < width * 0.62f;
        bool bottom = point.y > height * 0.045f && point.y < height * 0.075f && point.x > width * 0.38f && point.x < width * 0.62f;
        if (!top && !bottom)
            return false;

        float leftEdge = width * 0.38f;
        float rightEdge = width * 0.62f;
        float bevel = width * 0.035f;
        float x = point.x;
        float y = top ? point.y - height * 0.925f : point.y - height * 0.045f;
        float bandHeight = height * 0.03f;
        if (x < leftEdge + bevel && y < (leftEdge + bevel - x) * 0.48f)
            return false;
        if (x > rightEdge - bevel && y < (x - (rightEdge - bevel)) * 0.48f)
            return false;
        if (x < leftEdge + bevel && y > bandHeight - (leftEdge + bevel - x) * 0.48f)
            return false;
        if (x > rightEdge - bevel && y > bandHeight - (x - (rightEdge - bevel)) * 0.48f)
            return false;

        return true;
    }

    private static bool IsCornerTechLine(Vector2 point, int width, int height)
    {
        bool left = point.x > width * 0.07f && point.x < width * 0.24f;
        bool right = point.x > width * 0.76f && point.x < width * 0.93f;
        bool top = point.y > height * 0.88f && point.y < height * 0.895f;
        bool bottom = point.y > height * 0.105f && point.y < height * 0.12f;
        return (left || right) && (top || bottom);
    }

    private static Color BlendOver(Color under, Color over)
    {
        float alpha = over.a + under.a * (1f - over.a);
        if (alpha <= 0.0001f)
            return Color.clear;

        Color result = (over * over.a + under * under.a * (1f - over.a)) / alpha;
        result.a = alpha;
        return result;
    }

    private static void SaveRoundedPanel(string path, int width, int height, float radius, Color fillTop, Color fillBottom, Color outline, float outlineWidth)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            float vertical = y / (float)(height - 1);
            Color fill = Color.Lerp(fillBottom, fillTop, vertical);
            for (int x = 0; x < width; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                if (!IsInRoundedRect(point, width, height, radius))
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float edge = Mathf.Min(Mathf.Min(x + 0.5f, width - x - 0.5f), Mathf.Min(y + 0.5f, height - y - 0.5f));
                Color pixel = edge <= outlineWidth ? outline : fill;
                if (edge > outlineWidth && edge < outlineWidth + 18f)
                {
                    float blend = Mathf.InverseLerp(outlineWidth + 18f, outlineWidth, edge);
                    pixel = Color.Lerp(fill, outline, blend * 0.4f);
                }
                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        ImportSprite(path, new Vector4(radius, radius, radius, radius));
    }

    private static bool IsInRoundedRect(Vector2 point, float width, float height, float radius)
    {
        Vector2 half = new Vector2(width * 0.5f, height * 0.5f);
        Vector2 local = new Vector2(Mathf.Abs(point.x - half.x), Mathf.Abs(point.y - half.y));
        Vector2 inner = half - new Vector2(radius, radius);
        if (local.x <= inner.x || local.y <= inner.y)
            return local.x <= half.x && local.y <= half.y;
        return Vector2.Distance(local, inner) <= radius;
    }

    private static void ImportSprite(string path, Vector4 border)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = 100f;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }

    private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            bool intersects = ((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y + 0.0001f) + polygon[i].x;
            if (intersects)
                inside = !inside;
        }
        return inside;
    }

    private static float DistanceToPolygonEdge(Vector2 point, Vector2[] polygon)
    {
        float minDistance = float.MaxValue;
        for (int i = 0; i < polygon.Length; i++)
        {
            float distance = DistanceToSegment(point, polygon[i], polygon[(i + 1) % polygon.Length]);
            if (distance < minDistance)
                minDistance = distance;
        }
        return minDistance;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float denominator = Vector2.Dot(ab, ab);
        if (denominator <= 0.0001f)
            return Vector2.Distance(point, a);

        float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / denominator);
        return Vector2.Distance(point, a + ab * t);
    }

    private static float Hash01(int x, int y)
    {
        unchecked
        {
            int hash = x * 374761393 + y * 668265263;
            hash = (hash ^ (hash >> 13)) * 1274126177;
            hash ^= hash >> 16;
            return (hash & 0x7fffffff) / 2147483647f;
        }
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = Mathf.Clamp01(alpha);
        return color;
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Color Hex(string html, float alpha)
    {
        Color color;
        ColorUtility.TryParseHtmlString(html, out color);
        color.a = alpha;
        return color;
    }

    private struct PopupSprites
    {
        public Sprite outerPanel;
        public Sprite sectionPanel;
        public Sprite squareFrame;
        public Sprite buttonFrame;
        public Sprite profileFrame;
        public Sprite closeFrame;
        public Sprite closeIcon;
    }
}
