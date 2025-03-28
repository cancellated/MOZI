using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Level1_Manager : SingletonBase<Level1_Manager>
{
    public MouseControlObject[] MouseControlObjects; // 所有可控制的物体
    public Transform camera;
    public float checkRadius = 0.5f; // 达标检测范围半径
    private bool isLevelComplete = false; // 是否通关

    private Dictionary<MouseControlObject, bool> objectStatus = new Dictionary<MouseControlObject, bool>(); // 物体状态字典
    private Vector3 lastMousePosition; // 上一帧鼠标位置

    protected override void Initialize()
    {
    }
    void Start()
    {
        foreach (MouseControlObject obj in MouseControlObjects)
        {
            objectStatus[obj] = false;
        }
    }

    void Update()
    {
        if (!isLevelComplete)
        {
            HandleInput();
            CheckAllObjects();
        }
    }

    /// <summary>
    /// 处理用户输入
    /// </summary>
    private void HandleInput()
    {
        // 检测鼠标按下事件
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                MouseControlObject selectedObject = hit.transform.GetComponent<MouseControlObject>();
                if (selectedObject != null && !selectedObject.isLocked)
                {
                    selectedObject.lastMousePosition = Input.mousePosition;
                    selectedObject.isDragging = true;
                }
            }
        }

        // 检测鼠标释放事件
        if (Input.GetMouseButtonUp(0))
        {
            foreach (MouseControlObject obj in MouseControlObjects)
            {
                obj.isDragging = false;
                obj.isRotating = false;
                obj.isAdjustingDepth = false;
            }
        }

        // 如果正在拖动物体，处理变换
        foreach (MouseControlObject obj in MouseControlObjects)
        {
            if (obj.isDragging && !obj.isLocked)
            {
                Vector3 currentMousePosition = Input.mousePosition;
                //Vector3 mouseDelta = obj.lastMousePosition - currentMousePosition;
                Vector3 mouseDelta =currentMousePosition - obj.lastMousePosition;
                obj.HandleObjectTransformation(mouseDelta);
                obj.lastMousePosition = currentMousePosition;
            }
        }

        // 处理鼠标滚轮事件（调整深度）
        HandleMouseScrollWheel();
    }

    /// <summary>
    /// 处理鼠标滚轮事件（调整深度）
    /// </summary>
    private void HandleMouseScrollWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            foreach (MouseControlObject obj in MouseControlObjects)
            {
                if (obj.isDragging && !obj.isLocked)
                {
                    Vector3 newPosition = obj.transform.position;
                    newPosition.z += scroll * obj.depthSpeed;

                    // 限制与相机的距离
                    newPosition.z = Mathf.Clamp(newPosition.z, obj.minDistance, obj.maxDistance);

                    obj.transform.position = newPosition;

                    // 更新对称物体位置
                    obj.HandleSymmetricObjectUpdate();
                }
            }
        }
    }

    /// <summary>
    /// 检测所有物体是否到达目标位置
    /// </summary>
    private void CheckAllObjects()
    {
        bool allComplete = true;
        foreach (MouseControlObject obj in MouseControlObjects)
        {
            if (!obj.isComplete)
            {
                if (obj.CheckIfComplete())
                {
                    Debug.Log(obj.name + " 已达标！");
                }
                else
                {
                    allComplete = false;
                }
            }
        }

        if (allComplete && !isLevelComplete)
        {
            LevelComplete();
        }
    }
    #region 通关效果
    /// <summary>
    /// 触发通关逻辑
    /// </summary>
    private void LevelComplete()
    {
        isLevelComplete = true;
        Debug.Log("恭喜，你通关了！");
        StartCoroutine(CameraPullin());
        // 在这里可以添加通关后的逻辑，比如显示通关界面、播放音效等
        GameManager.Instance.CompleteLevel(1);
    }

    private IEnumerator CameraPullin()
    {
        if (camera != null) 
        {
            float journeyLength = 6f;//移动距离
            Vector3 startPosition = camera.position;
            Vector3 targetPosition = new Vector3(camera.position.x, camera.position.y, camera.position.z + journeyLength);
            float moveSpeed = 1.0f;
            float start_time = Time.time;
            while (Vector3.Distance(camera.position, targetPosition) > 0.01f)
            {
                float distCovered = (Time.time - start_time) * moveSpeed;
                float fracJourney = distCovered / journeyLength;
                camera.position = Vector3.Lerp(startPosition, targetPosition, fracJourney);
                yield return null;
            }
            camera.position = targetPosition;
        }
        else
        {
            Debug.LogError("未找到摄像机");
        }
    }

    #endregion
}
