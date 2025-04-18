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
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnCGEnter -= HandleCGEnter;
        GameEvents.OnCGComplete -= HandleCGComplete;
        
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

        private void PlayCurrentCG()
    {
        if (_currentCGId <= 0) return;
        
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
        }
        
        // 标记CG为已观看
        GameEvents.TriggerCGComplete(_currentCGId);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
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
