using UnityEngine;
using System.Collections.Generic;

public class InGameMatchDirector : MonoBehaviour
{
    [Header("--- [JMS 팀] 프리팹 데이터 (블루/블랙 계열) ---")]
    public List<GameObject> jmsPlayerPrefabs; 
    public GameObject jmsGKPrefab;             

    [Header("--- [KBC 팀] 프리팹 데이터 (레드 계열) ---")]
    public List<GameObject> kbcPlayerPrefabs;  
    public GameObject kbcGKPrefab;              

    [Header("--- 포메이션 위치 (부모 오브젝트) ---")]
    public Transform homeFormationParent; 
    public Transform awayFormationParent; 

    [Header("--- 상점 스킨 동기화용 리스트 ---")]
    public List<Material> bootsMaterials;
    public List<Color> hairColors;

    [Header("--- 한글 폰트 설정 (TextMeshPro용) ---")]
    // ⚠️ 인스펙터 창에서 한글을 지원하는 TMP Font Asset을 반드시 넣어주세요! (예: 창모, 나눔고딕 등)
    public TMPro.TMP_FontAsset koreanFont;

    private List<GameObject> homeFinalPrefabs;
    private GameObject homeFinalGK;
    private List<GameObject> awayFinalPrefabs;
    private GameObject awayFinalGK;

    void Start()
    {
        SetTeamsBySelection();
        SpawnAllUniquePlayers();
    }

    void SetTeamsBySelection()
    {
        if (GameDataManager.Instance == null)
        {
            homeFinalPrefabs = jmsPlayerPrefabs;
            homeFinalGK = jmsGKPrefab;
            awayFinalPrefabs = kbcPlayerPrefabs;
            awayFinalGK = kbcGKPrefab;
            return;
        }

        if (GameDataManager.Instance.selectedTeam == TeamType.KBC) 
        {
            homeFinalPrefabs = kbcPlayerPrefabs;
            homeFinalGK = kbcGKPrefab;
            awayFinalPrefabs = jmsPlayerPrefabs;
            awayFinalGK = jmsGKPrefab;
        }
        else 
        {
            homeFinalPrefabs = jmsPlayerPrefabs;
            homeFinalGK = jmsGKPrefab;
            awayFinalPrefabs = kbcPlayerPrefabs;
            awayFinalGK = kbcGKPrefab;
        }
    }

    void SpawnAllUniquePlayers()
    {
        if (GameDataManager.Instance == null) return;

        int myUserNumber = GameDataManager.Instance.selectedPlayerNumber; 

        // 🔵 1. 홈팀 (유저 팀) 11명 배치
        Transform[] homePositions = homeFormationParent.GetComponentsInChildren<Transform>();
        for (int i = 1; i <= 11; i++)
        {
            if (i > homePositions.Length - 1) break;

            GameObject spawnedPlayer = null;

            if (i == 1)
            {
                spawnedPlayer = Instantiate(homeFinalGK, homePositions[i].position, homePositions[i].rotation);
            }
            else
            {
                int prefabIndex = i - 2; 
                if (prefabIndex < homeFinalPrefabs.Count && homeFinalPrefabs[prefabIndex] != null)
                {
                    spawnedPlayer = Instantiate(homeFinalPrefabs[prefabIndex], homePositions[i].position, homePositions[i].rotation);
                    
                    int currentPlayerNum = prefabIndex + 1; 
                    if (currentPlayerNum == myUserNumber)
                    {
                        spawnedPlayer.tag = "Player"; 
                        ApplyCustomSkin(spawnedPlayer); 
                    }
                }
            }

            // 🔄 [방향 정정] 홈팀(왼쪽 진영)이 자기 골대를 보고 있었다면 방향을 반대(왼쪽)로 꺾어줍니다!
            if (spawnedPlayer != null)
            {
                spawnedPlayer.transform.forward = Vector3.left;
            }
        }

        // 🔴 2. 어웨이팀 (상대 AI 팀) 11명 배치
        Transform[] awayPositions = awayFormationParent.GetComponentsInChildren<Transform>();
        for (int i = 1; i <= 11; i++)
        {
            if (i > awayPositions.Length - 1) break;

            GameObject spawnedAway = null;

            if (i == 1)
            {
                spawnedAway = Instantiate(awayFinalGK, awayPositions[i].position, awayPositions[i].rotation);
            }
            else
            {
                int prefabIndex = i - 2;
                if (prefabIndex < awayFinalPrefabs.Count && awayFinalPrefabs[prefabIndex] != null)
                {
                    spawnedAway = Instantiate(awayFinalPrefabs[prefabIndex], awayPositions[i].position, awayPositions[i].rotation);
                }
            }

            // 🔄 [방향 정정] 어웨이팀(오른쪽 진영) 역시 반대(오른쪽)를 바라보게 정렬합니다!
            if (spawnedAway != null)
            {
                spawnedAway.transform.forward = Vector3.right;
            }
        }
    }

    // 🎨 내 선수에게 피부색, 머리색, 축구화 마테리얼을 확실하게 주입하는 함수
    void ApplyCustomSkin(GameObject playerObj)
    {
        int bootsID = GameDataManager.Instance.currentEquippedBoots;
        int hairID = GameDataManager.Instance.currentEquippedHair;

        Renderer[] allRenderers = playerObj.GetComponentsInChildren<Renderer>(true);
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null) continue;

            string meshNameLower = renderer.gameObject.name.ToLower();

            // 👟 1. 축구화 메쉬 처리
            if (meshNameLower.Contains("shoes") || meshNameLower.Contains("boot"))
            {
                if (renderer is SkinnedMeshRenderer skinnedRenderer && bootsMaterials.Count > 0)
                {
                    int matIndex = bootsID - 1;
                    if (matIndex >= 0 && matIndex < bootsMaterials.Count && bootsMaterials[matIndex] != null)
                    {
                        skinnedRenderer.material = bootsMaterials[matIndex];
                    }
                }
                continue; 
            }

            // 💇 2. 머리카락 메쉬 처리
            if (meshNameLower.Contains("hair"))
            {
                if (hairColors.Count > 0 && hairID > 0)
                {
                    int colorIndex = hairID - 1;
                    if (colorIndex >= 0 && colorIndex < hairColors.Count)
                    {
                        renderer.material.color = hairColors[colorIndex];
                    }
                }
                else
                {
                    renderer.material.color = GameDataManager.Instance.selectedHairColor;
                }
                continue; 
            }

            // 👤 3. [Ch38_Body 멀티 마테리얼 정밀 타격 로직]
            // 발견하신 'Ch38_Body' 메쉬를 타겟팅합니다.
            if (meshNameLower.Contains("ch38_body") || meshNameLower == "body")
            {
                // 메쉬가 가진 마테리얼 배열을 가져옵니다 (보통 피부, 유니폼 등이 슬롯별로 나뉘어 있음)
                Material[] sharedMats = renderer.materials;

                for (int m = 0; m < sharedMats.Length; m++)
                {
                    if (sharedMats[m] == null) continue;

                    string matNameLower = sharedMats[m].name.ToLower();

                    // 🎯 마테리얼 이름 중에 옷(cloth, tops, bottoms, uniform)과 관련된 마테리얼 슬롯은 철저히 패스!
                    if (matNameLower.Contains("cloth") || matNameLower.Contains("top") || 
                        matNameLower.Contains("bottom") || matNameLower.Contains("suit") || 
                        matNameLower.Contains("uniform") || matNameLower.Contains("jersey"))
                    {
                        continue; 
                    }

                    // 🎯 옷이 아닌 슬롯(피부 슬롯)만 골라내어 텍스처를 밀고 유저 피부색 주입!
                    if (sharedMats[m].HasProperty("_MainTex"))
                    {
                        sharedMats[m].mainTexture = null;
                    }
                    sharedMats[m].color = GameDataManager.Instance.selectedSkinColor;
                }

                // 변경된 마테리얼 배열을 렌더러에 다시 덮어씌워 적용합니다.
                renderer.materials = sharedMats;
            }
        }

        // 이름표 생성
        CreateNameTag(playerObj);
    }

    void CreateNameTag(GameObject playerObj)
    {
        GameObject nameTagObj = new GameObject("PlayerNameTag");
        nameTagObj.transform.SetParent(playerObj.transform);
        nameTagObj.transform.localPosition = new Vector3(0, 2.4f, 0); 
        
        TMPro.TextMeshPro textMesh = nameTagObj.AddComponent<TMPro.TextMeshPro>();
        
        // 🎯 인스펙터 창에서 유저님이 할당해 줄 한글 폰트를 깔끔하게 적용합니다.
        if (koreanFont != null)
        {
            textMesh.font = koreanFont;
        }

        string finalName = "손흥민";
        if (GameDataManager.Instance != null && !string.IsNullOrEmpty(GameDataManager.Instance.playerCustomName))
        {
            finalName = GameDataManager.Instance.playerCustomName;
        }

        textMesh.text = $"<color=yellow>★</color> {finalName}";
        textMesh.fontSize = 5; 
        textMesh.alignment = TMPro.TextAlignmentOptions.Center; 
        nameTagObj.AddComponent<LookAtCamera>();
    }
}