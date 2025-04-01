using UnityEngine;
using UnityEngine.Video;

public class StartSceneController : MonoBehaviour
{
    [Header("视频资源")]
    [SerializeField] private VideoClip introVideoClip;
    [SerializeField] private VideoPlayer videoPlayer;
    private bool _hasStarted = false; // 新增开始播放标志

    void Start()
    {
        // 初始化视频播放器但不自动播放
        videoPlayer.clip = introVideoClip;
        videoPlayer.playOnAwake = false;
        videoPlayer.loopPointReached += OnVideoEnd;
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

    private void OnVideoEnd(VideoPlayer source)
    {
        GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevelSelect);
    }
}