using UnityEngine;
using UnityEngine.UI;

public class LevelOverviewItem : MonoBehaviour
{
    [SerializeField] private Image scrollImage; // 卷轴图片
    [SerializeField] private Text levelNameText;
    [SerializeField] private Button button;
    
    private int levelId;
    private ShowStorybook storybook;
    
    public void Setup(int id, string name, ShowStorybook controller)
    {
        levelId = id;
        levelNameText.text = name;
        storybook = controller;
        
        button.onClick.AddListener(OnClick);
    }
    
    private void OnClick()
    {
        storybook.ShowDetailMenu(levelId);
    }
}