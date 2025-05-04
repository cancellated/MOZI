//using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class LevelOverviewItem : MonoBehaviour
{
    [SerializeField] private Image scrollImage; // 卷轴图片
    [SerializeField] private Text levelNameText;
    [SerializeField] private Button button;

    private bool isButtonClicked = false;
    
    private int levelId;
    private ShowStorybook storybook;
    
    public void Setup(int id, ShowStorybook controller)
    {
        levelId = id;
        //levelNameText.text = "第" + levelId+ "关";
        SetImage(id);
        storybook = controller;
        
        button.onClick.AddListener(OnClick);
    }

    // void Start() {
    //     button.onClick.AddListener(OnClick);
    //     //storybook = GameObject.Find("Book").GetComponent<ShowStorybook>();
    //     if (storybook == null) {
    //         Debug.LogError("未一级菜单选项未找到ShowStorybook");
    //     }
    // }
    private void SetImage(int id) {
        string path;
        switch (id) {
            case 1: path = "一级菜单_第一关选项"; break;
            case 2: path = "一级菜单_第二关选项"; break;
            case 3: path = "一级菜单_第三关选项"; break; 
            default: path = "一级菜单_第一关选项"; break;
        }
        scrollImage.sprite = Resources.Load<Sprite>("Images/UI/" + path);
        if (scrollImage.sprite == null) {
            Debug.LogError("未找到图片：" + "Images/UI/" + path); 
        }
    }
    private void OnClick()
    {
        if (isButtonClicked) {
            return;
        }
        storybook.ShowDetailMenu(levelId);
        isButtonClicked = true;
    }
}