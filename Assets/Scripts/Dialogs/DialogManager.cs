using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 对话系统核心控制器，负责：
/// 1. 管理对话序列的播放流程
/// 2. 实现逐字显示的文字效果
/// 3. 处理用户输入的中断逻辑
/// </summary>
public class DialogManager : SingletonBase<DialogManager>
{
    [Header("UI Components")]
    [Tooltip("对话弹窗的CanvasGroup组件，用于控制整体显隐")]
    [SerializeField] private CanvasGroup dialogPopup;
    [Tooltip("显示对话内容的Text组件")]
    [SerializeField] private Text dialogText;
    [Tooltip("显示当前说话角色名的Text组件")]
    [SerializeField] private Text characterName;

    private Queue<DialogData> _currentDialogs = new();
    private Coroutine _typingCoroutine;
    private bool _isSkipping;

    /// <summary>
    /// 初始化对话系统，隐藏对话界面
    /// </summary>
    protected override void Initialize()
    {
        dialogPopup.alpha = 0;
    }

    /// <summary>
    /// 开始播放对话序列（外部调用入口）
    /// </summary>
    /// <param name="dialogs">对话数据列表，包含角色名和对话内容</param>
    public void StartDialog(List<DialogData> dialogs)
    {
        _currentDialogs = new Queue<DialogData>(dialogs);
        StartCoroutine(PlayDialogs());
    }

    /// <summary>
    /// 对话播放主协程，控制整体流程：
    /// 1. 显示对话框
    /// 2. 逐个播放对话
    /// 3. 完成后隐藏对话框
    /// </summary>
    private IEnumerator PlayDialogs()
    {
        dialogPopup.alpha = 1;
        
        while (_currentDialogs.Count > 0)
        {
            var data = _currentDialogs.Dequeue();
            ShowText(data.Content, data.Character);
            
            // 修改输入检测：空格/回车/鼠标左键都可以触发
            yield return new WaitUntil(() => 
                Input.GetKeyDown(KeyCode.Space) || 
                Input.GetKeyDown(KeyCode.Return) || 
                Input.GetMouseButtonDown(0));
        }
        
        dialogPopup.alpha = 0;
    }

    /// <summary>
    /// 外部调用接口：立即跳过当前正在播放的对话
    /// </summary>
    public void SkipCurrentDialog()
    {
        _isSkipping = true;
        
        // 添加额外触发方式：当通过其他方式（如UI按钮）调用时，也响应按键
        if(Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return) || Input.GetMouseButton(0))
        {
            _isSkipping = true;
        }
    }

    private IEnumerator TypeText(string content, string character)
    {
        characterName.text = character;
        dialogText.text = "";
        _isSkipping = false;

        foreach (char c in content)
        {
            if (_isSkipping) 
            {
                // 立即显示完整文本并退出协程
                dialogText.text = content;
                break;
            }
            
            dialogText.text += c;
            yield return new WaitForSeconds(0.05f);
        }

        // 动画完成后清空执行中的协程引用
        _typingCoroutine = null;
    }

    private void ShowText(string content, string character)
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypeText(content, character));
    }
}