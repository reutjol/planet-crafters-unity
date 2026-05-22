using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ProfilePopupStyleApplier
{
    private const string GeneratedFolder = "Assets/Generated/ProfilePopupStyle";

    [MenuItem("Tools/Planet/Apply Profile Popup Style")]
    public static void ApplyFromMenu()
    {
        Debug.Log(Apply());
    }

    public static string Apply()
    {
        if (SceneManager.GetActiveScene().name != "PlanetScene")
            return "PlanetScene is not active; no profile popup changes applied.";

        ProfilePopupView view = FindSceneObject<ProfilePopupView>();
        if (view == null)
            return "ProfilePopupView was not found in PlanetScene.";

        System.IO.Directory.CreateDirectory(GeneratedFolder);
        ProfileSprites sprites = GenerateSprites();

        ApplyToPopup(view.transform, sprites);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        return "Profile popup sci-fi visual style applied. Existing content, avatars, and script references were preserved.";
    }

    private static void ApplyToPopup(Transform popup, ProfileSprites sprites)
    {
        ConfigureRoot(popup);
        ConfigureBackground(popup, sprites.outerPanel);
        ConfigureContainer(popup);
        ConfigureHeader(popup, sprites.closeFrame, sprites.closeIcon);
        ConfigureProfileInfo(popup, sprites.sectionPanel, sprites.avatarFrame, sprites.longButton, sprites.fieldFrame, sprites.editIcon);
        ConfigureAvatarArea(popup, sprites.sectionPanel, sprites.avatarSlot);
    }

    private static void ConfigureRoot(Transform popup)
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
        Transform block = popup.Find("block");
        if (block != null)
        {
            Image blockImage = block.GetComponent<Image>();
            if (blockImage != null)
            {
                blockImage.color = new Color(0f, 0f, 0f, 0f);
                blockImage.raycastTarget = true;
            }
        }

        Transform background = popup.Find("Background");
        if (background == null)
            return;

        SetCenter(background as RectTransform, Vector2.zero, new Vector2(940f, 1320f));

        Image image = background.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = outerPanel;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = false;
        }
    }

    private static void ConfigureContainer(Transform popup)
    {
        Transform container = popup.Find("PopupContainer");
        if (container == null)
            return;

        DisableLayoutComponents(container);
        SetCenter(container as RectTransform, Vector2.zero, new Vector2(830f, 1190f));
    }

    private static void ConfigureHeader(Transform popup, Sprite closeFrame, Sprite closeIcon)
    {
        Transform header = popup.Find("PopupContainer/HeaderSection");
        if (header == null)
            return;

        DisableLayoutComponents(header);
        SetCenter(header as RectTransform, new Vector2(0f, 500f), new Vector2(760f, 140f));

        TMP_Text title = header.Find("Text") != null ? header.Find("Text").GetComponent<TMP_Text>() : null;
        if (title != null)
        {
            SetCenter(title.rectTransform, new Vector2(-8f, -8f), new Vector2(610f, 112f));
            title.text = title.text.ToUpperInvariant();
            title.fontSize = 72f;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = Color.white;
            title.enableAutoSizing = true;
            title.fontSizeMin = 42f;
            title.fontSizeMax = 72f;
            title.outlineColor = Hex("#397CFF", 0.95f);
            title.outlineWidth = 0.22f;
        }

        Transform spacer = header.Find("Spacer");
        if (spacer != null)
            spacer.gameObject.SetActive(false);

        Transform closeButton = header.Find("CloseButton");
        if (closeButton == null)
            return;

        SetCenter(closeButton as RectTransform, new Vector2(330f, 8f), new Vector2(86f, 86f));
        Image closeImage = closeButton.GetComponent<Image>();
        if (closeImage != null)
        {
            closeImage.sprite = closeFrame;
            closeImage.type = Image.Type.Simple;
            closeImage.color = Color.white;
            closeImage.raycastTarget = true;
        }

        Button button = closeButton.GetComponent<Button>();
        if (button != null && closeImage != null)
            button.targetGraphic = closeImage;

        Image icon = ConfigureChildImage(closeButton, "SciFiCloseIcon", closeIcon, Vector2.zero, new Vector2(54f, 54f), true);
        icon.color = Color.white;
    }

    private static void ConfigureProfileInfo(Transform popup, Sprite sectionPanel, Sprite avatarFrame, Sprite longButton, Sprite fieldFrame, Sprite editIcon)
    {
        Transform profileInfo = popup.Find("PopupContainer/ProfileInfo");
        if (profileInfo == null)
            return;

        DisableLayoutComponents(profileInfo);
        SetCenter(profileInfo as RectTransform, new Vector2(0f, 245f), new Vector2(760f, 360f));

        Image infoImage = profileInfo.GetComponent<Image>();
        if (infoImage != null)
        {
            infoImage.sprite = sectionPanel;
            infoImage.type = Image.Type.Simple;
            infoImage.color = Color.white;
            infoImage.raycastTarget = false;
        }

        Transform left = profileInfo.Find("Left");
        if (left != null)
        {
            DisableLayoutComponents(left);
            SetCenter(left as RectTransform, new Vector2(-260f, -8f), new Vector2(250f, 290f));
            ConfigureLogoutButton(left.Find("logout"), longButton);
            ConfigureProfileAvatar(left.Find("ProfileBackground"), avatarFrame);
        }

        Transform right = profileInfo.Find("Right");
        if (right != null)
        {
            DisableLayoutComponents(right);
            SetCenter(right as RectTransform, new Vector2(165f, -8f), new Vector2(470f, 290f));
            ConfigureField(right.Find("FullName"), fieldFrame, editIcon, new Vector2(0f, 92f));
            ConfigureField(right.Find("username"), fieldFrame, editIcon, new Vector2(0f, 0f));
            ConfigureField(right.Find("mail"), fieldFrame, editIcon, new Vector2(0f, -92f));
        }
    }

    private static void ConfigureLogoutButton(Transform logout, Sprite longButton)
    {
        if (logout == null)
            return;

        SetCenter(logout as RectTransform, new Vector2(0f, 104f), new Vector2(230f, 74f));
        Image root = logout.GetComponent<Image>();
        if (root == null)
            root = logout.gameObject.AddComponent<Image>();

        root.sprite = longButton;
        root.type = Image.Type.Simple;
        root.color = Color.white;
        root.raycastTarget = true;

        Button button = logout.GetComponent<Button>();
        if (button != null)
            button.targetGraphic = root;

        Transform image = logout.Find("Image");
        if (image != null)
        {
            SetStretch(image as RectTransform);
            Image decorative = image.GetComponent<Image>();
            if (decorative != null)
                decorative.color = new Color(1f, 1f, 1f, 0f);
        }

        TMP_Text text = logout.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            SetCenter(text.rectTransform, Vector2.zero, new Vector2(180f, 46f));
            StyleWhiteText(text, 33f);
        }
    }

    private static void ConfigureProfileAvatar(Transform profileBackground, Sprite avatarFrame)
    {
        if (profileBackground == null)
            return;

        SetCenter(profileBackground as RectTransform, new Vector2(0f, -42f), new Vector2(230f, 230f));
        Image frame = profileBackground.GetComponent<Image>();
        if (frame != null)
        {
            frame.sprite = avatarFrame;
            frame.type = Image.Type.Simple;
            frame.color = Color.white;
            frame.raycastTarget = false;
        }

        Transform profileImage = profileBackground.Find("ProfileImage");
        if (profileImage != null)
        {
            SetCenter(profileImage as RectTransform, Vector2.zero, new Vector2(166f, 166f));
            Image avatar = profileImage.GetComponent<Image>();
            if (avatar != null)
            {
                avatar.color = Color.white;
                avatar.preserveAspect = true;
                avatar.raycastTarget = false;
            }
        }
    }

    private static void ConfigureField(Transform field, Sprite fieldFrame, Sprite editIcon, Vector2 position)
    {
        if (field == null)
            return;

        SetCenter(field as RectTransform, position, new Vector2(430f, 76f));

        Image background = field.GetComponent<Image>();
        if (background == null)
            background = field.gameObject.AddComponent<Image>();

        background.sprite = fieldFrame;
        background.type = Image.Type.Simple;
        background.color = Color.white;
        background.raycastTarget = false;

        TMP_Text label = field.Find("NameText") != null ? field.Find("NameText").GetComponent<TMP_Text>() : null;
        if (label != null)
        {
            SetCenter(label.rectTransform, new Vector2(-38f, 0f), new Vector2(320f, 48f));
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.fontSize = 26f;
            label.enableAutoSizing = true;
            label.fontSizeMin = 16f;
            label.fontSizeMax = 26f;
            label.color = Color.white;
            label.outlineColor = Hex("#130039", 0.9f);
            label.outlineWidth = 0.08f;
        }

        TMP_InputField input = field.GetComponentInChildren<TMP_InputField>(true);
        if (input != null)
        {
            SetCenter(input.GetComponent<RectTransform>(), new Vector2(-38f, 0f), new Vector2(320f, 54f));
            StyleInput(input);
        }

        ConfigureIconButton(field.Find("EditButton"), editIcon, new Vector2(188f, 0f), new Vector2(52f, 52f));
        ConfigureIconButton(field.Find("confirm"), editIcon, new Vector2(188f, 0f), new Vector2(52f, 52f));
    }

    private static void ConfigureIconButton(Transform target, Sprite icon, Vector2 position, Vector2 size)
    {
        if (target == null)
            return;

        SetCenter(target as RectTransform, position, size);
        Image image = target.GetComponent<Image>();
        if (image == null)
            image = target.gameObject.AddComponent<Image>();

        if (icon != null)
            image.sprite = icon;
        image.type = Image.Type.Simple;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;

        Button button = target.GetComponent<Button>();
        if (button != null)
            button.targetGraphic = image;
    }

    private static void ConfigureAvatarArea(Transform popup, Sprite sectionPanel, Sprite avatarSlot)
    {
        Transform title = popup.Find("PopupContainer/AvatarTitle");
        if (title != null)
        {
            SetCenter(title as RectTransform, new Vector2(0f, 8f), new Vector2(760f, 70f));
            TMP_Text titleText = title.GetComponent<TMP_Text>();
            if (titleText != null)
            {
                titleText.text = titleText.text.ToUpperInvariant();
                StyleWhiteText(titleText, 52f);
                titleText.outlineColor = Hex("#397CFF", 0.95f);
                titleText.outlineWidth = 0.2f;
            }
        }

        Transform avatarGrid = popup.Find("PopupContainer/AvatarGrid");
        if (avatarGrid == null)
            return;

        DisableLayoutComponents(avatarGrid);
        SetCenter(avatarGrid as RectTransform, new Vector2(0f, -322f), new Vector2(760f, 590f));

        Image gridPanel = avatarGrid.GetComponent<Image>();
        if (gridPanel != null)
        {
            gridPanel.sprite = sectionPanel;
            gridPanel.type = Image.Type.Simple;
            gridPanel.color = Color.white;
            gridPanel.raycastTarget = false;
        }

        Transform grid = avatarGrid.Find("Grid");
        if (grid == null)
            return;

        DisableLayoutComponents(grid);
        SetCenter(grid as RectTransform, new Vector2(0f, -18f), new Vector2(640f, 500f));

        AvatarSlotView[] slots = grid.GetComponentsInChildren<AvatarSlotView>(true);
        for (int i = 0; i < slots.Length; i++)
            ConfigureAvatarSlot(slots[i].transform, avatarSlot, GetAvatarSlotPosition(i));
    }

    private static void ConfigureAvatarSlot(Transform slot, Sprite avatarSlot, Vector2 position)
    {
        if (slot == null)
            return;

        SetCenter(slot as RectTransform, position, new Vector2(154f, 154f));
        Image slotImage = slot.GetComponent<Image>();
        if (slotImage != null)
        {
            slotImage.sprite = avatarSlot;
            slotImage.type = Image.Type.Simple;
            slotImage.color = Color.white;
            slotImage.raycastTarget = true;
        }

        Button button = slot.GetComponent<Button>();
        if (button != null && slotImage != null)
            button.targetGraphic = slotImage;

        Transform avatar = slot.Find("AvatarImage");
        if (avatar != null)
        {
            SetCenter(avatar as RectTransform, new Vector2(0f, 8f), new Vector2(108f, 108f));
            Image avatarImage = avatar.GetComponent<Image>();
            if (avatarImage != null)
            {
                avatarImage.color = Color.white;
                avatarImage.preserveAspect = true;
                avatarImage.raycastTarget = false;
            }
        }

        Transform mark = slot.Find("Mark");
        if (mark != null)
        {
            SetCenter(mark as RectTransform, new Vector2(0f, -52f), new Vector2(42f, 42f));
            Image markImage = mark.GetComponent<Image>();
            if (markImage != null)
                markImage.color = Hex("#42A4FF", 1f);
        }
    }

    private static Vector2 GetAvatarSlotPosition(int index)
    {
        int column = index % 3;
        int row = index / 3;
        return new Vector2((column - 1) * 225f, 164f - row * 176f);
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

    private static void StyleInput(TMP_InputField input)
    {
        Image image = input.GetComponent<Image>();
        if (image != null)
            image.color = new Color(0f, 0f, 0f, 0f);

        TMP_Text text = input.textComponent;
        if (text != null)
        {
            text.fontSize = 25f;
            text.enableAutoSizing = true;
            text.fontSizeMin = 16f;
            text.fontSizeMax = 25f;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.color = Color.white;
        }

        TMP_Text placeholder = input.placeholder as TMP_Text;
        if (placeholder != null)
        {
            placeholder.fontSize = 22f;
            placeholder.color = Hex("#B7C8FF", 0.62f);
        }
    }

    private static void StyleWhiteText(TMP_Text text, float fontSize)
    {
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.enableAutoSizing = true;
        text.fontSizeMin = 18f;
        text.fontSizeMax = fontSize;
        text.outlineColor = Hex("#15003F", 0.9f);
        text.outlineWidth = 0.14f;
    }

    private static void DisableLayoutComponents(Transform target)
    {
        if (target == null)
            return;

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

    private static void SetStretch(RectTransform rect)
    {
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

    private static ProfileSprites GenerateSprites()
    {
        string outerPath = GeneratedFolder + "/profile_outer_panel.png";
        string sectionPath = GeneratedFolder + "/profile_section_panel.png";
        string avatarFramePath = GeneratedFolder + "/profile_avatar_frame.png";
        string longButtonPath = GeneratedFolder + "/profile_logout_button.png";
        string fieldPath = GeneratedFolder + "/profile_field_frame.png";
        string avatarSlotPath = GeneratedFolder + "/profile_avatar_slot.png";
        string closeFramePath = "Assets/Generated/SettingsPopupStyle/settings_sci_fi_close_hex.png";
        string closeIconPath = "Assets/Generated/SettingsPopupStyle/settings_sci_fi_close_x.png";
        string editIconPath = GeneratedFolder + "/profile_edit_icon.png";

        SaveOuterPanel(outerPath, 960, 1360);
        SaveSciFiPanel(sectionPath, 920, 470, Hex("#08113B", 0.82f), Hex("#05071D", 0.88f), Hex("#5474FF", 0.82f), Hex("#20A7FF", 0.55f), 7f, 28f, true);
        SaveSciFiPanel(avatarFramePath, 320, 320, Hex("#7B28D9", 0.78f), Hex("#150337", 0.92f), Hex("#F1D6FF", 1f), Hex("#B73BFF", 0.92f), 10f, 26f, true);
        SaveSciFiPanel(longButtonPath, 430, 125, Hex("#A336FF", 0.98f), Hex("#220066", 0.98f), Hex("#F5DEFF", 1f), Hex("#B841FF", 0.94f), 8f, 20f, true);
        SaveSciFiPanel(fieldPath, 700, 125, Hex("#B03BFF", 0.96f), Hex("#1B005B", 0.98f), Hex("#F4DDFF", 1f), Hex("#793CFF", 0.95f), 8f, 20f, true);
        SaveSciFiPanel(avatarSlotPath, 260, 260, Hex("#8B32FF", 0.98f), Hex("#20045B", 0.98f), Hex("#F6DDFF", 1f), Hex("#A337FF", 0.95f), 10f, 26f, true);
        SaveEditIcon(editIconPath, 100, 100);

        return new ProfileSprites
        {
            outerPanel = LoadSprite(outerPath),
            sectionPanel = LoadSprite(sectionPath),
            avatarFrame = LoadSprite(avatarFramePath),
            longButton = LoadSprite(longButtonPath),
            fieldFrame = LoadSprite(fieldPath),
            avatarSlot = LoadSprite(avatarSlotPath),
            closeFrame = LoadSprite(closeFramePath),
            closeIcon = LoadSprite(closeIconPath),
            editIcon = LoadSprite(editIconPath)
        };
    }

    private static void SaveOuterPanel(string path, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Vector2[] polygon =
        {
            new Vector2(70f, 12f),
            new Vector2(width * 0.28f, 12f),
            new Vector2(width * 0.31f, 30f),
            new Vector2(width * 0.69f, 30f),
            new Vector2(width * 0.72f, 12f),
            new Vector2(width - 70f, 12f),
            new Vector2(width - 16f, 70f),
            new Vector2(width - 16f, height - 70f),
            new Vector2(width - 70f, height - 16f),
            new Vector2(width * 0.68f, height - 16f),
            new Vector2(width * 0.62f, height - 56f),
            new Vector2(width * 0.38f, height - 56f),
            new Vector2(width * 0.32f, height - 16f),
            new Vector2(70f, height - 16f),
            new Vector2(16f, height - 70f),
            new Vector2(16f, 70f)
        };

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
                    if (edgeDistance <= 48f)
                    {
                        float alpha = Mathf.Pow(1f - edgeDistance / 48f, 2.1f) * 0.9f;
                        texture.SetPixel(x, y, WithAlpha(Hex("#1E79FF", 1f), alpha));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                    continue;
                }

                Color fill = Color.Lerp(Hex("#060921", 0.98f), Hex("#11155A", 0.98f), vertical);
                float centerGlow = 1f - Mathf.Clamp01(Mathf.Abs(x / (float)width - 0.5f) * 2f);
                fill = Color.Lerp(fill, Hex("#2A197B", fill.a), centerGlow * 0.2f);

                float grain = Hash01(x, y) - 0.5f;
                fill.r = Mathf.Clamp01(fill.r + grain * 0.04f);
                fill.g = Mathf.Clamp01(fill.g + grain * 0.035f);
                fill.b = Mathf.Clamp01(fill.b + grain * 0.07f);

                if (Hash01(x * 17, y * 23) > 0.992f)
                    fill = Color.Lerp(fill, Color.white, 0.65f);

                Color pixel = fill;
                if (edgeDistance <= 8f)
                    pixel = Color.Lerp(Hex("#49B7FF", 1f), Color.white, Mathf.Clamp01(1f - edgeDistance / 8f) * 0.34f);
                else if (edgeDistance <= 24f)
                    pixel = Color.Lerp(pixel, Hex("#394DFF", 1f), (1f - (edgeDistance - 8f) / 16f) * 0.55f);

                if (IsOuterAccent(point, width, height))
                    pixel = Color.Lerp(pixel, Hex("#D843FF", 1f), 0.7f);

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        ImportSprite(path, Vector4.zero);
    }

    private static bool IsOuterAccent(Vector2 point, int width, int height)
    {
        bool top = point.y > height * 0.905f && point.y < height * 0.935f && point.x > width * 0.34f && point.x < width * 0.66f;
        bool bottom = point.y > height * 0.04f && point.y < height * 0.075f && point.x > width * 0.34f && point.x < width * 0.66f;
        return top || bottom;
    }

    private static void SaveSciFiPanel(string path, int width, int height, Color fillTop, Color fillBottom, Color border, Color glow, float borderWidth, float glowWidth, bool noise)
    {
        Vector2[] polygon =
        {
            new Vector2(42f, 0f),
            new Vector2(width - 42f, 0f),
            new Vector2(width, 42f),
            new Vector2(width, height - 42f),
            new Vector2(width - 42f, height),
            new Vector2(42f, height),
            new Vector2(0f, height - 42f),
            new Vector2(0f, 42f)
        };

        SavePolygonPanel(path, width, height, polygon, fillTop, fillBottom, border, glow, borderWidth, glowWidth, noise);
    }

    private static void SavePolygonPanel(string path, int width, int height, Vector2[] polygon, Color fillTop, Color fillBottom, Color border, Color glow, float borderWidth, float glowWidth, bool noise)
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
                        float alpha = Mathf.Pow(1f - edgeDistance / glowWidth, 2.15f) * glow.a;
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
                fill = Color.Lerp(fill, Hex("#962DFF", fill.a), centerGlow * 0.18f);

                if (noise)
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

    private static void SaveEditIcon(string path, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Vector2 a = new Vector2(width * 0.26f, height * 0.30f);
        Vector2 b = new Vector2(width * 0.70f, height * 0.74f);
        Vector2 tipA = new Vector2(width * 0.62f, height * 0.82f);
        Vector2 tipB = new Vector2(width * 0.80f, height * 0.64f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                float body = DistanceToSegment(point, a, b);
                float tip = DistanceToSegment(point, tipA, tipB);
                float distance = Mathf.Min(body, tip);

                if (distance > 7f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float alpha = distance <= 4f ? 1f : 1f - (distance - 4f) / 3f;
                Color color = Color.Lerp(Hex("#E4ECFF", alpha), Color.white, 0.5f);
                color.a = alpha;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        ImportSprite(path, Vector4.zero);
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

    private struct ProfileSprites
    {
        public Sprite outerPanel;
        public Sprite sectionPanel;
        public Sprite avatarFrame;
        public Sprite longButton;
        public Sprite fieldFrame;
        public Sprite avatarSlot;
        public Sprite closeFrame;
        public Sprite closeIcon;
        public Sprite editIcon;
    }
}
