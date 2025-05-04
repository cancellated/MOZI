using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToLevel : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private Transform character; // 角色对象
    [SerializeField] private float moveSpeed = 5f; // 移动速度
    [SerializeField] private AnimationCurve moveCurve = new(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1.1f), // 轻微过冲效果
        new Keyframe(1f, 1f)
    ); // 移动动画曲线

    [Header("动画设置")]
    [SerializeField] private Sprite[] walkSprites; // 行走动画序列
    [SerializeField] private float frameRate = 0.1f; // 帧间隔时间
    
    [Header("路径点设置")]
    [SerializeField] private List<Transform> waypoints = new(); // 添加路径点列表
    
    private int currentLevelIndex = 0; // 添加当前关卡索引
    private Coroutine moveCoroutine; // 添加移动协程引用
    
    private SpriteRenderer characterRenderer;
    private Coroutine animationCoroutine;

    void Start()
    {
        characterRenderer = character.GetComponent<SpriteRenderer>();
    }

    private IEnumerator MoveCharacter(int targetIndex)
    {
        if(waypoints.Count == 0 || character == null) yield break;

        // 开始移动动画
        if(walkSprites.Length > 0)
        {
            animationCoroutine = StartCoroutine(PlayWalkAnimation());
        }

        int startIndex = Mathf.Clamp(currentLevelIndex, 0, waypoints.Count-1);
        int endIndex = Mathf.Clamp(targetIndex, 0, waypoints.Count-1);

        Vector3 startPos = waypoints[startIndex].position;
        Vector3 endPos = waypoints[endIndex].position;
        float journeyLength = Vector3.Distance(startPos, endPos);
        float startTime = Time.time;

        while(Vector3.Distance(character.position, endPos) > 0.1f)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = Mathf.Clamp01(distanceCovered / journeyLength);
            float curveValue = moveCurve.Evaluate(fractionOfJourney);

            character.position = Vector3.Lerp(startPos, endPos, curveValue);
            yield return null;
        }

        character.position = endPos;
        currentLevelIndex = targetIndex;
        moveCoroutine = null;
        
        // 停止动画
        if(animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    private IEnumerator PlayWalkAnimation()
    {
        int currentFrame = 0;
        while(true)
        {
            characterRenderer.sprite = walkSprites[currentFrame];
            currentFrame = (currentFrame + 1) % walkSprites.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }
}
