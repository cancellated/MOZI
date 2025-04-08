using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("背景音乐")]
    public AudioClip backgroundMusicClip; // 添加OGG音频剪辑
    public AudioClip completeMusicClip;   // 添加OGG音频剪辑
    private AudioSource backgroundMusicSource;
    private AudioSource completeMusicSource;

    private void Awake()
    {
        // 创建AudioSource组件
        backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        completeMusicSource = gameObject.AddComponent<AudioSource>();
        
        // 设置音频剪辑
        backgroundMusicSource.clip = backgroundMusicClip;
        completeMusicSource.clip = completeMusicClip;
        
        // 配置音频源参数
        backgroundMusicSource.loop = true;
        completeMusicSource.loop = false;
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicClip != null)
        {
            backgroundMusicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null&& backgroundMusicClip != null)
        {
            backgroundMusicSource.Stop();
        }
    }

    public void PlayCompleteMusic()
    {
        if (completeMusicSource != null && !completeMusicSource.isPlaying&& completeMusicClip != null)
        {
            completeMusicSource.Play();
        }
    }

    public void StopCompleteMusic()
    {
        if (completeMusicSource != null&& completeMusicClip != null)
        {
            completeMusicSource.Stop();
        }
    }
}