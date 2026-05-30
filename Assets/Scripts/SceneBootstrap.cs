using UnityEngine;

/// <summary>
/// 场景引导器 — 运行时自动检测并搭建缺失的 GameObjects
/// 确保无论场景初始状态如何，按 Play 即可运行
/// </summary>
public static class SceneBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnSceneLoaded()
    {
        // 仅在 Play 模式下运行
        if (!Application.isPlaying) return;

        // 检查是否已有 Managers（应该由 EditorSetup 创建，若是则跳过）
        SunShadowController controller = Object.FindObjectOfType<SunShadowController>();
        if (controller != null)
        {
            // 场景已配置，只补全引用
            EnsureReferences(controller);
            return;
        }

        Debug.Log("[知雀] 检测到新场景，自动搭建中...");

        // 1. 确保有 MainCamera
        EnsureMainCamera();

        // 2. 确保有 Directional Light
        EnsureSunLight();

        // 3. 确保有 Sun Visual
        EnsureSunVisual();

        // 4. 确保有 Ground + Pole
        EnsureGround();

        // 5. 创建 Managers
        EnsureManagers();

        // 6. 确保有 UI Canvas
        EnsureUI();

        Debug.Log("[知雀] 场景自动搭建完成！");
    }

    static void EnsureMainCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            camObj.tag = "MainCamera";
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.5f, 0.7f, 1f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.fieldOfView = 50f;

        if (cam.transform.position == Vector3.zero)
            cam.transform.position = new Vector3(10f, 12f, -10f);
        cam.transform.LookAt(Vector3.zero);

        // 确保有轨道控制器
        if (!cam.gameObject.TryGetComponent<CameraOrbitController>(out _))
        {
            CameraOrbitController orbit = cam.gameObject.AddComponent<CameraOrbitController>();
            orbit.distance = 15f;
        }
    }

    static void EnsureSunLight()
    {
        Light[] lights = Object.FindObjectsOfType<Light>();
        Light dirLight = null;
        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional) { dirLight = l; break; }
        }

        if (dirLight == null)
        {
            GameObject sunObj = new GameObject("SunLight");
            dirLight = sunObj.AddComponent<Light>();
        }

        dirLight.type = LightType.Directional;
        dirLight.intensity = 2f;
        dirLight.color = new Color(1f, 0.95f, 0.85f);
        dirLight.shadows = LightShadows.Soft;
        dirLight.shadowStrength = 0.8f;

        dirLight.transform.position = new Vector3(10f, 15f, -10f);
        dirLight.transform.LookAt(Vector3.zero);

        RenderSettings.sun = dirLight;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
    }

    static void EnsureSunVisual()
    {
        GameObject existing = GameObject.Find("SunVisual");
        if (existing != null) return;

        GameObject sunVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sunVis.name = "SunVisual";
        sunVis.transform.position = new Vector3(10f, 15f, -10f);
        sunVis.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        Material glowMat = new Material(Shader.Find("Standard"));
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.5f) * 2f);
        glowMat.color = new Color(1f, 0.95f, 0.7f);

        MeshRenderer mr = sunVis.GetComponent<MeshRenderer>();
        mr.material = glowMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }

    static void EnsureGround()
    {
        GroundBuilder existing = Object.FindObjectOfType<GroundBuilder>();
        if (existing != null) return;

        GameObject groundCtrl = new GameObject("GroundController");
        groundCtrl.AddComponent<GroundBuilder>();
    }

    static void EnsureManagers()
    {
        SunShadowController existing = Object.FindObjectOfType<SunShadowController>();
        if (existing != null) return;

        GameObject mgr = new GameObject("Managers");
        SunShadowController controller = mgr.AddComponent<SunShadowController>();

        // 在下一帧设置引用（等待其他 GameObjects 创建完毕）
        EnsureReferences(controller);
    }

    static void EnsureReferences(SunShadowController controller)
    {
        // 使用延迟调用确保所有对象都创建完毕
        Object.FindObjectOfType<MonoBehaviour>()?.StartCoroutine(DelayedSetup(controller));
    }

    static System.Collections.IEnumerator DelayedSetup(SunShadowController controller)
    {
        yield return null; // 等一帧

        if (controller == null) yield break;

        // 设置方向光引用
        if (controller.sunLight == null)
        {
            Light[] lights = Object.FindObjectsOfType<Light>();
            foreach (Light l in lights)
                if (l.type == LightType.Directional) { controller.sunLight = l; break; }
        }

        // 设置太阳可视化
        if (controller.sunVisual == null)
        {
            GameObject sv = GameObject.Find("SunVisual");
            if (sv != null) controller.sunVisual = sv.transform;
        }
    }

    static void EnsureUI()
    {
        TimeUI existing = Object.FindObjectOfType<TimeUI>();
        if (existing != null) return;

        // EventSystem
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Canvas
        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        TimeUI timeUI = canvasObj.AddComponent<TimeUI>();
        UIBuilder builder = canvasObj.AddComponent<UIBuilder>();
        builder.timeUI = timeUI;
        canvasObj.AddComponent<UIInitializer>();
    }
}
