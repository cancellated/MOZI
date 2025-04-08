using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    public enum StoryType
    {
        PreLevel,  // 关卡前故事
        PostLevel  // 关卡后故事
    }
    [SerializeField] private Button button;
    public int targetLevelId;
    
    private LevelSelectionController _controller;

    private void Start()
    {
        _controller = FindFirstObjectByType<LevelSelectionController>();
        button.onClick.AddListener(OnClick);
    }

    public void UpdateVisualState(bool isUnlocked)
    {
        gameObject.SetActive(isUnlocked);
    }

    private void OnClick()
    {
        if (_controller != null)
        {
            _controller.OnLevelButtonClicked(targetLevelId);
        }
        else
        {
            Debug.LogError("找不到LevelSelectionController实例");
        }
    }

    public int GetStoryId(StoryType storyType)
    {
        // 生成唯一故事ID: 前故事=1000+levelId, 后故事=2000+levelId
        return storyType == StoryType.PreLevel ? 
            1000 + targetLevelId : 
            2000 + targetLevelId;
    }
}