using UnityEngine;

/// <summary>
/// 场景镜子控制脚本，负责镜子的移动、旋转和光线反射
/// </summary>
public class SceneMirrorControl : MonoBehaviour
{
    [Header("镜面参数")]
    [Tooltip("反射角度，范围0到360度")]
    [Range(0, 360)] public float reflectionAngle;
    
    [Tooltip("镜面法线向量，用于计算反射方向")]
    public Vector3 surfaceNormal;

    [Tooltip("当前镜子的索引（0-2）")]
    private int mirrorIndex;

    private bool isDragging = false;
    private Vector3 offset;

    /// <summary>
    /// 初始化镜子
    /// </summary>
    public void Initialize(int index)
    {
        mirrorIndex = index;
        UpdateSurfaceNormal();
    }

    private void OnMouseDown()
    {
        Debug.Log("鼠标按下事件触发");
        
        if (Input.GetMouseButton(0))
        {
            isDragging = true;
            // 添加Z轴偏移计算
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            offset = transform.position - Camera.main.ScreenToWorldPoint(mousePosition);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    private void Update()
    {
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
    private void HandleDrag()
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
    private void HandleRotation()
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
    private void UpdateSurfaceNormal()
    {
        //surfaceNormal = Quaternion.Euler(0, 0, transform.eulerAngles.z) * Vector3.up;
        surfaceNormal = transform.up;
    }
}