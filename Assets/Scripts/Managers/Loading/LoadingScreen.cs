using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingScreen : SingletonBase<LoadingScreen>
{
    [Header("UI Components")]
    [SerializeField] private CanvasGroup loadingCanvas;
    [SerializeField] private Text loadingText;

    private AsyncOperation _asyncOperation;

    protected override void Initialize()
    {
        Hide();
    }

    public void Show()
    {
        loadingCanvas.alpha = 1;
        loadingCanvas.interactable = true;
        loadingCanvas.blocksRaycasts = true;
    }

    public void Hide()
    {
        loadingCanvas.alpha = 0;
        loadingCanvas.interactable = false;
        loadingCanvas.blocksRaycasts = false;
    }

    public IEnumerator LoadSceneAsync(string sceneName)
    {
        if (_asyncOperation != null) yield break; // 防止重复加载
        
        Show();
        
        _asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        _asyncOperation.allowSceneActivation = false;

        while (!_asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(_asyncOperation.progress / 0.9f);
            loadingText.text = $"正在收拾行囊... {(int)(progress * 100)}%";

            if (_asyncOperation.progress >= 0.9f)
            {
                _asyncOperation.allowSceneActivation = true;
                yield return new WaitForSeconds(0.5f);
                Hide();
                _asyncOperation = null; // 重置加载操作
                yield break;
            }

            yield return null;
        }
    }
}
