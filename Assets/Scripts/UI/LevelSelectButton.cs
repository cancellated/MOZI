using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private int targetLevelID;
    [SerializeField] private GameObject lockedIcon;
    [SerializeField] private GameObject unlockedIcon;

    public void Setup(int levelID, string displayText)
    {
        targetLevelID = levelID;
        GetComponentInChildren<Text>().text = displayText;
        UpdateButtonState();
    }

    public void UpdateButtonState()
    {
        bool isUnlocked = GameManager.Instance.IsLevelUnlocked(targetLevelID);
        gameObject.SetActive(isUnlocked);
        
        if (lockedIcon != null) lockedIcon.SetActive(!isUnlocked);
        if (unlockedIcon != null) unlockedIcon.SetActive(isUnlocked);
    }

    public void OnClick()
    {
        if (GameManager.Instance.IsLevelUnlocked(targetLevelID))
        {
            GameManager.Instance.SetCurrentLevel(targetLevelID);
            GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevel);
        }
    }
}