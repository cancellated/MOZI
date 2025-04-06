using UnityEngine;

/// <summary>
/// 光线管理器，负责处理光线的发射、反射和命中检测
/// </summary>
public class LightBeamManager : MonoBehaviour
{
    [Header("光线设置")]
    [Tooltip("光线发射起点")]
    [SerializeField] private Transform lightOrigin;
    
    [Tooltip("用于渲染光线的LineRenderer组件")]
    [SerializeField] private LineRenderer beamRenderer;
    
    [Tooltip("光线可以反射的层")]
    [SerializeField] private LayerMask reflectLayers;
    
    [Tooltip("最大反射次数")]
    [SerializeField] private int maxReflections = 5;

    [Header("通关设置")]
    [Tooltip("目标区域的位置")]
    [SerializeField] private Transform targetZone;
    
    [Tooltip("目标区域的半径")]
    [SerializeField] private float targetRadius = 0.5f;

    /// <summary>
    /// 每帧更新光线路径并检查是否命中目标
    /// </summary>
    private void Update()
    {
        UpdateBeamPath();
        CheckTargetHit();
    }

    /// <summary>
    /// 更新光线路径，处理光线反射
    /// </summary>
    private void UpdateBeamPath()
    {
        Vector3 direction = lightOrigin.right;
        Vector3 currentPos = lightOrigin.position;
        
        beamRenderer.positionCount = 1;
        beamRenderer.SetPosition(0, currentPos);

        for (int i = 0; i < maxReflections; i++)
        {
            if (Physics.Raycast(currentPos, direction, out RaycastHit hit, 100f, reflectLayers))
            {
                beamRenderer.positionCount++;
                beamRenderer.SetPosition(beamRenderer.positionCount - 1, hit.point);
                
                if (hit.collider.CompareTag("Mirror"))
                {
                    direction = Vector3.Reflect(direction, hit.normal);
                    currentPos = hit.point;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 检查光线是否命中目标区域
    /// </summary>
    private void CheckTargetHit()
    {
        Vector3 endPos = beamRenderer.GetPosition(beamRenderer.positionCount - 1);
        if (Vector3.Distance(endPos, targetZone.position) < targetRadius)
        {
            GetComponent<Level2_Manager>().TriggerLevelComplete();
        }
    }
}