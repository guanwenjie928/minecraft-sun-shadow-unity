using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Unity Editor 自动化设置工具
/// 一键搭建场景：光照、阴影、摄像机、地面、UI Canvas
/// 使用方式: Tools > Minecraft Sun Shadow > Setup Scene
/// </summary>
public class EditorSetup : EditorWindow
{
    [MenuItem("Tools/Minecraft Sun Shadow/Setup Scene")]
    static void SetupScene()
    {
        Debug.Log("=== 开始搭建 Sun & Shadow 场景 ===");

        // 1. 确保场景已保存
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            SetupLighting();
            SetupGround();
            SetupCamera();
            SetupCanvas();
            SetupSunVisual();
            SetupManagers();

            // 保存场景
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("=== 场景搭建完成！按 Play 运行 ===");
        }
    }

    [MenuItem("Tools/Minecraft Sun Shadow/Setup Scene", true)]
    static bool ValidateSetupScene()
    {
        return !EditorApplication.isPlaying;
    }

    // ========== 光照系统 ==========
    static void SetupLighting()
    {
        // 清除已有光照
        foreach (Light light in FindObjectsOfType<Light>())
        {
            if (light.type == LightType.Directional)
                DestroyImmediate(light.gameObject);
        }

        // 创建方向光（太阳）
        GameObject sunObj = new GameObject("SunLight");
        Light sunLight = sunObj.AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.intensity = 2f;
        sunLight.color = new Color(1f, 0.95f, 0.85f);
        sunLight.shadows = LightShadows.Soft;
        sunLight.shadowStrength = 0.8f;
        sunLight.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;
        sunLight.shadowBias = 0.05f;
        sunLight.shadowNormalBias = 0.4f;
        sunLight.shadowNearPlane = 0.1f;
        sunLight.transform.position = new Vector3(10f, 15f, -10f);
        sunLight.transform.LookAt(Vector3.zero);

        // 渲染设置
        RenderSettings.sun = sunLight;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.3f, 0.5f, 0.8f);
        RenderSettings.ambientEquatorColor = new Color(0.2f, 0.3f, 0.4f);
        RenderSettings.ambientGroundColor = new Color(0.15f, 0.25f, 0.15f);
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.reflectionIntensity = 0.5f;

        // 雾效（远处有 Minecraft 风格的感觉）
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.7f, 0.8f, 0.9f);
        RenderSettings.fogStartDistance = 40f;
        RenderSettings.fogEndDistance = 80f;
    }

    // ========== 地面系统 ==========
    static void SetupGround()
    {
        GameObject groundCtrl = new GameObject("GroundController");
        GroundBuilder builder = groundCtrl.AddComponent<GroundBuilder>();
        builder.gridSize = 7;
        builder.blockSize = 0.96f;
        builder.blockHeight = 0.22f;
        builder.poleHeight = 5f;
        builder.autoBuild = true;
    }

    // ========== 摄像机 ==========
    static void SetupCamera()
    {
        // 确保有 MainCamera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        // 设置摄像机位置
        mainCam.transform.position = new Vector3(10f, 12f, -10f);
        mainCam.transform.LookAt(Vector3.zero);
        mainCam.backgroundColor = new Color(0.5f, 0.7f, 1f);
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.nearClipPlane = 0.1f;
        mainCam.farClipPlane = 100f;
        mainCam.fieldOfView = 50f;

        // 添加轨道控制
        CameraOrbitController orbit = mainCam.gameObject.AddComponent<CameraOrbitController>();
        orbit.target = null; // 默认看向原点
        orbit.distance = 15f;
    }

    // ========== 太阳可视化球体 ==========
    static void SetupSunVisual()
    {
        GameObject sunVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sunVis.name = "SunVisual";
        sunVis.transform.position = new Vector3(10f, 15f, -10f);
        sunVis.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        // 发光材质
        Material glowMat = new Material(Shader.Find("Standard"));
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.5f) * 2f);
        glowMat.color = new Color(1f, 0.95f, 0.7f);
        sunVis.GetComponent<MeshRenderer>().material = glowMat;
        sunVis.GetComponent<MeshRenderer>().shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;
        sunVis.GetComponent<MeshRenderer>().receiveShadows = false;
    }

    // ========== UI Canvas ==========
    static void SetupCanvas()
    {
        // 清除已有 Canvas
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null)
            DestroyImmediate(existingCanvas.gameObject);

        // 创建 Canvas
        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 创建 EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSys = new GameObject("EventSystem");
            eventSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSys.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 添加 TimeUI
        TimeUI timeUI = canvasObj.AddComponent<TimeUI>();

        // 创建信息面板
        CreateInfoPanel(canvasObj.transform, timeUI);
    }

    static void CreateInfoPanel(Transform parent, TimeUI timeUI)
    {
        // 使用运行时 UI 辅助脚本自动创建界面
        UIBuilder builder = parent.gameObject.AddComponent<UIBuilder>();
        builder.timeUI = timeUI;
        parent.gameObject.AddComponent<UIInitializer>();
    }

    // ========== 管理器对象 ==========
    static void SetupManagers()
    {
        GameObject managerObj = new GameObject("Managers");
        SunShadowController controller = managerObj.AddComponent<SunShadowController>();
        controller.sunLight = FindObjectOfType<Light>();
        controller.sunVisual = GameObject.Find("SunVisual")?.transform;
        controller.poleHeight = 5f;
        controller.sunDistance = 30f;

        // 检测是否存在 GroundBuilder 的 pole 引用
        GroundBuilder builder = FindObjectOfType<GroundBuilder>();
        if (builder != null && Application.isPlaying)
        {
            // BuildScene 会自动设置 pole 和 shadowIndicator 引用
        }
    }
}

/// <summary>运行时初始化 UI（自动创建按钮）</summary>
public class UIInitializer : MonoBehaviour
{
    void Start()
    {
        UIBuilder builder = GetComponent<UIBuilder>();
        if (builder != null)
            builder.Build();
    }
}
