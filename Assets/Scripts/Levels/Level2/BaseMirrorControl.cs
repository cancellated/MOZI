using UnityEngine;

/// <summary>
/// 基础镜子控制脚本，负责镜子的通用功能
/// </summary>
public class BaseMirrorControl : MonoBehaviour
{
    [Header("镜面参数")]
    [Tooltip("镜面法线向量，用于计算反射方向")]
    public Vector3 surfaceNormal;

    [Tooltip("标识镜子是否处于激活状态")]
    protected bool isActive = false;

    [Tooltip("镜子的初始位置，用于限制Z轴移动")]
    protected Vector3 initialPosition;

    private bool isDragging = false;
    private Vector3 offset;

    /// <summary>
    /// 初始化镜子
    /// </summary>
    public virtual void Initialize()
    {
        UpdateSurfaceNormal();
    }

    /// <summary>
    /// 处理鼠标按下事件
    /// </summary>
    protected bool canControl = true;

    /// <summary>
    /// 禁用控制功能
    /// </summary>
    public virtual void DisableControl()
    {
        canControl = false;
    }

    protected virtual void OnMouseDown()
    {
        if (!canControl) return;
        Debug.Log("鼠标按下事件触发");
        if (Input.GetMouseButton(0))
        {
            isDragging = true;
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            offset = transform.position - Camera.main.ScreenToWorldPoint(mousePosition);
        }
    }

    /// <summary>
    /// 处理鼠标释放事件
    /// </summary>
    protected virtual void OnMouseUp()
    {
        isDragging = false;
    }

    /// <summary>
    /// 每帧更新，处理拖动和旋转
    /// </summary>
    protected virtual void Update()
    {
        if (!canControl) return;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            HandleRotation();
        }
        else
        {
            HandleDrag();
        }
    }

    /// <summary>
    /// 处理拖动逻辑
    /// </summary>
    protected virtual void HandleDrag()
    {
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;
            newPosition.z = 0;

            // 限制移动范围
            newPosition.x = Mathf.Clamp(newPosition.x, -10f, 10f);
            newPosition.y = Mathf.Clamp(newPosition.y, -4f, 6f);

            transform.position = newPosition;
        }
    }

    /// <summary>
    /// 处理旋转逻辑
    /// </summary>
    protected virtual void HandleRotation()
    {
        if (isDragging)
        {
            float angleZ = Input.GetAxis("Mouse X") * 4f;
            transform.Rotate(0, 0, angleZ);
            UpdateSurfaceNormal();
        }
    }

    /// <summary>
    /// 更新镜面法线向量
    /// </summary>
    protected virtual void UpdateSurfaceNormal()
    {
        surfaceNormal = transform.up;
    }
}