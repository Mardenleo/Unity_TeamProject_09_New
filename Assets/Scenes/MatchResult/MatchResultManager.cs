using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MatchResultManager : MonoBehaviour
{
    [Header("결과 UI")]
    public TMP_Text scoreText;
    public TMP_Text resultText;
    public TMP_Text rewardText;
    public TMP_Text currentGoldText;

    [Header("이동할 씬 이름")]
    public string nextSceneName = "MainScene";

    private void Start()
    {
        if (GameDataManager.Instance == null)
        {
            if (scoreText != null) scoreText.text = "0   :   0";
            if (resultText != null) resultText.text = "결과 없음";
            if (rewardText != null) rewardText.text = "+0";
            if (currentGoldText != null) currentGoldText.text = "0G";
            return;
        }

        GameDataManager data = GameDataManager.Instance;

        if (scoreText != null)
            scoreText.text = $"{data.lastOurScore}   :   {data.lastEnemyScore}";

        if (resultText != null)
            resultText.text = data.lastMatchResultText;

        if (rewardText != null)
            rewardText.text = $"+ {data.lastRewardGold}";

        if (currentGoldText != null)
            currentGoldText.text = $"{data.currentGold}G";
    }

    public void OnClickNext()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main_Menu_Scene");
    }
}
