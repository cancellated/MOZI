using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Level1_Manager : SingletonBase<Level1_Manager>
{
<<<<<<< HEAD
    [Header("���пɿ��Ƶ�����")]
    [Tooltip("�����е�����")]
    [SerializeField] public MouseControlObject[] MouseControlObjects; // ���пɿ��Ƶ�����

    [Header("��Ҫ�ƽ��������")]
    [Tooltip("��Ҫ�ƽ��������")]
    public Transform camera;

    [Header("���ͼ")]
    [Tooltip("���ʱ���Ե�ͼƬ����")]
    public Material fadeInMaterial;
    private bool isLevelComplete = false; // �Ƿ�ͨ��
=======
    public MouseControlObject[] MouseControlObjects; // ���пɿ��Ƶ�����
    public Transform camera;
    public float checkRadius = 0.5f; // ����ⷶΧ�뾶
    private bool isLevelComplete = false; // �Ƿ�ͨ��
>>>>>>> 8d48690053d2c81e2483a67b2ba6c420f3ad59c1

    private Dictionary<MouseControlObject, bool> objectStatus = new Dictionary<MouseControlObject, bool>(); // ����״̬�ֵ�
    private Vector3 lastMousePosition; // ��һ֡���λ��

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
    /// �����û�����
    /// </summary>
    private void HandleInput()
    {
        // �����갴���¼�
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

        // �������ͷ��¼�
        if (Input.GetMouseButtonUp(0))
        {
            foreach (MouseControlObject obj in MouseControlObjects)
            {
                obj.isDragging = false;
                obj.isRotating = false;
                obj.isAdjustingDepth = false;
            }
        }

        // ��������϶����壬�����任
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

        // �����������¼���������ȣ�
        HandleMouseScrollWheel();
    }

    /// <summary>
    /// �����������¼���������ȣ�
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

                    // ����������ľ���
                    newPosition.z = Mathf.Clamp(newPosition.z, obj.minDistance, obj.maxDistance);

                    obj.transform.position = newPosition;

                    // ���¶Գ�����λ��
                    obj.HandleSymmetricObjectUpdate();
                }
            }
        }
    }
    #region ͨ��Ч��

    /// <summary>
    /// ������������Ƿ񵽴�Ŀ��λ��
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
                    Debug.Log(obj.name + " �Ѵ�꣡");
=======
                    Debug.Log(obj.name + " �Ѵ�꣡");
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
    #region ͨ��Ч��
>>>>>>> 8d48690053d2c81e2483a67b2ba6c420f3ad59c1
    /// <summary>
    /// ����ͨ���߼�
    /// </summary>
    private void LevelComplete()
    {
        isLevelComplete = true;
        Debug.Log("��ϲ����ͨ���ˣ�");
        StartCoroutine(CameraPullin());
<<<<<<< HEAD
        StartCoroutine(FadeToAlphaCoroutine(fadeInMaterial,1f, 1f));
        // ������������ͨ�غ���߼���������ʾͨ�ؽ��桢������Ч��
        GameManager.Instance.CompleteLevel(1);
=======
        // �������������ͨ�غ���߼���������ʾͨ�ؽ��桢������Ч��
        GameEvents.TriggerLevelComplete(0);
>>>>>>> 8d48690053d2c81e2483a67b2ba6c420f3ad59c1
    }
    /// <summary>
    /// ��ͷ�ƽ�
    /// </summary>
    /// <returns></returns>
    private IEnumerator CameraPullin()
    {
        if (camera != null) 
        {
            float journeyLength = 6f;//�ƶ�����
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
            Debug.LogError("δ�ҵ������");
        }
    }
    /// <summary>
    /// ����͸���Ľ���
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
