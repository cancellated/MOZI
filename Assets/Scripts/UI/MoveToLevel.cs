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
    
    private int departure = 0;
    private Coroutine moveCoroutine;
    private Coroutine animationCoroutine;

    private IEnumerator MoveCharacter(int destination)
    {
        // 设置动画方向
        pathAnimator.SetFloat("Speed", destination > departure ? 1f : -1f);
        
        // 触发过渡动画
        pathAnimator.SetTrigger("StartTransition");
        
        // 等待动画完成
        float animLength = GetAnimationDuration(departure, destination);
        yield return new WaitForSeconds(animLength);
        pathAnimator.SetInteger("Departure", destination);
        pathAnimator.SetInteger("Destination", destination);
        
        departure = destination;
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
        pathAnimator.SetInteger("Departure", departure);
        pathAnimator.SetInteger("Destination", targetLevel);
        
        moveCoroutine = StartCoroutine(MoveCharacter(targetLevel));
        yield return moveCoroutine;
    }

}
