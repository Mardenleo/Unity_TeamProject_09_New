using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [Header("--- 로비 플레이어 (외형 변경용) ---")]
    public GameObject lobbyPlayer;

    [Header("--- 인벤토리 UI 버튼 리스트 ---")]
    // ⚠️ [중요] 인스펙터 창에서 버튼을 등록할 때 "진짜 상점 ID 순서"대로 넣으셔야 합니다!
    // Element 0 = 1번 아이템 버튼
    // Element 1 = 2번 아이템 버튼 ... Element 7 = 8번 아이템 버튼!
    public List<Button> bootsButtons; 
    public List<Button> hairButtons;  

    [Header("--- 축구화 머테리얼 리스트 (총 8개) ---")]
    public List<Material> bootsMaterials;

    [Header("--- 머리 색상(스타일) 리스트 (총 12개) ---")]
    // 🎨 머리 스타일은 '색상'만 바꾸는 것이므로, 12가지 색상을 인스펙터에서 등록해줍니다!
    public List<Color> hairColors;

    void Start()
    {
        RefreshInventoryUI();
        ApplyEquippedEquipment();
    }

    // 🔄 상점 구매 기록을 확인해서 인벤토리 버튼을 잠그거나 풀어주는 함수
    public void RefreshInventoryUI()
    {
        if (GameDataManager.Instance == null) return;

        // 축구화 버튼 활성화 체크 (1번부터 시작)
        for (int i = 0; i < bootsButtons.Count; i++)
        {
            if (bootsButtons[i] == null) continue;
            int itemID = i + 1; 
            bool isOwned = GameDataManager.Instance.ownedBootsList.Contains(itemID);
            bootsButtons[i].interactable = isOwned; 
        }

        // 머리 스타일 버튼 활성화 체크 (1번부터 시작)
        for (int i = 0; i < hairButtons.Count; i++)
        {
            if (hairButtons[i] == null) continue;
            int itemID = i + 1; 
            bool isOwned = GameDataManager.Instance.ownedHairList.Contains(itemID);
            hairButtons[i].interactable = isOwned;
        }
    }

    // 👟 축구화 장착 함수
    public void EquipBoots(int bootsID)
    {
        if (GameDataManager.Instance == null) return;

        if (!GameDataManager.Instance.ownedBootsList.Contains(bootsID))
        {
            Debug.LogWarning($"❌ 아직 구매하지 않은 {bootsID}번 축구화입니다!");
            return;
        }

        GameDataManager.Instance.currentEquippedBoots = bootsID;
        ApplyEquippedEquipment();
        Debug.Log($"👟 {bootsID}번 축구화 장착 완료!");
    }

    // 💇 머리 스타일(색상) 장착 함수
    public void EquipHair(int hairID)
    {
        if (GameDataManager.Instance == null) return;

        if (!GameDataManager.Instance.ownedHairList.Contains(hairID))
        {
            Debug.LogWarning($"❌ 아직 구매하지 않은 {hairID}번 머리 스타일입니다!");
            return;
        }

        GameDataManager.Instance.currentEquippedHair = hairID;
        ApplyEquippedEquipment();
        Debug.Log($"💇 {hairID}번 머리 색상 변경 완료!");
    }

    // 🔄 진짜로 선수의 메쉬/색상을 갈아입히는 핵심 로직
    public void ApplyEquippedEquipment()
    {
        if (lobbyPlayer == null || GameDataManager.Instance == null) return;

        int bootsID = GameDataManager.Instance.currentEquippedBoots;
        int hairID = GameDataManager.Instance.currentEquippedHair;

        Transform[] allChildren = lobbyPlayer.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            // --------------------------------------------------
            // 👟 축구화 처리 (Ch38_Shoes)
            // --------------------------------------------------
            if (child.name.Contains("Ch38_Shoes"))
            {
                child.gameObject.SetActive(true); // 항상 켜둠
                
                SkinnedMeshRenderer renderer = child.GetComponent<SkinnedMeshRenderer>();
                if (renderer != null && bootsMaterials.Count > 0)
                {
                    int matIndex = bootsID - 1;

                    if (matIndex >= 0 && matIndex < bootsMaterials.Count)
                    {
                        if (bootsMaterials[matIndex] != null)
                        {
                            renderer.material = bootsMaterials[matIndex];
                        }
                    }
                }
            }

            // --------------------------------------------------
            // 💇 머리 스타일 처리 (Ch38_Hair) - 색상만 변경하도록 전면 수정!
            // --------------------------------------------------
            if (child.name.Contains("Ch38_Hair"))
            {
                child.gameObject.SetActive(true); // 대머리 방지

                Renderer hairRenderer = child.GetComponent<Renderer>();
                if (hairRenderer != null)
                {
                    // 💡 [핵심 수정] 
                    // 인스펙터의 hairColors 리스트에 등록된 12가지 색상 중 
                    // 현재 선택된 hairID에 맞는 색상을 실시간으로 씌워줍니다!
                    if (hairColors.Count > 0)
                    {
                        int colorIndex = hairID - 1; // ID는 1부터, 리스트는 0부터 시작하므로
                        
                        if (colorIndex >= 0 && colorIndex < hairColors.Count)
                        {
                            hairRenderer.material.color = hairColors[colorIndex];
                        }
                    }
                    else
                    {
                        // 등록된 색상이 없을 때만 커스텀 씬 기본 컬러 유지
                        hairRenderer.material.color = GameDataManager.Instance.selectedHairColor;
                    }
                }
            }
        }
    }
}