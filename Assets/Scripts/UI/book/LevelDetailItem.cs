using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 关卡详情项UI组件，管理单个关卡的物品展示
/// 包含三部分：左侧物品列表、中间描述区域、右侧图片区域
/// </summary>
public class LevelDetailItem : MonoBehaviour
{
    [Header("左侧物品列表")]
    [SerializeField] private Transform itemsContainer; // 物品按钮的父容器
    [SerializeField] private GameObject itemButtonPrefab; // 物品按钮预制体
    
    [Header("中间描述区域")] 
    [SerializeField] private Text descText; // 物品描述文本组件
    
    [Header("右侧图片区域")]
    [SerializeField] private Image itemImage; // 物品图片显示组件
    
    [Header("状态显示")]
    //[SerializeField] private GameObject lockedOverlay; // 未解锁状态遮罩
    [SerializeField] private GameObject completedCheckmark; // 完成状态标记
    
    private List<LevelConfigData.ItemInfo> currentItems; // 当前关卡的物品列表
    private int selectedItemIndex = 0; // 当前选中的物品索引

    /// <summary>
    /// 初始化关卡详情项
    /// </summary>
    /// <param name="config">关卡配置数据</param>
    /// <param name="isCompleted">是否已完成</param>
    /// <param name="isUnlocked">是否已解锁</param>
    public void Setup(LevelConfigData config, bool isCompleted, bool isUnlocked)
    {
        currentItems = config.items;
        selectedItemIndex = 0; // 默认选择第一个物品
        
        // 清空现有物品按钮
        ClearItemButtons();
        
        // 生成物品按钮列表
        CreateItemButtons();
        
        // 默认显示第一个物品的信息
        UpdateItemDisplay();
        
        // 设置关卡状态显示
        //UpdateStatusDisplay(isCompleted, isUnlocked);
    }

    /// <summary>
    /// 清空所有物品按钮
    /// </summary>
    private void ClearItemButtons()
    {
        foreach(Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 创建物品按钮列表
    /// </summary>
    private void CreateItemButtons()
    {
        for(int i = 0; i < currentItems.Count; i++)
        {
            int index = i; // 闭包捕获当前索引
            var buttonObj = Instantiate(itemButtonPrefab, itemsContainer);
            var button = buttonObj.GetComponent<Button>();
            var buttonText = buttonObj.GetComponentInChildren<Text>();
            
            buttonText.text = currentItems[i].name;
            button.onClick.AddListener(() => OnItemButtonClick(index));
        }
    }

    /// <summary>
    /// 物品按钮点击事件处理
    /// </summary>
    /// <param name="itemIndex">点击的物品索引</param>
    private void OnItemButtonClick(int itemIndex)
    {
        selectedItemIndex = itemIndex;
        UpdateItemDisplay();
    }

    /// <summary>
    /// 更新当前显示的物品信息
    /// </summary>
    private void UpdateItemDisplay()
    {
        if(currentItems == null || currentItems.Count == 0) return;
        
        var item = currentItems[selectedItemIndex];
        descText.text = item.description;
        string spritePath = "Images/Level_3/"+item.spritePath;
        // 动态加载并显示物品图片
        if(!string.IsNullOrEmpty(spritePath))
        {
            var sprite = Resources.Load<Sprite>(spritePath);
            itemImage.sprite = sprite;
        }
    }

    /// <summary>
    /// 更新关卡状态显示
    /// </summary>
    // private void UpdateStatusDisplay(bool isCompleted, bool isUnlocked)
    // {
    //     completedCheckmark.SetActive(isCompleted);
    //     lockedOverlay.SetActive(!isUnlocked);
    // }
}