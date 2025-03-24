using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControlObject : MonoBehaviour
{
    public Transform targetTransform; // �����Ŀ��λ��
    public Vector3 positionTolerance = new Vector3(0.5f, 0.5f, 0.5f); // λ�ò�ֵ���̶ȣ�X, Y, Z��
    public float rotationTolerance = 5f; // ��ת�ǶȲ�ֵ���̶ȣ�Z�ᣩ
    public bool isLocked = false; // �����Ƿ�����
    public bool isComplete = false; // �����Ƿ��Ѵ��
    private bool isMovingToTarget = false; // �Ƿ������ƶ���Ŀ��λ��
    // �ƶ���ز���
    public float moveSpeed = 5f;
    public Vector2 minXMaxX = new Vector2(-10f, 10f);
    public Vector2 minYMaxY = new Vector2(-10f, 10f);
    public float moveSymmetryRatio = 1f; // �ƶ��ԳƱ�ֵ��1Ϊ�ϸ�Գƣ�����ֵΪ����

    // ��ת��ز���
    public float rotateSpeed = 100f;

    // ������ز���
    public float depthSpeed = 1f;
    public float minDistance = 1f;
    public float maxDistance = 10f;
    public float scaleSymmetryRatio = 1f; // ���ŶԳƱ�ֵ��1Ϊ�ϸ�Գƣ�����ֵΪ����

    // �Գ����ĵ�
    public Vector3 symmetryCenter = Vector3.zero;
    // �Գ����������
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
    /// ��������ı任����
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
    /// ������ת����
    /// </summary>
    /// <param name="mouseDelta">����ƶ�������</param>
    public void HandleRotation(Vector3 mouseDelta)
    {
        isRotating = true;
        // ��Y����ת
        float rotateY = mouseDelta.x * rotateSpeed * Time.deltaTime;
        transform.Rotate(0, rotateY, 0);

        // �Գ�����ͬ����ת
        if (symmetricObject != null)
        {
            symmetricObject.transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
        }
    }

    /// <summary>
    /// ������ȵ���������ģ�����ţ�
    /// </summary>
    /// <param name="mouseDelta">����ƶ�������</param>
    public void HandleDepthAdjustment(Vector3 mouseDelta)
    {
        isAdjustingDepth = true;
        // ������괹ֱ�ƶ�����Z��λ��
        float depthChange = mouseDelta.y * depthSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position;
        newPosition.z += depthChange;

        // ����������ľ���
        newPosition.z = Mathf.Clamp(newPosition.z, minDistance, maxDistance);

        transform.position = newPosition;
    }

    /// <summary>
    /// �����ƶ�����
    /// </summary>
    /// <param name="mouseDelta">����ƶ�������</param>
    public void HandleMovement(Vector3 mouseDelta)
    {
        isRotating = false;
        isAdjustingDepth = false;
        // �����ƶ�����
        Vector3 moveDirection = new Vector3(mouseDelta.x, mouseDelta.y, 0);
        moveDirection.Normalize();

        // ����Ļ����ת��Ϊ���緽��
        Vector3 worldMoveDirection = Camera.main.transform.TransformDirection(moveDirection);
        worldMoveDirection.z = 0; // ������X-Yƽ��

        // �ƶ�����
        Vector3 newPosition = transform.position + worldMoveDirection * moveSpeed * Time.deltaTime;

        // �����ƶ���Χ
        newPosition.x = Mathf.Clamp(newPosition.x, minXMaxX.x, minXMaxX.y);
        newPosition.y = Mathf.Clamp(newPosition.y, minYMaxY.x, minYMaxY.y);

        transform.position = newPosition;
    }

    /// <summary>
    /// ����Գ������λ�ø���
    /// </summary>
    public void HandleSymmetricObjectUpdate()
    {
        if (symmetricObject != null)
        {
            // ��������ڶԳ����ĵ�ƫ����
            Vector3 offset = transform.position - symmetryCenter;
            Vector3 symmetricOffset = new Vector3();
            //x��yֵ��ѭ�ƶ���ֵ
            symmetricOffset.x = offset.x * moveSymmetryRatio;
            symmetricOffset.y = offset.y * moveSymmetryRatio;
            //zֵ��ѭ���ű�ֵ
            symmetricOffset.z = offset.z * scaleSymmetryRatio;

            // ����Գ������λ��
            Vector3 symmetricPosition = symmetryCenter - symmetricOffset;
            symmetricObject.transform.position = symmetricPosition;
        }
    }

    /// <summary>
    /// �����������¼���������ȣ�
    /// </summary>
    public void HandleMouseScrollWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            isAdjustingDepth = true;
            Vector3 newPosition = transform.position;
            newPosition.z += scroll * depthSpeed;

            // ����������ľ���
            newPosition.z = Mathf.Clamp(newPosition.z, minDistance, maxDistance);

            transform.position = newPosition;
        }
    }

    
    /// <summary>
    /// ��������Ƿ񵽴�Ŀ��λ�ú���ת�Ƕ�
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
    /// �������壬ֹͣ��Ӧ�û�����
    /// </summary>
    public void LockObject()
    {
        isLocked = true;
    }

    /// <summary>
    /// �ӵ�ǰλ���ƶ���Ŀ��λ��
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
