using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("--- 선택된 팀 정보 ---")]
    public TeamType selectedTeam = TeamType.None;

    [Header("--- 커스터마이징 데이터 ---")]
    public Color selectedSkinColor = Color.white;
    public Color selectedHairColor = Color.white; // 💡 string 대신 Color 타입으로 변경!

    [Header("--- 보유 재화 ---")]
    public int currentGold = 6000; 

    [Header("--- 선수 능력치 실제 수치 (0 ~ 100) ---")]

    public int attackStat =  40;
    public int speedStat = 40;   // 💡 기존 level 대신 실제 스텟 수치로 변경!
    public int passStat = 30;
    public int defenseStat = 40;

    // 각 스텟의 최대 제한 수치
    public const int MAX_STAT_VALUE = 100; // 💡 최대치 100 제한

    [Header("--- 인벤토리 및 상점 데이터 ---")]
    // 🌟 이 두 변수 이름의 대소문자가 정확해야 InventoryManager에서 에러가 안 납니다!
    public int currentEquippedBoots = 0;
    public int currentEquippedHair = 0;

    // 유저가 보유 중인 아이템 번호 리스트 (List 변수도 정확히 선언되었는지 확인)
    public System.Collections.Generic.List<int> ownedBootsList = new System.Collections.Generic.List<int>() { 0 };
    public System.Collections.Generic.List<int> ownedHairList = new System.Collections.Generic.List<int>() { 0 };

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