using UnityEngine;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("--- 플레이 정보 및 상태 ---")]
    public bool isCharacterCreated = false;
    public TeamType selectedTeam = TeamType.None;

    [Header("--- 커스터마이징 데이터 ---")]
    public string playerCustomName = "손흥민";
    public Color selectedSkinColor = Color.white;
    public Color selectedHairColor = Color.white;

    [Header("--- 보유 재화 ---")]
    public int currentGold = 400000;

    [Header("--- 선수 능력치 실제 수치 (0 ~ 100) ---")]
    public int attackStat = 40;
    public int speedStat = 40;
    public int passStat = 30;
    public int defenseStat = 40;

    public const int MAX_STAT_VALUE = 100;

    [Header("--- 인벤토리 및 상점 데이터 ---")]
    public int currentEquippedBoots = 0;
    public int currentEquippedHair = 0;

    [Header("--- 현재 유저가 선택한 선수 번호 ---")]
    public int selectedPlayerNumber = 6;

    public List<int> ownedBootsList = new List<int>() { 0 };
    public List<int> ownedHairList = new List<int>() { 0 };

    [Header("--- 최근 경기 결과 ---")]
    public int lastOurScore = 0;
    public int lastEnemyScore = 0;
    public int lastRewardGold = 0;
    public string lastMatchResultText = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveMatchResult(int ourScore, int enemyScore)
    {
        lastOurScore = ourScore;
        lastEnemyScore = enemyScore;

        if (ourScore > enemyScore)
        {
            lastMatchResultText = "승리";
            lastRewardGold = 750;
        }
        else if (ourScore == enemyScore)
        {
            lastMatchResultText = "무승부";
            lastRewardGold = 500;
        }
        else
        {
            lastMatchResultText = "패배";
            lastRewardGold = 250;
        }

        currentGold += lastRewardGold;
    }
}