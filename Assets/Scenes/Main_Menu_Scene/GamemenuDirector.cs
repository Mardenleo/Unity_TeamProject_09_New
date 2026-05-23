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
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(res));
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

    // --- 씬 전환 버튼 함수 ---
    public void StartGame() 
    { 
        if (PlayerPrefs.GetInt("IsFirstTime", 0) == 0) {
        SceneManager.LoadScene("CustomizingScene"); // 커스터마이징 씬으로
        }   
        else {
        SceneManager.LoadScene("LobbyScene"); // 이미 했으면 로비로
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
}