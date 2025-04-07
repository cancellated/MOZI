using UnityEngine;
using UnityEngine.Video;

public class StartSceneController : MonoBehaviour
{
    [Header("视频资源")]
    [SerializeField] private VideoClip introVideoClip;
    [SerializeField] private VideoPlayer videoPlayer;
    private bool _hasStarted = false;
    private bool _isTransitioning = false;

    void Start()
    {
        // 设置音频缓冲区大小
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.SetDirectAudioVolume(0, 1f);
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.EnableAudioTrack(0, true);
        
        videoPlayer.clip = introVideoClip;
        videoPlayer.playOnAwake = false;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.errorReceived += OnVideoError;
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