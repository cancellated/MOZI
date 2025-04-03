using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] public int targetLevelId;
    [SerializeField] private GameObject lockedIcon;
    [SerializeField] private GameObject unlockedIcon;

    public void Setup(int levelId, string displayText)
    {
        targetLevelId = levelId;
        GetComponentInChildren<Text>().text = displayText;
        UpdateButtonState();
    }

    public void UpdateButtonState()
    {
        bool isUnlocked = GameManager.Instance.IsLevelUnlocked(targetLevelId);
        gameObject.SetActive(isUnlocked);
        
        if (lockedIcon != null) lockedIcon.SetActive(!isUnlocked);
        if (unlockedIcon != null) unlockedIcon.SetActive(isUnlocked);
    }

    public void OnClick()
    {
        if (GameManager.Instance.IsLevelUnlocked(targetLevelId))
        {
            GameManager.Instance.SetCurrentLevel(targetLevelId);
            GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevel);
        }
    }
}