#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// CI/CD WebGL 构建入口
/// 由 GitHub Actions 中 game-ci/unity-builder 调用
/// Build Settings: buildMethod=WebGLBuilder.Build
/// </summary>
public static class WebGLBuilder
{
    public static void Build()
    {
        Debug.Log("[WebGLBuilder] 开始 WebGL 构建...");

        // 1. 添加构建场景
        string[] scenes = new string[]
        {
            "Assets/Scenes/MainScene.unity"
        };

        // 2. 配置 PlayerSettings
        PlayerSettings.WebGL.memorySize = 512;          // 512 MB 内存
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.Full;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.template = "APPLICATION:Default";
        PlayerSettings.runInBackground = false;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.colorSpace = ColorSpace.Gamma;

        // 性能优化
        PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
        QualitySettings.SetQualityLevel(5); // Ultra 画质
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.shadowDistance = 80f;
        QualitySettings.shadows = ShadowQuality.All;

        // 3. 执行构建
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "build/WebGL/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[WebGLBuilder] 构建成功！输出: {options.locationPathName}");
            Debug.Log($"[WebGLBuilder] 构建大小: {report.summary.totalSize / 1024 / 1024} MB");
        }
        else
        {
            Debug.LogError($"[WebGLBuilder] 构建失败: {report.summary.result}");
            EditorApplication.Exit(1);
        }
    }
}
#endif
