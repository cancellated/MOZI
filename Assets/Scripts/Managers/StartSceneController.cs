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

    [Header("动画设置")]
    [SerializeField] private float zoomDuration = 1.5f; 
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float initialZoom = 1.2f;
    [SerializeField] private Vector2 initialOffset = new(500, 0);

    private RectTransform _imageRect;
    private bool _isAnimating;
    private bool _hasStartedAnimation; // 新增动画状态标识
    private Vector2 _originalPosition;

    void Start()
    {
        _imageRect = fullscreenImage.GetComponent<RectTransform>();
        _originalPosition = _imageRect.anchoredPosition; 
        
        // 直接使用预制体初始状态，无需动态生成
        _imageRect.anchoredPosition = _originalPosition + initialOffset;
        _imageRect.localScale = Vector3.one * initialZoom;
        fullscreenImage.gameObject.SetActive(true);
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