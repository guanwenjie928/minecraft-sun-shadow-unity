using UnityEngine;

/// <summary>
/// 太阳与影子核心控制器
/// 管理太阳位置、时间流逝、季节切换，计算影子长度和方向
/// Science: shadow_length = pole_height / tan(sun_altitude)
/// </summary>
public class SunShadowController : MonoBehaviour
{
    [Header("光照组件")]
    public Light sunLight;                  // 方向光（太阳）
    public Transform sunVisual;             // 太阳可视化球体

    [Header("场景参考")]
    public Transform pole;                  // 标杆 Transform
    public Transform shadowIndicator;       // 影子方向指示器

    [Header("参数")]
    [Range(1f, 10f)]
    public float poleHeight = 5f;           // 标杆高度
    [Range(10f, 50f)]
    public float sunDistance = 30f;         // 太阳距原点距离

    [Header("当前状态（只读）")]
    [SerializeField, Range(0f, 24f)]
    private float currentHour = 12f;        // 当前时刻 (0-24)
    [SerializeField]
    private float sunAzimuth = 180f;        // 太阳方位角
    [SerializeField]
    private float sunAltitude = 45f;        // 太阳高度角
    [SerializeField]
    private float shadowLength = 5f;        // 影子长度
    [SerializeField]
    private float shadowAngle = 0f;         // 影子在地面的方向角
    [SerializeField]
    private Season currentSeason = Season.Equinox;  // 当前季节

    [Header("时间系统")]
    public bool autoPlay = false;
    public float timeSpeed = 0.1f;          // 自动播放速度

    // 材质引用，用于动态调整天空色
    private Material skyboxMat;
    private Color dawnColor = new Color(1f, 0.55f, 0.3f);     // 拂晓橙
    private Color noonColor = new Color(0.5f, 0.75f, 1f);      // 正午蓝
    private Color duskColor = new Color(0.8f, 0.35f, 0.5f);    // 黄昏紫
    private Color nightColor = new Color(0.05f, 0.05f, 0.15f); // 夜晚深蓝

    public enum Season
    {
        Winter,     // 冬至 - 太阳低，影子长
        Equinox,    // 春分/秋分 - 中等
        Summer      // 夏至 - 太阳高，影子短
    }

    // --- 暴露给 UI 的属性 ---
    public float CurrentHour => currentHour;
    public float SunAzimuth => sunAzimuth;
    public float SunAltitude => sunAltitude;
    public float ShadowLength => shadowLength;
    public float ShadowAngle => shadowAngle;
    public Season CurrentSeason => currentSeason;

    void Start()
    {
        // 尝试获取天空盒材质
        skyboxMat = RenderSettings.skybox;
        if (skyboxMat == null)
            skyboxMat = new Material(Shader.Find("Skybox/Procedural"));

        // 初始化到正午状态
        SetTime(12f);
        SetSeason(Season.Equinox);
    }

    void Update()
    {
        if (autoPlay)
        {
            currentHour += timeSpeed * Time.deltaTime;
            if (currentHour > 24f) currentHour -= 24f;
            if (currentHour < 0f) currentHour += 24f;
            UpdateSunPosition();
        }
    }

    // ========== 公开 API ==========

    /// <summary>设置时刻 (0-24)</summary>
    public void SetTime(float hour)
    {
        currentHour = Mathf.Clamp(hour, 0f, 24f);
        UpdateSunPosition();
    }

    /// <summary>设置季节</summary>
    public void SetSeason(Season season)
    {
        currentSeason = season;
        UpdateSunPosition();
    }

    /// <summary>获取太阳光的颜色（基于时刻）</summary>
    public Color GetSunColor()
    {
        if (currentHour < 5f || currentHour > 20f) return nightColor;
        if (currentHour < 7f) return Color.Lerp(dawnColor, noonColor, (currentHour - 5f) / 2f);
        if (currentHour < 17f) return noonColor;
        if (currentHour < 19f) return Color.Lerp(noonColor, duskColor, (currentHour - 17f) / 2f);
        return Color.Lerp(duskColor, nightColor, (currentHour - 19f) / 1f);
    }

    /// <summary>获取完整的太阳数据快照</summary>
    public SunData GetSunData()
    {
        return new SunData
        {
            hour = currentHour,
            azimuth = sunAzimuth,
            altitude = sunAltitude,
            shadowLength = shadowLength,
            shadowAngle = shadowAngle,
            season = currentSeason
        };
    }

    // ========== 核心计算 ==========

    private void UpdateSunPosition()
    {
        // 1. 计算太阳赤纬（基于季节）
        float declination = GetDeclination();

        // 2. 计算太阳时角（基于时刻）
        //    正午12点 = 0度，每小时间隔15度
        float hourAngle = (currentHour - 12f) * 15f;

        // 3. 计算太阳高度角
        //    假设观测纬度 ~40°N（中国华北地区）
        float latitude = 40f;
        float latRad = Mathf.Deg2Rad * latitude;
        float decRad = Mathf.Deg2Rad * declination;
        float haRad = Mathf.Deg2Rad * hourAngle;

        float sinAlt = Mathf.Sin(latRad) * Mathf.Sin(decRad)
                     + Mathf.Cos(latRad) * Mathf.Cos(decRad) * Mathf.Cos(haRad);
        sunAltitude = Mathf.Asin(sinAlt) * Mathf.Rad2Deg;

        // 4. 计算太阳方位角
        float sinDec = Mathf.Sin(decRad);
        float cosDec = Mathf.Cos(decRad);
        float sinLat = Mathf.Sin(latRad);
        float cosLat = Mathf.Cos(latRad);
        float sinHA = Mathf.Sin(haRad);
        float cosHA = Mathf.Cos(haRad);

        float cosAz = (sinDec - sinLat * sinAlt) / (cosLat * Mathf.Cos(Mathf.Asin(sinAlt)));
        cosAz = Mathf.Clamp(cosAz, -1f, 1f);
        float azimuthRad = Mathf.Acos(cosAz);

        // 上午方位角为负（偏东），下午为正（偏西）
        if (hourAngle > 0) azimuthRad = -azimuthRad;
        sunAzimuth = azimuthRad * Mathf.Rad2Deg + 180f;

        // 5. 计算影子长度: L = H / tan(altitude)
        if (sunAltitude > 0.1f)
            shadowLength = poleHeight / Mathf.Tan(sunAltitude * Mathf.Deg2Rad);
        else
            shadowLength = 100f; // 太阳在地平线以下，影子极长

        shadowLength = Mathf.Clamp(shadowLength, 0.1f, 50f);

        // 6. 计算影子方向角（与太阳方位角相反）
        shadowAngle = (sunAzimuth + 180f) % 360f;

        // 7. 更新太阳 Transform 位置
        float altRad = sunAltitude * Mathf.Deg2Rad;
        float azRad = sunAzimuth * Mathf.Deg2Rad;

        Vector3 sunPos = new Vector3(
            Mathf.Sin(azRad) * Mathf.Cos(altRad) * sunDistance,
            Mathf.Sin(altRad) * sunDistance,
            -Mathf.Cos(azRad) * Mathf.Cos(altRad) * sunDistance
        );

        if (sunVisual != null)
            sunVisual.position = sunPos;

        // 8. 更新方向光
        if (sunLight != null)
        {
            sunLight.transform.position = sunPos;
            sunLight.transform.LookAt(Vector3.zero);
            sunLight.intensity = Mathf.Clamp01(sunAltitude / 30f) * 2f;

            // 日出日落暖色调
            if (sunAltitude < 15f && sunAltitude > 0f)
                sunLight.color = Color.Lerp(new Color(1f, 0.6f, 0.3f), Color.white,
                    sunAltitude / 15f);
            else
                sunLight.color = Color.white;
        }

        // 9. 更新影子指示器
        UpdateShadowIndicator();

        // 10. 更新天空颜色
        UpdateSkyColor();
    }

    /// <summary>根据季节返回太阳赤纬角</summary>
    private float GetDeclination()
    {
        return currentSeason switch
        {
            Season.Winter => -23.5f,  // 冬至：南回归线
            Season.Equinox => 0f,     // 春分/秋分：赤道
            Season.Summer => 23.5f,   // 夏至：北回归线
            _ => 0f
        };
    }

    /// <summary>更新地面上的影子方向指示线</summary>
    private void UpdateShadowIndicator()
    {
        if (shadowIndicator == null || pole == null) return;

        // 影子从杆底向外延伸
        Vector3 poleBase = pole.position;
        float shadowRad = shadowAngle * Mathf.Deg2Rad;
        Vector3 shadowDir = new Vector3(Mathf.Sin(shadowRad), 0f, Mathf.Cos(shadowRad));

        // 放置指示器
        shadowIndicator.position = poleBase + shadowDir * (shadowLength / 2f);
        shadowIndicator.rotation = Quaternion.LookRotation(shadowDir, Vector3.up);

        // 缩放指示器长度
        Vector3 scale = shadowIndicator.localScale;
        scale.z = shadowLength;
        shadowIndicator.localScale = scale;

        // 可见性：太阳在地平线以下时隐藏
        shadowIndicator.gameObject.SetActive(sunAltitude > 0f);
    }

    /// <summary>动态更新天空颜色</summary>
    private void UpdateSkyColor()
    {
        Color skyColor;
        if (currentHour < 5f || currentHour > 20f)
            skyColor = nightColor;
        else if (currentHour < 6.5f)
            skyColor = Color.Lerp(nightColor, dawnColor, (currentHour - 5f) / 1.5f);
        else if (currentHour < 8f)
            skyColor = Color.Lerp(dawnColor, noonColor, (currentHour - 6.5f) / 1.5f);
        else if (currentHour < 16f)
            skyColor = noonColor;
        else if (currentHour < 18f)
            skyColor = Color.Lerp(noonColor, duskColor, (currentHour - 16f) / 2f);
        else if (currentHour < 19.5f)
            skyColor = Color.Lerp(duskColor, nightColor, (currentHour - 18f) / 1.5f);
        else
            skyColor = nightColor;

        // 用 Camera.main 的背景色来模拟天空（简单方案）
        if (Camera.main != null)
            Camera.main.backgroundColor = skyColor;

        // 环境光也随之改变
        RenderSettings.ambientLight = Color.Lerp(skyColor, Color.white, 0.3f);
    }
}

/// <summary>太阳数据结构，供 UI 使用</summary>
[System.Serializable]
public struct SunData
{
    public float hour;
    public float azimuth;
    public float altitude;
    public float shadowLength;
    public float shadowAngle;
    public SunShadowController.Season season;
}
