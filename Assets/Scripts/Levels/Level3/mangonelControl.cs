using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mangonelControl : MonoBehaviour
{
    [Header("投石机设置")]
    [SerializeField] private Transform launchPoint; // 发射点
    [SerializeField] private GameObject projectilePrefab; // 投射物预制体
    [SerializeField] private float minForce = 10f; // 最小发射力
    [SerializeField] private float maxForce = 30f; // 最大发射力
    [SerializeField] private float chargeTime = 2f; // 最大蓄力时间
    
    private float chargeStartTime;
    private bool isCharging;

    private EnemyManager enemyManager;

    [Header("发射限制")]
    [SerializeField] private bool canShoot = true;

    private GameObject projectile;
    public static mangonelControl Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
    }

    public void ResetLevel(){
        EnablePlayerAction();
    }


    private void Update()
    {
        if (!canShoot) return; // 如果不能射击，直接返回
        GetFire();
    }

    private void GetFire(){
        // 投石机控制逻辑
        if(Input.GetMouseButtonDown(0))
        {
            StartCharging();
        }
        
        if(Input.GetMouseButtonUp(0) && isCharging)
        {
            ReleaseProjectile();
        }
    }

    //开始蓄力
    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        Debug.Log("开始蓄力...");
    }

    
    //发射
    private void ReleaseProjectile()
    {
        isCharging = false;
        
        // 计算蓄力比例(0-1)
        float chargeRatio = Mathf.Clamp01((Time.time - chargeStartTime) / chargeTime);
        float launchForce = Mathf.Lerp(minForce, maxForce, chargeRatio);
        
        // 创建投射物
        projectile = Instantiate(projectilePrefab, launchPoint.position, launchPoint.rotation);
        
        // 施加力
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if(rb != null)
        {
            rb.AddForce(launchPoint.up * launchForce, ForceMode2D.Impulse);
        }
        
        Debug.Log($"发射! 蓄力比例: {chargeRatio}, 发射力: {launchForce}");
        //enemyManager.NotifyEnemyMove();
        canShoot = false; // 禁用玩家的下一次射击
        
    }

    public void EnablePlayerAction()
    {
        canShoot = true;
        if(projectile != null){
            Destroy(projectile);
        }
        projectile = null;
    }
}
