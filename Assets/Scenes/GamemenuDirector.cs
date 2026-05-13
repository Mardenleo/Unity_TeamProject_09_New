using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환 시 필요

public class GamemenuDirector : MonoBehaviour
{
    [Header("메뉴 패널 설정")]
    public GameObject mainMenuPanel;    // MainMenu 오브젝트 연결
    public GameObject inventoryPanel;   // Inventory 오브젝트 연결
    public GameObject shopPanel;        // Shop 오브젝트 연결
    public GameObject optionPanel;      // Option 오브젝트 연결

    // 게임 시작 버튼 (GameStart 버튼에 연결)
    public void StartGame()
    {
        // 실제 게임 플레이 씬 이름이 "SampleScene"이라면 아래 이름을 수정하세요.
        SceneManager.LoadScene("GameScene"); 
    }

    // 인벤토리 열기
    public void OpenInventory()
    {
        SetAllPanelsInactive();
        inventoryPanel.SetActive(true);
    }

    // 상점 열기
    public void OpenShop()
    {
        SetAllPanelsInactive();
        shopPanel.SetActive(true);
    }

    // 환경설정 열기
    public void OpenOption()
    {
        SetAllPanelsInactive();
        optionPanel.SetActive(true);
    }

    // 메인 메뉴로 돌아가기 (뒤로 가기 버튼에 연결)
    public void BackToMain()
    {
        SetAllPanelsInactive();
        mainMenuPanel.SetActive(true);
    }

    // 모든 패널을 한 번에 끄는 편의 기능
    private void SetAllPanelsInactive()
    {
        mainMenuPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(false);
        optionPanel.SetActive(false);
    }
}