using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControlObject : MonoBehaviour
{
    public Transform targetTransform; // 物体的目标位置
    public Vector3 positionTolerance = new Vector3(0.5f, 0.5f, 0.5f); // 位置差值容忍度（X, Y, Z）
    public float rotationTolerance = 5f; // 旋转角度差值容忍度（Z轴）
    public bool isLocked = false; // 物体是否被锁定
    public bool isComplete = false; // 物体是否已达标
    private bool isMovingToTarget = false; // 是否正在移动到目标位置
    // 移动相关参数
    public float moveSpeed = 5f;
    public Vector2 minXMaxX = new Vector2(-10f, 10f);
    public Vector2 minYMaxY = new Vector2(-10f, 10f);
    public float moveSymmetryRatio = 1f; // 移动对称比值，1为严格对称，其他值为相似

    // 旋转相关参数
    public float rotateSpeed = 100f;

    // 缩放相关参数
    public float depthSpeed = 1f;
    public float minDistance = 1f;
    public float maxDistance = 10f;
    public float scaleSymmetryRatio = 1f; // 缩放对称比值，1为严格对称，其他值为相似

    // 对称中心点
    public Vector3 symmetryCenter = Vector3.zero;
    // 对称物体的引用
    public GameObject symmetricObject;

    public Vector3 lastMousePosition;
    public bool isDragging = false;
    public bool isRotating = false;
    public bool isAdjustingDepth = false;

    void Update()
    {
        HandleSymmetricObjectUpdate();

    }


    /// <summary>
    /// 处理物体的变换操作
    /// </summary>
    public void HandleObjectTransformation(Vector3 mouseDelta)
    {
        if (isLocked) return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            HandleRotation(mouseDelta);
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            HandleDepthAdjustment(mouseDelta);
        }
        else
        {
            HandleMovement(mouseDelta);
        }
    }

    /// <summary>
    /// 处理旋转操作
    /// </summary>
    /// <param name="mouseDelta">鼠标移动的增量</param>
    public void HandleRotation(Vector3 mouseDelta)
    {
        isRotating = true;
        // 绕Y轴旋转
        float rotateY = mouseDelta.x * rotateSpeed * Time.deltaTime;
        transform.Rotate(0, rotateY, 0);

        // 对称物体同步旋转
        if (symmetricObject != null)
        {
            symmetricObject.transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
        }
    }

    /// <summary>
    /// 处理深度调整操作（模拟缩放）
    /// </summary>
    /// <param name="mouseDelta">鼠标移动的增量</param>
    public void HandleDepthAdjustment(Vector3 mouseDelta)
    {
        isAdjustingDepth = true;
        // 根据鼠标垂直移动调整Z轴位置
        float depthChange = mouseDelta.y * depthSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position;
        newPosition.z += depthChange;

        // 限制与相机的距离
        newPosition.z = Mathf.Clamp(newPosition.z, minDistance, maxDistance);

        transform.position = newPosition;
    }

    /// <summary>
    /// 处理移动操作
    /// </summary>
    /// <param name="mouseDelta">鼠标移动的增量</param>
    public void HandleMovement(Vector3 mouseDelta)
    {
        isRotating = false;
        isAdjustingDepth = false;
        // 计算移动方向
        Vector3 moveDirection = new Vector3(mouseDelta.x, mouseDelta.y, 0);
        moveDirection.Normalize();

        // 将屏幕方向转换为世界方向
        Vector3 worldMoveDirection = Camera.main.transform.TransformDirection(moveDirection);
        worldMoveDirection.z = 0; // 限制在X-Y平面

        // 移动物体
        Vector3 newPosition = transform.position + worldMoveDirection * moveSpeed * Time.deltaTime;

        // 限制移动范围
        newPosition.x = Mathf.Clamp(newPosition.x, minXMaxX.x, minXMaxX.y);
        newPosition.y = Mathf.Clamp(newPosition.y, minYMaxY.x, minYMaxY.y);

        transform.position = newPosition;
    }

    /// <summary>
    /// 处理对称物体的位置更新
    /// </summary>
    public void HandleSymmetricObjectUpdate()
    {
        if (symmetricObject != null)
        {
            // 计算相对于对称中心的偏移量
            Vector3 offset = transform.position - symmetryCenter;
            Vector3 symmetricOffset = new Vector3();
            //x、y值遵循移动比值
            symmetricOffset.x = offset.x * moveSymmetryRatio;
            symmetricOffset.y = offset.y * moveSymmetryRatio;
            //z值遵循缩放比值
            symmetricOffset.z = offset.z * scaleSymmetryRatio;

            // 计算对称物体的位置
            Vector3 symmetricPosition = symmetryCenter - symmetricOffset;
            symmetricObject.transform.position = symmetricPosition;
        }
    }

    /// <summary>
    /// 处理鼠标滚轮事件（调整深度）
    /// </summary>
    public void HandleMouseScrollWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            isAdjustingDepth = true;
            Vector3 newPosition = transform.position;
            newPosition.z += scroll * depthSpeed;

            // 限制与相机的距离
            newPosition.z = Mathf.Clamp(newPosition.z, minDistance, maxDistance);

            transform.position = newPosition;
        }
    }

    
    /// <summary>
    /// 检测物体是否到达目标位置和旋转角度
    /// </summary>
    /// <returns></returns>
    public bool CheckIfComplete()
    {
        if (isLocked) return false;

        bool positionMatch =
            Mathf.Abs(transform.position.x - targetTransform.position.x) <= positionTolerance.x &&
            Mathf.Abs(transform.position.y - targetTransform.position.y) <= positionTolerance.y &&
            Mathf.Abs(transform.position.z - targetTransform.position.z) <= positionTolerance.z;

        Quaternion targetRotation = targetTransform.rotation;
        float currentRotationZ = transform.eulerAngles.z;
        float targetRotationZ = targetRotation.eulerAngles.z;
        bool rotationMatch = Mathf.Abs(currentRotationZ - targetRotationZ) <= rotationTolerance;

        if (positionMatch && rotationMatch)
        {
            isComplete = true;
            StartCoroutine(MoveToTarget());
            return true;
        }
        return false;
    }

    /// <summary>
    /// 锁定物体，停止响应用户输入
    /// </summary>
    public void LockObject()
    {
        isLocked = true;
    }

    /// <summary>
    /// 从当前位置移动到目标位置
    /// </summary>
    private IEnumerator MoveToTarget()
    {
        isMovingToTarget = true;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float journeyLength = Vector3.Distance(startPosition, targetTransform.position);
        float start_time = Time.time;

        while (Vector3.Distance(transform.position, targetTransform.position) > 0.01f)
        {
            float distCovered = (Time.time - start_time) * moveSpeed;
            float fracJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, targetTransform.position, fracJourney);
            transform.rotation = Quaternion.Lerp(startRotation, targetTransform.rotation, fracJourney);
            HandleSymmetricObjectUpdate();
            yield return null;
        }

        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        HandleSymmetricObjectUpdate();
        LockObject();
        isMovingToTarget = false;
    }
}
