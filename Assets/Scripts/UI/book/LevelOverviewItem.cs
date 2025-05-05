//using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class LevelOverviewItem : MonoBehaviour
{
    [SerializeField] private Image scrollImage; // 卷轴图片
    [SerializeField] private Text levelNameText;
    [SerializeField] private Button button;

    private bool isButtonClicked = false;

    private bool isCompleted = false;
    
    private int levelId;

    private Animator animator;
    private ShowStorybook storybook;
    
    public void Setup(int id,bool isComplete, ShowStorybook controller)
    {
        levelId = id;
        //levelNameText.text = "第" + levelId+ "关";
        SetImage(id);
        isCompleted = isComplete;
        storybook = controller;
        if (!isCompleted) {
            this.GetComponent<Image>().color = new Color(1,1,1,0.5f);
            //return;
        }
        button.onClick.AddListener(OnClick);
    }

    void Start() {
        // button.onClick.AddListener(OnClick);
        // //storybook = GameObject.Find("Book").GetComponent<ShowStorybook>();
        // if (storybook == null) {
        //     Debug.LogError("未一级菜单选项未找到ShowStorybook");
        // }
        animator = GetComponent<Animator>();
    }
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
        animator.SetInteger("roll_ID", levelId);
        isButtonClicked = true;
    }
    public void ShowDetailMenu() {
        storybook.ShowDetailMenu(levelId);
    }
}