using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingScreen : SingletonBase<LoadingScreen>
{
    [Header("UI Components")]
    [SerializeField] private CanvasGroup loadingCanvas;
    [SerializeField] private Text loadingText;
    [SerializeField] private Image gifImage;
    [SerializeField] private Sprite[] gifFrames;
    [SerializeField] private float frameInterval = 0.1f;
    [SerializeField] private float moveDistance = 1150f; // 平移总距离

    [Header("动画设置")]
    [SerializeField] private float minDisplayTime = 2f; // 最小显示时长(秒)
    private float _loadStartTime;

    private AsyncOperation _asyncOperation;
    private Coroutine _animationCoroutine;
    private Vector2 _originalPosition; // 记录初始位置

    [Header("淡入淡出设置")]
    [SerializeField] private float fadeDuration = 0.5f; // 淡入淡出时长
    private Coroutine _fadeCoroutine;

    protected override void Initialize()
    {
        Hide();
        
        if (gifImage != null)
        {
            gifImage.gameObject.SetActive(false);
            _originalPosition = gifImage.rectTransform.anchoredPosition;
        }
    }

    public void Show()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        loadingCanvas.alpha = 0;
        loadingCanvas.interactable = true;
        loadingCanvas.blocksRaycasts = true;
        
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            loadingCanvas.alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        loadingCanvas.alpha = 1;
        
        // 开始播放GIF动画
        if (gifImage != null && gifFrames != null && gifFrames.Length > 0)
        {
            gifImage.gameObject.SetActive(true);
            _animationCoroutine = StartCoroutine(PlayGifAnimation());
        }
    }

    public void Hide()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            loadingCanvas.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        loadingCanvas.alpha = 0;
        loadingCanvas.interactable = false;
        loadingCanvas.blocksRaycasts = false;
        
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
        
        if (gifImage != null)
        {
            gifImage.gameObject.SetActive(false);
            gifImage.rectTransform.anchoredPosition = _originalPosition;
        }
    }

    private IEnumerator PlayGifAnimation()
    {
        int currentFrame = 0;
        while (true)
        {
            gifImage.sprite = gifFrames[currentFrame];
            currentFrame = (currentFrame + 1) % gifFrames.Length;
            yield return new WaitForSeconds(frameInterval);
        }
    }

    public IEnumerator LoadSceneAsync(string sceneName)
    {
        if (_asyncOperation != null) yield break;
        
        _loadStartTime = Time.time;
        Show();
        
        _asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        _asyncOperation.allowSceneActivation = false;

        while (!_asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(_asyncOperation.progress / 0.9f);
            loadingText.text = $"正在收拾行囊...";

            if (gifImage != null)
            {
                gifImage.rectTransform.anchoredPosition = _originalPosition + Vector2.right * (moveDistance * progress);
            }

            if (_asyncOperation.progress >= 0.9f)
            {
                // 计算已加载时间
                float elapsed = Time.time - _loadStartTime;
                if(elapsed < minDisplayTime)
                {
                    yield return new WaitForSeconds(minDisplayTime - elapsed);
                }
                
                _asyncOperation.allowSceneActivation = true;
                yield return new WaitForSeconds(0.5f);
                Hide();
                _asyncOperation = null;
                yield break;
            }

            yield return null;
        }
    }
}
