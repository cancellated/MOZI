using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Level1_Manager : SingletonBase<Level1_Manager>
{
<<<<<<< HEAD
    [Header("ËùÓÐ¿É¿ØÖÆµÄÎïÌå")]
    [Tooltip("³¡¾°ÖÐµÄÎïÌå")]
    [SerializeField] public MouseControlObject[] MouseControlObjects; // ËùÓÐ¿É¿ØÖÆµÄÎïÌå

    [Header("ÐèÒªÍÆ½øµÄÉãÏñ»ú")]
    [Tooltip("ÐèÒªÍÆ½øµÄÉãÏñ»ú")]
    public Transform camera;

    [Header("Íê³ÉÍ¼")]
    [Tooltip("Íê³ÉÊ±½¥ÏÔµÄÍ¼Æ¬²ÄÖÊ")]
    public Material fadeInMaterial;
    private bool isLevelComplete = false; // ÊÇ·ñÍ¨¹Ø
=======
    public MouseControlObject[] MouseControlObjects; // ï¿½ï¿½ï¿½Ð¿É¿ï¿½ï¿½Æµï¿½ï¿½ï¿½ï¿½ï¿½
    public Transform camera;
    public float checkRadius = 0.5f; // ï¿½ï¿½ï¿½ï¿½â·¶Î§ï¿½ë¾¶
    private bool isLevelComplete = false; // ï¿½Ç·ï¿½Í¨ï¿½ï¿½
>>>>>>> 8d48690053d2c81e2483a67b2ba6c420f3ad59c1

    private Dictionary<MouseControlObject, bool> objectStatus = new Dictionary<MouseControlObject, bool>(); // ï¿½ï¿½ï¿½ï¿½×´Ì¬ï¿½Öµï¿½
    private Vector3 lastMousePosition; // ï¿½ï¿½Ò»Ö¡ï¿½ï¿½ï¿½Î»ï¿½ï¿½

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
    /// ï¿½ï¿½ï¿½ï¿½ï¿½Ã»ï¿½ï¿½ï¿½ï¿½ï¿½
    /// </summary>
    private void HandleInput()
    {
        // ï¿½ï¿½ï¿½ï¿½ï¿½ê°´ï¿½ï¿½ï¿½Â¼ï¿½
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

        // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Í·ï¿½ï¿½Â¼ï¿½
        if (Input.GetMouseButtonUp(0))
        {
            foreach (MouseControlObject obj in MouseControlObjects)
            {
                obj.isDragging = false;
                obj.isRotating = false;
                obj.isAdjustingDepth = false;
            }
        }

        // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ï¶ï¿½ï¿½ï¿½ï¿½å£¬ï¿½ï¿½ï¿½ï¿½ï¿½ä»»
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

        // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Â¼ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½È£ï¿½
        HandleMouseScrollWheel();
    }

    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Â¼ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½È£ï¿½
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

                    // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ä¾ï¿½ï¿½ï¿½
                    newPosition.z = Mathf.Clamp(newPosition.z, obj.minDistance, obj.maxDistance);

                    obj.transform.position = newPosition;

                    // ï¿½ï¿½ï¿½Â¶Ô³ï¿½ï¿½ï¿½ï¿½ï¿½Î»ï¿½ï¿½
                    obj.HandleSymmetricObjectUpdate();
                }
            }
        }
    }
    #region Í¨¹ØÐ§¹û

    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ç·ñµ½´ï¿½Ä¿ï¿½ï¿½Î»ï¿½ï¿½
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
<<<<<<< HEAD
                    completeObj(obj);
                    Debug.Log(obj.name + " ÒÑ´ï±ê£¡");
=======
                    Debug.Log(obj.name + " ï¿½Ñ´ï¿½ê£¡");
>>>>>>> 8d48690053d2c81e2483a67b2ba6c420f3ad59c1
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
<<<<<<< HEAD
    
    private void completeObj(MouseControlObject obj)
    {
        GameObject targetObj = obj.gameObject;
        Transform targetTransform = targetObj.transform.GetChild(0);
        if (targetTransform != null)
        {
            Material targetMaterial = targetTransform.GetComponent<Material>();
            if (targetMaterial != null)
            {
                StartCoroutine(FadeToAlphaCoroutine(targetMaterial, 0f, 0.5f));
            }
        }
    }
=======
    #region Í¨ï¿½ï¿½Ð§ï¿½ï¿½
>>>>>>> 8d48690053d2c81e2483a67b2ba6c420f3ad59c1
    /// <summary>
    /// ï¿½ï¿½ï¿½ï¿½Í¨ï¿½ï¿½ï¿½ß¼ï¿½
    /// </summary>
    private void LevelComplete()
    {
        isLevelComplete = true;
        Debug.Log("ï¿½ï¿½Ï²ï¿½ï¿½ï¿½ï¿½Í¨ï¿½ï¿½ï¿½Ë£ï¿½");
        StartCoroutine(CameraPullin());
<<<<<<< HEAD
        StartCoroutine(FadeToAlphaCoroutine(fadeInMaterial,1f, 1f));
        // ÔÚÕâÀï¿ÉÒÔÌí¼ÓÍ¨¹ØºóµÄÂß¼­£¬±ÈÈçÏÔÊ¾Í¨¹Ø½çÃæ¡¢²¥·ÅÒôÐ§µÈ
        GameManager.Instance.CompleteLevel(1);
=======
        // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Í¨ï¿½Øºï¿½ï¿½ï¿½ß¼ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ê¾Í¨ï¿½Ø½ï¿½ï¿½æ¡¢ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Ð§ï¿½ï¿½
        GameEvents.TriggerLevelComplete(0);
>>>>>>> 8d48690053d2c81e2483a67b2ba6c420f3ad59c1
    }
    /// <summary>
    /// ¾µÍ·ÍÆ½ø
    /// </summary>
    /// <returns></returns>
    private IEnumerator CameraPullin()
    {
        if (camera != null) 
        {
            float journeyLength = 6f;//ï¿½Æ¶ï¿½ï¿½ï¿½ï¿½ï¿½
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
            Debug.LogError("Î´ï¿½Òµï¿½ï¿½ï¿½ï¿½ï¿½ï¿½");
        }
    }
    /// <summary>
    /// ²ÄÖÊÍ¸Ã÷µÄ½¥±ä
    /// </summary>
    /// <param name="targetMaterial"></param>
    /// <param name="targetAlpha"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private System.Collections.IEnumerator FadeToAlphaCoroutine(Material targetMaterial, float targetAlpha, float duration)
    {
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
    #endregion
}
