using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllableObject : MonoBehaviour
{
    public bool isComplete;//�Ƿ���Ŀ��
    // �ƶ���ز���
    public float moveSpeed = 5f;
    public Vector2 minXMaxX = new Vector2(-10f, 10f);
    public Vector2 minYMaxY = new Vector2(-10f, 10f);
    public float moveSymmetryRatio = 1f; // �ƶ��ԳƱ�ֵ��1Ϊ�ϸ�Գƣ�����ֵΪ����

    // ��ת��ز���
    public float rotateSpeed = 100f;
    public Vector2 minXRotateMaxXRotate = new Vector2(-360f, 360f);
    public Vector2 minYRotateMaxYRotate = new Vector2(-360f, 360f);

    // ������ز���
    public float depthSpeed = 1f;
    public float minDistance = 1f;
    public float maxDistance = 10f;
    public float scaleSymmetryRatio = 1f; // ���ŶԳƱ�ֵ��1Ϊ�ϸ�Գƣ�����ֵΪ����

    // �Գ����ĵ�
    public Vector3 symmetryCenter = Vector3.zero;
    // �Գ����������
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
    /// �����������
    /// </summary>
    private void HandleMouseInput()
    {
        // �����갴���¼�
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

        // �������ͷ��¼�
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            isRotating = false;
            isAdjustingDepth = false;
        }
    }

    /// <summary>
    /// ��������ı任����
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
    /// ������ת����
    /// </summary>
    /// <param name="mouseDelta">����ƶ�������</param>
    private void HandleRotation(Vector3 mouseDelta)
    {
        isRotating = true;
        // ��Y����ת
        float rotateY = mouseDelta.x * rotateSpeed * Time.deltaTime;
        transform.Rotate(0, rotateY, 0);

        // ��X����ת
        float rotateX = mouseDelta.y * rotateSpeed * Time.deltaTime;
        transform.Rotate(rotateX, 0, 0);

        // ������ת�Ƕ�
        LimitRotation();

        // �Գ�����ͬ����ת
        if (symmetricObject != null)
        {
            symmetricObject.transform.rotation = transform.rotation;
        }
    }

    /// <summary>
    /// ������ȵ���������ģ�����ţ�
    /// </summary>
    /// <param name="mouseDelta">����ƶ�������</param>
    private void HandleDepthAdjustment(Vector3 mouseDelta)
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
    private void HandleMovement(Vector3 mouseDelta)
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
    private void HandleSymmetricObjectUpdate()
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
    private void HandleMouseScrollWheel()
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
    /// ������ת�Ƕ�
    /// </summary>
    private void LimitRotation()
    {
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.x = Mathf.Clamp(currentRotation.x, minXRotateMaxXRotate.x, minXRotateMaxXRotate.y);
        currentRotation.y = Mathf.Clamp(currentRotation.y, minYRotateMaxYRotate.x, minYRotateMaxYRotate.y);
        transform.eulerAngles = currentRotation;
    }
}
