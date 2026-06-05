using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ShopSceneReferenceDesignApplier
{
    private const string ShopScenePath = "Assets/Scenes/ShopScene.unity";
    private const string GeneratedFolder = "Assets/Generated/ShopSceneStyle";
    private const float ReferenceWidth = 1080f;
    private const float ReferenceHeight = 1920f;

    [MenuItem("Tools/Shop/Apply Reference Shop Scene")]
    public static void ApplyFromMenu()
    {
        Debug.Log(Apply());
    }

    public static string Apply()
    {
        if (SceneManager.GetActiveScene().path != ShopScenePath)
            return "ShopScene is not active; no changes applied.";

        System.IO.Directory.CreateDirectory(GeneratedFolder);
        ShopSprites sprites = GenerateSprites();

        GameObject canvasObject = FindOrCreateCanvas();
        ConfigureCanvas(canvasObject);

        Transform canvas = canvasObject.transform;
        DestroyChild(canvas, "ShopSceneRoot");
        DestroyChild(canvas, "Image");

        GameObject root = CreateUiObject("ShopSceneRoot", canvas);
        SetStretch(root.GetComponent<RectTransform>());

        BuildBackground(root.transform, sprites);
        BuildTopHud(root.transform, sprites);
        BuildHeroHeader(root.transform, sprites);
        BuildBoosterArea(root.transform, sprites);
        BuildEarnCoinsBanner(root.transform, sprites);
        BuildBottomNavigation(root.transform, sprites);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        return "ShopScene reference shop hierarchy applied and saved.";
    }

    private static GameObject FindOrCreateCanvas()
    {
        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject != null)
            return canvasObject;

        canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.layer = LayerMask.NameToLayer("UI");
        return canvasObject;
    }

    private static void ConfigureCanvas(GameObject canvasObject)
    {
        Canvas canvas = EnsureComponent<Canvas>(canvasObject);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main != null ? Camera.main : Object.FindObjectOfType<Camera>();
        canvas.planeDistance = 1f;
        canvas.sortingOrder = 0;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvasObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;

        EnsureComponent<GraphicRaycaster>(canvasObject);
    }

    private static void BuildBackground(Transform root, ShopSprites sprites)
    {
        GameObject group = CreateGroup("Background", root);
        SetStretch(group.GetComponent<RectTransform>());

        Image background = CreateImage(group.transform, "StarfieldBackground", sprites.background, Vector2.zero, new Vector2(ReferenceWidth, ReferenceHeight), false, Image.Type.Simple);
        SetStretch(background.rectTransform);
        background.color = Color.white;

        GameObject decor = CreateGroup("SpaceDecor", group.transform);
        SetStretch(decor.GetComponent<RectTransform>());
        AddDecorGem(decor.transform, "BlueShard_Left", sprites.currencyCrystal, new Vector2(84f, -1242f), new Vector2(68f, 68f), -18f, Hex("8BCBFF", 0.32f));
        AddDecorGem(decor.transform, "PurpleShard_RightTop", sprites.currencyGem, new Vector2(977f, -185f), new Vector2(58f, 58f), 23f, Hex("A363FF", 0.28f));
        AddDecorGem(decor.transform, "BlueShard_RightMid", sprites.currencyCrystal, new Vector2(996f, -655f), new Vector2(78f, 78f), 31f, Hex("66AFFF", 0.26f));
        AddDecorGem(decor.transform, "PurpleShard_BottomLeft", sprites.currencyGem, new Vector2(80f, -1490f), new Vector2(50f, 50f), -22f, Hex("B26DFF", 0.22f));
        AddDecorGem(decor.transform, "BlueShard_BottomRight", sprites.currencyCrystal, new Vector2(1015f, -1240f), new Vector2(56f, 56f), -30f, Hex("74BFFF", 0.24f));

        Image outerFrame = CreateImage(group.transform, "OuterScreenFrame", sprites.outerFrame, Vector2.zero, new Vector2(ReferenceWidth, ReferenceHeight), false, Image.Type.Simple);
        SetStretch(outerFrame.rectTransform);
        outerFrame.color = Color.white;

        Image topGlow = AddLine(group.transform, "TopGlowLine", Vector2.zero, new Vector2(820f, 4f), Hex("4FCDFF", 0.72f));
        SetTopCenter(topGlow.rectTransform, 38f, 4f, 820f);
        Image bottomGlow = AddLine(group.transform, "BottomGlowLine", Vector2.zero, new Vector2(820f, 4f), Hex("7A45FF", 0.72f));
        SetTopCenter(bottomGlow.rectTransform, 1884f, 4f, 820f);
    }

    private static void BuildTopHud(Transform root, ShopSprites sprites)
    {
        GameObject topHud = CreateGroup("TopHUD", root);
        SetTopStretch(topHud.GetComponent<RectTransform>(), 0f, 172f, 0f, 0f);

        Image topFrame = CreateImage(topHud.transform, "TopHudFrame", sprites.topFrame, Vector2.zero, new Vector2(1048f, 152f), false, Image.Type.Simple);
        SetCenter(topFrame.rectTransform, new Vector2(0f, -8f), new Vector2(1048f, 152f));
        topFrame.color = Color.white;

        GameObject playerCluster = CreateGroup("PlayerCluster", topHud.transform);
        SetTopLeft(playerCluster.GetComponent<RectTransform>(), 42f, 44f, 330f, 126f);

        Image avatarFrame = CreateImage(playerCluster.transform, "AvatarHexFrame", sprites.avatarFrame, new Vector2(52f, -52f), new Vector2(112f, 112f), false, Image.Type.Simple);
        SetTopLeft(avatarFrame.rectTransform, 0f, 0f, 112f, 112f);
        avatarFrame.color = Color.white;
        Image avatar = CreateImage(playerCluster.transform, "AvatarImage", LoadSprite("Assets/Resources/Sprites/avatar Sprites/avatar.png"), new Vector2(52f, -52f), new Vector2(78f, 78f), true, Image.Type.Simple);
        SetTopLeft(avatar.rectTransform, 17f, 17f, 78f, 78f);
        avatar.color = Color.white;

        Image levelBadge = CreateImage(playerCluster.transform, "LevelBadge", sprites.levelBadge, new Vector2(98f, -85f), new Vector2(64f, 64f), false, Image.Type.Simple);
        SetTopLeft(levelBadge.rectTransform, 70f, 70f, 64f, 64f);
        levelBadge.color = Color.white;
        TextMeshProUGUI levelText = CreateText(levelBadge.transform, "LevelText", "12", 24f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, 0f), new Vector2(58f, 42f));
        levelText.fontStyle = FontStyles.Bold;

        TextMeshProUGUI playerName = CreateText(playerCluster.transform, "PlayerNameLabel", "PlayerName", 26f, Color.white, TextAlignmentOptions.Left, new Vector2(126f, -24f), new Vector2(210f, 40f));
        SetTopLeft(playerName.rectTransform, 126f, 8f, 210f, 40f);
        playerName.fontStyle = FontStyles.Bold;

        Image progressFrame = CreateImage(playerCluster.transform, "LevelProgressFrame", sprites.progressFrame, new Vector2(216f, -66f), new Vector2(220f, 34f), false, Image.Type.Simple);
        SetTopLeft(progressFrame.rectTransform, 126f, 50f, 220f, 34f);
        progressFrame.color = Color.white;
        Image progressFill = CreateImage(progressFrame.transform, "LevelProgressFill", sprites.progressFill, new Vector2(-54f, 0f), new Vector2(118f, 21f), false, Image.Type.Simple);
        progressFill.color = Color.white;
        TextMeshProUGUI progressText = CreateText(progressFrame.transform, "LevelProgressText", "4250 / 8000", 18f, Color.white, TextAlignmentOptions.Center, new Vector2(10f, 0f), new Vector2(180f, 30f));
        progressText.fontStyle = FontStyles.Bold;

        GameObject currencies = CreateGroup("CurrencyBar", topHud.transform);
        SetTopRight(currencies.GetComponent<RectTransform>(), 28f, 70f, 660f, 72f);
        BuildCurrencySlot(currencies.transform, "CoinsCurrency", sprites.currencyPanel, sprites.currencyCoin, "12.4K", new Vector2(-220f, 0f), true);
        BuildCurrencySlot(currencies.transform, "CrystalCurrency", sprites.currencyPanel, sprites.currencyCrystal, "8.7K", Vector2.zero, false);
        BuildCurrencySlot(currencies.transform, "GemCurrency", sprites.currencyPanel, sprites.currencyGem, "1.2K", new Vector2(220f, 0f), false);
    }

    private static void BuildHeroHeader(Transform root, ShopSprites sprites)
    {
        GameObject group = CreateGroup("HeroHeader", root);
        SetStretch(group.GetComponent<RectTransform>());

        Image titlePlate = CreateImage(group.transform, "TitlePlate", sprites.titlePlate, Vector2.zero, new Vector2(880f, 182f), false, Image.Type.Simple);
        SetTopCenter(titlePlate.rectTransform, 182f, 182f, 880f);
        titlePlate.color = Color.white;

        TextMeshProUGUI title = CreateText(group.transform, "TitleText", "BOOSTER SHOP", 78f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, -24f), new Vector2(780f, 94f));
        SetTopCenter(title.rectTransform, 220f, 94f, 780f);
        title.fontStyle = FontStyles.Bold;
        AddOutline(title.gameObject, Hex("8E4DFF", 0.86f), new Vector2(4f, -4f));

        TextMeshProUGUI subtitle = CreateText(group.transform, "SubtitleText", "POWER UP YOUR GAME", 29f, Hex("D5D1FF", 0.95f), TextAlignmentOptions.Center, new Vector2(0f, -100f), new Vector2(470f, 46f));
        SetTopCenter(subtitle.rectTransform, 315f, 46f, 470f);
        subtitle.fontStyle = FontStyles.Bold;

        Image leftRule = AddLine(group.transform, "SubtitleLeftRule", Vector2.zero, new Vector2(150f, 3f), Hex("7D4AFF", 0.5f));
        SetTopLeft(leftRule.rectTransform, 230f, 339f, 150f, 3f);
        Image rightRule = AddLine(group.transform, "SubtitleRightRule", Vector2.zero, new Vector2(150f, 3f), Hex("7D4AFF", 0.5f));
        SetTopLeft(rightRule.rectTransform, 700f, 339f, 150f, 3f);
    }

    private static void BuildBoosterArea(Transform root, ShopSprites sprites)
    {
        GameObject section = CreateGroup("BoostersSection", root);
        SetStretch(section.GetComponent<RectTransform>());

        Image header = CreateImage(section.transform, "BoostersTab", sprites.sectionTab, new Vector2(0f, -44f), new Vector2(430f, 96f), false, Image.Type.Simple);
        SetTopCenter(header.rectTransform, 400f, 96f, 430f);
        header.color = Color.white;
        TextMeshProUGUI headerText = CreateText(header.transform, "BoostersTabText", "BOOSTERS", 40f, Color.white, TextAlignmentOptions.Center, Vector2.zero, new Vector2(360f, 60f));
        headerText.fontStyle = FontStyles.Bold;

        Image leftRail = AddLine(section.transform, "TabLeftRail", Vector2.zero, new Vector2(260f, 4f), Hex("995AFF", 0.62f));
        SetTopLeft(leftRail.rectTransform, 122f, 446f, 260f, 4f);
        Image rightRail = AddLine(section.transform, "TabRightRail", Vector2.zero, new Vector2(260f, 4f), Hex("995AFF", 0.62f));
        SetTopLeft(rightRail.rectTransform, 698f, 446f, 260f, 4f);

        GameObject listPanel = CreateGroup("BoosterListPanel", section.transform);
        SetTopCenter(listPanel.GetComponent<RectTransform>(), 510f, 960f, 950f);
        Image panelImage = EnsureComponent<Image>(listPanel);
        panelImage.sprite = sprites.listPanel;
        panelImage.type = Image.Type.Simple;
        panelImage.color = Color.white;
        panelImage.raycastTarget = false;

        BuildBoosterCard(
            listPanel.transform,
            "BoosterCard_x2LastTile",
            sprites.cardPanel,
            LoadSprite("Assets/Resources/Sprites/Icons/multiplication.png"),
            "x2 LAST TILE",
            "Multiply the score from\nthe last placed hexagon.",
            72f,
            sprites);

        BuildBoosterCard(
            listPanel.transform,
            "BoosterCard_RemoveTile",
            sprites.cardPanel,
            LoadSprite("Assets/Resources/Sprites/Icons/removeTile.png"),
            "REMOVE TILE",
            "Remove one placed\nhexagon from the map.",
            366f,
            sprites);

        BuildBoosterCard(
            listPanel.transform,
            "BoosterCard_ExtraHex",
            sprites.cardPanel,
            LoadSprite("Assets/Resources/Sprites/Icons/addTile.png"),
            "EXTRA HEX",
            "Add one extra hexagon\ntile to your deck.",
            660f,
            sprites);

        AddHexOutline(listPanel.transform, "PanelDecorHex_TopRight", new Vector2(392f, -78f), new Vector2(55f, 55f), Hex("824BFF", 0.38f));
        AddHexOutline(listPanel.transform, "PanelDecorHex_BottomRight", new Vector2(408f, -885f), new Vector2(48f, 48f), Hex("824BFF", 0.3f));
    }

    private static void BuildBoosterCard(Transform parent, string name, Sprite cardSprite, Sprite artSprite, string title, string description, float top, ShopSprites sprites)
    {
        GameObject card = CreateGroup(name, parent);
        SetTopCenter(card.GetComponent<RectTransform>(), top, 252f, 888f);
        Image cardImage = EnsureComponent<Image>(card);
        cardImage.sprite = cardSprite;
        cardImage.type = Image.Type.Simple;
        cardImage.color = Color.white;
        cardImage.raycastTarget = false;

        Image art = CreateImage(card.transform, "BoosterArt", artSprite, new Vector2(-298f, 4f), new Vector2(236f, 206f), true, Image.Type.Simple);
        art.color = Color.white;

        TextMeshProUGUI titleText = CreateText(card.transform, "Title", title, 38f, Color.white, TextAlignmentOptions.Left, new Vector2(92f, 48f), new Vector2(305f, 60f));
        titleText.fontStyle = FontStyles.Bold;
        AddOutline(titleText.gameObject, Hex("05102C", 0.85f), new Vector2(2f, -2f));

        TextMeshProUGUI descText = CreateText(card.transform, "Description", description, 27f, Hex("D6D4EE", 0.96f), TextAlignmentOptions.TopLeft, new Vector2(98f, -30f), new Vector2(328f, 98f));
        descText.lineSpacing = 6f;
        descText.fontStyle = FontStyles.Normal;

        AddLine(card.transform, "PriceDivider", new Vector2(272f, 0f), new Vector2(3f, 180f), Hex("6A71E8", 0.72f));

        GameObject price = CreateGroup("Price", card.transform);
        SetCenter(price.GetComponent<RectTransform>(), new Vector2(346f, 52f), new Vector2(210f, 62f));
        Image coin = CreateImage(price.transform, "CoinIcon", sprites.currencyCoin, new Vector2(-72f, 0f), new Vector2(56f, 56f), false, Image.Type.Simple);
        coin.color = Color.white;
        TextMeshProUGUI dollar = CreateText(coin.transform, "Dollar", "$", 22f, Hex("FFF2A9", 1f), TextAlignmentOptions.Center, Vector2.zero, new Vector2(44f, 40f));
        dollar.fontStyle = FontStyles.Bold;
        TextMeshProUGUI priceText = CreateText(price.transform, "PriceText", "1,000", 34f, Color.white, TextAlignmentOptions.Left, new Vector2(16f, 0f), new Vector2(134f, 50f));
        priceText.fontStyle = FontStyles.Bold;

        GameObject button = CreateGroup("BuyButton", card.transform);
        SetCenter(button.GetComponent<RectTransform>(), new Vector2(344f, -66f), new Vector2(220f, 78f));
        Image buttonImage = EnsureComponent<Image>(button);
        buttonImage.sprite = sprites.buyButton;
        buttonImage.type = Image.Type.Simple;
        buttonImage.color = Color.white;
        buttonImage.raycastTarget = true;
        Button buyButton = EnsureComponent<Button>(button);
        buyButton.targetGraphic = buttonImage;

        TextMeshProUGUI buttonText = CreateText(button.transform, "BuyButtonLabel", "BUY", 37f, Color.white, TextAlignmentOptions.Center, Vector2.zero, new Vector2(180f, 56f));
        buttonText.fontStyle = FontStyles.Bold;
        AddOutline(buttonText.gameObject, Hex("6A25FF", 0.45f), new Vector2(2f, -2f));
    }

    private static void BuildEarnCoinsBanner(Transform root, ShopSprites sprites)
    {
        GameObject banner = CreateGroup("EarnCoinsBanner", root);
        SetTopCenter(banner.GetComponent<RectTransform>(), 1462f, 200f, 958f);
        Image bannerImage = EnsureComponent<Image>(banner);
        bannerImage.sprite = sprites.bannerPanel;
        bannerImage.type = Image.Type.Simple;
        bannerImage.color = Color.white;
        bannerImage.raycastTarget = false;

        Image coinPile = CreateImage(banner.transform, "CoinPileArt", LoadSprite("Assets/Resources/Sprites/Icons/Coins.png"), new Vector2(-360f, -2f), new Vector2(190f, 160f), true, Image.Type.Simple);
        coinPile.color = Color.white;

        TextMeshProUGUI title = CreateText(banner.transform, "EarnMoreTitle", "EARN MORE COINS", 30f, Hex("71E9FF", 1f), TextAlignmentOptions.Left, new Vector2(-75f, 30f), new Vector2(360f, 46f));
        title.fontStyle = FontStyles.Bold;

        TextMeshProUGUI body = CreateText(banner.transform, "EarnMoreBody", "Complete levels, watch ads,\nand collect daily rewards!", 25f, Hex("D8D6F0", 0.96f), TextAlignmentOptions.TopLeft, new Vector2(-75f, -36f), new Vector2(370f, 82f));
        body.lineSpacing = 4f;

        GameObject button = CreateGroup("GetCoinsButton", banner.transform);
        SetCenter(button.GetComponent<RectTransform>(), new Vector2(310f, -2f), new Vector2(290f, 84f));
        Image buttonImage = EnsureComponent<Image>(button);
        buttonImage.sprite = sprites.getCoinsButton;
        buttonImage.type = Image.Type.Simple;
        buttonImage.color = Color.white;
        buttonImage.raycastTarget = true;
        Button getCoinsButton = EnsureComponent<Button>(button);
        getCoinsButton.targetGraphic = buttonImage;

        TextMeshProUGUI label = CreateText(button.transform, "GetCoinsLabel", "GET COINS  >", 29f, Color.white, TextAlignmentOptions.Center, Vector2.zero, new Vector2(240f, 50f));
        label.fontStyle = FontStyles.Bold;
    }

    private static void BuildBottomNavigation(Transform root, ShopSprites sprites)
    {
        GameObject bottom = CreateGroup("BottomNavigation", root);
        SetBottomStretch(bottom.GetComponent<RectTransform>(), 0f, 188f, 0f, 0f);
        BottomBarController controller = EnsureComponent<BottomBarController>(bottom);

        Image panel = EnsureComponent<Image>(bottom);
        panel.sprite = sprites.navPanel;
        panel.type = Image.Type.Simple;
        panel.color = Color.white;
        panel.raycastTarget = false;

        GameObject container = CreateGroup("NavContainer", bottom.transform);
        SetStretch(container.GetComponent<RectTransform>());

        BuildNavItem(container.transform, "LobbyButton", "LOBBY", LoadSprite("Assets/Resources/Sprites/btns/planet.png"), sprites.navActive, new Vector2(-450f, 0f), false, controller, "GoPlanet");
        BuildNavItem(container.transform, "AchievementsButton", "ACHIEVEMENTS", LoadSprite("Assets/Resources/Sprites/btns/Achievements.png"), sprites.navActive, new Vector2(-270f, 0f), false, controller, "GoAchievements");
        BuildNavItem(container.transform, "WheelButton", "WHEEL", LoadSprite("Assets/Resources/Sprites/btns/wheel.png"), sprites.navActive, new Vector2(-90f, 0f), false, controller, "GoWheel");
        BuildNavItem(container.transform, "ProfileButton", "PROFILE", sprites.navProfile, sprites.navActive, new Vector2(90f, 0f), false, controller, null);
        BuildNavItem(container.transform, "ShopButton", "SHOP", LoadSprite("Assets/Resources/Sprites/btns/shop.png"), sprites.navActive, new Vector2(270f, 0f), true, controller, "GoShop");
        BuildNavItem(container.transform, "SettingsButton", "SETTINGS", LoadSprite("Assets/Resources/Sprites/btns/settings.png"), sprites.navActive, new Vector2(450f, 0f), false, controller, null);
    }

    private static void BuildCurrencySlot(Transform parent, string name, Sprite panelSprite, Sprite iconSprite, string value, Vector2 position, bool coinSymbol)
    {
        GameObject slot = CreateGroup(name, parent);
        SetCenter(slot.GetComponent<RectTransform>(), position, new Vector2(210f, 70f));
        Image panel = EnsureComponent<Image>(slot);
        panel.sprite = panelSprite;
        panel.type = Image.Type.Simple;
        panel.color = Color.white;
        panel.raycastTarget = false;

        Image icon = CreateImage(slot.transform, "Icon", iconSprite, new Vector2(-66f, 0f), new Vector2(52f, 52f), false, Image.Type.Simple);
        icon.color = Color.white;
        if (coinSymbol)
        {
            TextMeshProUGUI dollar = CreateText(icon.transform, "Dollar", "$", 20f, Hex("FFF2A9", 1f), TextAlignmentOptions.Center, Vector2.zero, new Vector2(42f, 36f));
            dollar.fontStyle = FontStyles.Bold;
        }

        TextMeshProUGUI label = CreateText(slot.transform, "Amount", value, 29f, Color.white, TextAlignmentOptions.Left, new Vector2(10f, 0f), new Vector2(96f, 48f));
        label.fontStyle = FontStyles.Bold;
        TextMeshProUGUI plus = CreateText(slot.transform, "Plus", "+", 40f, Color.white, TextAlignmentOptions.Center, new Vector2(77f, 3f), new Vector2(42f, 46f));
        plus.fontStyle = FontStyles.Bold;
    }

    private static void BuildNavItem(Transform parent, string name, string label, Sprite iconSprite, Sprite activeSprite, Vector2 position, bool selected, BottomBarController controller, string methodName)
    {
        GameObject item = CreateGroup(name, parent);
        SetCenter(item.GetComponent<RectTransform>(), position, new Vector2(176f, 170f));

        Image hit = EnsureComponent<Image>(item);
        hit.color = new Color(1f, 1f, 1f, 0f);
        hit.raycastTarget = true;

        Button button = EnsureComponent<Button>(item);
        button.targetGraphic = hit;

        if (!string.IsNullOrEmpty(methodName))
        {
            if (methodName == "GoPlanet")
                UnityEventTools.AddPersistentListener(button.onClick, controller.GoPlanet);
            else if (methodName == "GoAchievements")
                UnityEventTools.AddPersistentListener(button.onClick, controller.GoAchievements);
            else if (methodName == "GoWheel")
                UnityEventTools.AddPersistentListener(button.onClick, controller.GoWheel);
            else if (methodName == "GoShop")
                UnityEventTools.AddPersistentListener(button.onClick, controller.GoShop);
        }

        Image selectedBg = CreateImage(item.transform, "SelectedBG", activeSprite, Vector2.zero, new Vector2(178f, 170f), false, Image.Type.Simple);
        selectedBg.color = selected ? Color.white : new Color(1f, 1f, 1f, 0f);
        selectedBg.raycastTarget = false;

        Image icon = CreateImage(item.transform, "Icon", iconSprite, new Vector2(0f, 30f), new Vector2(68f, 68f), true, Image.Type.Simple);
        icon.color = selected ? Color.white : Hex("BDB8FF", 0.86f);

        TextMeshProUGUI text = CreateText(item.transform, "Label", label, label.Length > 10 ? 18f : 21f, selected ? Color.white : Hex("C9C5F2", 0.92f), TextAlignmentOptions.Center, new Vector2(0f, -48f), new Vector2(164f, 44f));
        text.fontStyle = FontStyles.Bold;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14f;
        text.fontSizeMax = label.Length > 10 ? 18f : 21f;
    }

    private static void AddDecorGem(Transform parent, string name, Sprite sprite, Vector2 topLeftPosition, Vector2 size, float rotation, Color color)
    {
        Image image = CreateImage(parent, name, sprite, Vector2.zero, size, true, Image.Type.Simple);
        SetTopLeft(image.rectTransform, topLeftPosition.x, -topLeftPosition.y, size.x, size.y);
        image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        image.color = color;
    }

    private static Image AddLine(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        Image line = CreateImage(parent, name, null, position, size, false, Image.Type.Simple);
        line.sprite = null;
        line.color = color;
        line.raycastTarget = false;
        return line;
    }

    private static void AddHexOutline(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        Image hex = CreateImage(parent, name, LoadSprite(GeneratedFolder + "/shop_hex_outline.png"), position, size, false, Image.Type.Simple);
        hex.color = color;
        hex.raycastTarget = false;
    }

    private static void AddOutline(GameObject target, Color color, Vector2 distance)
    {
        Outline outline = target.GetComponent<Outline>();
        if (outline == null)
            outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }

    private static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, bool preserveAspect, Image.Type type)
    {
        GameObject child = CreateUiObject(name, parent, typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = child.GetComponent<RectTransform>();
        SetCenter(rect, position, size);

        Image image = child.GetComponent<Image>();
        image.sprite = sprite;
        image.type = type;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = false;
        image.color = Color.white;
        return image;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string value, float fontSize, Color color, TextAlignmentOptions alignment, Vector2 position, Vector2 size)
    {
        GameObject child = CreateUiObject(name, parent, typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = child.GetComponent<RectTransform>();
        SetCenter(rect, position, size);

        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = LoadFont();
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.characterSpacing = 0f;
        text.wordSpacing = 0f;
        text.paragraphSpacing = 0f;
        return text;
    }

    private static GameObject CreateGroup(string name, Transform parent)
    {
        return CreateUiObject(name, parent, typeof(RectTransform));
    }

    private static GameObject CreateUiObject(string name, Transform parent, params System.Type[] components)
    {
        bool hasRectTransform = false;
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == typeof(RectTransform))
            {
                hasRectTransform = true;
                break;
            }
        }

        System.Type[] finalComponents;
        if (hasRectTransform)
        {
            finalComponents = components;
        }
        else
        {
            finalComponents = new System.Type[components.Length + 1];
            finalComponents[0] = typeof(RectTransform);
            for (int i = 0; i < components.Length; i++)
                finalComponents[i + 1] = components[i];
        }

        GameObject gameObject = new GameObject(name, finalComponents);
        gameObject.layer = LayerMask.NameToLayer("UI");
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        return gameObject;
    }

    private static T EnsureComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }

    private static void DestroyChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
            Object.DestroyImmediate(child.gameObject);
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

    private static void SetTopStretch(RectTransform rect, float top, float height, float left, float right)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -top);
        rect.sizeDelta = new Vector2(-(left + right), height);
        rect.offsetMin = new Vector2(left, rect.offsetMin.y);
        rect.offsetMax = new Vector2(-right, rect.offsetMax.y);
        rect.localScale = Vector3.one;
    }

    private static void SetBottomStretch(RectTransform rect, float bottom, float height, float left, float right)
    {
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, bottom);
        rect.sizeDelta = new Vector2(-(left + right), height);
        rect.offsetMin = new Vector2(left, rect.offsetMin.y);
        rect.offsetMax = new Vector2(-right, rect.offsetMax.y);
        rect.localScale = Vector3.one;
    }

    private static void SetTopCenter(RectTransform rect, float top, float height, float width)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -top);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
    }

    private static void SetTopLeft(RectTransform rect, float left, float top, float width, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(left, -top);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
    }

    private static void SetTopRight(RectTransform rect, float right, float top, float width, float height)
    {
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-right, -top);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
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

    private static ShopSprites GenerateSprites()
    {
        string backgroundPath = GeneratedFolder + "/shop_starfield_background.png";
        string outerFramePath = GeneratedFolder + "/shop_outer_screen_frame.png";
        string topFramePath = GeneratedFolder + "/shop_top_frame.png";
        string avatarFramePath = GeneratedFolder + "/shop_avatar_hex_frame.png";
        string levelBadgePath = GeneratedFolder + "/shop_level_badge.png";
        string progressFramePath = GeneratedFolder + "/shop_progress_frame.png";
        string progressFillPath = GeneratedFolder + "/shop_progress_fill.png";
        string currencyPanelPath = GeneratedFolder + "/shop_currency_panel.png";
        string currencyCoinPath = GeneratedFolder + "/shop_currency_coin.png";
        string currencyCrystalPath = GeneratedFolder + "/shop_currency_crystal.png";
        string currencyGemPath = GeneratedFolder + "/shop_currency_gem.png";
        string titlePlatePath = GeneratedFolder + "/shop_title_plate.png";
        string sectionTabPath = GeneratedFolder + "/shop_section_tab.png";
        string listPanelPath = GeneratedFolder + "/shop_list_panel.png";
        string cardPanelPath = GeneratedFolder + "/shop_card_panel.png";
        string buyButtonPath = GeneratedFolder + "/shop_buy_button.png";
        string bannerPanelPath = GeneratedFolder + "/shop_banner_panel.png";
        string getCoinsButtonPath = GeneratedFolder + "/shop_get_coins_button.png";
        string navPanelPath = GeneratedFolder + "/shop_nav_panel.png";
        string navActivePath = GeneratedFolder + "/shop_nav_active.png";
        string navProfilePath = GeneratedFolder + "/shop_nav_profile_icon.png";
        string hexOutlinePath = GeneratedFolder + "/shop_hex_outline.png";

        SaveBackgroundTexture(backgroundPath, 1080, 1920);
        SavePanelTexture(outerFramePath, 1080, 1920, 44f, Hex("041244", 0.06f), Hex("28B6FF", 0.9f), 5f, true);
        SavePanelTexture(topFramePath, 1048, 152, 34f, Hex("07123D", 0.30f), Hex("31B8FF", 0.8f), 4f, true);
        SaveHexTexture(avatarFramePath, 150, 150, Hex("2A135B", 0.55f), Hex("BB79FF", 0.95f), 7f);
        SaveHexTexture(levelBadgePath, 96, 96, Hex("5126C9", 0.95f), Hex("D68AFF", 1f), 5f);
        SavePanelTexture(progressFramePath, 256, 42, 18f, Hex("0C133D", 0.82f), Hex("9368FF", 0.92f), 3f, false);
        SavePanelTexture(progressFillPath, 180, 28, 12f, Hex("6B46FF", 0.95f), Hex("A885FF", 0.95f), 2f, false);
        SavePanelTexture(currencyPanelPath, 250, 76, 24f, Hex("0B123A", 0.88f), Hex("6E52FF", 0.86f), 3f, false);
        SaveCoinIcon(currencyCoinPath, 96, 96);
        SaveCrystalIcon(currencyCrystalPath, 96, 96, Hex("58DBFF", 1f), Hex("2371FF", 1f));
        SaveCrystalIcon(currencyGemPath, 96, 96, Hex("E574FF", 1f), Hex("7325D6", 1f));
        SavePanelTexture(titlePlatePath, 900, 210, 64f, Hex("111147", 0.70f), Hex("935AFF", 0.95f), 6f, true);
        SavePanelTexture(sectionTabPath, 460, 104, 44f, Hex("2A155D", 0.90f), Hex("A956FF", 0.96f), 5f, true);
        SavePanelTexture(listPanelPath, 980, 980, 34f, Hex("07123A", 0.56f), Hex("785EFF", 0.72f), 4f, true);
        SavePanelTexture(cardPanelPath, 920, 260, 22f, Hex("071C4B", 0.76f), Hex("5C62D6", 0.82f), 3f, false);
        SavePanelTexture(buyButtonPath, 260, 92, 28f, Hex("6F2EFF", 0.96f), Hex("C079FF", 1f), 5f, true);
        SavePanelTexture(bannerPanelPath, 980, 210, 24f, Hex("08164B", 0.72f), Hex("7259E6", 0.82f), 3f, false);
        SavePanelTexture(getCoinsButtonPath, 310, 92, 28f, Hex("145CC9", 0.96f), Hex("63C7FF", 0.96f), 4f, true);
        SavePanelTexture(navPanelPath, 1080, 194, 34f, Hex("06113E", 0.88f), Hex("3C7EFF", 0.72f), 4f, false);
        SavePanelTexture(navActivePath, 190, 180, 28f, Hex("3B146F", 0.80f), Hex("9A55FF", 0.96f), 4f, true);
        SaveProfileIcon(navProfilePath, 96, 96);
        SaveHexOutline(hexOutlinePath, 96, 96);

        AssetDatabase.Refresh();

        return new ShopSprites
        {
            background = LoadSprite(backgroundPath),
            outerFrame = LoadSprite(outerFramePath),
            topFrame = LoadSprite(topFramePath),
            avatarFrame = LoadSprite(avatarFramePath),
            levelBadge = LoadSprite(levelBadgePath),
            progressFrame = LoadSprite(progressFramePath),
            progressFill = LoadSprite(progressFillPath),
            currencyPanel = LoadSprite(currencyPanelPath),
            currencyCoin = LoadSprite(currencyCoinPath),
            currencyCrystal = LoadSprite(currencyCrystalPath),
            currencyGem = LoadSprite(currencyGemPath),
            titlePlate = LoadSprite(titlePlatePath),
            sectionTab = LoadSprite(sectionTabPath),
            listPanel = LoadSprite(listPanelPath),
            cardPanel = LoadSprite(cardPanelPath),
            buyButton = LoadSprite(buyButtonPath),
            bannerPanel = LoadSprite(bannerPanelPath),
            getCoinsButton = LoadSprite(getCoinsButtonPath),
            navPanel = LoadSprite(navPanelPath),
            navActive = LoadSprite(navActivePath),
            navProfile = LoadSprite(navProfilePath)
        };
    }

    private static void SaveBackgroundTexture(string path, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            float v = (float)y / (height - 1);
            for (int x = 0; x < width; x++)
            {
                float u = (float)x / (width - 1);
                Color baseColor = Color.Lerp(Hex("020615", 1f), Hex("08134A", 1f), v);
                float nebula = Mathf.PerlinNoise(u * 3.8f + 4.1f, v * 7.2f + 1.6f);
                float band = Mathf.Sin((u * 2.5f + v * 6.0f) * Mathf.PI);
                float violet = Mathf.Clamp01((nebula - 0.44f) * 1.7f + Mathf.Max(0f, band) * 0.12f);
                baseColor = Color.Lerp(baseColor, Hex("421285", 1f), violet * 0.45f);
                baseColor = Color.Lerp(baseColor, Hex("0C4BA0", 1f), Mathf.Clamp01(nebula - 0.62f) * 0.25f);
                texture.SetPixel(x, y, baseColor);
            }
        }

        System.Random random = new System.Random(1209);
        for (int i = 0; i < 260; i++)
        {
            int x = random.Next(18, width - 18);
            int y = random.Next(18, height - 18);
            int r = random.Next(1, 3);
            Color color = random.NextDouble() > 0.65 ? Hex("A975FF", 1f) : Color.white;
            color.a = Mathf.Lerp(0.36f, 0.95f, (float)random.NextDouble());
            DrawCircle(texture, x, y, r, color);
            if (random.NextDouble() > 0.84)
            {
                DrawLine(texture, new Vector2(x - 8, y), new Vector2(x + 8, y), Hex("A9E7FF", 0.55f), 1f);
                DrawLine(texture, new Vector2(x, y - 8), new Vector2(x, y + 8), Hex("A9E7FF", 0.55f), 1f);
            }
        }

        for (int i = 0; i < 18; i++)
        {
            int x = random.Next(0, width);
            int y = random.Next(0, height);
            float radius = random.Next(12, 34);
            DrawPolygon(texture, new[]
            {
                new Vector2(x, y + radius),
                new Vector2(x + radius * 0.8f, y + radius * 0.2f),
                new Vector2(x + radius * 0.45f, y - radius * 0.85f),
                new Vector2(x - radius * 0.55f, y - radius * 0.6f),
                new Vector2(x - radius * 0.85f, y + radius * 0.18f)
            }, Hex("6B5D9D", 0.28f));
        }

        SaveTexture(path, texture, Vector4.zero);
    }

    private static void SavePanelTexture(string path, int width, int height, float cut, Color fill, Color border, float borderWidth, bool glow)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color transparent = new Color(0f, 0f, 0f, 0f);
        Vector2[] polygon = CutRect(width, height, cut);

        for (int y = 0; y < height; y++)
        {
            float v = (float)y / (height - 1);
            for (int x = 0; x < width; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                bool inside = PointInPolygon(point, polygon);
                if (!inside)
                {
                    float outsideDistance = MinDistanceToEdges(point, polygon);
                    if (glow && outsideDistance < borderWidth * 2.4f)
                    {
                        Color glowColor = border;
                        glowColor.a *= Mathf.Clamp01(1f - outsideDistance / (borderWidth * 2.4f)) * 0.34f;
                        texture.SetPixel(x, y, glowColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, transparent);
                    }
                    continue;
                }

                float edgeDistance = MinDistanceToEdges(point, polygon);
                Color pixel = fill;
                pixel = Color.Lerp(pixel, Color.white, Mathf.Clamp01(v - 0.78f) * 0.08f);
                pixel = Color.Lerp(pixel, Hex("4D28B1", pixel.a), Mathf.Clamp01(1f - Mathf.Abs(v - 0.5f) * 2f) * 0.08f);

                if (edgeDistance < borderWidth)
                {
                    pixel = border;
                    pixel.a = border.a;
                }
                else if (edgeDistance < borderWidth + 5f)
                {
                    pixel = Color.Lerp(pixel, border, Mathf.Clamp01(1f - (edgeDistance - borderWidth) / 5f) * 0.45f);
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        SaveTexture(path, texture, new Vector4(cut, cut, cut, cut));
    }

    private static void SaveHexTexture(string path, int width, int height, Color fill, Color border, float borderWidth)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        float radius = Mathf.Min(width, height) * 0.44f;
        Vector2[] polygon = HexPoints(center, radius);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                if (!PointInPolygon(p, polygon))
                {
                    texture.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                    continue;
                }

                float d = MinDistanceToEdges(p, polygon);
                Color c = d < borderWidth ? border : fill;
                texture.SetPixel(x, y, c);
            }
        }

        SaveTexture(path, texture, Vector4.zero);
    }

    private static void SaveHexOutline(string path, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Vector2[] polygon = HexPoints(new Vector2(width * 0.5f, height * 0.5f), Mathf.Min(width, height) * 0.42f);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                bool inside = PointInPolygon(p, polygon);
                float d = MinDistanceToEdges(p, polygon);
                texture.SetPixel(x, y, inside && d < 3.5f ? Color.white : new Color(0f, 0f, 0f, 0f));
            }
        }
        SaveTexture(path, texture, Vector4.zero);
    }

    private static void SaveCoinIcon(string path, int width, int height)
    {
        Texture2D texture = ClearTexture(width, height);
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        DrawCircle(texture, (int)center.x, (int)center.y, width * 0.42f, Hex("FFB31E", 1f));
        DrawCircle(texture, (int)center.x, (int)center.y, width * 0.34f, Hex("FFD84F", 1f));
        DrawCircle(texture, (int)center.x, (int)center.y, width * 0.25f, Hex("F59B17", 1f));
        DrawCircle(texture, (int)(center.x + width * 0.08f), (int)(center.y + height * 0.12f), width * 0.08f, Hex("FFF2A0", 0.72f));
        SaveTexture(path, texture, Vector4.zero);
    }

    private static void SaveCrystalIcon(string path, int width, int height, Color light, Color dark)
    {
        Texture2D texture = ClearTexture(width, height);
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        Vector2[] gem =
        {
            new Vector2(center.x, height * 0.9f),
            new Vector2(width * 0.82f, height * 0.62f),
            new Vector2(width * 0.70f, height * 0.18f),
            new Vector2(width * 0.30f, height * 0.18f),
            new Vector2(width * 0.18f, height * 0.62f)
        };
        DrawPolygon(texture, gem, dark);
        DrawPolygon(texture, new[] { gem[0], gem[1], center }, light);
        DrawPolygon(texture, new[] { gem[0], center, gem[4] }, Color.Lerp(light, Color.white, 0.25f));
        DrawPolygon(texture, new[] { gem[2], gem[3], center }, Color.Lerp(dark, Color.black, 0.25f));
        DrawLine(texture, gem[0], gem[2], Color.white, 2f);
        DrawLine(texture, gem[0], gem[3], Hex("FFFFFF", 0.55f), 2f);
        DrawLine(texture, gem[1], gem[4], Hex("FFFFFF", 0.4f), 2f);
        SaveTexture(path, texture, Vector4.zero);
    }

    private static void SaveProfileIcon(string path, int width, int height)
    {
        Texture2D texture = ClearTexture(width, height);
        DrawCircle(texture, width / 2, (int)(height * 0.62f), width * 0.18f, Hex("D8D7FF", 1f));
        DrawCircle(texture, width / 2, (int)(height * 0.28f), width * 0.28f, Hex("A4A6FF", 0.92f));
        DrawCircle(texture, width / 2, (int)(height * 0.28f), width * 0.19f, new Color(0f, 0f, 0f, 0f));
        SaveTexture(path, texture, Vector4.zero);
    }

    private static void SaveTexture(string path, Texture2D texture, Vector4 border)
    {
        texture.Apply();
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }

    private static Texture2D ClearTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                texture.SetPixel(x, y, clear);
        return texture;
    }

    private static Vector2[] CutRect(int width, int height, float cut)
    {
        return new[]
        {
            new Vector2(cut, 0f),
            new Vector2(width - cut, 0f),
            new Vector2(width, cut),
            new Vector2(width, height - cut),
            new Vector2(width - cut, height),
            new Vector2(cut, height),
            new Vector2(0f, height - cut),
            new Vector2(0f, cut)
        };
    }

    private static Vector2[] HexPoints(Vector2 center, float radius)
    {
        Vector2[] points = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (90f + i * 60f);
            points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
        return points;
    }

    private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    private static float MinDistanceToEdges(Vector2 point, Vector2[] polygon)
    {
        float min = float.MaxValue;
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Length];
            float d = DistanceToSegment(point, a, b);
            if (d < min)
                min = d;
        }
        return min;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(point - a, ab) / Mathf.Max(0.0001f, Vector2.Dot(ab, ab));
        t = Mathf.Clamp01(t);
        return Vector2.Distance(point, a + ab * t);
    }

    private static void DrawCircle(Texture2D texture, int cx, int cy, float radius, Color color)
    {
        int r = Mathf.CeilToInt(radius);
        for (int y = cy - r; y <= cy + r; y++)
        {
            if (y < 0 || y >= texture.height)
                continue;
            for (int x = cx - r; x <= cx + r; x++)
            {
                if (x < 0 || x >= texture.width)
                    continue;
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (d <= radius)
                {
                    Color existing = texture.GetPixel(x, y);
                    texture.SetPixel(x, y, AlphaBlend(existing, color));
                }
            }
        }
    }

    private static void DrawPolygon(Texture2D texture, Vector2[] polygon, Color color)
    {
        int minX = Mathf.Clamp(Mathf.FloorToInt(MinX(polygon)), 0, texture.width - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt(MaxX(polygon)), 0, texture.width - 1);
        int minY = Mathf.Clamp(Mathf.FloorToInt(MinY(polygon)), 0, texture.height - 1);
        int maxY = Mathf.Clamp(Mathf.CeilToInt(MaxY(polygon)), 0, texture.height - 1);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (PointInPolygon(new Vector2(x + 0.5f, y + 0.5f), polygon))
                {
                    Color existing = texture.GetPixel(x, y);
                    texture.SetPixel(x, y, AlphaBlend(existing, color));
                }
            }
        }
    }

    private static void DrawLine(Texture2D texture, Vector2 a, Vector2 b, Color color, float thickness)
    {
        float length = Vector2.Distance(a, b);
        int steps = Mathf.CeilToInt(length);
        for (int i = 0; i <= steps; i++)
        {
            Vector2 p = Vector2.Lerp(a, b, steps == 0 ? 0f : (float)i / steps);
            DrawCircle(texture, Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), thickness, color);
        }
    }

    private static float MinX(Vector2[] points)
    {
        float value = points[0].x;
        for (int i = 1; i < points.Length; i++)
            value = Mathf.Min(value, points[i].x);
        return value;
    }

    private static float MaxX(Vector2[] points)
    {
        float value = points[0].x;
        for (int i = 1; i < points.Length; i++)
            value = Mathf.Max(value, points[i].x);
        return value;
    }

    private static float MinY(Vector2[] points)
    {
        float value = points[0].y;
        for (int i = 1; i < points.Length; i++)
            value = Mathf.Min(value, points[i].y);
        return value;
    }

    private static float MaxY(Vector2[] points)
    {
        float value = points[0].y;
        for (int i = 1; i < points.Length; i++)
            value = Mathf.Max(value, points[i].y);
        return value;
    }

    private static Color AlphaBlend(Color below, Color above)
    {
        float a = above.a + below.a * (1f - above.a);
        if (a <= 0.0001f)
            return new Color(0f, 0f, 0f, 0f);
        return new Color(
            (above.r * above.a + below.r * below.a * (1f - above.a)) / a,
            (above.g * above.a + below.g * below.a * (1f - above.a)) / a,
            (above.b * above.a + below.b * below.a * (1f - above.a)) / a,
            a);
    }

    private static TMP_FontAsset LoadFont()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/Montserrat-Bold SDF.asset");
        if (font == null)
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        return font;
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Color Hex(string hex, float alpha)
    {
        Color color;
        if (!ColorUtility.TryParseHtmlString("#" + hex, out color))
            color = Color.white;
        color.a = alpha;
        return color;
    }

    private struct ShopSprites
    {
        public Sprite background;
        public Sprite outerFrame;
        public Sprite topFrame;
        public Sprite avatarFrame;
        public Sprite levelBadge;
        public Sprite progressFrame;
        public Sprite progressFill;
        public Sprite currencyPanel;
        public Sprite currencyCoin;
        public Sprite currencyCrystal;
        public Sprite currencyGem;
        public Sprite titlePlate;
        public Sprite sectionTab;
        public Sprite listPanel;
        public Sprite cardPanel;
        public Sprite buyButton;
        public Sprite bannerPanel;
        public Sprite getCoinsButton;
        public Sprite navPanel;
        public Sprite navActive;
        public Sprite navProfile;
    }
}
