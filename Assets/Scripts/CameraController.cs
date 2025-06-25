using UnityEngine;

public class CameraController : MonoBehaviour
{
    // --- 新增：持有对摄像机组件的引用 ---
    private Camera cam;

    [Header("速度设置")]
    [Tooltip("摄像机通过W/A/S/D移动的速度")]
    public float moveSpeed = 10f;

    [Tooltip("摄像机通过Q/E旋转的速度（度/秒）")]
    public float rotationSpeed = 80f;

    // --- 修改：将原来的Zoom变量替换为FOV相关的变量 ---
    [Header("视野缩放设置 (FOV)")]
    [Tooltip("通过滚轮调整FOV的速度")]
    public float fovZoomSpeed = 25f;

    [Tooltip("最小视野（拉近）")]
    public float minFov = 20f;

    [Tooltip("最大视野（拉远）")]
    public float maxFov = 90f;


    [Header("旋转焦点设置")]
    [Tooltip("决定旋转中心的射线检测最大距离")]
    public float focusDistance = 100f;

    [Tooltip("射线只与这些层级的物体发生碰撞")]
    public LayerMask focusLayers;
    
    // --- 在 Awake 中获取 Camera 组件 ---
    void Awake()
    {
        // 获取并缓存挂载在同一个GameObject上的Camera组件
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraController 脚本所在的 GameObject 上没有找到 Camera 组件！");
            this.enabled = false;
        }
    }


    void Update()
    {
        HandleMovement();
        HandleZoom(); // 调用新的FOV缩放方法
    }

    private Vector3 GetFocusPoint()
    {
        Ray cameraRay = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(cameraRay, out RaycastHit hit, focusDistance, focusLayers))
        {
            return hit.point;
        }
        else
        {
            return cameraRay.GetPoint(focusDistance / 2f);
        }
    }
  
    private void HandleMovement()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = transform.right;
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDirection += forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= forward;
        if (Input.GetKey(KeyCode.D)) moveDirection += right;
        if (Input.GetKey(KeyCode.A)) moveDirection -= right;

        if (moveDirection != Vector3.zero)
        {
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 处理鼠标滚轮，通过改变Field of View来实现缩放
    /// </summary>
    private void HandleZoom()
    {
        // 1. 获取鼠标滚轮的滚动值
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Approximately(scrollInput, 0))
        {
            return;
        }

        // 2. 计算新的目标FOV值
        // 滚轮前滚 (scrollInput > 0)，视野应该减小 (拉近)
        // 滚轮后滚 (scrollInput < 0)，视野应该增大 (拉远)
        // 所以我们用减法
        float newFov = cam.fieldOfView - scrollInput * fovZoomSpeed;

        // 3. 使用Mathf.Clamp将新的FOV值限制在预设的最大和最小范围内
        cam.fieldOfView = Mathf.Clamp(newFov, minFov, maxFov);
    }
}