using UnityEngine;
using UnityEngine.UI;

public class LevelDetailItem : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text descText;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject completedCheckmark;

    public void Setup(string title, string description, Sprite icon, bool isCompleted, bool isUnlocked)
    {
        titleText.text = title;
        descText.text = description;
        iconImage.sprite = icon;
        
        completedCheckmark.SetActive(isCompleted);
        lockedOverlay.SetActive(!isUnlocked);
        
        // 根据解锁状态调整显示
        if (!isUnlocked)
        {
            titleText.color = Color.gray;
            descText.color = Color.gray;
            iconImage.color = Color.gray;
        }
        else
        {
            titleText.color = Color.white;
            descText.color = Color.white;
            iconImage.color = Color.white;
        }
    }
}