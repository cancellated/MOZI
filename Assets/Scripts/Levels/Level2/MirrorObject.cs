using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 镜子对象类，继承自BaseMirrorControl
/// </summary>
public class MirrorObject : BaseMirrorControl, IPointerClickHandler
{
    [Header("镜面参数")]
    [Range(0, 360)] public float reflectionAngle;

    [SerializeField] private GameObject mirrorPrefab;
    [SerializeField] private int mirrorIndex;

    private GameObject sceneMirrorInstance;

    public void OnPointerClick(PointerEventData eventData)
    {
        // if (!isActive)
        // {
        //     isActive = true;
        //     InstantiateSceneMirror();
        // }
    }

    private void InstantiateSceneMirror()
    {
        if (mirrorPrefab != null)
        {
            sceneMirrorInstance = Instantiate(mirrorPrefab, initialPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("未找到对应的3D镜子预制体！");
        }
    }

   
}