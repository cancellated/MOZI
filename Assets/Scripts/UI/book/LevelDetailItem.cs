using UnityEngine;
using UnityEngine.UI;

public class LevelDetailItem : MonoBehaviour
{
    [SerializeField] private Text titleText;//物品名字
    [SerializeField] private Text descText;//描述文字
    [SerializeField] private Image iconImage;//右侧图片显示
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject completedCheckmark;
    [SerializeField] private Transform itemsContainer; // 新增物品容器
    [SerializeField] private GameObject itemPrefab; // 物品预制体

    public void Setup(LevelConfigData config, bool isCompleted, bool isUnlocked)
    {
        titleText.text = $"关卡 {config.levelId}";
        descText.text = $"包含 {config.items.Count} 个物品";
        
        // 清空现有物品
        foreach(Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 生成物品列表
        foreach(var item in config.items)
        {
            var itemObj = Instantiate(itemPrefab, itemsContainer);
            var itemText = itemObj.GetComponentInChildren<Text>();
            itemText.text = $"{item.name}: {item.description}";
        }
        
        completedCheckmark.SetActive(isCompleted);
        lockedOverlay.SetActive(!isUnlocked);
        
        Color textColor = isUnlocked ? Color.white : Color.gray;
        titleText.color = textColor;
        descText.color = textColor;
        iconImage.color = textColor;
    }
}