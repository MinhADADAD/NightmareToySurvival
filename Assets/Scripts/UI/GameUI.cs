using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GameUI : MonoBehaviour
{
    public GameObject menuPanel, helpPanel, pausePanel, resultPanel, hud, bossPanel;
    public Text healthText, scoreText, waveText, countdownText, bossText, resultTitle, resultScore, highScoreText, warningText;
    public Image healthFill, bossFill, damageFlash;
    public RectTransform crosshair;
    public Button playButton, helpButton, quitButton, helpBackButton, resumeButton, pauseRestartButton,
        pauseMenuButton, resultRestartButton, resultMenuButton;
    public Slider menuMusic, menuEffects, pauseMusic, pauseEffects;
    bool helpVisible;
    Canvas canvas;
    static readonly Color Ink = new Color(.025f, .035f, .08f, .96f);
    static readonly Color Accent = new Color(.2f, .85f, .84f);
    static readonly Color Paper = new Color(.94f, .96f, 1);

    void Start()
    {
        canvas = GetComponent<Canvas>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Configure the saved scene's reticle too; no scene rebuild is required.
        Text reticle = crosshair.GetComponent<Text>();
        reticle.text = "+"; reticle.fontSize = 36; reticle.fontStyle = FontStyle.Bold;
        reticle.color = new Color(1f, .92f, .35f);
        crosshair.sizeDelta = new Vector2(48, 48);
        Outline outline = crosshair.GetComponent<Outline>();
        if (!outline) outline = crosshair.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, .95f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        GameManager game = GameManager.Instance;
        playButton.onClick.AddListener(game.Play);
        helpButton.onClick.AddListener(delegate { helpVisible = true; });
        helpBackButton.onClick.AddListener(delegate { helpVisible = false; });
        quitButton.onClick.AddListener(game.Quit); resumeButton.onClick.AddListener(game.Resume);
        pauseRestartButton.onClick.AddListener(game.Restart); pauseMenuButton.onClick.AddListener(game.Menu);
        resultRestartButton.onClick.AddListener(game.Restart); resultMenuButton.onClick.AddListener(game.Menu);
        BindVolume(menuMusic, true); BindVolume(pauseMusic, true);
        BindVolume(menuEffects, false); BindVolume(pauseEffects, false);
    }
    void BindVolume(Slider slider, bool music)
    {
        slider.value = music ? AudioManager.Instance.MusicVolume : AudioManager.Instance.EffectsVolume;
        slider.onValueChanged.AddListener(delegate(float value) {
            if (music) AudioManager.Instance.SetMusic(value); else AudioManager.Instance.SetEffects(value);
        });
    }
    void Update()
    {
        GameManager game = GameManager.Instance;
        bool menu = game.State == GameState.Menu, paused = game.State == GameState.Paused;
        menuPanel.SetActive(menu && !helpVisible); helpPanel.SetActive(menu && helpVisible);
        if (paused && !pausePanel.activeSelf)
        { pauseMusic.value = AudioManager.Instance.MusicVolume; pauseEffects.value = AudioManager.Instance.EffectsVolume; }
        pausePanel.SetActive(paused);
        bool ended = game.State == GameState.Won || game.State == GameState.Lost;
        resultPanel.SetActive(ended && game.gameOver.Visible); hud.SetActive(!menu);
        crosshair.gameObject.SetActive(GameManager.IsPlaying);
        if (GameManager.IsPlaying) crosshair.anchoredPosition = (Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f)) / canvas.scaleFactor;
        Cursor.visible = true;
        healthText.text = "MÁU  " + game.player.currentHealth + " / " + game.player.startingHealth;
        healthFill.fillAmount = (float)game.player.currentHealth / game.player.startingHealth;
        scoreText.text = "ĐIỂM  " + game.score.Score;
        highScoreText.text = "KỶ LỤC  " + game.score.HighScore;
        waveText.text = "ĐỢT " + game.waves.WaveNumber + " / " + game.waves.waves.Length + "\nQUÁI CÒN LẠI  " + game.waves.Remaining;
        countdownText.text = game.waves.Countdown > 0 && !ended ? "ĐỢT " + game.waves.WaveNumber + " BẮT ĐẦU SAU " + Mathf.CeilToInt(game.waves.Countdown) : "";
        warningText.text = game.waves.SpawnWarning;
        EnemyHealth boss = game.waves.Boss;
        bossPanel.SetActive(boss && !boss.IsDead && !menu && !ended);
        if (boss) { bossFill.fillAmount = (float)boss.currentHealth / boss.startingHealth; bossText.text = "HELLEPHANT • CHÚA ÁC MỘNG   " + boss.currentHealth + " / " + boss.startingHealth; }
        damageFlash.color = new Color(1, .05f, .12f, Mathf.Clamp01(1 - (Time.time - game.player.LastHitTime) * 3) * .22f);
        resultTitle.text = game.State == GameState.Won ? "BÌNH MINH ĐÃ ĐẾN" : "CƠN ÁC MỘNG CHƯA DỨT";
        resultScore.text = (game.State == GameState.Won ? "Bạn đã vượt qua cả 10 đợt!" : "Bạn đã đến đợt " + game.waves.WaveNumber + ".") +
            "\nĐIỂM  " + game.score.Score + "     •     KỶ LỤC  " + game.score.HighScore;
    }
    void OnDestroy() { Cursor.visible = true; }

    // Called by the Editor setup tool. Every UI object/reference is saved in scene.
    public void BuildUI()
    {
        Canvas cv = gameObject.AddComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
        gameObject.AddComponent<GraphicRaycaster>();
        if (!FindObjectOfType<EventSystem>()) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        hud = Rect("HUD", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
        RectTransform hp = Panel("Health", hud.transform, new Vector2(0,1), new Vector2(0,1), new Vector2(206,-68), new Vector2(372,104));
        healthText = Label(hp, "MÁU 100 / 100", new Vector2(0,22), new Vector2(336,30), 22);
        healthFill = Bar(hp, new Vector2(0,-5), new Vector2(332,12), Accent);
        scoreText = Label(hp, "ĐIỂM 0", new Vector2(0,-31), new Vector2(336,26), 18);
        RectTransform wave = Panel("Wave", hud.transform, Vector2.one, Vector2.one, new Vector2(-174,-60), new Vector2(308,88));
        waveText = Label(wave, "ĐỢT 0 / 10", Vector2.zero, new Vector2(284,72), 21);
        countdownText = Label(hud.transform, "", new Vector2(0,-145), new Vector2(650,38), 24);
        countdownText.rectTransform.anchorMin = countdownText.rectTransform.anchorMax = new Vector2(.5f,1);
        warningText = Label(hud.transform, "", new Vector2(0,-188), new Vector2(920,50), 18);
        warningText.rectTransform.anchorMin = warningText.rectTransform.anchorMax = new Vector2(.5f,1);
        bossPanel = Panel("Boss", hud.transform, new Vector2(.5f,0), new Vector2(.5f,0), new Vector2(0,55), new Vector2(620,66)).gameObject;
        bossText = Label(bossPanel.transform, "HELLEPHANT", new Vector2(0,13), new Vector2(595,28), 20);
        bossFill = Bar(bossPanel.transform, new Vector2(0,-14), new Vector2(580,14), new Color(1,.32f,.38f));
        Text reticle = Label(transform, "+", Vector2.zero, new Vector2(32,32), 30); reticle.color = Accent; crosshair = reticle.rectTransform;
        damageFlash = Rect("DamageFlash", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject.AddComponent<Image>();
        damageFlash.color = Color.clear; damageFlash.raycastTarget = false;

        menuPanel = Overlay("MainMenu");
        Label(menuPanel.transform, "NIGHTMARE", new Vector2(0,224), new Vector2(950,70), 56).color = Accent;
        Label(menuPanel.transform, "TOY SURVIVAL", new Vector2(0,173), new Vector2(850,52), 34);
        Label(menuPanel.transform, "Một căn phòng. Mười đợt quái. Sống sót đến bình minh.", new Vector2(0,126), new Vector2(1000,36), 20);
        highScoreText = Label(menuPanel.transform, "KỶ LỤC 0", new Vector2(0,88), new Vector2(600,30), 20);
        playButton = ButtonAt(menuPanel.transform, "CHƠI", 31);
        helpButton = ButtonAt(menuPanel.transform, "HƯỚNG DẪN", -25);
        quitButton = ButtonAt(menuPanel.transform, "THOÁT", -81);
        menuMusic = Volume(menuPanel.transform, "Nhạc nền", -154);
        menuEffects = Volume(menuPanel.transform, "Hiệu ứng", -199);

        helpPanel = Overlay("Instructions");
        Label(helpPanel.transform, "SỐNG SÓT TRONG PHÒNG ĐỒ CHƠI", new Vector2(0,220), new Vector2(1080,55), 30).color = Accent;
        Label(helpPanel.transform,
            "WASD / phím mũi tên: di chuyển\nChuột: ngắm trên sàn • Giữ chuột trái: bắn\nEsc: tạm dừng / tiếp tục\n\nZombunny: nhanh, ít máu • ZomBear: cân bằng\nHellephant: chậm, nhiều máu, đòn đánh mạnh\n\nTường và đồ chơi chặn đạn. Luôn di chuyển để giữ khoảng cách.\nDọn sạch quái để sang đợt mới; nghỉ 5 giây và hồi 15 máu.\nHạ boss ở đợt 10 và toàn bộ quái còn lại để chiến thắng.",
            new Vector2(0,20), new Vector2(1080,350), 23);
        helpBackButton = ButtonAt(helpPanel.transform, "QUAY LẠI", -230);

        pausePanel = Overlay("Pause");
        Label(pausePanel.transform, "TẠM DỪNG", new Vector2(0,210), new Vector2(800,60), 40).color = Accent;
        resumeButton = ButtonAt(pausePanel.transform, "TIẾP TỤC", 102);
        pauseRestartButton = ButtonAt(pausePanel.transform, "CHƠI LẠI", 46);
        pauseMenuButton = ButtonAt(pausePanel.transform, "VỀ MENU", -10);
        pauseMusic = Volume(pausePanel.transform, "Nhạc nền", -104);
        pauseEffects = Volume(pausePanel.transform, "Hiệu ứng", -153);
        resultPanel = Overlay("Result");
        resultTitle = Label(resultPanel.transform, "", new Vector2(0,156), new Vector2(1120,70), 39); resultTitle.color = Accent;
        resultScore = Label(resultPanel.transform, "", new Vector2(0,54), new Vector2(1000,90), 25);
        resultRestartButton = ButtonAt(resultPanel.transform, "CHƠI LẠI", -60);
        resultMenuButton = ButtonAt(resultPanel.transform, "VỀ MENU", -116);
        hud.SetActive(false); helpPanel.SetActive(false); pausePanel.SetActive(false); resultPanel.SetActive(false);
        crosshair.gameObject.SetActive(false);
    }
    GameObject Overlay(string name)
    {
        RectTransform r = Rect(name, transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image bg = r.gameObject.AddComponent<Image>(); bg.color = Ink; return r.gameObject;
    }
    static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        RectTransform r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent, false); r.anchorMin = min; r.anchorMax = max; r.anchoredPosition = pos; r.sizeDelta = size; return r;
    }
    static RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        RectTransform r = Rect(name,parent,min,max,pos,size); Image bg = r.gameObject.AddComponent<Image>();
        bg.color = new Color(.025f,.035f,.08f,.8f); bg.raycastTarget = false; return r;
    }
    static Text Label(Transform parent, string text, Vector2 pos, Vector2 size, int fontSize)
    {
        Text t = Rect("Text",parent,Vector2.one*.5f,Vector2.one*.5f,pos,size).gameObject.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); t.text = text; t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter; t.color = Paper; t.raycastTarget = false; return t;
    }
    static Image Bar(Transform parent, Vector2 pos, Vector2 size, Color color)
    {
        RectTransform bg = Panel("Bar",parent,Vector2.one*.5f,Vector2.one*.5f,pos,size);
        Image fill = Rect("Fill",bg,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject.AddComponent<Image>();
#if UNITY_EDITOR
        fill.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#endif
        fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.color = color; fill.raycastTarget = false; return fill;
    }
    static Button ButtonAt(Transform parent, string text, float y)
    {
        RectTransform r = Rect(text,parent,Vector2.one*.5f,Vector2.one*.5f,new Vector2(0,y),new Vector2(352,46));
        Image image = r.gameObject.AddComponent<Image>(); image.color = new Color(.1f,.24f,.3f);
        Button b = r.gameObject.AddComponent<Button>(); b.targetGraphic = image;
        ColorBlock colors = b.colors; colors.highlightedColor = new Color(.4f,1,1); colors.pressedColor = Accent; b.colors = colors;
        Label(r,text,Vector2.zero,new Vector2(340,42),22); return b;
    }
    static Slider Volume(Transform parent, string text, float y)
    {
        Label(parent,text,new Vector2(-160,y),new Vector2(160,30),19);
        RectTransform r = Rect(text,parent,Vector2.one*.5f,Vector2.one*.5f,new Vector2(92,y),new Vector2(285,26));
        Image bg = r.gameObject.AddComponent<Image>(); bg.color = new Color(.12f,.2f,.28f);
        Slider s = r.gameObject.AddComponent<Slider>();
        RectTransform fill = Rect("Fill",r,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        fill.gameObject.AddComponent<Image>().color = Accent; s.fillRect = fill;
        RectTransform handle = Rect("Handle",r,Vector2.zero,Vector2.one,Vector2.zero,new Vector2(16,0));
        Image hi = handle.gameObject.AddComponent<Image>(); hi.color = Color.white; s.handleRect = handle; s.targetGraphic = hi;
        s.minValue = 0; s.maxValue = 1; s.value = .5f; return s;
    }
}
