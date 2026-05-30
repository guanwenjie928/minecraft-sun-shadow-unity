using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 运行时 UI 构建器 — 自动创建所有界面元素
/// 无需手动拖拽，启动时自动生成完整的控制面板
/// </summary>
public class UIBuilder : MonoBehaviour
{
    public TimeUI timeUI;

    public void Build()
    {
        if (timeUI == null)
            timeUI = GetComponent<TimeUI>();
        if (timeUI == null) return;

        CreateHourButtons();
        CreateTimeSlider();
        CreateSeasonButtons();
        CreateInfoPanel();
        CreateAutoPlayToggle();

        // 刷新 TimeUI 的引用
        timeUI.hourButtons = hourButtonList.ToArray();
    }

    private List<Button> hourButtonList = new List<Button>();

    // ========== 24小时按钮栏 ==========
    void CreateHourButtons()
    {
        // 容器
        GameObject bar = MakeUIObject("HourBar", transform);
        HorizontalLayoutGroup barLayout = bar.AddComponent<HorizontalLayoutGroup>();
        barLayout.spacing = 2f;
        barLayout.padding = new RectOffset(5, 5, 5, 5);
        barLayout.childForceExpandWidth = true;
        barLayout.childForceExpandHeight = true;
        barLayout.childControlWidth = true;
        barLayout.childControlHeight = true;

        RectTransform barRt = bar.GetComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0f, 1f);
        barRt.anchorMax = new Vector2(1f, 1f);
        barRt.pivot = new Vector2(0.5f, 1f);
        barRt.anchoredPosition = new Vector2(0f, -10f);
        barRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 36f);

        // 24 个按钮
        for (int i = 0; i < 24; i++)
        {
            GameObject btnObj = MakeUIObject($"Btn_{i}h", bar.transform);
            Button btn = btnObj.AddComponent<Button>();
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.3f);

            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.4f, 0.5f, 0.7f);
            cb.pressedColor = new Color(0.2f, 0.6f, 0.9f);
            btn.colors = cb;

            // 文字
            GameObject label = MakeUIObject("Label", btnObj.transform);
            TMP_Text txt = label.AddComponent<TextMeshProUGUI>();
            txt.text = i.ToString();
            txt.fontSize = 8;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            RectTransform lblRt = label.GetComponent<RectTransform>();
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero;
            lblRt.offsetMax = Vector2.zero;

            hourButtonList.Add(btn);
        }
    }

    // ========== 时间滑动条 ==========
    void CreateTimeSlider()
    {
        GameObject sliderObj = MakeUIObject("TimeSlider", transform);
        RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0f, 1f);
        sliderRt.anchorMax = new Vector2(1f, 1f);
        sliderRt.pivot = new Vector2(0.5f, 1f);
        sliderRt.anchoredPosition = new Vector2(0f, -52f);
        sliderRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 24f;

        // 背景
        GameObject bg = MakeUIObject("Background", sliderObj.transform);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.2f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // 填充
        GameObject fill = MakeUIObject("Fill", bg.transform);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.6f, 0.9f);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        // 滑块
        GameObject handle = MakeUIObject("Handle", sliderObj.transform);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(1f, 0.85f, 0.2f);
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 16f);
        handleRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30f);

        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;

        timeUI.timeSlider = slider;
    }

    // ========== 季节按钮 ==========
    void CreateSeasonButtons()
    {
        GameObject bar = MakeUIObject("SeasonBar", transform);
        HorizontalLayoutGroup barLayout = bar.AddComponent<HorizontalLayoutGroup>();
        barLayout.spacing = 8f;
        barLayout.padding = new RectOffset(10, 10, 5, 5);
        barLayout.childAlignment = TextAnchor.MiddleCenter;

        RectTransform barRt = bar.GetComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0f, 1f);
        barRt.anchorMax = new Vector2(1f, 1f);
        barRt.pivot = new Vector2(0.5f, 1f);
        barRt.anchoredPosition = new Vector2(0f, -90f);
        barRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 44f);

        // 三个季节按钮
        timeUI.winterButton = CreateSeasonButton("❄ 冬至", bar.transform);
        timeUI.equinoxButton = CreateSeasonButton("🌱 春分", bar.transform);
        timeUI.summerButton = CreateSeasonButton("☀ 夏至", bar.transform);
    }

    Button CreateSeasonButton(string label, Transform parent)
    {
        GameObject btnObj = MakeUIObject($"SeasonBtn_{label}", parent);
        Button btn = btnObj.AddComponent<Button>();
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.3f);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 120f);

        GameObject lbl = MakeUIObject("Label", btnObj.transform);
        TMP_Text txt = lbl.AddComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = 14;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;

        RectTransform lblRt = lbl.GetComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = Vector2.zero;
        lblRt.offsetMax = Vector2.zero;

        return btn;
    }

    // ========== 信息面板 ==========
    void CreateInfoPanel()
    {
        // 主面板
        GameObject panel = MakeUIObject("InfoPanel", transform);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.7f);

        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.01f, 0.01f);
        panelRt.anchorMax = new Vector2(0.35f, 0.6f);
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4f;
        vlg.padding = new RectOffset(12, 12, 10, 10);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // 信息文本
        timeUI.infoTime = CreateInfoText("当前时刻: --", panel.transform);
        timeUI.infoSunAltitude = CreateInfoText("太阳高度角: --", panel.transform);
        timeUI.infoSunAzimuth = CreateInfoText("太阳方位角: --", panel.transform);
        timeUI.infoShadowLength = CreateInfoText("影子长度: --", panel.transform);
        timeUI.infoShadowAngle = CreateInfoText("影子方向: --", panel.transform);
        timeUI.infoSeason = CreateInfoText("当前季节: --", panel.transform);

        // 分隔+知识提示
        GameObject sep = MakeUIObject("Separator", panel.transform);
        RectTransform sepRt = sep.GetComponent<RectTransform>();
        sepRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 16f);

        timeUI.infoTips = CreateInfoText("", panel.transform);
        if (timeUI.infoTips != null)
        {
            timeUI.infoTips.fontSize = 11;
            timeUI.infoTips.color = new Color(1f, 0.9f, 0.5f);
            timeUI.infoTips.enableWordWrapping = true;
        }
    }

    TMP_Text CreateInfoText(string text, Transform parent)
    {
        GameObject obj = MakeUIObject("Info_" + text.GetHashCode(), parent);
        TMP_Text tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 13;
        tmp.color = new Color(0.9f, 0.9f, 1f);
        tmp.alignment = TextAlignmentOptions.Left;
        return tmp;
    }

    // ========== 自动播放开关 ==========
    void CreateAutoPlayToggle()
    {
        GameObject toggleObj = MakeUIObject("AutoPlayToggle", transform);
        Toggle toggle = toggleObj.AddComponent<Toggle>();

        RectTransform rt = toggleObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.02f, 0.97f);
        rt.anchorMax = new Vector2(0.15f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // 背景
        GameObject bg = MakeUIObject("Background", toggleObj.transform);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.2f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // 勾选框
        GameObject check = MakeUIObject("Checkmark", bg.transform);
        Image checkImg = check.AddComponent<Image>();
        checkImg.color = new Color(0.2f, 0.8f, 0.3f);
        RectTransform checkRt = check.GetComponent<RectTransform>();
        checkRt.anchorMin = new Vector2(0.1f, 0.2f);
        checkRt.anchorMax = new Vector2(0.4f, 0.8f);
        checkRt.offsetMin = Vector2.zero;
        checkRt.offsetMax = Vector2.zero;

        toggle.graphic = checkImg;
        toggle.targetGraphic = bgImg;

        // 标签
        GameObject labelObj = MakeUIObject("Label", toggleObj.transform);
        TMP_Text label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = "▶ 自动";
        label.fontSize = 10;
        label.color = Color.white;

        RectTransform lblRt = labelObj.GetComponent<RectTransform>();
        lblRt.anchorMin = new Vector2(0.45f, 0f);
        lblRt.anchorMax = new Vector2(1f, 1f);
        lblRt.offsetMin = Vector2.zero;
        lblRt.offsetMax = Vector2.zero;

        timeUI.autoPlayToggle = toggle;
    }

    // ========== 辅助 ==========
    GameObject MakeUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        return obj;
    }
}
