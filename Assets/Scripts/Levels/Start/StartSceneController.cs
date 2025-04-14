using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class StartSceneController : MonoBehaviour
{
    [Header("视频资源")]
    [SerializeField] private VideoClip introVideoClip;
    [SerializeField] private VideoPlayer videoPlayer;
    private bool _hasStarted = false;
    private bool _isTransitioning = false;

    void Start()
    {
        if (videoPlayer != null && introVideoClip != null)
        {
            videoPlayer.clip = introVideoClip;
            videoPlayer.loopPointReached += OnVideoFinished; // 添加播放完成监听
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("视频播放器或视频剪辑未设置");
        }
    }

    void Update()
    {
        if (!_hasStarted && Input.anyKeyDown)
        {
            _hasStarted = true;
            StartCoroutine(StartGameSequence());
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (!_isTransitioning)
        {
            StartCoroutine(StartGameSequence());
        }
    }

        private IEnumerator StartGameSequence()
    {
        if (_isTransitioning) yield break;
        
        _isTransitioning = true;
        videoPlayer.Stop();
        // 加载选关场景
        GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevelSelect);
    }
}