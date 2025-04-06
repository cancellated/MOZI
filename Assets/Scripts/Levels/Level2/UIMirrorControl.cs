using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI镜子控制脚本，负责点击UI图片并生成场景中的镜子
/// </summary>
public class UIMirrorControl : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [Header("镜子设置")]
    [Tooltip("场景中对应的3D镜子预制体")]
    [SerializeField] private GameObject mirrorPrefab;

    [Tooltip("当前镜子的索引（0-2）")]
    [SerializeField] private int mirrorIndex;

    private bool isDragging = false;
    private Vector3 initialPosition;
    private GameObject tempMirrorInstance; // 临时镜子实例

    /// <summary>
    /// 处理点击事件，创建实例并开始拖动
    /// </summary>
    private float clickStartTime; // 记录点击开始时间

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("UI对象被点击");
        isDragging = true;
        clickStartTime = Time.time; // 记录点击开始时间
        
        // 创建临时镜子实例
        if (mirrorPrefab != null)
        {
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = Camera.main.nearClipPlane + 1;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPoint);
            worldPosition.z = 0;
            
            tempMirrorInstance = Instantiate(mirrorPrefab, worldPosition, Quaternion.identity);
        }

        // 添加点击事件处理
        StartCoroutine(CheckForClickWithoutDrag());
    }

    private System.Collections.IEnumerator CheckForClickWithoutDrag()
    {
        float clickDuration = 0.2f; // 点击持续时间阈值
        while (Time.time - clickStartTime < clickDuration)
        {
            if (!Input.GetMouseButton(0)) // 如果鼠标已经松开
            {
                OnEndDrag(null); // 调用结束拖动逻辑
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 处理拖动事件，拖动实例
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && tempMirrorInstance != null)
        {
            // 获取鼠标在摄像机视角下的世界坐标
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = Camera.main.nearClipPlane + 1; // 设置合适的Z值
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPoint);
            worldPosition.z = 0; // 固定Z值为0
            
            // 更新临时镜子的位置
            tempMirrorInstance.transform.position = worldPosition;
        }
    }

    /// <summary>
    /// 处理拖动结束事件，完成实例的放置
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging && tempMirrorInstance != null)
        {
            // 初始化场景镜子
            var mirrorControl = tempMirrorInstance.GetComponent<SceneMirrorControl>();
            if (mirrorControl != null)
            {
                mirrorControl.Initialize(mirrorIndex);
            }
            
            isDragging = false;
            tempMirrorInstance = null; // 清除临时实例引用
            
            // 禁用或销毁UI图标
            gameObject.SetActive(false); // 或者 Destroy(gameObject);
        }
    }
}