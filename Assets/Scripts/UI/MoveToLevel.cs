using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToLevel : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private Transform character; // 角色对象
    [SerializeField] private float moveSpeed = 5f; // 移动速度
    [SerializeField] private AnimationCurve moveCurve; // 移动动画曲线

    private readonly List<Vector3> waypoints = new(); // 路径点
    private Coroutine moveCoroutine;
    private int currentLevelIndex = 0;

    // 设置路径点（可在编辑器中手动设置或通过代码动态生成）
    public void SetWaypoints(List<Transform> points)
    {
        waypoints.Clear();
        foreach(var point in points)
        {
            waypoints.Add(point.position);
        }
    }

    // 移动到指定关卡
    public void MoveToTargetLevel(int targetLevelIndex)
    {
        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveCharacter(targetLevelIndex));
    }

    private IEnumerator MoveCharacter(int targetIndex)
    {
        if(waypoints.Count == 0 || character == null) yield break;

        int startIndex = Mathf.Clamp(currentLevelIndex, 0, waypoints.Count-1);
        int endIndex = Mathf.Clamp(targetIndex, 0, waypoints.Count-1);

        Vector3 startPos = waypoints[startIndex];
        Vector3 endPos = waypoints[endIndex];
        float journeyLength = Vector3.Distance(startPos, endPos);
        float startTime = Time.time;

        while(Vector3.Distance(character.position, endPos) > 0.1f)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;
            fractionOfJourney = moveCurve.Evaluate(fractionOfJourney);

            character.position = Vector3.Lerp(startPos, endPos, fractionOfJourney);
            yield return null;
        }

        character.position = endPos;
        currentLevelIndex = targetIndex;
        moveCoroutine = null;
    }
}
