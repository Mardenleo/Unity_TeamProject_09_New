using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    [Header("--- 스텟별 강화 버튼 ---")]
    public Button btnUpgradeAttack; 
    public Button btnUpgradeSpeed;
    public Button btnUpgradePass;
    public Button btnUpgradeDefense;

    [Header("--- 스텟 UI 게이지 바 ---")]
    public Image imgBarAttack;     
    public Image imgBarSpeed;
    public Image imgBarPass;
    public Image imgBarDefense;

    [Header("--- 스텟 수치 텍스트 ---")]
    public TextMeshProUGUI txtAttackStat; 
    public TextMeshProUGUI txtSpeedStat;   
    public TextMeshProUGUI txtPassStat;
    public TextMeshProUGUI txtDefenseStat;

    [Header("--- 로비 재화 UI 연동 ---")]
    public TextMeshProUGUI txtLobbyGold; 

    [Header("--- 화면용 알림 로그 텍스트 ---")]
    public TextMeshProUGUI txtLogMessage; 

    [Header("--- 강화 기본 설정 ---")]
    public int upgradeCost = 3000;       

    void Start()
    {
        UpdateUpgradeUI();
        if (txtLogMessage != null) txtLogMessage.text = ""; 
    }

    public void OnClickUpgradeAttack() { TryUpgrade(ref GameDataManager.Instance.attackStat, "파워"); }
    public void OnClickUpgradeSpeed() { TryUpgrade(ref GameDataManager.Instance.speedStat, "스피드"); }
    public void OnClickUpgradePass() { TryUpgrade(ref GameDataManager.Instance.passStat, "패스"); }
    public void OnClickUpgradeDefense() { TryUpgrade(ref GameDataManager.Instance.defenseStat, "수비력"); }

    private int GetSuccessProbability(int currentStat)
    {
        if (currentStat < 60)       return 60; 
        else if (currentStat < 70)  return 40; 
        else if (currentStat < 90)  return 15; 
        else                        return 5;  
    }

    private void TryUpgrade(ref int currentStatValue, string statName)
    {
        if (GameDataManager.Instance == null) return;

        // 🌟 [안전장치] 만약 이미 100이면 버튼이 눌려도 실행 안 되게 리턴
        if (currentStatValue >= GameDataManager.MAX_STAT_VALUE)
        {
            SetLogMessage($"⚠️ {statName}은(는) 이미 최고레벨입니다.", Color.yellow);
            return;
        }

        if (GameDataManager.Instance.currentGold < upgradeCost)
        {
            SetLogMessage("❌ 강화에 필요한 골드가 부족합니다!", Color.red);
            return; 
        }

        GameDataManager.Instance.currentGold -= upgradeCost;

        int currentChance = GetSuccessProbability(currentStatValue);
        int randomSuccess = Random.Range(0, 100);

        if (randomSuccess < currentChance)
        {
            int increaseAmount = Random.Range(5, 11); 
            currentStatValue += increaseAmount;
            currentStatValue = Mathf.Clamp(currentStatValue, 0, GameDataManager.MAX_STAT_VALUE);
            
            SetLogMessage($"🎉 {statName} 강화 성공! (+{increaseAmount})", Color.green);

            // 🌟 만약 이번 강화로 딱 100(맥스)을 찍었다면 최고레벨 문구 출력
            if (currentStatValue >= GameDataManager.MAX_STAT_VALUE)
            {
                SetLogMessage($"👑 {statName} 최고레벨 달성!", Color.yellow);
            }
        }
        else
        {
            SetLogMessage($"😭 {statName} 강화 실패...", Color.red);
        }

        UpdateUpgradeUI();
    }

    private void SetLogMessage(string message, Color textColor)
    {
        if (txtLogMessage == null) return;
        txtLogMessage.text = message;
        txtLogMessage.color = textColor;
    }

    public void UpdateUpgradeUI()
    {
        if (GameDataManager.Instance == null) return;

        var data = GameDataManager.Instance;

        if (txtLobbyGold != null) txtLobbyGold.text = $"{data.currentGold.ToString("N0")} G";

        // 📊 1. 파워 연동 및 버튼 활성화 여부 체크
        if (imgBarAttack != null) imgBarAttack.fillAmount = (float)data.attackStat / GameDataManager.MAX_STAT_VALUE;
        if (txtAttackStat != null) txtAttackStat.text = $"파워({data.attackStat})";
        // 💡 수치가 100 이상이면 노란색 버튼(GameObject)을 통째로 숨깁니다!
        if (btnUpgradeAttack != null) btnUpgradeAttack.interactable = data.attackStat < GameDataManager.MAX_STAT_VALUE;


        // 📊 2. 스피드 연동 및 버튼 체크
        if (imgBarSpeed != null) imgBarSpeed.fillAmount = (float)data.speedStat / GameDataManager.MAX_STAT_VALUE;
        if (txtSpeedStat != null) txtSpeedStat.text = $"스피드({data.speedStat})";
        if (btnUpgradeSpeed != null) btnUpgradeSpeed.gameObject.SetActive(data.speedStat < GameDataManager.MAX_STAT_VALUE);

        // 📊 3. 패스 연동 및 버튼 체크
        if (imgBarPass != null) imgBarPass.fillAmount = (float)data.passStat / GameDataManager.MAX_STAT_VALUE;
        if (txtPassStat != null) txtPassStat.text = $"패스({data.passStat})";
        if (btnUpgradePass != null) btnUpgradePass.gameObject.SetActive(data.passStat < GameDataManager.MAX_STAT_VALUE);

        // 📊 4. 수비력 연동 및 버튼 체크
        if (imgBarDefense != null) imgBarDefense.fillAmount = (float)data.defenseStat / GameDataManager.MAX_STAT_VALUE;
        if (txtDefenseStat != null) txtDefenseStat.text = $"수비력({data.defenseStat})";
        if (btnUpgradeDefense != null) btnUpgradeDefense.gameObject.SetActive(data.defenseStat < GameDataManager.MAX_STAT_VALUE);
    }
}