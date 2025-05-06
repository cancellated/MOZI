using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapDialog : MonoBehaviour
{
    #region 序列化字段
    [Header("对话设置")]
    [SerializeField] private int levelId;   // 关卡ID
    [SerializeField] private CanvasGroup canvasGroup;   //对话框的CanvasGroup
    [SerializeField] private Image characterImage; // 人物头像
    [SerializeField] private Text dialogText; // 对话内容
    [SerializeField] private Image darkBackground; // 暗色背景
    #endregion
    
    #region 私有变量
    private Coroutine _typingCoroutine;
    private bool _isTypingComplete;
    private bool _shouldSkipCurrentText;
    #endregion
    
    #region Unity生命周期
    void Start()
    {
        // 初始化隐藏对话元素
        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // 添加事件监听
        GameEvents.OnMapStoryEnter += HandleMapStoryEnter;
        GameEvents.OnMapStoryComplete += HandleMapStoryComplete;

        // 游戏最初对话
        if(levelId == 0 && !GameManager.Instance.IsStoryCompleted(3000)) 
        {
            Debug.Log("游戏开始对话");
            GameEvents.TriggerMapStoryEnter(3000); // 触发初始对话
        }
    }
    #endregion
    
    #region 事件处理
    private void HandleMapStoryEnter(int storyId)
    {
        levelId = storyId - GameManager.StoryConfig.MapSroryOffset;
        if(!GameManager.Instance.IsStoryCompleted(storyId))
        {
            StartCoroutine(ShowMapDialog());
        }
    }
    private void HandleMapStoryComplete(int storyId)
    {
        if(storyId == levelId + 3000)
        {
            if(canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
    #endregion
    
    #region 对话核心逻辑
    //显示地图对话
    private IEnumerator ShowMapDialog()
    {

        //显示对话界面
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // 从DialogConfigManager获取对话配置
        var dialogs = DialogConfigManager.GetDialogsByStoryId(levelId + 3000);
        if (dialogs != null && dialogs.Count > 0)
        {
            // 播放对话
            yield return StartCoroutine(PlayDialogs(dialogs));
        }
        else
        {
            Debug.LogError($"找不到地图对话配置，storyId: {levelId + 3000}");
        }


        // 触发地图故事完成事件
        GameEvents.TriggerMapStoryComplete(levelId + 3000);
    }

    //播放对话
    private IEnumerator PlayDialogs(List<DialogData> dialogs)
    {
        Debug.Log("[地图对话系统] 开始播放对话序列");
        if(canvasGroup != null) canvasGroup.alpha = 1;
        
        foreach (var dialog in dialogs)
        {
            // 显示对话内容
            ShowText(dialog.Content, dialog.Character);
            
            yield return new WaitUntil(() => _isTypingComplete);
            
            // 等待玩家输入继续
            bool inputDetected = false;
            while (!inputDetected)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    inputDetected = true;
                }
                yield return null;
            }
        }
        
        Debug.Log("[地图对话系统] 对话播放完成");
        if(canvasGroup != null) canvasGroup.alpha = 0;
    }

    private void ShowText(string content, string character)
    {
        // 处理角色立绘
        if(!string.IsNullOrEmpty(character)) 
        {
            string charPath = $"Images/Dialog/Character/{character}";
            var sprite = Resources.Load<Sprite>(charPath);
            
            if(sprite != null)
            {
                characterImage.sprite = sprite;
                characterImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"角色立绘加载失败：{charPath}");
                characterImage.gameObject.SetActive(false);
            }
        }
        else
        {
            characterImage.gameObject.SetActive(false);
        }
        
        // 设置对话文本
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        _typingCoroutine = StartCoroutine(TypeText(content));
    }

    private IEnumerator TypeText(string content)
    {
        dialogText.text = "";
        _isTypingComplete = false;
        _shouldSkipCurrentText = false;
        
        foreach (char c in content)
        {
            if (_shouldSkipCurrentText)
            {
                dialogText.text = content;
                break;
            }
            
            dialogText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
    
        _isTypingComplete = true;
        _typingCoroutine = null;
    }

    void Update()
    {
        if (_typingCoroutine != null && !_isTypingComplete)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                _shouldSkipCurrentText = true;
            }
        }
        #if UNITY_EDITOR
        // 编辑器模式下按F2跳过整个对话
        if (Input.GetKey(KeyCode.F2) && canvasGroup != null && canvasGroup.alpha > 0)
        {
            SkipAllDialogs();
        }
        #endif
    }

    #if UNITY_EDITOR
    private void SkipAllDialogs()
    {
        Debug.Log("[编辑器] 跳过所有地图对话");
        GameEvents.TriggerMapStoryComplete(GameManager.Instance.GetCurrentStory());
        GameEvents.TriggerMapLock(false);
    }
    #endif
    #endregion

    void OnDestroy()
    {
        // 移除事件监听
        GameEvents.OnMapStoryEnter -= HandleMapStoryEnter;
    }
}



