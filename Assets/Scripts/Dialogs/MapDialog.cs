using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MapDialog : MonoBehaviour
{
    #region 序列化字段
    [Header("对话设置")]
    [SerializeField] private int levelId;
    [SerializeField] private float triggerDistance = 1f;
    [SerializeField] private CanvasGroup canvasGroup;   //对话框的CanvasGroup
    [SerializeField] private Image characterImage; // 人物头像
    [SerializeField] private Text dialogText; // 对话内容
    [SerializeField] private Image darkBackground; // 暗色背景
    
    [Header("对话配置")]
    [SerializeField] private TextAsset dialogConfig;
    #endregion
    
    #region 私有变量
    private Transform _player;
    private bool _hasTriggered;
    private List<MapDialogData> _dialogs = new();
    private Coroutine _typingCoroutine;
    private bool _isTypingComplete;
    #endregion
    
    #region Unity生命周期
    void Start()
    {
        //_player = GameObject.FindGameObjectWithTag("Player").transform;
        _hasTriggered = false;
        
        // 初始化隐藏对话元素
        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // 添加事件监听
        GameEvents.OnMapDialogEnter += HandleMapDialogEnter;
    }
    #endregion
    
    #region 事件处理
    private void HandleMapDialogEnter(int dialogId)
    {
        if(!_hasTriggered && dialogId == levelId + 3000)
        {
            StartCoroutine(ShowMapDialog());
            _hasTriggered = true;
        }
    }
    #endregion
    
    #region 对话核心逻辑
    void Update()
    {
/*        if (!_hasTriggered && Vector2.Distance(transform.position, _player.position) <= triggerDistance)
        {
            StartCoroutine(ShowMapDialog());
            _hasTriggered = true;
        }*/
    }
    //显示地图对话
    private IEnumerator ShowMapDialog()
    {

        //显示对话界面
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1; // 设置透明度为 1
            canvasGroup.interactable = true; // 允许交互
            canvasGroup.blocksRaycasts = true; // 阻挡射线
        }

        // 解析对话配置
        ParseDialogConfig();
        
        // 播放对话
        yield return StartCoroutine(PlayDialogs());
        
        // 对话完成后隐藏对话元素
        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    //解析对话配置
    private void ParseDialogConfig()
    {
        if (dialogConfig == null) return;
        
        var lines = dialogConfig.text.Split('\n');
        if (lines.Length < 3) return;

        // 处理表头
        var headerLine = lines[1].TrimEnd('\r', '\n');
        var headers = headerLine.Split(',');
        var headerMap = new Dictionary<string, int>();
        
        for (int i = 0; i < headers.Length; i++)
        {
            string header = headers[i].Trim();
            headerMap[header] = i;
        }

        // 处理数据行
        for (int i = 2; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            var fields = lines[i].Split(',');
            _dialogs.Add(new MapDialogData(fields, headerMap));
        }
    }

    //播放对话
    private IEnumerator PlayDialogs()
    {
        foreach (var dialog in _dialogs)
        {
            // 处理角色头像显示
            if (!string.IsNullOrEmpty(dialog.Character))
            {
                characterImage.sprite = Resources.Load<Sprite>($"Images/Dialog/Character/{dialog.Character}");
                characterImage.gameObject.SetActive(true);
            }
            else
            {
                characterImage.gameObject.SetActive(false);
            }
            
            // 显示对话内容
            dialogText.text = "";
            _isTypingComplete = false;
            
            _typingCoroutine = StartCoroutine(TypeText(dialog.Content, dialog.Character));
            
            yield return new WaitUntil(() => _isTypingComplete);
            
            // 等待玩家点击继续
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }
    }

    private IEnumerator TypeText(string content, string character)
    {
        foreach (char c in content)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
        _isTypingComplete = true;
    }
    #endregion

        void OnDestroy()
    {
        // 移除事件监听
        GameEvents.OnMapDialogEnter -= HandleMapDialogEnter;
    }
}
