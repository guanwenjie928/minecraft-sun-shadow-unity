using UnityEngine;

/// <summary>
/// 鼠标轨道摄像机控制器
/// 右键拖拽旋转，滚轮缩放，中键平移
/// </summary>
public class CameraOrbitController : MonoBehaviour
{
    [Header("目标")]
    public Transform target;                 // 观察目标（通常是场景中心）

    [Header("旋转设置")]
    [Range(0.1f, 10f)]
    public float rotationSpeed = 3f;
    public bool invertVertical = false;

    [Header("缩放设置")]
    [Range(5f, 50f)]
    public float distance = 15f;
    public float minDistance = 5f;
    public float maxDistance = 40f;
    [Range(0.5f, 5f)]
    public float zoomSpeed = 2f;

    [Header("平移设置")]
    [Range(0.1f, 5f)]
    public float panSpeed = 1f;

    [Header("角度限制")]
    [Range(-89f, 0f)]
    public float minPitch = -30f;
    [Range(0f, 89f)]
    public float maxPitch = 80f;

    // 当前状态
    private float currentYaw = 45f;     // 水平旋转角
    private float currentPitch = 45f;   // 俯仰角
    private Vector3 targetOffset = Vector3.zero; // 平移偏移

    // 输入状态
    private Vector3 lastMousePos;
    private bool isRotating;
    private bool isPanning;

    void Start()
    {
        if (target == null)
        {
            // 没有目标时看向原点
            GameObject go = new GameObject("CameraTarget");
            go.transform.position = Vector3.zero;
            target = go.transform;
        }

        // 初始化角度
        Vector3 dir = (transform.position - target.position).normalized;
        currentPitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
        currentYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        distance = Vector3.Distance(transform.position, target.position);
    }

    void LateUpdate()
    {
        HandleInput();
        UpdateCameraTransform();
    }

    void HandleInput()
    {
        // --- 旋转（右键拖拽） ---
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
            isRotating = false;

        if (isRotating)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            currentYaw += delta.x * rotationSpeed * 0.2f;
            currentPitch -= delta.y * rotationSpeed * 0.2f * (invertVertical ? -1f : 1f);
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            lastMousePos = Input.mousePosition;
        }

        // --- 缩放（滚轮） ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * zoomSpeed * distance * 0.3f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // --- 平移（中键拖拽） ---
        if (Input.GetMouseButtonDown(2))
        {
            isPanning = true;
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(2))
            isPanning = false;

        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            Vector3 right = transform.right;
            Vector3 up = transform.up;

            // 忽略上下旋转分量，只在地面平移
            up.y = 0;
            up.Normalize();
            right.y = 0;
            right.Normalize();

            targetOffset -= (right * delta.x + up * delta.y) * panSpeed * 0.01f;
            lastMousePos = Input.mousePosition;
        }

        // --- 键盘快捷键 ---
        // R 键重置视角
        if (Input.GetKeyDown(KeyCode.R))
            ResetView();
    }

    void UpdateCameraTransform()
    {
        if (target == null) return;

        // 计算球坐标位置
        float pitchRad = currentPitch * Mathf.Deg2Rad;
        float yawRad = currentYaw * Mathf.Deg2Rad;

        Vector3 orbitPos = new Vector3(
            Mathf.Sin(yawRad) * Mathf.Cos(pitchRad) * distance,
            Mathf.Sin(pitchRad) * distance,
            -Mathf.Cos(yawRad) * Mathf.Cos(pitchRad) * distance
        );

        Vector3 targetPos = target.position + targetOffset;
        transform.position = targetPos + orbitPos;
        transform.LookAt(targetPos);
    }

    /// <summary>重置摄像机视角</summary>
    public void ResetView()
    {
        currentYaw = 45f;
        currentPitch = 45f;
        distance = 15f;
        targetOffset = Vector3.zero;
    }

    /// <summary>聚焦到某个位置</summary>
    public void FocusOn(Vector3 position, float newDistance = 15f)
    {
        targetOffset = position;
        distance = newDistance;
    }
}
