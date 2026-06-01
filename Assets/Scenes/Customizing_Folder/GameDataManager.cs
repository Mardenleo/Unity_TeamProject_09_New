using UnityEngine;
using System.Collections.Generic;


public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("--- 플레이 정보 및 상태 ---")]
    public bool isCharacterCreated = false; // 최초 캐릭터 생성 여부
    public TeamType selectedTeam = TeamType.None; // 기존 프로젝트의 TeamType과 연동됩니다.

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

    // 각 스텟의 최대 제한 수치
    public const int MAX_STAT_VALUE = 100; 

    [Header("--- 인벤토리 및 상점 데이터 ---")]
    public int currentEquippedBoots = 0;
    public int currentEquippedHair = 0;
    [Header("--- 현재 유저가 선택한 선수 번호 ---")]
    // 예: 7을 넣으면 player (7) 프리팹이 유저의 주인공 캐릭터가 됩니다.
    public int selectedPlayerNumber = 6;

    // 유저가 보유 중인 아이템 번호 리스트
    public List<int> ownedBootsList = new List<int>() { 0 };
    public List<int> ownedHairList = new List<int>() { 0 };

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

}