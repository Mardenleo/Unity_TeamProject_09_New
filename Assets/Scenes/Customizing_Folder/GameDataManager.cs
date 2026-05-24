using UnityEngine;
using System.Collections.Generic;

// 💡 [수정] public enum TeamType 선언부를 중복 방지를 위해 삭제했습니다!
// 유니티가 프로젝트에 원래 존재하던 TeamType을 자동으로 인식합니다.

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("--- 플레이 정보 및 상태 ---")]
    public bool isCharacterCreated = false; // 최초 캐릭터 생성 여부
    public TeamType selectedTeam = TeamType.None; // 기존 프로젝트의 TeamType과 연동됩니다.

    [Header("--- 커스터마이징 데이터 ---")]
    public Color selectedSkinColor = Color.white;
    public Color selectedHairColor = Color.white; 

    [Header("--- 보유 재화 ---")]
    public int currentGold = 6000; 

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