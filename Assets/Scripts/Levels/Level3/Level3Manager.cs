using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level3Manager : MonoBehaviour
{
    public static Level3Manager Instance { get; private set; }
    
    public int currrentRound = 0;//每回合玩家先动，然后敌人动，为一回合
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 游戏开始生成第一个敌人
        EnemyManager.Instance.SpawnEnemy();
    }

    // 玩家行动结束
    public void OnPlayerActionComplete()
    {
        // 通知敌人行动
        EnemyManager.Instance.StartEnemyTurn();
    }

    // 敌人行动结束
    public void OnEnemyTurnComplete()
    {
        currrentRound++;
        // 允许玩家再次行动
        mangonelControl.Instance.EnablePlayerAction();
    }
    public void ResetLevel(){
        currrentRound = 0;
        EnemyManager.Instance.ResetLevel();
        mangonelControl.Instance.ResetLevel();
    }
    public void GameOver(){
        GameEvents.TriggerLevelComplete(3);
    }
    public void GameWin(){
        // 游戏胜利逻辑
        Debug.Log("You win!");
        GameOver();
    }
    public void GameLose(){
        // 游戏失败逻辑
        Debug.Log("You lose!"); 
    }
}
