using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapDialog : MonoBehaviour
{
    [Header("对话设置")]
    [SerializeField] private int levelId;
    [SerializeField] private float triggerDistance = 1f;
    [SerializeField] private Image characterImage; // 人物头像
    [SerializeField] private Text dialogText; // 对话内容
    [SerializeField] private Image darkBackground; // 暗色背景

    private Transform _player;
    private bool _hasTriggered;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _hasTriggered = false;
        
        // 初始化隐藏对话元素
        characterImage.gameObject.SetActive(false);
        dialogText.gameObject.SetActive(false);
        darkBackground.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!_hasTriggered && Vector3.Distance(transform.position, _player.position) <= triggerDistance)
        {
            //StartCoroutine(ShowMapDialog());
            _hasTriggered = true;
        }
    }

    // private IEnumerator ShowMapDialog()
    // {
    //     // 显示暗色背景
    //     darkBackground.gameObject.SetActive(true);
        
    //     // 获取对话内容
        
    // }
}
