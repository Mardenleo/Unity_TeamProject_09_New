using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("--- UI 연동 ---")]
    public TextMeshProUGUI goldText; 

    void Start()
    {
        UpdateGoldUI();
    }

    public void UpdateGoldUI()
    {
        if (GameDataManager.Instance != null && goldText != null)
        {
            goldText.text = $"{GameDataManager.Instance.currentGold} G";
        }
    }

    // ==========================================
    // 💡 [우회용 함수] 유니티 에디터 버튼 연결용 (인자 없음)
    // ==========================================
    
    // --- 축구화 버튼들 ---
    public void Click_BuyBoots_1() { BuyBoots(1, 5000); }  // 1번 축구화 (5000G)
    public void Click_BuyBoots_2() { BuyBoots(2, 7500); }  // 2번 축구화 (7500G)
    public void Click_BuyBoots_3() { BuyBoots(3, 10000); } // 3번 축구화 (10000G)
    public void Click_BuyBoots_4() { BuyBoots(4, 5000); }  // 4번 축구화 (5000G)
    public void Click_BuyBoots_5() { BuyBoots(5, 1000); }  // 5번 축구화 (1000G) 
    public void Click_BuyBoots_6() { BuyBoots(6, 3000); }  // 6번 축구화 (3000G)
    public void Click_BuyBoots_7() { BuyBoots(7, 7500); }  // 7번 축구화 (7500G)
    public void Click_BuyBoots_8() { BuyBoots(8, 3000); }  // 8번 축구화 (3000G)
    // --- 머리 스타일 버튼들 ---
    public void Click_BuyHair_1() { BuyHair(1, 3000); }    
    public void Click_BuyHair_2() { BuyHair(2, 3000); }    
    public void Click_BuyHair_3() { BuyHair(3, 3000); }    
    public void Click_BuyHair_4() { BuyHair(4, 3000); }    
    public void Click_BuyHair_5() { BuyHair(5, 3000); }    
    public void Click_BuyHair_6() { BuyHair(6, 3000); }    
    public void Click_BuyHair_7() { BuyHair(7, 3000); }    
    public void Click_BuyHair_8() { BuyHair(8, 3000); }    
    public void Click_BuyHair_9() { BuyHair(9, 3000); }    
    public void Click_BuyHair_10() { BuyHair(10, 3000); }    
    public void Click_BuyHair_11() { BuyHair(11, 3000); }    
    public void Click_BuyHair_12() { BuyHair(12, 3000); }    


    // ==========================================
    // ⚙️ 실제 구매 로직 (기존과 동일)
    // ==========================================
    private void BuyBoots(int bootsID, int price)
    {
        if (GameDataManager.Instance == null) return;
        if (GameDataManager.Instance.ownedBootsList.Contains(bootsID)) return;

        if (GameDataManager.Instance.currentGold >= price)
        {
            GameDataManager.Instance.currentGold -= price;
            GameDataManager.Instance.ownedBootsList.Add(bootsID);
            UpdateGoldUI();
            Debug.Log($"👟 {bootsID}번 축구화 구매 성공!");
        }
    }

    private void BuyHair(int hairID, int price)
    {
        if (GameDataManager.Instance == null) return;
        if (GameDataManager.Instance.ownedHairList.Contains(hairID)) return;

        if (GameDataManager.Instance.currentGold >= price)
        {
            GameDataManager.Instance.currentGold -= price;
            GameDataManager.Instance.ownedHairList.Add(hairID);
            UpdateGoldUI();
            Debug.Log($"💇 {hairID}번 머리 구매 성공!");
        }
    }
    public void GoToLobby()
    {
        if (GameDataManager.Instance == null)
        {
            return;
        }

        // 💡 데이터 매니저에 저장된 팀 정보를 가져옵니다.
        TeamType playerTeam = GameDataManager.Instance.selectedTeam;

        // 팀 선택에 따른 로비 씬 분기 처리
        if (playerTeam == TeamType.KBC)
        {
            SceneManager.LoadScene("Red_LobbyScene");
        }
        else if (playerTeam == TeamType.JMS)
        {
            SceneManager.LoadScene("Black_LobbyScene");
        }
    }
    public void GoToMainMenu()
    {
        // 현재 폴더 구조에 있는 "Main_Menu_Scene" 이름과 일치시킵니다.
        SceneManager.LoadScene("Main_Menu_Scene");
    }

}