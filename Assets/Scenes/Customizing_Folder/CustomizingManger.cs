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
    
    // 🎨 [확실한 해결책] 유저가 UI 버튼을 눌러 선택한 색상을 실시간으로 임시 보관할 변수들
    // 초기화 부실로 인한 버그를 막기 위해 기본값은 기본 피부색인 흰색(Color.white)으로 설정합니다.
    private Color temporarySkinColor = Color.white;
    private Color temporaryHairColor = Color.white;

    private TeamType currentChosenTeam = TeamType.None;

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
                
                // 🎯 [데이터 각인] 머리 색상 클릭 순간 변수에 저장!
                temporaryHairColor = newColor;
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
            
            // 🎯 [데이터 각인] 피부 색상 클릭 순간 변수에 저장!
            temporarySkinColor = newColor;
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

        // 2. 입력된 커스텀 이름 전송
        if (nameInput != null && !string.IsNullOrEmpty(nameInput.text))
        {
            GameDataManager.Instance.playerCustomName = nameInput.text;
        }

        // 3. 인게임에서 내가 조종할 고유 등번호 연동 지정
        // selectedCharacterIndex가 0(JMS)이면 7번 프리팹을 유저로, 1(KBC)이면 10번 프리팹을 유저로 인식시킵니다.
        GameDataManager.Instance.selectedPlayerNumber = (selectedCharacterIndex == 0) ? 7 : 10;

        // 4. 캐릭터 생성 완료 플래그 도장 찍기
        GameDataManager.Instance.isCharacterCreated = true;

        // 5. 🎯 [핵심 변경] 런타임 마테리얼에서 뜯지 않고 변수에 온전하게 기록된 컬러 값을 매니저에 확실하게 다이렉트 전송!
        GameDataManager.Instance.selectedSkinColor = temporarySkinColor;
        GameDataManager.Instance.selectedHairColor = temporaryHairColor;

        // 6. 씬 전환 출발!
        if (currentChosenTeam == TeamType.KBC) SceneManager.LoadScene("Red_LobbyScene");
        else if (currentChosenTeam == TeamType.JMS) SceneManager.LoadScene("Black_LobbyScene");
    }
}