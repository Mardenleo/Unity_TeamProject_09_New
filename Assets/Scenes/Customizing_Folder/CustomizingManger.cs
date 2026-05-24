using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; 

public enum TeamType { None, KBC, JMS }

public class CustomizingManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject viewTeam; 
    public GameObject viewName; 
    public GameObject viewHair; 
    public GameObject viewSkin; 

    [Header("Input")]
    public TMP_InputField nameInput; 

    [Header("Character Preview Settings")]
    public Camera previewCamera;           
    private int selectedCharacterIndex = 0; // 💡 유저가 선택한 캐릭터 번호 (JMS = 0, KBC = 1)

    [System.Serializable]
    public struct CharacterData
    {
        public Transform characterTransform;  
        public SkinnedMeshRenderer hairRenderer; 
        public SkinnedMeshRenderer skinRenderer; 
        public Vector3 cameraPosition;        
    }

    [Header("Characters Info")]
    public CharacterData[] characters; 

    [Header("Audio Settings")]
    public AudioSource bgmSource;       
    public AudioSource sfxSource;       
    public AudioClip buttonClickSound;
    

    void Start()
    {
        OpenTeamTab();
        
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }

        UpdatePreviewCharacter(0);
    }

    // 캐릭터(팀) 버튼을 누를 때 인덱스 갱신
    public void SelectCharacter(int index)
    {
        PlayClickSound();
        if (index < 0 || index >= characters.Length) return;

        selectedCharacterIndex = index; // 💡 여기서 0 또는 1이 실시간 저장됨
        UpdatePreviewCharacter(index);
    }

    private void UpdatePreviewCharacter(int index)
    {
        if (previewCamera != null && characters.Length > index)
        {
            previewCamera.transform.position = characters[index].cameraPosition;
        }
    }

    public void SetHairColor(string colorHex)
    {
        PlayClickSound();
        if (ColorUtility.TryParseHtmlString(colorHex, out Color newColor))
        {
            var currentTarget = characters[selectedCharacterIndex].hairRenderer;
            if (currentTarget != null)
            {
                currentTarget.material.color = newColor;
            }
        }
    }

    public void SetSkinColor(string colorHex)
    {
        PlayClickSound();
        var currentTarget = characters[selectedCharacterIndex].skinRenderer;
        if (currentTarget == null) return;

        if (ColorUtility.TryParseHtmlString(colorHex, out Color newColor))
        {
            currentTarget.material.color = newColor;
        }
    }
    
    // 탭 전환 함수들
    public void OpenTeamTab() { PlayClickSound(); SetAllViewsOff(); viewTeam.SetActive(true); }
    public void OpenNameTab() { PlayClickSound(); SetAllViewsOff(); viewName.SetActive(true); }
    public void OpenHairTab() { PlayClickSound(); SetAllViewsOff(); viewHair.SetActive(true); }
    public void OpenSkinTab() { PlayClickSound(); SetAllViewsOff(); viewSkin.SetActive(true); }

    private void SetAllViewsOff()
    {
        if(viewTeam != null) viewTeam.SetActive(false);
        if(viewName != null) viewName.SetActive(false);
        if(viewHair != null) viewHair.SetActive(false);
        if(viewSkin != null) viewSkin.SetActive(false);
    }

    private void PlayClickSound()
    {
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.Stop(); 
            sfxSource.clip = buttonClickSound;
            sfxSource.Play();
        }
    }

    private TeamType currentChosenTeam = TeamType.None;

    // UI에서 팀 버튼 누를 때 실행 (KBC / JMS 문자열 전달)
    public void SelectTeam(string teamName)
    {
        if (teamName == "KBC") currentChosenTeam = TeamType.KBC;
        else if (teamName == "JMS") currentChosenTeam = TeamType.JMS;
    }

    // [적용] 버튼 핵심 함수 (데이터 복사 및 씬 이동)
    public void OnClickSaveAndGoToLobby()
    {
        if (currentChosenTeam == TeamType.None || GameDataManager.Instance == null) return;

        // 1. 선택한 팀 데이터 저장
        GameDataManager.Instance.selectedTeam = currentChosenTeam;

        // 🔥 [추가] 이제 캐릭터를 만든 적이 있다고 도장을 찍어줍니다!
        // 이 한 줄이 있어야 메인 메뉴에서 다시 게임 시작을 누를 때 커스텀 창을 스킵합니다!
        GameDataManager.Instance.isCharacterCreated = true;

        // 2. 현재 선택된 캐릭터의 피부색/머리색 실제 마테리얼 컬러 낚아채기
        if (characters != null && characters.Length > selectedCharacterIndex)
        {
            // 피부색 추출 및 전송
            if (characters[selectedCharacterIndex].skinRenderer != null)
            {
                GameDataManager.Instance.selectedSkinColor = characters[selectedCharacterIndex].skinRenderer.material.color;
            }

            // 머리색 추출 및 전송
            if (characters[selectedCharacterIndex].hairRenderer != null)
            {
                GameDataManager.Instance.selectedHairColor = characters[selectedCharacterIndex].hairRenderer.material.color;
            }
        }

        // 3. 씬 전환 출발!
        if (currentChosenTeam == TeamType.KBC) SceneManager.LoadScene("Red_LobbyScene");
        else if (currentChosenTeam == TeamType.JMS) SceneManager.LoadScene("Black_LobbyScene");
    }
}