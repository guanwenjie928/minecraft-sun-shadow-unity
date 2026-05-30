using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 时间与季节 UI 管理器
/// 提供 24 小时时间轴、季节切换按钮、知识面板
/// </summary>
public class TimeUI : MonoBehaviour
{
    [Header("核心引用")]
    public SunShadowController sunController;

    [Header("时间按钮")]
    public Button[] hourButtons;              // 24 个时间按钮
    public Slider timeSlider;                 // 时间滑动条

    [Header("季节按钮")]
    public Button winterButton;
    public Button equinoxButton;
    public Button summerButton;

    [Header("信息显示")]
    public TMP_Text infoTime;                 // 时刻显示
    public TMP_Text infoSunAltitude;          // 太阳高度角
    public TMP_Text infoSunAzimuth;           // 太阳方位角
    public TMP_Text infoShadowLength;         // 影子长度
    public TMP_Text infoShadowAngle;          // 影子方向
    public TMP_Text infoSeason;               // 当前季节
    public TMP_Text infoTips;                 // 科学知识提示

    [Header("自动播放")]
    public Toggle autoPlayToggle;

    [Header("知识库")]
    [TextArea(3, 10)]
    public string[] tipsWinter;
    [TextArea(3, 10)]
    public string[] tipsEquinox;
    [TextArea(3, 10)]
    public string[] tipsSummer;

    private Color normalButtonColor = new Color(0.25f, 0.25f, 0.3f);
    private Color activeButtonColor = new Color(0.2f, 0.6f, 0.9f);
    private Color seasonActiveColor = new Color(1f, 0.75f, 0.2f);

    void Start()
    {
        if (sunController == null)
            sunController = FindObjectOfType<SunShadowController>();

        SetupButtons();
        SetupSlider();
        SetupSeasonButtons();
        SetupAutoPlay();
        InitializeDefaultTips();
    }

    void Update()
    {
        if (sunController == null) return;
        UpdateInfoDisplay();
    }

    // ========== 时间按钮 ==========

    void SetupButtons()
    {
        for (int i = 0; i < hourButtons.Length && i < 24; i++)
        {
            int hour = i;
            hourButtons[i].onClick.AddListener(() => OnHourClicked(hour));

            // 更新按钮文字
            TMP_Text label = hourButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = $"{hour}时";
        }
    }

    void SetupSlider()
    {
        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = 24f;
            timeSlider.wholeNumbers = false;
            timeSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    void OnHourClicked(int hour)
    {
        if (sunController != null)
            sunController.SetTime(hour);
        HighlightHourButton(hour);
        if (timeSlider != null)
            timeSlider.value = hour;
    }

    void OnSliderChanged(float value)
    {
        if (sunController != null)
            sunController.SetTime(value);
        HighlightHourButton(Mathf.RoundToInt(value));
    }

    void HighlightHourButton(int activeHour)
    {
        for (int i = 0; i < hourButtons.Length && i < 24; i++)
        {
            ColorBlock cb = hourButtons[i].colors;
            cb.normalColor = (i == activeHour) ? activeButtonColor : normalButtonColor;
            hourButtons[i].colors = cb;
        }
    }

    // ========== 季节按钮 ==========

    void SetupSeasonButtons()
    {
        if (winterButton != null)
            winterButton.onClick.AddListener(() => SetSeason(SunShadowController.Season.Winter));
        if (equinoxButton != null)
            equinoxButton.onClick.AddListener(() => SetSeason(SunShadowController.Season.Equinox));
        if (summerButton != null)
            summerButton.onClick.AddListener(() => SetSeason(SunShadowController.Season.Summer));
    }

    void SetSeason(SunShadowController.Season season)
    {
        if (sunController != null)
        {
            sunController.SetSeason(season);
            UpdateSeasonHighlight(season);
            UpdateTips(season);
        }
    }

    void UpdateSeasonHighlight(SunShadowController.Season active)
    {
        SetButtonHighlight(winterButton, active == SunShadowController.Season.Winter);
        SetButtonHighlight(equinoxButton, active == SunShadowController.Season.Equinox);
        SetButtonHighlight(summerButton, active == SunShadowController.Season.Summer);
    }

    void SetButtonHighlight(Button btn, bool active)
    {
        if (btn == null) return;
        ColorBlock cb = btn.colors;
        cb.normalColor = active ? seasonActiveColor : normalButtonColor;
        btn.colors = cb;
    }

    // ========== 自动播放 ==========

    void SetupAutoPlay()
    {
        if (autoPlayToggle != null)
            autoPlayToggle.onValueChanged.AddListener(val =>
            {
                if (sunController != null)
                    sunController.autoPlay = val;
            });
    }

    // ========== 信息更新 ==========

    void UpdateInfoDisplay()
    {
        SunShadowController ctrl = sunController;

        if (infoTime != null)
            infoTime.text = $"当前时刻: {ctrl.CurrentHour:F1} 时";

        if (infoSunAltitude != null)
            infoSunAltitude.text = $"太阳高度角: {ctrl.SunAltitude:F1}°";

        if (infoSunAzimuth != null)
            infoSunAzimuth.text = $"太阳方位角: {ctrl.SunAzimuth:F1}°";

        if (infoShadowLength != null)
            infoShadowLength.text = $"影子长度: {ctrl.ShadowLength:F2} 米";

        if (infoShadowAngle != null)
            infoShadowAngle.text = $"影子方向: {ctrl.ShadowAngle:F1}°";

        if (infoSeason != null)
        {
            string seasonName = ctrl.CurrentSeason switch
            {
                SunShadowController.Season.Winter => "冬至",
                SunShadowController.Season.Equinox => "春分/秋分",
                SunShadowController.Season.Summer => "夏至",
                _ => "未知"
            };
            infoSeason.text = $"当前季节: {seasonName}";
        }
    }

    void UpdateTips(SunShadowController.Season season)
    {
        if (infoTips == null) return;

        string[] tips = season switch
        {
            SunShadowController.Season.Winter => tipsWinter,
            SunShadowController.Season.Equinox => tipsEquinox,
            SunShadowController.Season.Summer => tipsSummer,
            _ => tipsEquinox
        };

        if (tips != null && tips.Length > 0)
        {
            // 根据当前时刻选择合适的提示
            float hour = sunController != null ? sunController.CurrentHour : 12f;
            int tipIndex;
            if (hour < 8f) tipIndex = 0;
            else if (hour < 12f) tipIndex = Mathf.Min(1, tips.Length - 1);
            else if (hour < 16f) tipIndex = Mathf.Min(2, tips.Length - 1);
            else tipIndex = Mathf.Min(3, tips.Length - 1);

            infoTips.text = tips[tipIndex];
        }
    }

    void InitializeDefaultTips()
    {
        tipsWinter = new string[]
        {
            "冬至清晨：太阳从东南方升起，高度角很低，影子又长又淡。" +
            "这时太阳在南回归线上方，北半球白天最短。",
            "冬至上午：太阳高度角逐渐升高，但最高也只有约26.5°（北纬40°地区）。" +
            "影子依然很长，约为杆高的2倍。",
            "冬至正午：这是冬至日太阳最高的时刻，但影子长度仍然是杆高的2倍左右。" +
            "影子指向北方，因为太阳在正南方。",
            "冬至傍晚：太阳快速西沉，影子迅速变长。" +
            "16:30 左右太阳就落山了，北半球迎来漫长的夜晚。"
        };

        tipsEquinox = new string[]
        {
            "春分清晨：太阳从正东方升起，高度角适中。" +
            "春分日昼夜等长，太阳直射赤道。",
            "春分上午：太阳稳定上升，影子逐渐缩短。" +
            "上午9点影子长度约等于杆高，方向指向西北。",
            "春分正午：太阳到达最高点约50°（北纬40°），" +
            "影子长度约等于杆高的0.84倍，指向正北。",
            "春分傍晚：太阳在正西方落下，影子指向东方。" +
            "18:00左右日落，昼夜各12小时。"
        };

        tipsSummer = new string[]
        {
            "夏至清晨：太阳从东北方早早升起，4:30左右天就亮了。" +
            "太阳直射北回归线(23.5°N)，北半球白天最长。",
            "夏至上午：太阳快速攀升，影子迅速缩短。" +
            "到10点时太阳高度角已超过50°，影子比杆还短。",
            "夏至正午：太阳几乎在头顶！高度角约73.5°（北纬40°），" +
            "影子极短，只有杆高的0.3倍。这就是为什么夏天正午几乎看不到影子。",
            "夏至傍晚：太阳迟迟不落，19:30还能看到余晖。" +
            "影子再次拉长，指向东方。"
        };

        UpdateTips(SunShadowController.Season.Equinox);
    }
}
