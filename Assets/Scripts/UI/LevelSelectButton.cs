using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    public enum StoryType
    {
        PreLevel,  // 关卡前故事
        PostLevel  // 关卡后故事
    }

    [SerializeField] public int targetLevelId;
    [SerializeField] private Button button;
    
    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    public void UpdateVisualState(bool isUnlocked)
    {
        gameObject.SetActive(isUnlocked);

    }

    private void OnClick()
    {
        LevelSelectionController.Instance.OnLevelButtonClicked(targetLevelId);
    }

    public int GetStoryId(StoryType storyType)
    {
        // 生成唯一故事ID: 前故事=1000+levelId, 后故事=2000+levelId
        return storyType == StoryType.PreLevel ? 
            1000 + targetLevelId : 
            2000 + targetLevelId;
    }
}