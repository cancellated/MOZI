using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllableObject : MonoBehaviour
{
    public bool isComplete;//是否达成目的
    // 移动相关参数
    public float moveSpeed = 5f;
    public Vector2 minXMaxX = new Vector2(-10f, 10f);
    public Vector2 minYMaxY = new Vector2(-10f, 10f);
    public float moveSymmetryRatio = 1f; // 移动对称比值，1为严格对称，其他值为相似

    // 旋转相关参数
    public float rotateSpeed = 100f;
    public Vector2 minXRotateMaxXRotate = new Vector2(-360f, 360f);
    public Vector2 minYRotateMaxYRotate = new Vector2(-360f, 360f);

    // 缩放相关参数
    public float depthSpeed = 1f;
    public float minDistance = 1f;
    public float maxDistance = 10f;
    public float scaleSymmetryRatio = 1f; // 缩放对称比值，1为严格对称，其他值为相似

    // 对称中心点
    public Vector3 symmetryCenter = Vector3.zero;
    // 对称物体的引用
    public GameObject symmetricObject;

    private Vector3 lastMousePosition;
    private bool isDragging = false;
    private bool isRotating = false;
    private bool isAdjustingDepth = false;

    void Update()
    {
        HandleMouseInput();
        HandleObjectTransformation();
        HandleSymmetricObjectUpdate();

    }

    /// <summary>
    /// 处理鼠标输入
    /// </summary>
    private void HandleMouseInput()
    {
        // 检测鼠标按下事件
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    isDragging = true;
                    lastMousePosition = Input.mousePosition;
                }
            }
        }

        // 检测鼠标释放事件
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            isRotating = false;
            isAdjustingDepth = false;
        }
    }

    /// <summary>
    /// 处理物体的变换操作
    /// </summary>
    private void HandleObjectTransformation()
    {
        if (!isDragging) return;

        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 mouseDelta = lastMousePosition - currentMousePosition;

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

        lastMousePosition = currentMousePosition;
    }

    /// <summary>
    /// 处理旋转操作
    /// </summary>
    /// <param name="mouseDelta">鼠标移动的增量</param>
    private void HandleRotation(Vector3 mouseDelta)
    {
        isRotating = true;
        // 绕Y轴旋转
        float rotateY = mouseDelta.x * rotateSpeed * Time.deltaTime;
        transform.Rotate(0, rotateY, 0);

        // 绕X轴旋转
        float rotateX = mouseDelta.y * rotateSpeed * Time.deltaTime;
        transform.Rotate(rotateX, 0, 0);

        // 限制旋转角度
        LimitRotation();

        // 对称物体同步旋转
        if (symmetricObject != null)
        {
            symmetricObject.transform.rotation = transform.rotation;
        }
    }

    /// <summary>
    /// 处理深度调整操作（模拟缩放）
    /// </summary>
    /// <param name="mouseDelta">鼠标移动的增量</param>
    private void HandleDepthAdjustment(Vector3 mouseDelta)
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
    private void HandleMovement(Vector3 mouseDelta)
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
    private void HandleSymmetricObjectUpdate()
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
    private void HandleMouseScrollWheel()
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
    /// 限制旋转角度
    /// </summary>
    private void LimitRotation()
    {
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.x = Mathf.Clamp(currentRotation.x, minXRotateMaxXRotate.x, minXRotateMaxXRotate.y);
        currentRotation.y = Mathf.Clamp(currentRotation.y, minYRotateMaxYRotate.x, minYRotateMaxYRotate.y);
        transform.eulerAngles = currentRotation;
    }
}
