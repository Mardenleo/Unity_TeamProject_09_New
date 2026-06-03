using UnityEngine;
using System.Collections.Generic;

public class InGameMatchDirector : MonoBehaviour
{
    public static InGameMatchDirector Instance { get; private set; }

    [Header("--- 팀별 선수 프리팹 리스트 ---")]
    public List<GameObject> homePlayerPrefabs = new List<GameObject>();
    public List<GameObject> awayPlayerPrefabs = new List<GameObject>();

    [Header("--- 턴제 선택 시퀀스 UI 마스터 그룹 ---")]
    public GameObject actionUIGroup; 

    [Header("--- 구역별 세부 UI 패널 ---")]
    public GameObject panelArea1and2;
    public GameObject panelArea3and4;
    public GameObject panelArea5;

    [Header("--- 🎥 카메라 오브젝트 연동 ---")]
    public GameObject camBroadCast;
    public GameObject camCloseUp;

    [Header("--- ⚽ 경기장 아웃 라인 제한 반경 ---")]
    public float fieldHalfLengthX = 150f;
    public float fieldHalfWidthZ = 100f;

    private GameObject currentPossessor = null; 
    private List<InGamePlayerAI> allPlayersInGame = new List<InGamePlayerAI>();
    private Transform ballTransform;
    private bool isBallOutOfBounds = false;
    
    // 버그 방지를 위해 항상 대문자로 통일하여 관리합니다. (RED 또는 BLUE)
    private string userTeamColor = "BLUE"; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CloseAllSequencePanels();
        
        if (camBroadCast != null) camBroadCast.SetActive(true);
        if (camCloseUp != null) camCloseUp.SetActive(false);

        GameObject ball = GameObject.FindWithTag("Ball");
        if(ball != null) ballTransform = ball.transform;

        AssignTeamsAndSpawnPlayers();
    }

    void Update()
    {
        if (Time.timeScale == 0f || ballTransform == null) return;

        CheckBallBounds();

        if (!isBallOutOfBounds) SetClosestPlayersToChaseBall();
        else ResetChasingFlags();
    }

    void CheckBallBounds()
    {
        Vector3 ballPos = ballTransform.position;
        if (Mathf.Abs(ballPos.x) > fieldHalfLengthX || Mathf.Abs(ballPos.z) > fieldHalfWidthZ)
        {
            if (!isBallOutOfBounds)
            {
                isBallOutOfBounds = true;
                Debug.LogWarning("⚽ 공이 아웃라인을 벗어났습니다. 추적을 중지합니다.");
            }
        }
        else
        {
            isBallOutOfBounds = false;
        }
    }

    void AssignTeamsAndSpawnPlayers()
    {
        GameObject homeObj = GameObject.Find("Home_Formation");
        GameObject awayObj = GameObject.Find("Away_Formation");

        string rawTeamData = "JMS";
        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedTeam != TeamType.None)
        {
            rawTeamData = GameDataManager.Instance.selectedTeam.ToString().ToUpper().Trim();
        }

        bool isHomeOurTeam = false;
        bool isAwayOurTeam = false;

        // 💡 [버그 수정] 대문자로 완벽히 변환하여 어떤 문자열이 들어와도 오작동하지 않게 고정
        if (rawTeamData.Contains("JMS") || rawTeamData.Contains("JMS"))
        {
            isHomeOurTeam = true;
            userTeamColor = "JMS";
            Debug.Log("★ 아군 설정 확정: Home_Formation (BLUE)");
        }
        else if (rawTeamData.Contains("KBC") || rawTeamData.Contains("KBC"))
        {
            isAwayOurTeam = true;
            userTeamColor = "KBC";
            Debug.Log("★ 아군 설정 확정: Away_Formation (RED)");
        }
        else
        {
            // 예외 상황 시 기본값 배정
            isHomeOurTeam = true; 
            userTeamColor = "JMS";
            Debug.LogWarning($"⚠️ 팀 판별 애매함 ({rawTeamData}). 기본 BLUE 팀으로 대체합니다.");
        }

        if (homeObj != null) SpawnPlayersAtFormation(homeObj.transform, isHomeOurTeam, homePlayerPrefabs, "player");
        if (awayObj != null) SpawnPlayersAtFormation(awayObj.transform, isAwayOurTeam, awayPlayerPrefabs, "Rplayer");
    }

    void SpawnPlayersAtFormation(Transform formationParent, bool isOurTeam, List<GameObject> prefabList, string namePrefix)
    {
        for (int i = 0; i < formationParent.childCount; i++)
        {
            Transform posTransform = formationParent.GetChild(i);
            if (posTransform == null || i >= prefabList.Count || prefabList[i] == null) continue;

            GameObject newPlayerObj = Instantiate(prefabList[i], posTransform.position, posTransform.rotation);
            newPlayerObj.transform.SetParent(posTransform);
            newPlayerObj.name = $"{namePrefix} ({i + 1})";

            InGamePlayerAI playerAI = newPlayerObj.GetComponent<InGamePlayerAI>();
            if (playerAI != null)
            {
                playerAI.isOurTeam = isOurTeam; 
                allPlayersInGame.Add(playerAI);
            }
        }
    }

    void SetClosestPlayersToChaseBall()
    {
        if (allPlayersInGame.Count == 0 || ballTransform == null) return;

        InGamePlayerAI closestOurTeam = null;
        InGamePlayerAI closestEnemyTeam = null;
        float minOurDistance = float.MaxValue;
        float minEnemyDistance = float.MaxValue;

        foreach (InGamePlayerAI player in allPlayersInGame)
        {
            if (player == null) continue;
            player.isChasingBall = false;

            float distance = Vector3.Distance(player.transform.position, ballTransform.position);
            if (player.isOurTeam)
            {
                if (distance < minOurDistance) { minOurDistance = distance; closestOurTeam = player; }
            }
            else
            {
                if (distance < minEnemyDistance) { minEnemyDistance = distance; closestEnemyTeam = player; }
            }
        }

        if (closestOurTeam != null) closestOurTeam.isChasingBall = true;
        if (closestEnemyTeam != null) closestEnemyTeam.isChasingBall = true;
    }

    public void ResetChasingFlags()
    {
        foreach (InGamePlayerAI player in allPlayersInGame)
        {
            if (player != null) player.isChasingBall = false;
        }
    }

    private void CloseAllSequencePanels()
    {
        if (actionUIGroup != null) actionUIGroup.SetActive(false);
        if (panelArea1and2 != null) panelArea1and2.SetActive(false);
        if (panelArea3and4 != null) panelArea3and4.SetActive(false);
        if (panelArea5 != null) panelArea5.SetActive(false);
    }

    public void TriggerSelectSequence(GameObject player, bool isAirBall = false)
    {
        currentPossessor = player;
        Time.timeScale = 0f; 

        CloseAllSequencePanels();

        if (camBroadCast != null) camBroadCast.SetActive(false);
        if (camCloseUp != null) 
        {
            camCloseUp.SetActive(true);
            camCloseUp.transform.position = player.transform.position + player.transform.forward * 4f + Vector3.up * 2.5f;
            camCloseUp.transform.LookAt(player.transform.position + Vector3.up * 1.2f);
        }

        if (actionUIGroup != null) actionUIGroup.SetActive(true);

        if (isAirBall)
        {
            if (panelArea5 != null) panelArea5.SetActive(true);
            return;
        }

        Vector3 playerPos = player.transform.position;
        float halfFieldX = 0f;
        float penaltyAreaSideZ = 15f;

        if (playerPos.x < halfFieldX)
        {
            if (panelArea1and2 != null) panelArea1and2.SetActive(true);
        }
        else
        {
            if (Mathf.Abs(playerPos.z) > penaltyAreaSideZ)
            {
                if (panelArea3and4 != null) panelArea3and4.SetActive(true);
            }
            else
            {
                if (panelArea1and2 != null) panelArea1and2.SetActive(true);
            }
        }
    }

    // 💡 [방향 연산] 대문자 매칭 구조로 완벽 안전 세팅
    public Vector3 GetTargetDirection(GameObject kicker, string targetType)
    {
        if (targetType == "Shoot" || targetType == "Cross")
        {
            Vector3 enemyGoalPos = Vector3.zero;

            // 대문자로 안전하게 체크합니다.
            if (userTeamColor == "BLUE")
            {
                enemyGoalPos = new Vector3(145f, 0f, 0f); 
            }
            else
            {
                enemyGoalPos = new Vector3(-145f, 0f, 0f);
            }

            Vector3 shootDir = (enemyGoalPos - kicker.transform.position);
            shootDir.y = 0f;
            return shootDir.normalized;
        }

        if (targetType == "Pass")
        {
            InGamePlayerAI closestTeammate = null;
            float minDistance = float.MaxValue;

            foreach (InGamePlayerAI player in allPlayersInGame)
            {
                if (player == null || player.gameObject == kicker || !player.isOurTeam) continue;

                float dist = Vector3.Distance(kicker.transform.position, player.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestTeammate = player;
                }
            }

            if (closestTeammate != null)
            {
                Vector3 passDir = (closestTeammate.transform.position - kicker.transform.position);
                passDir.y = 0f;
                return passDir.normalized;
            }
        }

        return kicker.transform.forward;
    }

    public void OnClickNormalPass() { ResumeGame(); if (currentPossessor != null) currentPossessor.GetComponent<InGamePlayerAI>()?.ExecutePass(); }
    public void OnClickNormalShoot() { ResumeGame(); if (currentPossessor != null) currentPossessor.GetComponent<InGamePlayerAI>()?.ExecuteShoot(); }
    public void OnClickDribble() { ResumeGame(); if (currentPossessor != null) currentPossessor.GetComponent<InGamePlayerAI>()?.ExecuteDribble(); }
    public void OnClickCross() { ResumeGame(); if (currentPossessor != null) currentPossessor.GetComponent<InGamePlayerAI>()?.ExecuteCross(); }
    public void OnClickShedding() { ResumeGame(); }
    public void OnClickHeadingPass() { ResumeGame(); if (currentPossessor != null) currentPossessor.GetComponent<InGamePlayerAI>()?.ExecuteHeadingPass(); }
    public void OnClickHeadingShoot() { ResumeGame(); if (currentPossessor != null) currentPossessor.GetComponent<InGamePlayerAI>()?.ExecuteHeadingShoot(); }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        CloseAllSequencePanels();
        isBallOutOfBounds = false;

        if (camBroadCast != null) camBroadCast.SetActive(true);
        if (camCloseUp != null) camCloseUp.SetActive(false);
    }
}