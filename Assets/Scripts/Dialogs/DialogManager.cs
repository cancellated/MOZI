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
    private bool _isTypingComplete;
    private bool _shouldSkipCurrentText;

    /// <summary>
    /// 单例初始化方法
    /// </summary>
    protected override void Initialize()
    {
        GameEvents.OnStoryEnter += OnStoryEnter;
    
        // 测试代码
        #if UNITY_EDITOR
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals("Dialog"))
        {
            Debug.Log("编辑器直接运行对话场景，使用测试storyId");
            OnStoryEnter(1001); // 使用你的测试storyId
        }
        #endif
    }

    private void OnStoryEnter(int storyId)
    {
        Debug.Log($"收到故事进入事件，storyId: {storyId}");
        var dialogs = DialogConfigManager.Instance.GetDialogsByStoryId(storyId);
        
        if(dialogs != null) 
        {
            Debug.Log($"成功加载对话配置，共{dialogs.Count}条对话");
            // 添加对话内容验证
            foreach(var dialog in dialogs)
            {
                if(string.IsNullOrEmpty(dialog.Character) || string.IsNullOrEmpty(dialog.Content))
                {
                    Debug.LogError($"发现空对话数据 - 角色: '{dialog.Character}', 内容: '{dialog.Content}'");
                }
            }
            HandleDialogStart(dialogs);
        }
        else
        {
            Debug.LogError($"无法加载storyId={storyId}的对话配置");
        }
    }

    private void HandleDialogStart(List<DialogData> dialogs)
    {
        Debug.Log("开始处理对话队列");
        
        // 将列表转为字典并按ID排序
        var sortedDialogs = new SortedDictionary<int, DialogData>();
        foreach(var dialog in dialogs)
        {
            if(int.TryParse(dialog.DialogID, out int id))
            {
                sortedDialogs.Add(id, dialog);
            }
            else
            {
                Debug.LogError($"无效的DialogID格式: {dialog.DialogID}");
            }
        }
        
        // 将排序后的对话存入队列
        _currentDialogs = new Queue<DialogData>(sortedDialogs.Values);
        StartCoroutine(PlayDialogs());
    }

    private IEnumerator PlayDialogs()
    {
        Debug.Log("[对话系统] 开始播放对话序列");
        dialogPopup.alpha = 1;
        
        while (_currentDialogs.Count > 0)
        {
            var data = _currentDialogs.Dequeue();
            ShowText(data.Content, data.Character);
            
            // 等待打字完成
            yield return new WaitUntil(() => _typingCoroutine == null);
            
            Debug.Log($"[对话系统] 等待用户输入 (剩余:{_currentDialogs.Count}条)");
            
            // 等待用户输入切换到下一句
            bool inputDetected = false;
            while (!inputDetected)
            {
                if (Input.GetMouseButtonDown(0) || 
                    Input.GetKeyDown(KeyCode.Space) ||
                    Input.GetKeyDown(KeyCode.Return))
                {
                    inputDetected = true;
                }
                yield return null;
            }
        }
        
        Debug.Log("[对话系统] 对话播放完成");
        dialogPopup.alpha = 0;
        int currentLevel = GameManager.Instance.GetCurrentLevel();
        GameEvents.TriggerLevelEnter(currentLevel);
    }

    private IEnumerator TypeText(string content, string character)
    {
        Debug.Log($"开始逐字显示文本 - 内容:{content}");
        characterName.text = character;
        dialogText.text = "";
        _isTypingComplete = false;
        _shouldSkipCurrentText = false;
        
        foreach (char c in content)
        {
            if (_shouldSkipCurrentText)
            {
                Debug.Log("立即完成当前对话显示");
                dialogText.text = content;
                break;
            }
            
            dialogText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
    
        _isTypingComplete = true;
        _typingCoroutine = null;
        Debug.Log("文本显示完成");
    }

    // 修改SkipCurrentDialog方法
    public void SkipCurrentDialog()
    {
        if (_typingCoroutine != null && !_isSkipping)
        {
            _isSkipping = true;
            Debug.Log("主动调用跳过当前对话");
        }
    }

    private void ShowText(string content, string character)
    {
        Debug.Log($"[对话系统] 显示对话 - 角色:{character}");
        
        if(dialogText == null) Debug.LogError("dialogText未赋值!");
        if(characterName == null) Debug.LogError("characterName未赋值!");
    
        characterName.text = character;
        
        // 启用打字机效果
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        _typingCoroutine = StartCoroutine(TypeText(content, character));
    }

    void Update()
    {
        if (_typingCoroutine != null && !_isTypingComplete)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log($"[输入检测] 检测到跳过输入");
                _shouldSkipCurrentText = true;
            }
        }
    }
}