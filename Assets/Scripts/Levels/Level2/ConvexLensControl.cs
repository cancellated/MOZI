using UnityEngine;

public class ConvexLensControl : BaseMirrorControl
{
    [Header("凸透镜参数")]
    public float focalLength = 5f;

    public Vector3 HandleLightPass(Vector3 incomingDirection, Vector3 hitPoint)
    {
        Vector3 lensCenter = transform.position;
        
        // 根据入射方向自动调整法线方向，使两面透光一致
        Vector3 effectiveNormal = Vector3.Dot(incomingDirection, surfaceNormal) > 0 
            ? surfaceNormal 
            : -surfaceNormal;
            
        Vector3 focusPoint = lensCenter + effectiveNormal * focalLength;
        Vector3 outgoingDirection = (focusPoint - hitPoint).normalized;

        return outgoingDirection;


        
    }
}