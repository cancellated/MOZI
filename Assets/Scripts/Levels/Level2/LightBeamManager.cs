using UnityEngine;

/// <summary>
/// 光线管理器，负责处理光线的发射、反射和命中检测
/// </summary>
public class LightBeamManager : MonoBehaviour
{
    [Header("光线设置")]
    [Tooltip("光线发射起点，通常是一个空物体的Transform")]
    [SerializeField] private Transform lightOrigin;
    
    [Tooltip("用于渲染光线的LineRenderer组件，负责在场景中绘制光线路径")]
    [SerializeField] private LineRenderer beamRenderer;
    
    [Tooltip("光线可以反射的层，用于限制光线只能与特定层交互")]
    [SerializeField] private LayerMask reflectLayers;
    
    [Tooltip("最大反射次数，限制光线在场景中最多可以反射多少次")]
    [SerializeField] private int maxReflections = 5;

    [Header("通关设置")]
    [Tooltip("目标区域的位置，用于检测光线是否命中目标")]
    [SerializeField] private Transform targetZone;
    
    [Tooltip("目标区域的半径，用于定义光线命中的有效范围")]
    [SerializeField] private float targetRadius = 0.5f;

    /// <summary>
    /// 每帧更新光线路径并检查是否命中目标
    /// </summary>
    private void Update()
    {
        UpdateBeamPath(); // 更新光线路径
        CheckTargetHit(); // 检查是否命中目标
    }

    /// <summary>
    /// 更新光线路径，处理光线反射
    /// </summary>
    private bool hasPassedThroughLens = false;

    private void UpdateBeamPath()
    {
        hasPassedThroughLens = false; // 重置状态
        Vector3 direction = lightOrigin.right;
        Vector3 currentPos = lightOrigin.position;
        
        beamRenderer.positionCount = 1;
        beamRenderer.SetPosition(0, currentPos);

        for (int i = 0; i < maxReflections; i++)
        {
            if (Physics.Raycast(currentPos, direction, out RaycastHit hit, 100f))
            {
                beamRenderer.positionCount++;
                beamRenderer.SetPosition(beamRenderer.positionCount - 1, hit.point);
                
                if (hit.collider.CompareTag("ConvexLens"))
                {
                    hasPassedThroughLens = true; // 标记已通过凸透镜
                }
                if (hit.collider.CompareTag("Mirror"))
                {
                    var mirrorControl = hit.collider.GetComponent<BaseMirrorControl>();
                    if (mirrorControl != null)
                    {
                        // 检查光线是否从正面照射
                        if (Vector3.Dot(direction, -mirrorControl.surfaceNormal) > 0)
                        {
                            direction = Vector3.Reflect(direction, mirrorControl.surfaceNormal);
                            currentPos = hit.point;
                        }
                        else
                        {
                            break; // 如果从背面照射，结束反射
                        }
                    }
                }
                else if (hit.collider.CompareTag("ConvexLens"))
                {
                    var lensControl = hit.collider.GetComponent<ConvexLensControl>();
                    if (lensControl != null)
                    {
                        direction = lensControl.HandleLightPass(direction, hit.point);
                    }
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
        Collider targetCollider = targetZone.GetComponent<Collider>();
        
        if (targetCollider != null && targetCollider.bounds.Contains(endPos) && hasPassedThroughLens)
        {
            // 禁用所有镜子/透镜的控制
        var mirrors = FindObjectsOfType<BaseMirrorControl>();
        foreach (var mirror in mirrors)
        {
            mirror.DisableControl();
        }
            GetComponent<Level2_Manager>().TriggerLevelComplete();
        }
    }
}