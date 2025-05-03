using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

/// <summary>
/// 敌人管理器，负责敌人生成、波次控制和移动管理
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("敌人生成设置")]
    [SerializeField] private GameObject enemyPrefab; // 敌人预制体
    [SerializeField] private int totalEnemies = 5; // 总敌人数

    [Header("UI")]
    [SerializeField] private Text enemyCountText; // 显示当前敌人数量的文本


    private List<GameObject> activeEnemies = new List<GameObject>(); // 当前活跃的敌人列表
    private int spawnedCount = 0; // 已生成的敌人数
    private bool canSpawnEnemies = true; // 是否可以生成敌人

    public static EnemyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        
    }
    //更新游戏设定
    public void ResetLevel(){
        spawnedCount = 0;
        activeEnemies.Clear();
        canSpawnEnemies = true;
        UpdateEnemyCountText();
    }
    // 更新敌人数量显示
    private void UpdateEnemyCountText(){
        enemyCountText.text = $"敌人数量: {activeEnemies.Count}/{totalEnemies}";
    }

    /// <summary>
    /// 移动所有活跃的敌人
    /// </summary>
    private void MoveAllEnemies()
    {
        foreach(GameObject enemy in activeEnemies)
        {
            if(enemy != null)
            {
                // 调用每个敌人的移动方法
                enemy.GetComponent<EnemyController>().MoveOneStep();
            }
        }
    }

    /// <summary>
    /// 生成敌人
    /// </summary>
    public void SpawnEnemy()
    {
        if(spawnedCount >= totalEnemies) {
            Debug.Log("已达到最大敌人数量");
        };
        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        activeEnemies.Add(enemy);
        spawnedCount++;
        
        // 设置敌人死亡回调
        enemy.GetComponent<EnemyController>().OnDeath += () => 
        {
            activeEnemies.Remove(enemy);
            // 敌人死亡后立即检查是否可以生成新敌人
            if(activeEnemies.Count == 0) 
            {
                canSpawnEnemies = true;
            }
            UpdateEnemyCountText();
        };
        UpdateEnemyCountText();
        canSpawnEnemies = false; // 生成后等待回合结束
    }


     

    // public void SpawnInitialEnemy()
    // {
    //     if(spawnPoints.Length == 0) return;
        
    //     Transform spawnPoint = spawnPoints[0];
    //     GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
    //     activeEnemies.Add(enemy);
    //     spawnedCount++;
        
    //     enemy.GetComponent<EnemyController>().OnDeath += () => 
    //     {
    //         activeEnemies.Remove(enemy);
    //         //Level3Manager.Instance.OnEnemyTurnComplete();
    //     };
    // }

    // 开始敌人回合
    public void StartEnemyTurn()
    {
        if(activeEnemies.Count > 0)
        {
            MoveAllEnemies();
            if(canSpawnEnemies){
                SpawnEnemy();
                canSpawnEnemies = false;
            }else{
                canSpawnEnemies = true;
            }
        }
        else if(spawnedCount < totalEnemies)
        {
            SpawnEnemy();
            canSpawnEnemies = false;
        }
        else
        {
            // 所有敌人都已经死亡，通知游戏结束
            Level3Manager.Instance.GameWin();
        }
        Level3Manager.Instance.OnEnemyTurnComplete();
    }
    // 敌人胜利
    public void EnemyWin(){
        Level3Manager.Instance.GameLose();
    }
}