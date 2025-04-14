using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private Button button;
    public int targetLevelId;
    
    private LevelSelectionController _controller;

    private void Start()
    {
        _controller = FindFirstObjectByType<LevelSelectionController>();
        button.onClick.AddListener(OnClick);
    }

    public void UpdateVisualState(bool isUnlocked)
    {
        gameObject.SetActive(isUnlocked);
    }

    private void OnClick()
    {
        if (_controller != null)
        {
            _controller.OnLevelButtonClicked(targetLevelId);
        }
        else
        {
            Debug.LogError("找不到LevelSelectionController实例");
        }
    }
}