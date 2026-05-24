using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // TMP_Dropdown 사용을 위해 필수
using UnityEngine.EventSystems;

public class GamemenuDirector : MonoBehaviour
{
    [Header("메뉴 패널 설정")]
    public GameObject mainMenuPanel;
    public GameObject optionPanel;

    [Header("환경설정 UI 요소")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown; 

    [Header("오디오 소스")]
    public AudioSource bgmSource;

    private List<Resolution> resolutions = new List<Resolution>();

    void Start()
    {
        // 1. 해상도 리스트 초기화 및 UI 세팅
        InitResolution();

        // 2. 저장된 설정 로드 및 UI 반영
        LoadSettings();

        // 3. 시작 시 패널 상태 설정 (옵션창은 끄고 메인메뉴는 켜기)
        if (optionPanel != null) optionPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // --- 해상도 관련 로직 ---
    private void InitResolution()
    {
        string[] basicRes = { "1920 x 1080", "1280 x 720", "2560 x 1440" };
        foreach (string res in basicRes){
            if (resolutionDropdown != null) resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(res));
        }
        if (resolutionDropdown == null) return;

        resolutions.Clear();
        resolutionDropdown.ClearOptions();

        int currentResIndex = 0;
        // 시스템 지원 해상도 전체 가져오기
        Resolution[] allResolutions = Screen.resolutions;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            if (allResolutions[i].width < 1024) continue; // 너무 낮은 해상도 제외

            string option = allResolutions[i].width + " x " + allResolutions[i].height;
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option));
            resolutions.Add(allResolutions[i]);

            // 현재 모니터 해상도와 일치하는 인덱스 찾기
            if (allResolutions[i].width == Screen.currentResolution.width &&
                allResolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = resolutions.Count - 1;
            }
        }

        resolutionDropdown.RefreshShownValue();
        // 저장된 해상도 값이 있으면 불러오고, 없으면 현재 해상도 인덱스 사용
        resolutionDropdown.value = PlayerPrefs.GetInt("ResIndex", currentResIndex);
    }

    // --- 설정 불러오기 ---
    private void LoadSettings()
    {
        float bgmVol = PlayerPrefs.GetFloat("BGM_Volume", 0.5f);
        float sfxVol = PlayerPrefs.GetFloat("SFX_Volume", 0.5f);
        bool isFull = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        if (bgmSlider != null) bgmSlider.value = bgmVol;
        if (sfxSlider != null) sfxSlider.value = sfxVol;
        if (fullscreenToggle != null) fullscreenToggle.isOn = isFull;

        if (bgmSource != null)
        {
            bgmSource.volume = bgmVol;
            if (!bgmSource.isPlaying) bgmSource.Play();
        }

        Screen.fullScreen = isFull;
    }

    // --- 🎯 [핵심 수정] 씬 전환 버튼 함수 ---
    public void StartGame() 
    { 
        // 1. 데이터 매니저가 없는 최초 시점(예외 상황)에는 안전하게 커스텀 씬으로 보냄
        if (GameDataManager.Instance == null)
        {
            SceneManager.LoadScene("CustomizingScene");
            return;
        }

        // 2. 💡 이미 캐릭터를 만든 적이 있는가?
        if (GameDataManager.Instance.isCharacterCreated)
        {
            // 데이터 매니저에 저장된 팀 정보를 확인해서 해당 팀의 로비로 즉시 이동!
            if (GameDataManager.Instance.selectedTeam == TeamType.KBC)
            {
                Debug.Log("🏠 이미 생성된 레드 팀 캐릭터가 있습니다. 레드 로비로 바로 이동!");
                SceneManager.LoadScene("Red_LobbyScene");
            }
            else if (GameDataManager.Instance.selectedTeam == TeamType.JMS)
            {
                Debug.Log("🏠 이미 생성된 블랙 팀 캐릭터가 있습니다. 블랙 로비로 바로 이동!");
                SceneManager.LoadScene("Black_LobbyScene");
            }
            else
            {
                // 생성은 체크되었는데 팀이 None 상태라면 예외 처리로 커스텀 씬 이동
                SceneManager.LoadScene("CustomizingScene");
            }
        }   
        else 
        {
            // 3. 💡 최초 실행일 경우: 캐릭터를 새로 만들어야 하므로 커스터마이징 씬으로!
            Debug.Log("🎨 생성된 캐릭터가 없으므로 커스터마이징 씬으로 이동합니다.");
            SceneManager.LoadScene("CustomizingScene"); 
        }
    }

    public void OpenInventory() 
    {
        // 인벤토리 패널 대신 씬으로 이동
        SceneManager.LoadScene("InventoryScene"); 
    }

    public void OpenShop() 
    { 
        // 상점 패널 대신 씬으로 이동
        SceneManager.LoadScene("ShopScene");
    }

    // --- 환경설정 조작 함수 ---
    public void OpenOption() 
    { 
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(true); 
    }

    public void BackToMain() 
    { 
        if (optionPanel != null) optionPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true); 
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void SaveAndCloseOption()
    {
        // 1. 값 저장
        if (bgmSlider != null) PlayerPrefs.SetFloat("BGM_Volume", bgmSlider.value);
        if (sfxSlider != null) PlayerPrefs.SetFloat("SFX_Volume", sfxSlider.value);
        if (fullscreenToggle != null) PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        if (resolutionDropdown != null) PlayerPrefs.SetInt("ResIndex", resolutionDropdown.value);
        
        PlayerPrefs.Save();

        // 2. 해상도 및 전체화면 즉시 적용
        if (resolutionDropdown != null && resolutions.Count > resolutionDropdown.value)
        {
            Resolution res = resolutions[resolutionDropdown.value];
            Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn);
        }

        BackToMain();
    }

    public void CancelAndCloseOption()
    {
        LoadSettings(); // 저장된 값으로 되돌리기
        BackToMain();
    }

    public void SetBGMVolume() 
    { 
        if (bgmSource != null && bgmSlider != null) 
            bgmSource.volume = bgmSlider.value; 
    }

    public void SetSFXVolume()
    {
        if (sfxSlider != null)
            Debug.Log($"효과음 볼륨 변경: {sfxSlider.value}");
    }

    public void ResetGameData()
    {
        // 1. 유니티 로컬 저장소(환경설정, 이전 기록 등) 싹 다 밀어버리기
        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.Save();

        // 2. 싱글톤 데이터 매니저 주머니 완전 리셋
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.isCharacterCreated = false;       // 캐릭터 생성 여부 리셋
            GameDataManager.Instance.selectedTeam = TeamType.None;     // 선택 팀 리셋
            GameDataManager.Instance.currentGold = 6000;               // 초기 자금 리셋
            
            // 장착 및 인벤토리 데이터 초기화
            GameDataManager.Instance.currentEquippedBoots = 0;
            GameDataManager.Instance.currentEquippedHair = 0;
            GameDataManager.Instance.ownedBootsList = new List<int>() { 0 };
            GameDataManager.Instance.ownedHairList = new List<int>() { 0 };

            // 기본 능력치 리셋
            GameDataManager.Instance.attackStat = 40;
            GameDataManager.Instance.speedStat = 40;
            GameDataManager.Instance.passStat = 30;
            GameDataManager.Instance.defenseStat = 40;
            
            // 커스텀 색상 기본값 복구
            GameDataManager.Instance.selectedSkinColor = Color.white;
            GameDataManager.Instance.selectedHairColor = Color.white;
        }

        Debug.Log("♻️ 모든 게임 데이터 초기화 완료! 메인 메뉴를 새로고침합니다.");

        // 3. 🔥 [핵심 변경] 현재 메인 메뉴 씬을 처음부터 다시 로드해서 UI를 깨끗하게 리셋합니다!
        // 이렇게 하면 LoadSettings 오류도 안 나고 볼륨 슬라이더도 알아서 초기화됩니다.
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

}