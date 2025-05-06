using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;

public class CGController : MonoBehaviour
{
    [Header("CG视频资源映射")]
    [SerializeField] private List<CGVideoMapping> cgVideoMappings = new();


    [Header("视频播放器")] 
    [SerializeField] private VideoPlayer videoPlayer;
    
    [Header("淡入设置")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 0.5f;
    
    private int _currentCGId;
    private bool _isPlaying;

    [System.Serializable]
    public class CGVideoMapping
    {
        public int cgId;
        public VideoClip videoClip;
    }

    private void Awake()
    {
        GameEvents.OnCGEnter += HandleCGEnter;
        GameEvents.OnCGComplete += HandleCGComplete;
        GameEvents.OnChapterComplete += HandleChapterComplete;
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnCGEnter -= HandleCGEnter;
        GameEvents.OnCGComplete -= HandleCGComplete;
        GameEvents.OnChapterComplete -= HandleChapterComplete;
        
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    private void PlayCurrentCG()
    {
        if (_currentCGId <= 0) return;
        
        StartCoroutine(FadeAndPlayCG());
    }

    private IEnumerator FadeAndPlayCG()
    {
        // 淡入效果
        if(fadeCanvas != null)
        {
            fadeCanvas.alpha = 1;
            float elapsed = 0f;
            while(elapsed < fadeDuration)
            {
                fadeCanvas.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            fadeCanvas.alpha = 0;
        }

        // 播放CG
        VideoClip clip = GetVideoClipByCGId(_currentCGId);
        if (videoPlayer != null && clip != null)
        {
            videoPlayer.clip = clip;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();
            _isPlaying = true;
            Debug.Log($"开始播放CG: {_currentCGId}");
        }
        else
        {
            Debug.LogError($"CG资源未配置或播放器未设置: CG ID {_currentCGId}");
            HandleCGComplete(_currentCGId);
        }
    }
    private void HandleCGEnter(int cgId)
    {
        _currentCGId = cgId;
        _isPlaying = true;
        PlayCurrentCG();
    }

    private void HandleCGComplete(int cgId)
    {
        if (!_isPlaying) return;
        _currentCGId = cgId;
        _isPlaying = false;
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.clip = null; // 释放视频资源
            Resources.UnloadUnusedAssets(); // 释放未使用资源
        }
        
        // 标记CG为已观看
        GameEvents.TriggerCGComplete(_currentCGId);
    }

    private void HandleChapterComplete(int chapterId)
    {
        int cgId = chapterId + GameManager.CGConfig.ChapterCGOffset;
        _currentCGId = cgId;
        _isPlaying = true;
        PlayCurrentCG();
    }
    private void OnVideoFinished(VideoPlayer vp)
    {
        if(_currentCGId == 10003)
        {
            GameEvents.TriggerCGEnter(20001);
            return;
        }
        HandleCGComplete(_currentCGId);
    }

    private void Update()
    {
        if (_isPlaying && Input.anyKeyDown)
        {
            HandleCGComplete(_currentCGId);
        }
    }

    private VideoClip GetVideoClipByCGId(int cgId)
    {
        foreach (var mapping in cgVideoMappings)
        {
            if (mapping.cgId == cgId)
            {
                return mapping.videoClip;
            }
        }
        return null;
    }
}
