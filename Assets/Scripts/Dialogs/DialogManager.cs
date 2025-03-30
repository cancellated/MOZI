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

    private void HandleDialogEnd()
    {
        
    }

    /// <summary>
    /// 对话播放主协程
    /// </summary>
    private IEnumerator PlayDialogs()
    {
        dialogPopup.alpha = 255;
        
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

    }

    /// <summary>
    /// 立即跳过当前对话
    /// </summary>

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