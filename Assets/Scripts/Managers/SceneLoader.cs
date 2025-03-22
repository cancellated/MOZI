using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : SingletonBase<SceneLoader>
{
    protected override void Initialize()
    {
        Debug.Log("场景加载器已初始化");
    }
    public void LoadLevel(int levelId)
    {
        string sceneName = GameManager.Instance.GetLevelScene(levelId);
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"场景加载失败: {sceneName}");
        }
    }
}