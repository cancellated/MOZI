using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartSceneController : MonoBehaviour
{
    [Header("开场图片")]
    [SerializeField] private Image fullscreenImage;
    [Header("视频资源")]
    [SerializeField] private VideoClip introVideoClip;

    [Header("动画设置")]
    [SerializeField] private float zoomDuration = 1.5f; 
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float initialZoom = 1.2f;
    [SerializeField] private Vector2 initialOffset = new(500, 0); // 新增平移参数（单位：像素）

    private RectTransform _imageRect;
    private bool _isAnimating;
    private Vector2 _originalPosition; // 新增位置记录

    void Start()
    {
        _imageRect = fullscreenImage.GetComponent<RectTransform>();
        _originalPosition = _imageRect.anchoredPosition; // 记录原始中心位置
        
        // 初始化偏移状态
        _imageRect.anchoredPosition = _originalPosition + initialOffset;
        _imageRect.localScale = Vector3.one * initialZoom;
        fullscreenImage.gameObject.SetActive(true);
    }

    void Update()
    {
        if (Input.anyKeyDown && !_isAnimating)
        {
            StartCoroutine(PlayZoomAnimation());
        }
    }

    private System.Collections.IEnumerator PlayZoomAnimation()
    {
        _isAnimating = true;
        float timer = 0f;
        Vector3 startScale = _imageRect.localScale;
        Vector2 startPos = _imageRect.anchoredPosition;
        
        while (timer < zoomDuration)
        {
            timer += Time.deltaTime;
            float progress = zoomCurve.Evaluate(timer / zoomDuration);
            
            // 同时插值位置和缩放
            _imageRect.localScale = Vector3.Lerp(startScale, Vector3.one, progress);
            _imageRect.anchoredPosition = Vector2.Lerp(startPos, _originalPosition, progress);
            
            yield return null;
        }

        // 动画完成后处理视频播放
        if (GameManager.Instance.IsFirstLaunch() && introVideoClip != null)
        {
            VideoManager.Instance.PlayIntroVideo(introVideoClip, () => 
            {
                GameManager.Instance.CompleteFirstLaunch();
                SceneManager.LoadScene(GameManager.Instance.LevelSelectScene);
            });
        }
        else
        {
            SceneManager.LoadScene(GameManager.Instance.LevelSelectScene);
        }
        
        _isAnimating = false;
    }
}