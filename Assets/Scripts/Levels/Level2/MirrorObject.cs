using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 镜子对象类，用于控制镜子的交互行为
/// 继承自MonoBehaviour并实现IPointerClickHandler接口以处理点击事件
/// </summary>
public class MirrorObject : MonoBehaviour, IPointerClickHandler
{
    [Header("镜面参数")]
    [Tooltip("反射角度，范围0到360度")]
    [Range(0, 360)] public float reflectionAngle;
    
    [Tooltip("镜面法线向量，用于计算反射方向")]
    public Vector3 surfaceNormal;

    [Tooltip("标识镜子是否处于激活状态")]
    private bool isActive = false;
    
    [Tooltip("镜子的初始位置，用于限制Z轴移动")]
    private Vector3 initialPosition;

    [Tooltip("场景中对应的3D镜子预制体")]
    [SerializeField] private GameObject mirrorPrefab;

    [Tooltip("当前镜子的索引（0-2）")]
    [SerializeField] private int mirrorIndex;

    private GameObject sceneMirrorInstance;

    /// <summary>
    /// 处理点击事件，激活镜子并在场景中实例化对应的3D镜子
    /// </summary>
    /// <param name="eventData">点击事件数据</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isActive)
        {
            ActivateMirror();
            InstantiateSceneMirror();
        }
    }

    /// <summary>
    /// 激活镜子，记录初始位置
    /// </summary>
    private void ActivateMirror()
    {
        isActive = true;
        initialPosition = transform.position;
    }

    /// <summary>
    /// 在场景中实例化对应的3D镜子
    /// </summary>
    private void InstantiateSceneMirror()
    {
        if (mirrorPrefab != null)
        {
            sceneMirrorInstance = Instantiate(mirrorPrefab, initialPosition, Quaternion.identity);
            // 可以根据需要设置3D镜子的初始位置和旋转
        }
        else
        {
            Debug.LogError("未找到对应的3D镜子预制体！");
        }
    }

    /// <summary>
    /// 每帧更新，处理镜子的移动和旋转
    /// </summary>
    private void Update()
    {
        if (isActive)
        {
            HandleMovement();
            HandleRotation();
        }
    }

    /// <summary>
    /// 处理镜子的移动，限制在XY平面
    /// </summary>
    private void HandleMovement()
    {
        // 将屏幕坐标转换为世界坐标
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // 保持Z轴不变，只在XY平面移动
        transform.position = new Vector3(mousePosition.x, mousePosition.y, initialPosition.z);
    }

    /// <summary>
    /// 处理镜子的旋转，使用鼠标右键控制
    /// </summary>
    private void HandleRotation()
    {
        // 当按下鼠标右键时进行旋转
        if (Input.GetMouseButton(1))
        {
            // 根据鼠标X轴移动量计算旋转角度
            float angle = Input.GetAxis("Mouse X") * 2f;
            // 绕Z轴旋转
            transform.Rotate(0, 0, angle);
            // 更新镜面法线
            UpdateSurfaceNormal();
        }
    }

    /// <summary>
    /// 更新镜面法线向量
    /// </summary>
    private void UpdateSurfaceNormal()
    {
        // 根据当前旋转角度计算法线向量
        surfaceNormal = Quaternion.Euler(0, 0, transform.eulerAngles.z) * Vector3.up;
        
        // 同时更新场景中3D镜子的法线
        if (sceneMirrorInstance != null)
        {
            var sceneMirror = sceneMirrorInstance.GetComponent<MirrorObject>();
            if (sceneMirror != null)
            {
                sceneMirror.surfaceNormal = surfaceNormal;
            }
        }
    }
}