using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("--- 로비 플레이어 (외형 변경용) ---")]
    public GameObject lobbyPlayer;

    void Start()
    {
        // 씬이 켜지면 현재 장착 중인 데이터대로 선수의 장비를 입혀줍니다.
        ApplyEquippedEquipment();
    }

    // 💡 축구화 장착 버튼을 눌렀을 때 실행할 함수 (버튼 OnClick에 연결할 것)
    public void EquipBoots(int bootsID)
    {
        if (GameDataManager.Instance == null) return;

        // 보유 중인 아이템인지 확인 (나중에 상점 연동용 예외처리)
        if (!GameDataManager.Instance.ownedBootsList.Contains(bootsID))
        {
            Debug.LogWarning("❌ 아직 구매하지 않은 축구화입니다!");
            return;
        }

        // 데이터 저장 및 외형 변경
        GameDataManager.Instance.currentEquippedBoots = bootsID;
        ApplyEquippedEquipment();
        Debug.Log($"👟 {bootsID}번 축구화 장착 완료!");
    }

    // 💡 머리 스타일 장착 버튼을 눌렀을 때 실행할 함수
    public void EquipHair(int hairID)
    {
        if (GameDataManager.Instance == null) return;

        if (!GameDataManager.Instance.ownedHairList.Contains(hairID))
        {
            Debug.LogWarning("❌ 아직 구매하지 않은 머리 스타일입니다!");
            return;
        }

        GameDataManager.Instance.currentEquippedHair = hairID;
        ApplyEquippedEquipment();
        Debug.Log($"💇 {hairID}번 머리 장착 완료!");
    }

    // 🔄 현재 데이터 매니저에 저장된 값에 세팅에 따라 선수 오브젝트를 껐다 켜는 핵심 함수
    public void ApplyEquippedEquipment()
    {
        if (lobbyPlayer == null || GameDataManager.Instance == null) return;

        int bootsID = GameDataManager.Instance.currentEquippedBoots;
        int hairID = GameDataManager.Instance.currentEquippedHair;

        // 💡 플레이어 자식 중에서 "Boots_0", "Hair_1" 같은 이름을 찾아 활성화/비활성화 합니다.
        // (선수 모델링 자식 오브젝트들의 이름 구조에 맞게 이 부분을 나중에 살짝 다듬어줄 겁니다.)
        Transform[] allChildren = lobbyPlayer.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            // 예시: 이름이 Boots_로 시작하는 오브젝트들 제어
            if (child.name.StartsWith("Boots_"))
            {
                child.gameObject.SetActive(child.name == $"Boots_{bootsID}");
            }
            // 예시: 이름이 Hair_로 시작하는 오브젝트들 제어
            if (child.name.StartsWith("Hair_"))
            {
                child.gameObject.SetActive(child.name == $"Hair_{hairID}");
            }
        }
    }
}