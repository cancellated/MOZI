using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Level1_Manager : SingletonBase<Level1_Manager>
{
    [Header("所有可控制的物体")]
    [Tooltip("场景中的物体")]
    [SerializeField] public MouseControlObject[] MouseControlObjects; //所有可控制的物体

    [Header("需要推进的摄像机")]
    [Tooltip("需要推进的摄像机")]
    public Transform camera;

    [Header("完成时需要改变的对象")]
    [Tooltip("完成时渐显的图片材质")]
    public Renderer imageRenderer;//渐显完成图
    public Renderer shadowRenderer;//渐亮阴影
    public GameObject AllObject;//完成时消失的物体
    private bool isLevelComplete = false; // 是否通关

    private Dictionary<MouseControlObject, bool> objectStatus = new Dictionary<MouseControlObject, bool>(); // ����״̬�ֵ�
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
                Vector3 mouseDelta = currentMousePosition - obj.lastMousePosition;
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
    #region 通关逻辑

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
                    completeObj(obj);
                    Debug.Log(obj.name + "已达标！");
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

    private void completeObj(MouseControlObject obj)
    {
        GameObject targetObj = obj.gameObject;
        Transform targetTransform = targetObj.transform.GetChild(0);
        if (targetTransform != null)
        {
            Renderer renderer = targetTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                StartCoroutine(FadeInCoroutine(renderer, Color.white, 0.5f));
            }
            else
            {
                Debug.LogError("子物体上没有Renderer组件！");
            }
        }
        else
        {
            Debug.LogError("没有找到子物体！");
        }
    }
    /// <summary>
    /// 触发通关逻辑
    /// </summary>
    private void LevelComplete()
    {
        isLevelComplete = true;
        Debug.Log("你过关！");
        StartCoroutine(CameraPullin());
        StartCoroutine(FadeToAlphaCoroutine(imageRenderer, 1f, 1f));//图片显现
        StartCoroutine(FadeToAlphaCoroutine(shadowRenderer, 0f, 4f));//阴影消失
        // 在这里可以添加通关后的逻辑，比如显示通关界面、播放音效等
        GameEvents.TriggerLevelComplete(0);
    }
    /// <summary>
    /// 镜头推进
    /// </summary>
    /// <returns></returns>
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
            AllObject.gameObject.SetActive(false);
            camera.position = targetPosition;
        }
        else
        {
            Debug.LogError("未找到摄像机");
        }
    }


    /// <summary>
    /// 材质透明的渐变
    /// </summary>
    /// <param name="targetMaterial"></param>
    /// <param name="targetAlpha"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private System.Collections.IEnumerator FadeToAlphaCoroutine(Renderer renderer, float targetAlpha, float duration)
    {
        Material targetMaterial = renderer.material;
        float startAlpha = targetMaterial.color.a;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            Color color = targetMaterial.color;
            color.a = alpha;
            targetMaterial.color = color;
            yield return null;
        }
    }
    /// <summary>
    /// 材质颜色的改变
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="targetColor"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private System.Collections.IEnumerator FadeInCoroutine(Renderer renderer, Color targetColor, float duration)
    {
        Material targetMaterial = renderer.material;
        Color startColor = targetMaterial.color;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color color = Color.Lerp(startColor, targetColor, elapsed / duration);
            targetMaterial.color = color;
            yield return null;
        }
    }
    #endregion
}
