using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : SingletonBase<VideoManager>
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;

    #region 单例初始化
    protected override void Initialize()
    {
        // 自动获取组件引用
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 配置视频播放器
        videoPlayer.playOnAwake = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
    }
    #endregion

    /// <summary>
    /// 播放开场视频
    /// </summary>
    /// <param name="videoClip">视频资源</param>
    /// <param name="onComplete">播放完成回调</param>
    public void PlayIntroVideo(VideoClip videoClip, System.Action onComplete)
    {
        videoPlayer.clip = videoClip;
        videoPlayer.loopPointReached += source => OnVideoEnd(onComplete);
        
        videoPlayer.Prepare();
        videoPlayer.Play();
        audioSource.Play();
    }

    private void OnVideoEnd(System.Action callback)
    {
        videoPlayer.Stop();
        audioSource.Stop();
        callback?.Invoke();
    }

    // 在VideoManager中添加首帧获取方法
    public IEnumerator GetFirstFrame(VideoClip clip, System.Action<Texture2D> callback)
    {
        VideoPlayer tempPlayer = gameObject.AddComponent<VideoPlayer>();
        tempPlayer.clip = clip;
        tempPlayer.sendFrameReadyEvents = true;
        
        Texture2D frameTexture = null;
        bool frameReady = false;
        
        tempPlayer.frameReady += (source, frameIdx) => 
        {
            if (frameIdx == 0 && !frameReady)
            {
                frameTexture = new Texture2D((int)tempPlayer.width, (int)tempPlayer.height);
                RenderTexture renderTexture = source.texture as RenderTexture;
                if (renderTexture != null)
                {
                    RenderTexture.active = renderTexture;
                    frameTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                    frameTexture.Apply();
                    RenderTexture.active = null;
                }
                frameReady = true;
                Destroy(tempPlayer);
            }
        };
        
        tempPlayer.Play();
        yield return new WaitUntil(() => frameReady);
        callback?.Invoke(frameTexture);
    }
}