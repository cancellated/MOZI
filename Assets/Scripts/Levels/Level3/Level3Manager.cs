using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level3Manager : MonoBehaviour
{
    public static Level3Manager Instance { get; private set; }
    
    [Header("音频管理")] 
    [SerializeField] private AudioManager audioManager;

    [SerializeField] AudioClip mangonelAttack;
    [SerializeField] AudioClip enemyDie;
    [SerializeField] AudioClip winMusic;
    [SerializeField] AudioClip loseMusic;
    public GameObject winPanel;
    public GameObject losePanel;
    public int currrentRound = 0;//每回合玩家先动，然后敌人动，为一回合

    private bool isStopped = false;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 播放背景音乐
        if (audioManager != null)
        {
            audioManager.PlayBackgroundMusic();
        }
        // 游戏开始生成第一个敌人
        EnemyManager.Instance.SpawnEnemy();
    }

    // 玩家行动结束
    public void OnPlayerActionComplete()
    {
        if(isStopped){
            return;
        }
        // 通知敌人行动
        EnemyManager.Instance.StartEnemyTurn();
    }

    // 敌人行动结束
    public void OnEnemyTurnComplete()
    {
        if(isStopped){
            return;
        }
        currrentRound++;
        // 允许玩家再次行动
        mangonelControl.Instance.EnablePlayerAction();
    }
    //重置游戏，失败后重新开始
    public void ResetLevel(){
        currrentRound = 0;
        isStopped = false;
        EnemyManager.Instance.ResetLevel();
        mangonelControl.Instance.ResetLevel();
        EnemyManager.Instance.SpawnEnemy();
        if (audioManager!= null){
            audioManager.StopCompleteMusic();
            audioManager.PlayBackgroundMusic();
        }
    }
    //游戏通关
    public void GameOver(){
        GameEvents.TriggerLevelComplete(3);
    }
    public void GameWin(){
        // 游戏胜利逻辑
        Debug.Log("You win!");
        isStopped = true;
        // 切换为通关音乐
        if (audioManager != null)
        {
            audioManager.StopBackgroundMusic();
            PlayMusic(3);
        }
        winPanel.SetActive(true);
        //GameOver();
    }
    public void GameLose(){
        // 游戏失败逻辑
        Debug.Log("You lose!"); 

        losePanel.SetActive(true);
        isStopped = true;

    }
    public void PlayMusic(int index){
        if (audioManager!= null) {
            switch (index) {
                case 1:
                    audioManager.completeMusicSource.clip = mangonelAttack;
                    audioManager.PlayCompleteMusic();
                    break; 
                case 2:
                    audioManager.completeMusicSource.clip = enemyDie;
                    audioManager.PlayCompleteMusic();
                    break;
                case 3:
                    audioManager.StopBackgroundMusic();
                    audioManager.completeMusicSource.clip = winMusic;
                    audioManager.PlayCompleteMusic();
                    break;
                case 4:
                    audioManager.StopBackgroundMusic();
                    audioManager.completeMusicSource.clip = loseMusic;
                    audioManager.PlayCompleteMusic();
                    break;
                default:
                    Debug.Log("查无此乐");
                    break; // 或者抛出异常，根据需要处理无效的索引
            }
        }
    }
    public void StopMusic(){
        if (audioManager!= null) {
            audioManager.StopCompleteMusic();
        }
    }
}
