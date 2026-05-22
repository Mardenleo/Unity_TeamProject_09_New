using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    [Header("--- 스텟별 강화 버튼 ---")]
    public Button btnUpgradeAttack; // 💡 파워 버튼 추가
    public Button btnUpgradeSpeed;
    public Button btnUpgradePass;
    public Button btnUpgradeDefense;

    [Header("--- 스텟 UI 슬라이더 / 텍스트 ---")]
    public Slider sliderAttack;     // 💡 파워 슬라이더 추가
    public Slider sliderSpeed;
    public Slider sliderPass;
    public Slider sliderDefense;

    public TextMeshProUGUI txtAttackStat; // 💡 파워 텍스트 추가
    public TextMeshProUGUI txtSpeedStat;   
    public TextMeshProUGUI txtPassStat;
    public TextMeshProUGUI txtDefenseStat;

    [Header("--- 로비 재화 UI 연동 ---")]
    public TextMeshProUGUI txtLobbyGold; 

    [Header("--- 강화 기본 설정 ---")]
    public int upgradeCost = 3000;       // 💡 기본 비용 3,000원으로 인상!

    void Start()
    {
        UpdateUpgradeUI();
    }

    // 💡 각 버튼 클릭 시 데이터 매니저의 진짜 변수들을 배달 보냅니다.
    public void OnClickUpgradeAttack()
    {
        TryUpgrade(ref GameDataManager.Instance.attackStat, "파워");
    }

    public void OnClickUpgradeSpeed()
    {
        TryUpgrade(ref GameDataManager.Instance.speedStat, "스피드");
    }

    public void OnClickUpgradePass()
    {
        TryUpgrade(ref GameDataManager.Instance.passStat, "패스 정확도");
    }

    public void OnClickUpgradeDefense()
    {
        TryUpgrade(ref GameDataManager.Instance.defenseStat, "수비력");
    }

    // 🎲 [기획 핵심] 현재 스텟 수치에 따라 실시간으로 다른 확률을 뱉어내는 룰북
    private int GetSuccessProbability(int currentStat)
    {
        if (currentStat < 60)       return 60; // 0 ~ 59 구간 : 60%
        else if (currentStat < 70)  return 40; // 60 ~ 69 구간 : 40%
        else if (currentStat < 90)  return 15; // 70 ~ 89 구간 : 15%
        else                        return 5;  // 90 ~ 100 구간 : 5%
    }

    // 🛠️ 0~100 스텟 구간별 확률 적용 강화 핵심 로직
    private void TryUpgrade(ref int currentStatValue, string statName)
    {
        if (GameDataManager.Instance == null) return;

        // 1. 이미 100 만점인지 체크
        if (currentStatValue >= GameDataManager.MAX_STAT_VALUE)
        {
            Debug.Log($"{statName}은(는) 이미 최고 수치(100)입니다!");
            return;
        }

        // 2. 돈 체크 (3,000G)
        if (GameDataManager.Instance.currentGold < upgradeCost)
        {
            Debug.LogWarning("🚨 골드가 부족합니다! 강화 불가.");
            return;
        }

        // 3. 돈 차감
        GameDataManager.Instance.currentGold -= upgradeCost;

        // 4. 현재 스텟에 따른 '실시간 맞춤형 확률' 받아오기
        int currentChance = GetSuccessProbability(currentStatValue);

        // 🎲 5. 성공/실패 주사위 굴리기 (0 ~ 99)
        int randomSuccess = Random.Range(0, 100);

        if (randomSuccess < currentChance)
        {
            // 🎉 강화 성공! 5 ~ 10 사이 상승 수치 결정
            int increaseAmount = Random.Range(5, 11); 
            
            currentStatValue += increaseAmount;
            // 100 절대 안 넘어가게 잠금
            currentStatValue = Mathf.Clamp(currentStatValue, 0, GameDataManager.MAX_STAT_VALUE);

            Debug.Log($"🎉 {statName} 강화 성공! (+{increaseAmount} 상승! 적용 확률: {currentChance}%, 현재 스텟: {currentStatValue})");
        }
        else
        {
            // 😭 강화 실패
            Debug.Log($"😭 {statName} 강화 실패... (적용 확률: {currentChance}%, 현재 수치 유지: {currentStatValue})");
        }

        // 화면 싹 새로고침
        UpdateUpgradeUI();
    }

    // 📊 슬라이더 게이지 및 텍스트 전체 리프레시 (파워 추가 버전)
    public void UpdateUpgradeUI()
    {
        if (GameDataManager.Instance == null) return;

        var data = GameDataManager.Instance;

        // 재화 UI
        if (txtLobbyGold != null)
        {
            txtLobbyGold.text = string.Format("{0:#,###} G", data.currentGold);
        }

        // 1. 파워(Attack) UI 갱신
        if (sliderAttack != null) sliderAttack.value = (float)data.attackStat / GameDataManager.MAX_STAT_VALUE;
        if (txtAttackStat != null) txtAttackStat.text = $"{data.attackStat} / 100";

        // 2. 스피드 UI 갱신
        if (sliderSpeed != null) sliderSpeed.value = (float)data.speedStat / GameDataManager.MAX_STAT_VALUE;
        if (txtSpeedStat != null) txtSpeedStat.text = $"{data.speedStat} / 100";

        // 3. 패스 UI 갱신
        if (sliderPass != null) sliderPass.value = (float)data.passStat / GameDataManager.MAX_STAT_VALUE;
        if (txtPassStat != null) txtPassStat.text = $"{data.passStat} / 100";

        // 4. 수비력 UI 갱신
        if (sliderDefense != null) sliderDefense.value = (float)data.defenseStat / GameDataManager.MAX_STAT_VALUE;
        if (txtDefenseStat != null) txtDefenseStat.text = $"{data.defenseStat} / 100";
    }
}