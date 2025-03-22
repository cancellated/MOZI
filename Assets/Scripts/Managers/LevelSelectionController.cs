using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionController : SingletonBase<LevelSelectionController>
{
    [Header("组件绑定")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    protected override void Initialize()
    {
        GenerateLevelButtons();
        GameEvents.OnLevelUnlocked.AddListener(UpdateButtonState);
    }

    private void GenerateLevelButtons()
    {
        for (int i = 1; i <= GameManager.Instance.TotalLevels; i++)
        {
            GameObject btn = Instantiate(buttonPrefab, buttonContainer);
            SetupButton(btn, i);
        }
    }

    private void SetupButton(GameObject buttonObj, int levelId)
    {
        Button btn = buttonObj.GetComponent<Button>();
        Text btnText = buttonObj.GetComponentInChildren<Text>();

        btnText.text = $"关卡 {levelId}";
        btn.interactable = GameManager.Instance.IsLevelUnlocked(levelId);
        btn.onClick.AddListener(() =>
            GameEvents.OnLevelSelected.Invoke(levelId));
    }

    private void UpdateButtonState(int levelId)
    {
        Transform btn = buttonContainer.GetChild(levelId - 1);
        btn.GetComponent<Button>().interactable = true;
    }
}