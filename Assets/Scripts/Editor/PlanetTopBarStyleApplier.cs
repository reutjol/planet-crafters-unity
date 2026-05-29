using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PlanetTopBarStyleApplier
{
    private const string GeneratedFolder = "Assets/Generated/TopBarStyle";

    [MenuItem("Tools/Planet/Apply Top Bar Style")]
    public static void ApplyFromMenu()
    {
        Debug.Log(Apply());
    }

    public static string Apply()
    {
        if (SceneManager.GetActiveScene().name != "PlanetScene")
            return "PlanetScene is not active; no changes applied.";

        System.IO.Directory.CreateDirectory(GeneratedFolder);
        TopBarSprites sprites = GenerateSprites();

        GameObject topBarObject = GameObject.Find("MainUICanvas/TopBar");
        if (topBarObject == null)
            return "TopBar not found.";

        Transform topBar = topBarObject.transform;
        ConfigureTopBarRoot(topBarObject);
        ConfigureTopPanel(topBar, sprites.topPanel);
        ConfigureProfileArea(topBarObject, topBar, sprites.avatarFrame, sprites.levelBadge);
        ConfigureMoneyArea(topBar, sprites.moneyPanel);
        ConfigureSettingsButton(topBar, sprites.settingsFrame);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        return "Planet top bar layout applied. Money scripts were not modified.";
    }

    private static void ConfigureTopBarRoot(GameObject topBarObject)
    {
        RectTransform topRect = topBarObject.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0f, 1f);
        topRect.anchorMax = new Vector2(1f, 1f);
        topRect.pivot = new Vector2(0.5f, 1f);
        topRect.anchoredPosition = Vector2.zero;
        topRect.sizeDelta = new Vector2(0f, 260f);

        Image topImage = topBarObject.GetComponent<Image>();
        if (topImage != null)
            topImage.color = new Color(1f, 1f, 1f, 0f);
    }

    private static void ConfigureTopPanel(Transform topBar, Sprite topPanelSprite)
    {
        GameObject panel = EnsureChild(topBar, "TopBarPanel", typeof(Image));
        panel.transform.SetAsFirstSibling();

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, -8f);
        panelRect.sizeDelta = new Vector2(-64f, 220f);

        Image panelImage = panel.GetComponent<Image>();
        panelImage.sprite = topPanelSprite;
        panelImage.type = Image.Type.Sliced;
        panelImage.color = Color.white;
        panelImage.raycastTarget = false;
    }

    private static void ConfigureProfileArea(GameObject topBarObject, Transform topBar, Sprite avatarFrameSprite, Sprite levelBadgeSprite)
    {
        Transform profileButton = topBar.Find("ProfileButton");
        if (profileButton == null)
            return;

        profileButton.SetAsLastSibling();
        RectTransform profileRect = profileButton as RectTransform;
        profileRect.anchorMin = new Vector2(0f, 0.5f);
        profileRect.anchorMax = new Vector2(0f, 0.5f);
        profileRect.pivot = new Vector2(0.5f, 0.5f);
        profileRect.anchoredPosition = new Vector2(150f, -8f);
        profileRect.sizeDelta = new Vector2(210f, 210f);

        Image rootImage = profileButton.GetComponent<Image>();
        Sprite avatarSprite = rootImage != null ? rootImage.sprite : null;
        if (rootImage != null)
        {
            rootImage.sprite = null;
            rootImage.color = new Color(1f, 1f, 1f, 0f);
            rootImage.raycastTarget = true;
        }

        Image frameImage = ConfigureChildImage(profileButton, "AvatarFrame", avatarFrameSprite, Vector2.zero, new Vector2(210f, 190f), false);
        frameImage.transform.SetAsFirstSibling();

        Image avatarImage = ConfigureChildImage(profileButton, "AvatarImage", avatarSprite, new Vector2(-8f, -4f), new Vector2(146f, 146f), true);
        avatarImage.transform.SetAsLastSibling();

        GameObject badge = EnsureChild(profileButton, "LevelBadge", typeof(Image), typeof(PlayerLevelBadgeView), typeof(PlayerLevelBadgeController));
        badge.transform.SetAsLastSibling();
        RectTransform badgeRect = badge.GetComponent<RectTransform>();
        SetCentered(badgeRect, new Vector2(78f, -62f), new Vector2(94f, 84f));

        Image badgeImage = badge.GetComponent<Image>();
        badgeImage.sprite = levelBadgeSprite;
        badgeImage.color = Color.white;
        badgeImage.raycastTarget = false;

        TextMeshProUGUI levelText = ConfigureLevelText(badge.transform);
        ServerPlayerLevelProvider provider = EnsureComponent<ServerPlayerLevelProvider>(topBarObject);
        PlayerLevelBadgeView badgeView = badge.GetComponent<PlayerLevelBadgeView>();
        PlayerLevelBadgeController badgeController = badge.GetComponent<PlayerLevelBadgeController>();

        SerializedObject viewObject = new SerializedObject(badgeView);
        SerializedProperty levelTextProperty = viewObject.FindProperty("levelText");
        if (levelTextProperty != null)
            levelTextProperty.objectReferenceValue = levelText;
        viewObject.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject controllerObject = new SerializedObject(badgeController);
        SerializedProperty providerProperty = controllerObject.FindProperty("levelProviderBehaviour");
        SerializedProperty viewProperty = controllerObject.FindProperty("view");
        if (providerProperty != null)
            providerProperty.objectReferenceValue = provider;
        if (viewProperty != null)
            viewProperty.objectReferenceValue = badgeView;
        controllerObject.ApplyModifiedPropertiesWithoutUndo();
        badgeController.Refresh();

        TopBarProfileController profileController = topBarObject.GetComponent<TopBarProfileController>();
        if (profileController != null)
        {
            SerializedObject profileObject = new SerializedObject(profileController);
            SerializedProperty imageProperty = profileObject.FindProperty("profileImage");
            if (imageProperty != null)
                imageProperty.objectReferenceValue = avatarImage;
            profileObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static TextMeshProUGUI ConfigureLevelText(Transform badge)
    {
        GameObject textObject = EnsureChild(badge, "LevelText", typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI levelText = textObject.GetComponent<TextMeshProUGUI>();
        levelText.text = "12";
        levelText.fontSize = 42f;
        levelText.fontStyle = FontStyles.Bold;
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.color = Color.white;
        levelText.raycastTarget = false;
        levelText.enableAutoSizing = true;
        levelText.fontSizeMin = 22f;
        levelText.fontSizeMax = 42f;
        levelText.outlineColor = Hex("#050515", 0.85f);
        levelText.outlineWidth = 0.18f;
        return levelText;
    }

    private static void ConfigureMoneyArea(Transform topBar, Sprite moneyPanelSprite)
    {
        Transform moneyContainer = topBar.Find("MoneyContainer");
        if (moneyContainer == null)
            return;

        moneyContainer.SetAsLastSibling();
        RectTransform moneyRect = moneyContainer as RectTransform;
        moneyRect.anchorMin = new Vector2(0.5f, 0.5f);
        moneyRect.anchorMax = new Vector2(0.5f, 0.5f);
        moneyRect.pivot = new Vector2(0.5f, 0.5f);
        moneyRect.anchoredPosition = new Vector2(0f, -8f);
        moneyRect.sizeDelta = new Vector2(420f, 118f);

        Image moneyImage = moneyContainer.GetComponent<Image>();
        if (moneyImage == null)
            moneyImage = moneyContainer.gameObject.AddComponent<Image>();
        moneyImage.sprite = moneyPanelSprite;
        moneyImage.type = Image.Type.Sliced;
        moneyImage.color = Color.white;
        moneyImage.raycastTarget = false;

        HorizontalLayoutGroup layout = moneyContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.padding = new RectOffset(42, 42, 0, 0);
            layout.spacing = 30f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        Transform moneyIcon = moneyContainer.Find("MoneyIcon");
        if (moneyIcon != null)
        {
            RectTransform iconRect = moneyIcon as RectTransform;
            iconRect.sizeDelta = new Vector2(86f, 86f);
            Image iconImage = moneyIcon.GetComponent<Image>();
            if (iconImage != null)
                iconImage.preserveAspect = true;
        }

        Transform moneyText = moneyContainer.Find("MoneyText");
        if (moneyText != null)
        {
            RectTransform moneyTextRect = moneyText as RectTransform;
            moneyTextRect.sizeDelta = new Vector2(210f, 76f);
            TextMeshProUGUI text = moneyText.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.fontSize = 56f;
                text.fontStyle = FontStyles.Bold;
                text.alignment = TextAlignmentOptions.MidlineLeft;
                text.color = Color.white;
                text.enableAutoSizing = true;
                text.fontSizeMin = 34f;
                text.fontSizeMax = 56f;
                text.outlineColor = Hex("#050515", 0.9f);
                text.outlineWidth = 0.16f;
            }
        }
    }

    private static void ConfigureSettingsButton(Transform topBar, Sprite settingsFrameSprite)
    {
        Transform settingsButton = topBar.Find("SettingsButton");
        if (settingsButton == null)
            return;

        settingsButton.SetAsLastSibling();
        RectTransform settingsRect = settingsButton as RectTransform;
        settingsRect.anchorMin = new Vector2(1f, 0.5f);
        settingsRect.anchorMax = new Vector2(1f, 0.5f);
        settingsRect.pivot = new Vector2(0.5f, 0.5f);
        settingsRect.anchoredPosition = new Vector2(-150f, -8f);
        settingsRect.sizeDelta = new Vector2(152f, 136f);

        Image rootImage = settingsButton.GetComponent<Image>();
        Sprite settingsSprite = rootImage != null ? rootImage.sprite : LoadSprite("Assets/Resources/Sprites/Buttons&Icons/settingsIcon.png");
        if (rootImage != null)
        {
            rootImage.sprite = settingsFrameSprite;
            rootImage.type = Image.Type.Simple;
            rootImage.color = Color.white;
            rootImage.raycastTarget = true;
        }

        Image iconImage = ConfigureChildImage(settingsButton, "SettingsIcon", settingsSprite, Vector2.zero, new Vector2(78f, 78f), true);
        iconImage.transform.SetAsLastSibling();
    }

    private static Image ConfigureChildImage(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, bool preserveAspect)
    {
        GameObject child = EnsureChild(parent, name, typeof(Image));
        RectTransform rect = child.GetComponent<RectTransform>();
        SetCentered(rect, position, size);

        Image image = child.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = false;
        return image;
    }

    private static void SetCentered(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static T EnsureComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private static GameObject EnsureChild(Transform parent, string name, params System.Type[] components)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing.gameObject;

        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] != typeof(RectTransform) && child.GetComponent(components[i]) == null)
                child.AddComponent(components[i]);
        }
        return child;
    }

    private static TopBarSprites GenerateSprites()
    {
        string topPanelPath = GeneratedFolder + "/top_bar_panel.png";
        string moneyPanelPath = GeneratedFolder + "/top_money_panel.png";
        string avatarFramePath = GeneratedFolder + "/avatar_hex_outline.png";
        string levelBadgePath = GeneratedFolder + "/level_hex_badge.png";
        string settingsFramePath = GeneratedFolder + "/settings_hex_panel.png";

        SaveRoundedPanel(topPanelPath, 960, 220, 48f, Hex("#151033", 0.90f), Hex("#050715", 0.92f), Hex("#6E5BB7", 0.9f), 6f);
        SaveHex(moneyPanelPath, 520, 150, new[]
        {
            new Vector2(56, 10), new Vector2(464, 10), new Vector2(512, 75),
            new Vector2(464, 140), new Vector2(56, 140), new Vector2(8, 75)
        }, Hex("#090A20", 0.86f), Hex("#3E326F", 0.86f), 5f, new Vector4(58, 58, 58, 58));
        SaveHex(avatarFramePath, 240, 220, new[]
        {
            new Vector2(55, 12), new Vector2(185, 12), new Vector2(232, 110),
            new Vector2(185, 208), new Vector2(55, 208), new Vector2(8, 110)
        }, Hex("#090A1E", 0.58f), Hex("#C6BFFF", 0.96f), 8f, new Vector4(50, 50, 50, 50));
        SaveHex(levelBadgePath, 132, 118, new[]
        {
            new Vector2(32, 8), new Vector2(100, 8), new Vector2(124, 59),
            new Vector2(100, 110), new Vector2(32, 110), new Vector2(8, 59)
        }, Hex("#111126", 0.94f), Hex("#B9AEFF", 0.96f), 6f, new Vector4(30, 30, 30, 30));
        SaveHex(settingsFramePath, 170, 150, new[]
        {
            new Vector2(42, 8), new Vector2(128, 8), new Vector2(162, 75),
            new Vector2(128, 142), new Vector2(42, 142), new Vector2(8, 75)
        }, Hex("#090A20", 0.76f), Hex("#5A4A96", 0.9f), 5f, new Vector4(42, 42, 42, 42));

        return new TopBarSprites
        {
            topPanel = LoadSprite(topPanelPath),
            moneyPanel = LoadSprite(moneyPanelPath),
            avatarFrame = LoadSprite(avatarFramePath),
            levelBadge = LoadSprite(levelBadgePath),
            settingsFrame = LoadSprite(settingsFramePath)
        };
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
                if (edge > outlineWidth && edge < outlineWidth + 16f)
                {
                    float blend = Mathf.InverseLerp(outlineWidth + 16f, outlineWidth, edge);
                    pixel = Color.Lerp(fill, outline, blend * 0.35f);
                }
                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        ImportSprite(path, new Vector4(radius, radius, radius, radius));
    }

    private static void SaveHex(string path, int width, int height, Vector2[] points, Color fill, Color outline, float outlineWidth, Vector4 border)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                if (!IsInPolygon(point, points))
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float edge = float.MaxValue;
                for (int i = 0; i < points.Length; i++)
                    edge = Mathf.Min(edge, DistanceToSegment(point, points[i], points[(i + 1) % points.Length]));

                Color pixel = edge <= outlineWidth ? outline : fill;
                if (edge > outlineWidth && edge < outlineWidth + 14f)
                {
                    float blend = Mathf.InverseLerp(outlineWidth + 14f, outlineWidth, edge);
                    pixel = Color.Lerp(fill, outline, blend * 0.4f);
                }
                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        ImportSprite(path, border);
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

    private static bool IsInPolygon(Vector2 point, Vector2[] polygon)
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

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float denominator = Vector2.Dot(ab, ab);
        if (denominator <= 0.0001f)
            return Vector2.Distance(point, a);
        float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / denominator);
        return Vector2.Distance(point, a + (ab * t));
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

    private struct TopBarSprites
    {
        public Sprite topPanel;
        public Sprite moneyPanel;
        public Sprite avatarFrame;
        public Sprite levelBadge;
        public Sprite settingsFrame;
    }
}
