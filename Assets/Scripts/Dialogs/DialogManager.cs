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
/// 5. 背景音乐控制
/// </summary>
public class DialogManager : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("对话弹窗的CanvasGroup组件")]
    [SerializeField] private CanvasGroup dialogPopup;
    [Tooltip("显示对话内容的Text组件")]
    [SerializeField] private Text dialogText;
    [Tooltip("显示当前说话角色名的Text组件")]
    [SerializeField] private Text characterName;

    [Header("音频组件")]
    [Tooltip("BGM音频源")]
    [SerializeField] private AudioSource bgmSource;

    [Header("图像组件")]
    [Tooltip("角色立绘Image组件")]
    [SerializeField] private Image characterImage;
    [Tooltip("背景图Image组件")] 
    [SerializeField] private Image backgroundImage;

    private Queue<DialogData> _currentDialogs = new();
    private Coroutine _typingCoroutine;
    private bool _isSkipping;
    private bool _isTypingComplete;
    private bool _shouldSkipCurrentText;
    private AudioClip _currentBGM;

    void Awake()
    {
        GameEvents.OnStoryEnter += OnStoryEnter;
        
        int currentStory = GameManager.Instance.GetCurrentStory();
        if (currentStory > 0)
        {
            Debug.Log($"立即加载故事ID: {currentStory}");
            OnStoryEnter(currentStory);
        }
    }

    void OnDestroy()
    {
        GameEvents.OnStoryEnter -= OnStoryEnter;
        StopBGM();
    }

    private void OnStoryEnter(int storyId)
    {
        Debug.Log($"收到故事进入事件，storyId: {storyId}");
        var dialogs = DialogConfigManager.GetDialogsByStoryId(storyId);
        
        if(dialogs != null) 
        {
            Debug.Log($"成功加载对话配置，共{dialogs.Count}条对话");
            PlayBGM(dialogs[0].BGM);
            
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

    private void PlayBGM(string bgmName)
    {
        if (string.IsNullOrEmpty(bgmName) || bgmName == "None") 
        {
            // 如果配置为None则停止当前BGM
            if(bgmSource.isPlaying)
            {
                bgmSource.Stop();
                _currentBGM = null;
            }
            return;
        }

        var bgmClip = Resources.Load<AudioClip>($"Audios/对话/{bgmName}");
        if (bgmClip == null)
        {
            Debug.LogError($"无法加载BGM: {bgmName}");
            return;
        }

        if(bgmClip != _currentBGM || !bgmSource.isPlaying)
        {
            _currentBGM = bgmClip;
            bgmSource.clip = _currentBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    private void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
        _currentBGM = null;
    }

    private void HandleDialogStart(List<DialogData> dialogs)
    {
        Debug.Log("开始处理对话队列");
        
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
        
        _currentDialogs = new Queue<DialogData>(sortedDialogs.Values);
        StartCoroutine(PlayDialogs());
    }

    private IEnumerator PlayDialogs()
    {
        Debug.Log("[对话系统] 开始播放对话序列");
        dialogPopup.alpha = 1;
        
        while (_currentDialogs.Count > 0)
        {
            var data = _currentDialogs.Peek();
            PlayBGM(data.BGM);
            ShowText(data.Content, data.Character, data.Background); // 新增background参数
            
            yield return new WaitUntil(() => _isTypingComplete);
            
            bool inputDetected = false;
            while (!inputDetected)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    inputDetected = true;
                }
                yield return null;
            }
            
            _currentDialogs.Dequeue();
        }
        
        Debug.Log("[对话系统] 对话播放完成");
        dialogPopup.alpha = 0;
        StopBGM();
        
        if(_currentDialogs.Count == 0)
        {
            int currentStory = GameManager.Instance.GetCurrentStory();
            GameManager.Instance.CompeleteStory(currentStory);
            
            if (currentStory >= 2000)
            {
                GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevelSelect);
            }
            else
            {
                int levelId = currentStory % 1000;
                GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevel, levelId);
            }
        }
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
                dialogText.text = content;
                break;
            }
            
            dialogText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
    
        _isTypingComplete = true;
        _typingCoroutine = null;
    }

    public void SkipCurrentDialog()
    {
        if (_typingCoroutine != null && !_isSkipping)
        {
            _isSkipping = true;
            Debug.Log("主动调用跳过当前对话");
        }
    }

    private void ShowText(string content, string character, string background)
    {
        Debug.Log($"[对话系统] 显示对话 - 角色:{character} 背景:{background}");
        
        // 加载并显示背景图 - 添加资源存在性检查
        if(!string.IsNullOrEmpty(background))
        {
            string bgPath = $"Images/Dialog/Background/{background}";
            var bgSprite = Resources.Load<Sprite>(bgPath);
            
            if(bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
                backgroundImage.gameObject.SetActive(true);
                Debug.Log($"成功加载背景图: {bgPath}");
            }
            else
            {
                // 添加更详细的错误信息
                Debug.LogError($"背景图加载失败，请检查以下内容：\n" +
                             $"1. 文件路径: Assets/Resources/{bgPath}.png\n" +
                             $"2. 文件是否存在于项目中\n" +
                             $"3. 文件导入设置是否正确(Sprite类型)\n" +
                             $"4. 文件名大小写是否匹配");
                backgroundImage.gameObject.SetActive(false);
                
                // 测试Resources文件夹是否存在
                var testObj = Resources.Load<UnityEngine.Object>("Images/Dialog/Background");
                Debug.Log($"Resources文件夹测试: {testObj != null}");
            }
        }
        
        // 加载并显示角色立绘（旁白不显示）
        if(!string.IsNullOrEmpty(character))
        {
            if(character == "旁白")
            {
                characterImage.gameObject.SetActive(false);
                Debug.Log("旁白对话，不显示角色立绘");
            }
            else
            {
                string charPath = $"Images/Dialog/Character/{character}";
                var sprite = Resources.Load<Sprite>(charPath);
                if(sprite != null)
                {
                    characterImage.sprite = sprite;
                    characterImage.gameObject.SetActive(true);
                    Debug.Log($"成功加载角色立绘: {charPath}");
                }
                else
                {
                    Debug.LogWarning($"无法加载角色立绘，请检查资源是否存在: {charPath}");
                    characterImage.gameObject.SetActive(false);
                }
            }
            
        }
        
        if(dialogText == null) Debug.LogError("dialogText未赋值!");
        if(characterName == null) Debug.LogError("characterName未赋值!");
    
        characterName.text = character;
        
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
                _shouldSkipCurrentText = true;
            }
        }
    }
}