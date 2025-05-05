using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("敌人属性")]
    private Vector3[] pathPoints = {new Vector3(-12f,-5f,5f),new Vector3(-8.5f,-5f,5f),
    new Vector3(-5f,-5f,5f),new Vector3(-1.5f,-5f,5f),new Vector3(2f,-5f,5f)}; // 敌人移动一格距离
    private int currentPointIndex = 0; // 当前路径点索引;
    public bool isAlive = true;
    internal Action OnDeath;

    private Animator animator;

    private void Start()
    {
        transform.position = pathPoints[currentPointIndex];
        animator = GetComponent<Animator>();
    }

    public void MoveOneStep()
    {
        if(!isAlive) return;
        
        if (currentPointIndex >= pathPoints.Length - 1){
            ArriveDestination();
        }
        else{
            currentPointIndex++; // 移动到下一个路径点
            transform.position = pathPoints[currentPointIndex]; // 更新敌人位置
        }
    }

    public void OnCollisionEnter2D(Collision2D other) 
    { 
        Debug.Log("敌人碰撞");
        if (other.collider.CompareTag("Stone")){
            animator.SetBool("isDie",true);
            Level3Manager.Instance.PlayMusic(2);
            // 敌人死亡函数在动画中调用
            Debug.Log("敌人死亡");
            isAlive = false;
            OnDeath?.Invoke();
        }
    }

    public void Die()
    {               
        if (isAlive) return;
        // 触发死亡事件
        //OnDeath?.Invoke();
        Level3Manager.Instance.StopMusic();
        // 销毁游戏对象
        Destroy(gameObject);
    }

    //有敌人到达终点
    private void ArriveDestination(){
        EnemyManager.Instance.EnemyWin(); // 调用敌人胜利相关逻辑
    }
}