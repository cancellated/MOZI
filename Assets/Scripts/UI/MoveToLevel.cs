using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToLevel : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private Transform character;

    [Header("关卡动画")]
    [SerializeField] private Animator pathAnimator;
    [SerializeField] private List<AnimationClip> levelTransitionAnims;
    [SerializeField] private float animTransitionTime = 0.5f;
    
    private int currentLevelIndex = 0;
    private Coroutine moveCoroutine;
    private SpriteRenderer characterRenderer;
    private Coroutine animationCoroutine;

    void Start()
    {
        characterRenderer = character.GetComponent<SpriteRenderer>();
    }

    private IEnumerator MoveCharacter(int targetIndex)
    {
        // 设置Animator参数
        pathAnimator.SetInteger("Departure", currentLevelIndex);
        pathAnimator.SetInteger("Destination", targetIndex);
        
        // 设置动画方向
        pathAnimator.SetFloat("Speed", targetIndex > currentLevelIndex ? 1f : -1f);
        
        // 触发过渡动画
        pathAnimator.SetTrigger("StartTransition");
        
        // 等待动画完成
        float animLength = GetAnimationDuration(currentLevelIndex, targetIndex);
        yield return new WaitForSeconds(animLength);
        
        currentLevelIndex = targetIndex;
        moveCoroutine = null;
        
        if(animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

    }

    private float GetAnimationDuration(int fromLevel, int toLevel)
    {
        // 根据关卡索引返回对应动画时长
        int animIndex = Mathf.Abs(fromLevel - toLevel) - 1;
        if(animIndex >= 0 && animIndex < levelTransitionAnims.Count)
        {
            return levelTransitionAnims[animIndex].length;
        }
        return 1.0f;
    }

    public IEnumerator MoveToTargetLevel(int targetLevel)
    {
        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        
        // 根据目标关卡ID设置动画参数
        pathAnimator.SetInteger("Departure", currentLevelIndex);
        pathAnimator.SetInteger("Destination", targetLevel);
        
        moveCoroutine = StartCoroutine(MoveCharacter(targetLevel));
        yield return moveCoroutine;
    }
}
