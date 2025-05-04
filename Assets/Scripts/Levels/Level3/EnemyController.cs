using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("敌人属性")]
    private Vector3[] pathPoints = {new Vector3(-12f,-4f,5f),new Vector3(-8.5f,-4f,5f),
    new Vector3(-5f,-4f,5f),new Vector3(-1.5f,-4f,5f),new Vector3(2f,-4f,5f)}; // 敌人移动一格距离
    private int currentPointIndex = 0; // 当前路径点索引;
    public bool isAlive = true;
    internal Action OnDeath;

    private void Start()
    {
        transform.position = pathPoints[currentPointIndex];
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
            Die(); // 敌人死亡
            Debug.Log("敌人死亡");
        }
    }

    public void Die()
    {
        if(!isAlive) return;
        
        isAlive = false;
        
        // 触发死亡事件
        OnDeath?.Invoke();
        
        // 销毁游戏对象
        Destroy(gameObject, 0.5f);
    }

    //有敌人到达终点
    private void ArriveDestination(){
        EnemyManager.Instance.EnemyWin(); // 调用敌人胜利相关逻辑
    }
}