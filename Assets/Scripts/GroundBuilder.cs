using UnityEngine;

/// <summary>
/// 程序化生成 Minecraft 风格方块地面
/// 使用 1024x1024 纹理贴图给每个方块的 6 个面
/// </summary>
[ExecuteAlways]
public class GroundBuilder : MonoBehaviour
{
    [Header("网格设置")]
    [Range(3, 15)]
    public int gridSize = 7;                 // N×N 方块网格
    public float blockSize = 0.96f;          // 单方块边长（留0.04缝隙）
    public float blockHeight = 0.22f;        // 方块高度

    [Header("纹理引用")]
    public Texture2D grassTopTex;            // 顶部草皮
    public Texture2D grassSideTex;           // 侧面草皮
    public Texture2D dirtTex;                // 底部泥土

    [Header("标杆设置")]
    public float poleHeight = 5f;            // 标杆高度
    public Texture2D poleTex;                // 标杆纹理

    [Header("生成状态")]
    public bool autoBuild = true;            // 自动构建

    private GameObject groundParent;
    private GameObject poleObj;

    void Start()
    {
        if (Application.isPlaying && autoBuild)
            BuildScene();
    }

    /// <summary>构建整个场景</summary>
    public void BuildScene()
    {
        // 清除旧物件
        if (groundParent != null)
        {
            if (Application.isPlaying)
                Destroy(groundParent);
            else
                DestroyImmediate(groundParent);
        }
        if (poleObj != null)
        {
            if (Application.isPlaying)
                Destroy(poleObj);
            else
                DestroyImmediate(poleObj);
        }

        // 创建父节点
        groundParent = new GameObject("GroundGrid");
        groundParent.transform.SetParent(transform);

        // 创建材质
        Material grassTopMat = CreateMaterial(grassTopTex, 0.85f);
        Material grassSideMat = CreateMaterial(grassSideTex, 0.9f);
        Material dirtMat = CreateMaterial(dirtTex, 1.0f);

        // 生成方块网格
        float halfGrid = (gridSize - 1) * blockSize / 2f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                CreateBlock(
                    new Vector3(x * blockSize - halfGrid, -blockHeight / 2f, z * blockSize - halfGrid),
                    grassTopMat, grassSideMat, dirtMat
                );
            }
        }

        // 构建标杆（中央位置）
        BuildPole();
    }

    /// <summary>创建单个 Minecraft 方块</summary>
    private void CreateBlock(Vector3 position, Material topMat, Material sideMat, Material bottomMat)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = $"Block_{position.x:F0}_{position.z:F0}";
        block.transform.SetParent(groundParent.transform);
        block.transform.position = position;
        block.transform.localScale = new Vector3(blockSize, blockHeight, blockSize);

        // Cube 的默认材质顺序:
        // [0]=+Z前, [1]=-Z后, [2]=+Y上, [3]=-Y下, [4]=-X左, [5]=+X右
        MeshRenderer renderer = block.GetComponent<MeshRenderer>();
        Material[] materials = new Material[6];
        materials[0] = sideMat;  // +Z (前)
        materials[1] = sideMat;  // -Z (后)
        materials[2] = topMat;   // +Y (上) 草顶
        materials[3] = bottomMat;// -Y (下) 泥土
        materials[4] = sideMat;  // -X (左)
        materials[5] = sideMat;  // +X (右)
        renderer.materials = materials;

        // 生成/接受阴影
        block.AddComponent<ShadowCaster>();
    }

    /// <summary>构建带条纹纹理的标杆 + 影子指示器</summary>
    private void BuildPole()
    {
        poleObj = new GameObject("Pole");
        poleObj.transform.SetParent(transform);
        poleObj.transform.position = Vector3.zero; // 原点

        // 标杆圆柱体
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "PoleMesh";
        cylinder.transform.SetParent(poleObj.transform);
        cylinder.transform.localPosition = new Vector3(0f, poleHeight / 2f, 0f);
        cylinder.transform.localScale = new Vector3(0.3f, poleHeight / 2f, 0.3f);

        Material poleMat = CreateMaterial(poleTex, 0.6f);
        if (poleTex == null)
            poleMat.color = new Color(0.55f, 0.4f, 0.2f); // 木质棕色回落
        cylinder.GetComponent<MeshRenderer>().material = poleMat;
        cylinder.AddComponent<ShadowCaster>();

        // 顶部球体（太阳可视化参考点）
        GameObject topBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        topBall.name = "PoleTop";
        topBall.transform.SetParent(poleObj.transform);
        topBall.transform.localPosition = new Vector3(0f, poleHeight, 0f);
        topBall.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        Material ballMat = new Material(Shader.Find("Standard"));
        ballMat.color = new Color(1f, 0.85f, 0.2f); // 金色
        topBall.GetComponent<MeshRenderer>().material = ballMat;
        topBall.AddComponent<ShadowCaster>();

        // 影子方向指示器
        GameObject shadowIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shadowIndicator.name = "ShadowIndicator";
        shadowIndicator.transform.SetParent(poleObj.transform);
        shadowIndicator.transform.localPosition = Vector3.zero;
        shadowIndicator.transform.localScale = new Vector3(0.08f, 0.01f, 5f);

        Material shadowMat = new Material(Shader.Find("Standard"));
        shadowMat.color = new Color(0f, 0f, 0f, 0.7f);
        ShadowRenderHelper shadowHelper = shadowIndicator.AddComponent<ShadowRenderHelper>();
        shadowHelper.SetTransparentMaterial();
        shadowIndicator.GetComponent<MeshRenderer>().material = shadowHelper.transparentMaterial;

        // 将 pole 和 shadowIndicator 引用传给 SunShadowController
        SunShadowController controller = FindObjectOfType<SunShadowController>();
        if (controller != null)
        {
            controller.pole = cylinder.transform;
            controller.shadowIndicator = shadowIndicator.transform;
        }
    }

    /// <summary>创建带纹理的 Standard 材质（纹理为 null 时使用纯色回落）</summary>
    private Material CreateMaterial(Texture2D texture, float roughness)
    {
        Material mat = new Material(Shader.Find("Standard"));
        if (texture != null)
        {
            mat.mainTexture = texture;
        }
        else
        {
            // 回落纯色 —— Minecraft 风格
            mat.color = new Color(0.3f, 0.55f, 0.2f); // 草绿
        }
        mat.SetFloat("_Glossiness", 1f - roughness);
        mat.SetFloat("_Metallic", 0f);
        mat.enableInstancing = true;
        return mat;
    }

    /// <summary>编辑器下自动生成</summary>
    void OnValidate()
    {
#if UNITY_EDITOR
        if (autoBuild && !Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) BuildScene();
            };
        }
#endif
    }
}

/// <summary>自动配置阴影投射/接收的辅助组件</summary>
public class ShadowCaster : MonoBehaviour
{
    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
    }
}

/// <summary>半透明影子材质辅助</summary>
public class ShadowRenderHelper : MonoBehaviour
{
    public Material transparentMaterial;

    public void SetTransparentMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        mat.color = new Color(0f, 0f, 0f, 0.5f);
        transparentMaterial = mat;
    }
}
