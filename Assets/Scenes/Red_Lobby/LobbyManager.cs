using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("--- 4개의 메인 뷰 패널 ---")]
    public GameObject viewStats;
    public GameObject viewBoots;
    public GameObject viewHair;
    public GameObject viewUpgrade;

    [Header("--- 로비 플레이어 캐릭터 설정 ---")]
    public GameObject lobbyPlayer; // 하이어라키의 Lobby_Player를 연결할 곳

    void Start()
    {
        // 1. 커스텀 창에서 넘어온 색상을 필드의 선수에게 입히기
        ApplyCustomColors();
        
        // 2. 시작할 때 기본 스텟 창만 켜기
        if (viewStats != null) viewStats.SetActive(true);
        ShowStatsMenu();
    }

    void ApplyCustomColors()
    {
    // 데이터 매니저나 로비 플레이어가 비어있다면 리턴
    if (GameDataManager.Instance == null || lobbyPlayer == null) return;

    // 로비 플레이어의 자식들 중에서 모든 SkinnedMeshRenderer를 샅샅이 뒤집니다.
    SkinnedMeshRenderer[] renderers = lobbyPlayer.GetComponentsInChildren<SkinnedMeshRenderer>();

    foreach (SkinnedMeshRenderer rend in renderers)
    {
        // 💡 이름에 대소문자 상관없이 'body'가 들어가면 피부색 적용!
        if (rend.name.ToLower().Contains("body"))
        {
            rend.material.color = GameDataManager.Instance.selectedSkinColor;
        }
        
        // 💡 이름에 대소문자 상관없이 'hair'가 들어가면 머리색 적용!
        if (rend.name.ToLower().Contains("hair"))
        {
            rend.material.color = GameDataManager.Instance.selectedHairColor;
        }
    }
}

    // --- 탭 메뉴 패널 전환 함수들 ---
    public void ShowStatsMenu()
    {
        viewStats.SetActive(true);      // 스텟창은 켜고
        viewUpgrade.SetActive(false);   // 강화 버튼은 숨깁니다.

        if(viewBoots != null) viewBoots.SetActive(false);
        if(viewHair != null) viewHair.SetActive(false);
    }

    // 💡 [능력치 강화] 탭 버튼을 눌렀을 때 (원하시는 기능)
    public void ShowUpgradeMenu()
    {
        viewStats.SetActive(true);      // 스텟창을 그대로 띄워둔 상태에서
        viewUpgrade.SetActive(true);    // 그 옆에 강화 버튼들을 싹 나타나게 합니다.

        if(viewBoots != null) viewBoots.SetActive(false);
        if(viewHair != null) viewHair.SetActive(false);
    }

    public void ShowBootsMenu()
    {
        if(viewBoots != null) viewBoots.SetActive(true);
        viewStats.SetActive(false);
        viewUpgrade.SetActive(false);
        if(viewHair != null) viewHair.SetActive(false);
    }

    public void ShowHairMenu()
    {
        if(viewHair != null) viewHair.SetActive(true);
        viewStats.SetActive(false);
        viewUpgrade.SetActive(false);
        if(viewBoots != null) viewBoots.SetActive(false);
    }

    private void SetAllViewsFalse()
    {
        viewStats.SetActive(false);
        viewBoots.SetActive(false);
        viewHair.SetActive(false);
        viewUpgrade.SetActive(false);
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Main_Menu_Scene");
    }
    public void GoToShop()
    {
        SceneManager.LoadScene("ShopScene");
    }
}