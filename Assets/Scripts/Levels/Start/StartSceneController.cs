using UnityEngine;
using UnityEngine.Video;

public class StartSceneController : MonoBehaviour
{
    [Header("视频资源")]
    [SerializeField] private VideoClip introVideoClip;
    [SerializeField] private VideoPlayer videoPlayer;
    private bool _hasStarted = false;
    private bool _isTransitioning = false; // 新增标志防止重复切换

    void Start()
    {
        videoPlayer.clip = introVideoClip;
        videoPlayer.playOnAwake = false;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.errorReceived += OnVideoError; // 添加错误回调
    }

    void OnDestroy()
    {
        // 清理事件监听
        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.errorReceived -= OnVideoError;
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogWarning($"视频播放错误: {message}");
        if (!_isTransitioning)
        {
            OnVideoEnd(source);
        }
    }

    private void OnVideoEnd(VideoPlayer source)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        
        // 停止并清理视频播放器
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = null;
        }
        
        GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevelSelect);
    }

    void Update()
    {
        if (!_hasStarted && Input.anyKeyDown)
        {
            // 首次点击开始播放
            _hasStarted = true;
            videoPlayer.Play();
        }
        else if (_hasStarted && Input.anyKeyDown)
        {
            // 播放中点击跳过
            videoPlayer.Stop();
            OnVideoEnd(videoPlayer);
        }
    }
}