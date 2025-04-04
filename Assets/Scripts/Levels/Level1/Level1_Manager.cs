using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Level1_Manager : MonoBehaviour
{
    [Header("所有可控制的物体")]
    [Tooltip("场景中的物体")]
    [SerializeField] public MouseControlObject[] MouseControlObjects; //所有可控制的物体

    [Header("需要推进的摄像机")]
    [Tooltip("需要推进的摄像机")]
    public Transform movingCamera;

    [Header("完成时需要改变的对象")]
    [Tooltip("完成时渐显的图片材质")]
    public Renderer imageRenderer;//渐显完成图
    public Renderer shadowRenderer;//渐亮阴影
    public GameObject AllObject;//完成时消失的物体
    private bool isLevelComplete = false; // 是否通关

    private Dictionary<MouseControlObject, bool> objectStatus = new();
    private Vector3 lastMousePosition; // 上一帧鼠标位置

    //protected override void Initialize()
    //{
    //}
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
                    CompleteObj(obj);
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

    private void CompleteObj(MouseControlObject obj)
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
        if (isLevelComplete) return; // 防止重复调用
        
        isLevelComplete = true;
   
        // 使用StopAllCoroutines确保没有残留的协程
        StopAllCoroutines();
        
        // 启动新的协程组
        StartCoroutine(LevelCompleteSequence(OnLevelSequenceFinished));
    
    }
    private IEnumerator LevelCompleteSequence(Action onComplete)
    {
        int currentLevelId = GameManager.Instance.GetCurrentLevel();

        // 记录完成的协程数量
        int completedCount = 0;
        int totalCoroutines = 3; // 总协程数

     StartCoroutine(RunParallelCoroutine(CameraPullin(), () => completedCount++));
     StartCoroutine(RunParallelCoroutine(FadeToAlphaCoroutine(imageRenderer, 1f, 1f), () => completedCount++));
     StartCoroutine(RunParallelCoroutine(FadeToAlphaCoroutine(shadowRenderer, 0f, 4f), () => completedCount++));

    yield return new WaitUntil(() => completedCount >= totalCoroutines);
        Debug.Log("关卡完成！");
        // 确保所有效果完成后再通知GameManager
        //GameEvents.TriggerLevelComplete(currentLevelId);

        // 显式释放资源
        //Resources.UnloadUnusedAssets();
        // 通过回调通知外部
    onComplete?.Invoke();
    }
    // 定义回调方法
private void OnLevelSequenceFinished()
{
    // 此时协程已完全结束，安全调用
    int currentLevelId = GameManager.Instance.GetCurrentLevel();
    //GameManager.Instance.CompleteLevel(currentLevelId);
}

    private IEnumerator RunParallelCoroutine(IEnumerator coroutine, Action onComplete)
    {
        yield return StartCoroutine(coroutine);
        onComplete?.Invoke();
    }
    /// <summary>
    /// 镜头推进
    /// </summary>
    /// <returns></returns>
    private IEnumerator CameraPullin()
    {
        if (movingCamera != null)
        {
            float journeyLength = 6f;//移动距离
            Vector3 startPosition = movingCamera.position;
            Vector3 targetPosition = new(movingCamera.position.x, movingCamera.position.y, movingCamera.position.z + journeyLength);
            float moveSpeed = 1.0f;
            float start_time = Time.time;
            while (Vector3.Distance(movingCamera.position, targetPosition) > 0.01f)
            {
                float distCovered = (Time.time - start_time) * moveSpeed;
                float fracJourney = distCovered / journeyLength;
                movingCamera.position = Vector3.Lerp(startPosition, targetPosition, fracJourney);
                yield return null;
            }
            AllObject.gameObject.SetActive(false);
            movingCamera.position = targetPosition;
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
    protected void OnDestroy()
{
    // 确保销毁时停止所有协程
    StopAllCoroutines();
    //base.OnDestroy();
}
}
