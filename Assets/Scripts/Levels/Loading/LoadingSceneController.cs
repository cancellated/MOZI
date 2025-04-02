using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    private AsyncOperation _asyncOperation;
    private bool _isLoading = false;

    void Start()
    {
        Debug.Log("开始加载场景");
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        if (_isLoading) yield break;
        _isLoading = true;

        string targetScene = SceneManager.Instance.GetTargetScene();
        Debug.Log($"目标场景: {targetScene}");
        
        // 确保场景存在
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("目标场景名为空");
            yield break;
        }

        // 开始异步加载
        _asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(targetScene);
        if (_asyncOperation == null)
        {
            Debug.LogError("异步加载操作创建失败");
            yield break;
        }

        _asyncOperation.allowSceneActivation = false;
        
        // 更新进度条
        while (!_asyncOperation.isDone)
        {
            if (_asyncOperation == null) yield break;
            
            float progress = Mathf.Clamp01(_asyncOperation.progress / 0.9f);
            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = $"正在收拾行囊... {(int)(progress * 100)}%";
            
            if (_asyncOperation.progress >= 0.9f)
            {
                Debug.Log("场景加载完成，准备切换");
                _asyncOperation.allowSceneActivation = true;
                yield break;
            }
            
            yield return null;
        }
    }

    void OnDestroy()
    {
        // 清理资源
        if (_asyncOperation != null)
        {
            _asyncOperation.allowSceneActivation = true;
            _asyncOperation = null;
        }
    }
}