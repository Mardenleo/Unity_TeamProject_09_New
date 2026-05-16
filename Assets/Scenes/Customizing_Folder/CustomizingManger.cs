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

    [Header("Audio Settings")]
    // ★ 이 스크립트가 붙은 오브젝트에 AudioSource를 넣고 여기에 연결하세요.
    public AudioSource bgmSource;       // 배경음악 전용 (인스펙터에서 연결)
    public AudioSource sfxSource;       // 버튼 효과음 전용 (인스펙터에서 연결)
    public AudioClip buttonClickSound;
    
    private string savedName;

    void Start()
    {
        
        // 처음 시작할 때 이름 입력창만 켜기
        OpenNameTab();
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }
    [Header("Target Mesh")]
    public SkinnedMeshRenderer hairRenderer; // Ch38_Hair 오브젝트 연결
    public SkinnedMeshRenderer skinRenderer;

    // 색상을 바꾸는 경우 (참고 이미지의 머리카락 색상 변경)
    public void SetHairColor(string colorHex)
    {
        PlayClickSound();
        if (ColorUtility.TryParseHtmlString(colorHex, out Color newColor))
        {
            hairRenderer.material.color = newColor; // 마테리얼 색상 변경
        }
    }

    public void SetSkinColor(string colorHex)
    {
        PlayClickSound();
        if (skinRenderer == null)
        {
            Debug.LogError("skinRenderer가 연결되지 않았습니다!");
            return;
        }

        if (ColorUtility.TryParseHtmlString(colorHex, out Color newColor))
        {
            skinRenderer.material.color = newColor; // 마테리얼 색상 변경
        }
    }
    
    // 탭 전환 함수들
    public void OpenNameTab() { 
        PlayClickSound();
        SetAllViewsOff(); 
        viewName.SetActive(true); 
        }
    public void OpenHairTab() { 
        PlayClickSound();
        SetAllViewsOff(); 
        viewHair.SetActive(true); 
        }
    public void OpenSkinTab() { 
        PlayClickSound();
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
        PlayClickSound();
        savedName = nameInput.text;
        Debug.Log("저장된 선수 이름: " + savedName);
        // 여기서 실제로 캐릭터 데이터를 저장하거나 다음 씬으로 넘기는 로직 추가 (추가 사항)
    }
    private void PlayClickSound()
    {
        // audioSource와 사운드 파일이 잘 등록되어 있는지 확인 후 재생
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.Stop(); // 효과음만 끊습니다. (배경음악은 무사함!)
            sfxSource.clip = buttonClickSound;
            sfxSource.Play();
        }
    }
}