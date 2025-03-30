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
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("动画设置")]
    [SerializeField] private float zoomDuration = 1.5f; 
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float initialZoom = 1.2f;
    [SerializeField] private Vector2 initialOffset = new(500, 0);

    private RectTransform _imageRect;
    private bool _isAnimating;
    private bool _hasStartedAnimation;
    private Vector2 _originalPosition;

void Start()
{
    _imageRect = fullscreenImage.GetComponent<RectTransform>();
    _originalPosition = _imageRect.anchoredPosition;
    
    // 初始化视频播放器
    videoPlayer.clip = introVideoClip;
    videoPlayer.playOnAwake = false;
    videoPlayer.sendFrameReadyEvents = true;
    videoPlayer.errorReceived += OnVideoError; // 添加错误回调
    
    // 设置视频首帧回调
    videoPlayer.frameReady += OnFirstFrameReady;
    videoPlayer.Prepare();
}

// 添加视频错误处理方法
private void OnVideoError(VideoPlayer source, string message)
{
    Debug.LogWarning($"视频播放错误: {message}");
    // 强制继续播放
    if(videoPlayer.isPrepared && !videoPlayer.isPlaying)
    {
        videoPlayer.Play();
    }
}

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (!_hasStartedAnimation) // 首次点击触发动画
            {
                _hasStartedAnimation = true;
                StartCoroutine(PlayZoomAnimation());
            }
            else if (_isAnimating) // 动画过程中点击跳过
            {
                StopAllCoroutines();
                CompleteAnimationImmediately();
            }
        }
    }
    private void CompleteAnimationImmediately()
    {
        _imageRect.localScale = Vector3.one;
        _imageRect.anchoredPosition = _originalPosition;
        _isAnimating = false;
        
        GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevelSelect);
    }

        private void OnFirstFrameReady(VideoPlayer source, long frameIdx)
    {
        // 获取视频首帧纹理
        RenderTexture renderTexture = source.texture as RenderTexture;
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        
        // 设置为开场图片
        fullscreenImage.sprite = Sprite.Create(texture, 
            new Rect(0, 0, texture.width, texture.height), 
            Vector2.one * 0.5f);
            
        // 初始化图片位置和缩放
        _imageRect.anchoredPosition = _originalPosition + initialOffset;
        _imageRect.localScale = Vector3.one * initialZoom;
        fullscreenImage.gameObject.SetActive(true);
        
        if(!source.isPlaying)
        {
            source.Play();
        }
    
        // 取消回调并停止视频
        videoPlayer.frameReady -= OnFirstFrameReady;
        videoPlayer.Stop();
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
            
            _imageRect.localScale = Vector3.Lerp(startScale, Vector3.one, progress);
            _imageRect.anchoredPosition = Vector2.Lerp(startPos, _originalPosition, progress);
            
            yield return null;
        }

        GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevelSelect);
        yield break;
    }
}