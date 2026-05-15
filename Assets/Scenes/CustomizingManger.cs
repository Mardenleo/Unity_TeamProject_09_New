using UnityEngine;
using TMPro; // TextMeshPro를 사용한다면 필수

public class CustomizingManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject viewName;
    public GameObject viewHair;
    public GameObject viewSkin;

    [Header("Input")]
    public TMP_InputField nameInput; // 선수 이름 입력창

    [Header("Character Preview")]
    public Transform characterTransform; // 회전시킬 캐릭터
    
    private string savedName;

    void Start()
    {
        // 처음 시작할 때 이름 입력창만 켜기
        OpenNameTab();
    }
[   Header("Target Mesh")]
    public SkinnedMeshRenderer hairRenderer; // Ch38_Hair 오브젝트 연결

    // 색상을 바꾸는 경우 (참고 이미지의 머리카락 색상 변경)
    public void SetHairColor(string colorHex)
    {
        if (ColorUtility.TryParseHtmlString(colorHex, out Color newColor))
        {
            hairRenderer.material.color = newColor; // 마테리얼 색상 변경
        }
    }
    
    // 탭 전환 함수들
    public void OpenNameTab() { 
        SetAllViewsOff(); 
        viewName.SetActive(true); 
        }
    public void OpenHairTab() { 
        SetAllViewsOff(); 
        viewHair.SetActive(true); 
        }
    public void OpenSkinTab() { 
        SetAllViewsOff(); 
        viewSkin.SetActive(true); 
        }

    private void SetAllViewsOff()
    {
        viewName.SetActive(false);
        viewHair.SetActive(false);
        viewSkin.SetActive(false);
    }

    // '적용' 버튼 클릭 시 실행될 함수
    public void ApplyCustomizing()
    {
        savedName = nameInput.text;
        Debug.Log("저장된 선수 이름: " + savedName);
        // 여기서 실제로 캐릭터 데이터를 저장하거나 다음 씬으로 넘기는 로직 추가 (추가 사항)
    }
}