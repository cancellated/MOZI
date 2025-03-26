using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 对话系统核心控制器，功能包括：
/// 1. 管理对话序列的播放流程
/// 2. 实现逐字显示的文字效果
/// 3. 处理用户输入的中断逻辑
/// 4. 与全局事件系统集成
/// </summary>
public class DialogManager : SingletonBase<DialogManager>
{
    [Header("UI Components")]
    [Tooltip("对话弹窗的CanvasGroup组件")]
    [SerializeField] private CanvasGroup dialogPopup;
    [Tooltip("显示对话内容的Text组件")]
    [SerializeField] private Text dialogText;
    [Tooltip("显示当前说话角色名的Text组件")]
    [SerializeField] private Text characterName;

    private Queue<DialogData> _currentDialogs = new();
    private Coroutine _typingCoroutine;
    private bool _isSkipping;

    /// <summary>
    /// 单例初始化方法
    /// </summary>
    protected override void Initialize()
    {
        dialogPopup.alpha = 0;
        GameEvents.OnDialogStart.AddListener(HandleDialogStart);
        GameEvents.OnDialogEnd.AddListener(HandleDialogEnd);
    }

    /// <summary>
    /// 事件处理：开始对话
    /// </summary>
    private void HandleDialogStart(List<DialogData> dialogs)
    {
        _currentDialogs = new Queue<DialogData>(dialogs);
        StartCoroutine(PlayDialogs());
    }

    /// <summary>
    /// 事件处理：结束对话
    /// </summary>
    /// <param name="context">场景上下文类型</param>
    /// <remarks>
    /// 根据不同的场景上下文执行对应的场景跳转逻辑
    /// </remarks>
    private void HandleDialogEnd(GameEvents.SceneContextType context)
    {
        string targetScene = context switch
        {
            GameEvents.SceneContextType.LevelSelection => GameManager.Instance.LevelSelectScene,
            GameEvents.SceneContextType.StoryTrigger => SceneLoader.GetCurrentSceneName(),
            _ => GameManager.Instance.StartScene
        };

        if (context != GameEvents.SceneContextType.StoryTrigger)
        {
            SceneLoader.Instance.LoadSceneDirect(targetScene);
        }
    }

    /// <summary>
    /// 对话播放主协程
    /// </summary>
    /// <remarks>
    /// 执行流程：
    /// 1. 显示对话框
    /// 2. 逐个播放对话条目
    /// 3. 监听用户输入推进流程
    /// 4. 完成后隐藏对话框
    /// </remarks>
    private IEnumerator PlayDialogs()
    {
        dialogPopup.alpha = 1;
        
        while (_currentDialogs.Count > 0)
        {
            var data = _currentDialogs.Dequeue();
            ShowText(data.Content, data.Character);
            
            yield return new WaitUntil(() => 
                Input.GetKeyDown(KeyCode.Space) || 
                Input.GetKeyDown(KeyCode.Return) || 
                Input.GetMouseButtonDown(0));
        }
        
        dialogPopup.alpha = 0;
        GameEvents.OnDialogEnd.Invoke(GameEvents.SceneContextType.StoryTrigger);
    }

    /// <summary>
    /// 立即跳过当前对话
    /// </summary>
    /// <remarks>
    /// 支持多种触发方式：
    /// 1. 通过UI按钮调用
    /// 2. 通过键盘快捷键（空格/回车）
    /// 3. 通过鼠标左键点击
    /// </remarks>
    public void SkipCurrentDialog()
    {
        _isSkipping = true;
        
        if(Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return) || Input.GetMouseButton(0))
        {
            _isSkipping = true;
        }
    }

    /// <summary>
    /// 文本逐字显示协程
    /// </summary>
    private IEnumerator TypeText(string content, string character)
    {
        characterName.text = character;
        dialogText.text = "";
        _isSkipping = false;

        foreach (char c in content)
        {
            if (_isSkipping) 
            {
                dialogText.text = content;
                break;
            }
            
            dialogText.text += c;
            yield return new WaitForSeconds(0.05f);
        }

        _typingCoroutine = null;
    }

    /// <summary>
    /// 启动文本显示流程
    /// </summary>
    private void ShowText(string content, string character)
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypeText(content, character));
    }
}