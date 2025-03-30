using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private int targetLevelID;
    
    public void Setup(int levelID, string displayText)
    {
        targetLevelID = levelID;
        GetComponentInChildren<Text>().text = displayText;
    }

    public void OnClick()
    {
        GameEvents.TriggerSceneTransition(GameEvents.SceneTransitionType.ToLevel);
    }
}