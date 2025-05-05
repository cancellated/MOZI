using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    [SerializeField] private float lifeTime = 4f; // 石头存在时间
    private bool hasCollided = false;
    
    void Start()
    {
        // 3秒后自动销毁并通知回合结束
        StartCoroutine(DestroyAfterTime());
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        if(!hasCollided)
        {
            Destroy(gameObject);
            if(Level3Manager.Instance != null)
            {
                Level3Manager.Instance.OnPlayerActionComplete();
            }
        }
    }
    // 碰撞检测
    private void OnCollisionEnter(Collision collision)
    {
        if(hasCollided) return; // 防止多次调用
        hasCollided = true;
        // 碰撞到敌人或其他物体时也可以提前销毁
        Destroy(gameObject);
        // 确保Level3Manager实例存在
        if(Level3Manager.Instance != null)
        {
            Level3Manager.Instance.OnPlayerActionComplete();
        }
    }
    
}
