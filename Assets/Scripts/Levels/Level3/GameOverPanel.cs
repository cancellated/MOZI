using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    public void Win(){
        Level3Manager.Instance.GameOver();
        this.gameObject.SetActive(false); 
    }
    public void Lose(){
        Level3Manager.Instance.ResetLevel();//重置游戏
        this.gameObject.SetActive(false); 
    }
    
}
