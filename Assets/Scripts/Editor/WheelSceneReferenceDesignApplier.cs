using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class WheelSceneReferenceDesignApplier
{
    private const string GeneratedFolder = "Assets/Generated/WheelDesign";

    [MenuItem("Tools/Wheel/Apply Reference Wheel Design")]
    public static void ApplyFromMenu()
    {
        Debug.Log(Apply());
    }

    public static string Apply()
    {
        System.IO.Directory.CreateDirectory(GeneratedFolder);

        string spinPath = GeneratedFolder + "/spin_hexagon.png";
        string panelPath = GeneratedFolder + "/countdown_panel.png";
        string badgePath = GeneratedFolder + "/reward_hex_frame.png";

        SavePolygonTexture(
            spinPath,
            256,
            256,
            new[]
            {
                new Vector2(128, 12), new Vector2(232, 68), new Vector2(232, 188),
                new Vector2(128, 244), new Vector2(24, 188), new Vector2(24, 68)
            },
            Hex("28105E", 0.92f),
            Hex("A263FF", 0.95f),
            11f);

        SavePolygonTexture(
            panelPath,
            640,
            180,
            new[]
            {
                new Vector2(80, 12), new Vector2(560, 12), new Vector2(628, 90), new Vector2(560, 168),
                new Vector2(80, 168), new Vector2(12, 90)
            },
            Hex("28105E", 0.92f),
            Hex("A263FF", 0.95f),
            8f);

        SavePolygonTexture(
            badgePath,
            128,
            128,
            new[]
            {
                new Vector2(64, 8), new Vector2(112, 36), new Vector2(112, 92),
                new Vector2(64, 120), new Vector2(16, 92), new Vector2(16, 36)
            },
            Hex("15135F", 0.34f),
            Hex("83E6FF", 0.95f),
            7f);

        Sprite spinSprite = LoadSprite(spinPath);
        Sprite panelSprite = LoadSprite(panelPath);
        Sprite badgeSprite = LoadSprite(badgePath);
        Sprite backgroundSprite = LoadSprite("Assets/Resources/Sprites/background.png");
        Sprite wheelBackgroundSprite = LoadSprite("Assets/Resources/Sprites/Wheel/wheelbackground2.png");
        Sprite wheelOutlineSprite = LoadSprite("Assets/Resources/Sprites/Wheel/wheelOutline.png");
        Sprite pointerSprite = LoadSprite("Assets/Resources/Sprites/Wheel/arrow.png");

        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject == null)
            return "WheelScene redesign failed: Canvas was not found.";

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main != null ? Camera.main : Object.FindObjectOfType<Camera>();
            canvas.planeDistance = 1f;
            canvas.sortingOrder = 0;
        }

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        GameObject background = EnsureChild(canvasObject.transform, "WheelReferenceBackground");
        background.transform.SetSiblingIndex(0);
        SetStretch(RectOf(background));
        Image backgroundImage = EnsureImage(background);
        backgroundImage.sprite = backgroundSprite;
        backgroundImage.color = Color.white;
        backgroundImage.type = Image.Type.Simple;
        backgroundImage.preserveAspect = false;
        backgroundImage.raycastTarget = false;

        Transform screen = canvasObject.transform.Find("WheelScreen");
        if (screen == null)
            return "WheelScene redesign failed: WheelScreen was not found.";
        SetStretch(RectOf(screen.gameObject));
        screen.SetSiblingIndex(1);

        Transform content = screen.Find("Content");
        if (content == null)
            return "WheelScene redesign failed: WheelScreen/Content was not found.";
        SetStretch(RectOf(content.gameObject));
        DisableContentLayout(content);

        Transform header = content.Find("Header");
        if (header != null)
            header.gameObject.SetActive(false);

        Transform wheelArea = content.Find("WheelArea");
        if (wheelArea == null)
            return "WheelScene redesign failed: WheelArea was not found.";
        SetStretch(RectOf(wheelArea.gameObject));

        Transform wheelRoot = wheelArea.Find("WheelRoot");
        if (wheelRoot == null)
            return "WheelScene redesign failed: WheelRoot was not found.";
        SetCenter(RectOf(wheelRoot.gameObject), new Vector2(0f, 285f), new Vector2(1000f, 1000f));

        ConfigureWheelBase(wheelRoot, wheelBackgroundSprite);
        ConfigureSegments(wheelRoot);
        ConfigureStaticWheelChrome(wheelArea, wheelOutlineSprite, pointerSprite);
        ConfigureSpinButton(wheelArea, spinSprite);
        ConfigureCooldownPanel(wheelArea, panelSprite);
        RemoveBottomNav(canvasObject.transform);
        ConfigureRewardPreview(badgeSprite);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        return "WheelScene redesign applied and saved.";
    }

    private static void ConfigureWheelBase(Transform wheelRoot, Sprite wheelBackgroundSprite)
    {
        Transform wheelImageTransform = wheelRoot.Find("Wheel");
        if (wheelImageTransform == null)
            return;

        SetCenter(RectOf(wheelImageTransform.gameObject), Vector2.zero, new Vector2(1030f, 1030f));
        Image wheelImage = EnsureImage(wheelImageTransform.gameObject);
        wheelImage.sprite = wheelBackgroundSprite;
        wheelImage.color = Hex("FFFFFF", 0.86f);
        wheelImage.raycastTarget = false;
    }

    private static void ConfigureSegments(Transform wheelRoot)
    {
        GameObject segmentRoot = EnsureChild(wheelRoot, "ReferenceWheelSegments");
        ClearChildren(segmentRoot.transform);
        SetCenter(RectOf(segmentRoot), Vector2.zero, new Vector2(1000f, 1000f));
        segmentRoot.transform.SetSiblingIndex(1);

        Color[] segmentColors =
        {
            Hex("0D72CE", 0.88f), Hex("3C0BA0", 0.94f), Hex("2B086F", 0.96f), Hex("420996", 0.94f),
            Hex("25075E", 0.96f), Hex("390584", 0.94f), Hex("2A096F", 0.96f), Hex("4A0AA8", 0.94f)
        };

        for (int i = 0; i < 8; i++)
        {
            GameObject segment = new GameObject("Segment_" + (i + 1).ToString("00"), typeof(RectTransform), typeof(CanvasRenderer), typeof(WheelSegmentGraphic));
            segment.transform.SetParent(segmentRoot.transform, false);
            SetCenter(RectOf(segment), Vector2.zero, new Vector2(1000f, 1000f));
            WheelSegmentGraphic graphic = segment.GetComponent<WheelSegmentGraphic>();
            graphic.color = segmentColors[i];
            float centerAngle = 90f - (i * 45f);
            graphic.Configure(centerAngle - 22.5f, centerAngle + 22.5f, 0.08f, 0.465f, 1.2f);
            graphic.raycastTarget = false;
        }

        GameObject separatorRoot = EnsureChild(wheelRoot, "ReferenceWheelSeparators");
        ClearChildren(separatorRoot.transform);
        SetCenter(RectOf(separatorRoot), Vector2.zero, new Vector2(1000f, 1000f));
        separatorRoot.transform.SetSiblingIndex(2);

        for (int i = 0; i < 8; i++)
        {
            float angle = 112.5f - (i * 45f);
            GameObject line = new GameObject("Separator_" + (i + 1).ToString("00"), typeof(RectTransform), typeof(Image));
            line.transform.SetParent(separatorRoot.transform, false);
            RectTransform lineRect = RectOf(line);
            lineRect.anchorMin = new Vector2(0.5f, 0.5f);
            lineRect.anchorMax = new Vector2(0.5f, 0.5f);
            lineRect.pivot = new Vector2(0.5f, 0f);
            lineRect.anchoredPosition = Vector2.zero;
            lineRect.sizeDelta = new Vector2(5f, 455f);
            lineRect.localScale = Vector3.one;
            lineRect.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);

            Image lineImage = line.GetComponent<Image>();
            lineImage.color = Hex("160034", 0.78f);
            lineImage.raycastTarget = false;
        }

        Transform slicesContainer = wheelRoot.Find("SlicesContainer");
        if (slicesContainer != null)
        {
            SetCenter(RectOf(slicesContainer.gameObject), Vector2.zero, new Vector2(1000f, 1000f));
            slicesContainer.SetSiblingIndex(3);
        }
    }

    private static void ConfigureStaticWheelChrome(Transform wheelArea, Sprite wheelOutlineSprite, Sprite pointerSprite)
    {
        GameObject halo = EnsureChild(wheelArea, "WheelReferenceOuterHalo");
        SetCenter(RectOf(halo), new Vector2(0f, 285f), new Vector2(1110f, 1110f));
        Image haloImage = EnsureImage(halo);
        haloImage.sprite = wheelOutlineSprite;
        haloImage.color = Hex("B965FF", 0.34f);
        haloImage.raycastTarget = false;
        halo.transform.SetSiblingIndex(0);

        Transform outline = wheelArea.Find("WheelOutline");
        if (outline != null)
        {
            SetCenter(RectOf(outline.gameObject), new Vector2(0f, 285f), new Vector2(1060f, 1060f));
            Image outlineImage = EnsureImage(outline.gameObject);
            outlineImage.sprite = wheelOutlineSprite;
            outlineImage.color = Hex("FFFFFF", 0.98f);
            outlineImage.raycastTarget = false;
        }

        Transform pointer = wheelArea.Find("Pointer");
        if (pointer != null)
        {
            SetCenter(RectOf(pointer.gameObject), new Vector2(0f, 830f), new Vector2(135f, 190f));
            Image pointerImage = EnsureImage(pointer.gameObject);
            pointerImage.sprite = pointerSprite;
            pointerImage.color = Color.white;
            pointerImage.preserveAspect = true;
            pointerImage.raycastTarget = false;
        }

        GameObject floatingGem = EnsureChild(wheelArea, "WheelReferenceFloatingGem");
        SetCenter(RectOf(floatingGem), new Vector2(380f, 245f), new Vector2(72f, 96f));
        floatingGem.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
        Image floatingGemImage = EnsureImage(floatingGem);
        floatingGemImage.sprite = pointerSprite;
        floatingGemImage.color = Hex("FF31F4", 0.96f);
        floatingGemImage.preserveAspect = true;
        floatingGemImage.raycastTarget = false;
    }

    private static void ConfigureSpinButton(Transform wheelArea, Sprite spinSprite)
    {
        Transform spinButton = wheelArea.Find("SpinButton");
        if (spinButton == null)
            return;

        SetCenter(RectOf(spinButton.gameObject), new Vector2(0f, 285f), new Vector2(300f, 300f));
        Image spinImage = EnsureImage(spinButton.gameObject);
        spinImage.sprite = spinSprite;
        spinImage.color = Color.white;
        spinImage.preserveAspect = true;
        spinImage.raycastTarget = true;

        Button button = spinButton.GetComponent<Button>();
        if (button != null)
            button.targetGraphic = spinImage;

        TMP_Text spinLabel = spinButton.GetComponentInChildren<TMP_Text>(true);
        if (spinLabel == null)
        {
            GameObject labelObject = EnsureChild(spinButton, "SpinLabel");
            spinLabel = EnsureText(labelObject);
        }

        SetCenter(RectOf(spinLabel.gameObject), Vector2.zero, new Vector2(230f, 95f));
        ConfigureText(spinLabel, "SPIN", 50f, Color.white, TextAlignmentOptions.Center);
    }

    private static void ConfigureCooldownPanel(Transform wheelArea, Sprite panelSprite)
    {
        GameObject cooldownPanel = EnsureChild(wheelArea, "WheelReferenceCooldownPanel");
        SetCenter(RectOf(cooldownPanel), new Vector2(0f, -365f), new Vector2(560f, 150f));
        Image cooldownPanelImage = EnsureImage(cooldownPanel);
        cooldownPanelImage.sprite = panelSprite;
        cooldownPanelImage.color = Color.white;
        cooldownPanelImage.raycastTarget = false;

        GameObject cooldownLabel = EnsureChild(cooldownPanel.transform, "CooldownLabel");
        SetCenter(RectOf(cooldownLabel), new Vector2(0f, 35f), new Vector2(470f, 36f));
        TextMeshProUGUI cooldownLabelText = EnsureText(cooldownLabel);
        ConfigureText(cooldownLabelText, "NEXT FREE SPIN IN", 22f, Hex("E2C9FF", 1f), TextAlignmentOptions.Center);

        Transform cooldownText = wheelArea.Find("CooldownText");
        if (cooldownText == null)
            cooldownText = cooldownPanel.transform.Find("CooldownText");

        if (cooldownText != null)
        {
            cooldownText.SetParent(cooldownPanel.transform, false);
            SetCenter(RectOf(cooldownText.gameObject), new Vector2(0f, -24f), new Vector2(500f, 72f));
            TMP_Text tmp = cooldownText.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                ConfigureText(tmp, "04:00:00", 48f, Color.white, TextAlignmentOptions.Center);
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 28f;
                tmp.fontSizeMax = 48f;
                tmp.overflowMode = TextOverflowModes.Overflow;
            }
        }
    }

    private static void RemoveBottomNav(Transform canvasTransform)
    {
        Transform navBar = canvasTransform.Find("WheelReferenceBottomNav");
        if (navBar != null)
            Object.DestroyImmediate(navBar.gameObject);
    }

    private static void ConfigureRewardPreview(Sprite badgeSprite)
    {
        WheelSlicesBuilder builder = FindSceneObject<WheelSlicesBuilder>();
        WheelConfig wheelConfig = null;
        WheelSliceView slicePrefab = null;
        RectTransform builderContainer = null;

        if (builder != null)
        {
            SerializedObject builderObject = new SerializedObject(builder);
            SerializedProperty configProperty = builderObject.FindProperty("wheelConfig");
            SerializedProperty prefabProperty = builderObject.FindProperty("slicePrefab");
            SerializedProperty containerProperty = builderObject.FindProperty("slicesContainer");
            SerializedProperty radiusProperty = builderObject.FindProperty("rewardRadius");
            SerializedProperty sizeProperty = builderObject.FindProperty("rewardViewSize");
            SerializedProperty angleProperty = builderObject.FindProperty("firstRewardAngle");

            if (configProperty != null)
                wheelConfig = configProperty.objectReferenceValue as WheelConfig;
            if (prefabProperty != null)
                slicePrefab = prefabProperty.objectReferenceValue as WheelSliceView;
            if (containerProperty != null)
                builderContainer = containerProperty.objectReferenceValue as RectTransform;
            if (radiusProperty != null)
                radiusProperty.floatValue = 342f;
            if (sizeProperty != null)
                sizeProperty.vector2Value = new Vector2(160f, 130f);
            if (angleProperty != null)
                angleProperty.floatValue = 90f;

            builderObject.ApplyModifiedPropertiesWithoutUndo();
        }

        StyleWheelSlicePrefab(slicePrefab, badgeSprite);

        if (builderContainer == null || slicePrefab == null || wheelConfig == null || wheelConfig.rewards == null || wheelConfig.rewards.Count <= 0)
            return;

        ClearChildren(builderContainer);
        WheelRewardIconProvider previewIconProvider = new WheelRewardIconProvider();
        float angleStep = 360f / wheelConfig.rewards.Count;

        for (int i = 0; i < wheelConfig.rewards.Count; i++)
        {
            WheelRewardDto reward = wheelConfig.rewards[i];
            GameObject sliceObject = PrefabUtility.InstantiatePrefab(slicePrefab.gameObject, builderContainer) as GameObject;
            WheelSliceView slice = sliceObject != null ? sliceObject.GetComponent<WheelSliceView>() : Object.Instantiate(slicePrefab, builderContainer);
            if (slice == null)
                continue;

            slice.name = "RewardBadge_" + (i + 1).ToString("00");
            slice.SetTitle(WheelRewardTextFormatter.Format(reward));
            slice.SetIcon(previewIconProvider.GetIcon(reward));

            RectTransform sliceRect = slice.GetComponent<RectTransform>();
            float angle = 90f - (i * angleStep);
            float radians = angle * Mathf.Deg2Rad;
            SetCenter(sliceRect, new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * 342f, new Vector2(160f, 130f));
        }
    }

    private static void StyleWheelSlicePrefab(WheelSliceView prefab, Sprite frameSprite)
    {
        if (prefab == null)
            return;

        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(prefabPath))
            return;

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        if (rootRect != null)
            rootRect.sizeDelta = new Vector2(160f, 130f);

        GameObject frame = EnsureChild(root.transform, "IconFrame");
        SetCenter(RectOf(frame), new Vector2(0f, 22f), new Vector2(92f, 92f));
        frame.transform.SetSiblingIndex(0);
        Image frameImage = EnsureImage(frame);
        frameImage.sprite = frameSprite;
        frameImage.color = Color.white;
        frameImage.preserveAspect = true;
        frameImage.raycastTarget = false;

        Transform iconTransform = root.transform.Find("Icon");
        Image iconImage = null;
        if (iconTransform != null)
        {
            SetCenter(RectOf(iconTransform.gameObject), new Vector2(0f, 22f), new Vector2(62f, 62f));
            iconTransform.SetSiblingIndex(1);
            iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
                iconImage.color = Color.white;
            }
        }

        Transform textTransform = root.transform.Find("Text");
        TMP_Text titleText = null;
        if (textTransform != null)
        {
            SetCenter(RectOf(textTransform.gameObject), new Vector2(0f, -48f), new Vector2(160f, 46f));
            textTransform.SetSiblingIndex(2);
            titleText = textTransform.GetComponent<TMP_Text>();
            if (titleText != null)
                ConfigureText(titleText, titleText.text, 28f, Color.white, TextAlignmentOptions.Center);
        }

        WheelSliceView view = root.GetComponent<WheelSliceView>();
        if (view != null)
        {
            SerializedObject viewObject = new SerializedObject(view);
            SerializedProperty iconProperty = viewObject.FindProperty("iconImage");
            SerializedProperty textProperty = viewObject.FindProperty("titleText");
            if (iconProperty != null)
                iconProperty.objectReferenceValue = iconImage;
            if (textProperty != null)
                textProperty.objectReferenceValue = titleText;
            viewObject.ApplyModifiedPropertiesWithoutUndo();
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static T FindSceneObject<T>() where T : Object
    {
        T[] objects = Resources.FindObjectsOfTypeAll<T>();
        for (int i = 0; i < objects.Length; i++)
        {
            Component component = objects[i] as Component;
            if (component != null && component.gameObject.scene.IsValid())
                return objects[i];

            GameObject gameObject = objects[i] as GameObject;
            if (gameObject != null && gameObject.scene.IsValid())
                return objects[i];
        }

        return null;
    }

    private static void DisableContentLayout(Transform content)
    {
        LayoutGroup[] layoutGroups = content.GetComponents<LayoutGroup>();
        for (int i = 0; i < layoutGroups.Length; i++)
            layoutGroups[i].enabled = false;

        ContentSizeFitter[] fitters = content.GetComponents<ContentSizeFitter>();
        for (int i = 0; i < fitters.Length; i++)
            fitters[i].enabled = false;
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject EnsureChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
            return child.gameObject;

        GameObject created = new GameObject(name, typeof(RectTransform));
        created.transform.SetParent(parent, false);
        return created;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    private static RectTransform RectOf(GameObject gameObject)
    {
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        if (rect == null)
            rect = gameObject.AddComponent<RectTransform>();
        return rect;
    }

    private static void SetStretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void SetCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void SetBottomStretch(RectTransform rect, float height)
    {
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, height);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static Image EnsureImage(GameObject gameObject)
    {
        Image image = gameObject.GetComponent<Image>();
        if (image == null)
            image = gameObject.AddComponent<Image>();
        return image;
    }

    private static TextMeshProUGUI EnsureText(GameObject gameObject)
    {
        TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
        if (text == null)
            text = gameObject.AddComponent<TextMeshProUGUI>();
        return text;
    }

    private static void ConfigureText(TMP_Text text, string value, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.outlineColor = Hex("12002C", 0.95f);
        text.outlineWidth = 0.18f;
    }

    private static Color Hex(string value, float alpha)
    {
        if (!value.StartsWith("#"))
            value = "#" + value;

        Color color;
        if (!ColorUtility.TryParseHtmlString(value, out color))
            color = Color.white;

        color.a = alpha;
        return color;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float denominator = Vector2.Dot(ab, ab);
        if (denominator <= 0.0001f)
            return Vector2.Distance(point, a);

        float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / denominator);
        return Vector2.Distance(point, a + (ab * t));
    }

    private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            bool intersects = ((polygon[i].y > point.y) != (polygon[j].y > point.y))
                && (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y + 0.00001f) + polygon[i].x);

            if (intersects)
                inside = !inside;
        }

        return inside;
    }

    private static void SavePolygonTexture(string path, int width, int height, Vector2[] polygon, Color fill, Color outline, float outlineWidth)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                if (!PointInPolygon(point, polygon))
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float edgeDistance = float.MaxValue;
                for (int i = 0; i < polygon.Length; i++)
                {
                    Vector2 a = polygon[i];
                    Vector2 b = polygon[(i + 1) % polygon.Length];
                    edgeDistance = Mathf.Min(edgeDistance, DistanceToSegment(point, a, b));
                }

                Color pixel = edgeDistance <= outlineWidth ? outline : fill;
                if (edgeDistance > outlineWidth && edgeDistance < outlineWidth + 10f)
                {
                    float blend = Mathf.InverseLerp(outlineWidth + 10f, outlineWidth, edgeDistance);
                    pixel = Color.Lerp(fill, outline, blend * 0.35f);
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        ImportGeneratedSprite(path);
    }

    private static void ImportGeneratedSprite(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = 100f;
        importer.SaveAndReimport();
    }
}
