using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class InGameMatchDirector : MonoBehaviour
{
    public static InGameMatchDirector Instance { get; private set; }

    [Header("선수 프리팹")]
    public List<GameObject> homePlayerPrefabs = new List<GameObject>();
    public List<GameObject> awayPlayerPrefabs = new List<GameObject>();

    [Header("UI")]
    public GameObject actionUIGroup;
    public GameObject panelArea1and2;
    public GameObject panelArea3and4;
    public GameObject panelArea5;
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text resultText;
    public TMP_Text areaText;

    [Header("카메라")]
    public GameObject camBroadCast;
    public GameObject camCloseUp;

    [Header("필드 좌표")]
    public float fieldMinX = -35f;
    public float fieldMaxX = 105f;
    public float fieldMinZ = -115f;
    public float fieldMaxZ = -45f;
    public float playerY = 0f;
    public float ballY = 0.3f;

    [Header("경기 설정")]
    public float matchTime = 120f;
    public int currentArea = 1;
    public int area1SuccessCount = 0;
    public int ourScore = 0;
    public int enemyScore = 0;

    [Header("확률")]
    public int area1PassRate = 95;
    public int area1DribbleRate = 95;
    public int area1ShootRate = 8;
    public int area2PassRate = 70;
    public int area2DribbleRate = 70;
    public int area2ShootRate = 16;
    public int area34DribbleRate = 50;
    public int area34CrossRate = 50;
    public int area34ShootRate = 30;
    public int area5SheddingRate = 80;
    public int area5HeadingPassRate = 80;
    public int area5HeadingShootRate = 40;
    public int enemyCounterGoalRate = 10;

    private float currentTime;
    private bool isMatchOver = false;
    private bool isSequenceOpen = false;

    private string userTeam = "JMS";
    private float ourGoalX;
    private float enemyGoalX;
    private float centerZ;

    private Transform ballTransform;
    private Rigidbody ballRb;

    private readonly List<InGamePlayerAI> ourPlayers = new List<InGamePlayerAI>();
    private readonly List<InGamePlayerAI> enemyPlayers = new List<InGamePlayerAI>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentTime = matchTime;
        centerZ = (fieldMinZ + fieldMaxZ) * 0.5f;

        FindBall();
        SetupUserTeam();
        SpawnAllPlayers();
        MovePlayersByArea(1);

        CloseAllPanels();
        ShowBroadcastCamera();
        UpdateUI();

        StartCoroutine(KickOffRoutine("경기 시작!"));
    }

    private void Update()
    {
        if (isMatchOver) return;

        if (Time.timeScale > 0f)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0f)
            {
                currentTime = 0f;
                EndMatch();
            }
        }

        UpdateUI();
    }

    private void FindBall()
    {
        GameObject ball = GameObject.FindWithTag("Ball");

        if (ball != null)
        {
            ballTransform = ball.transform;
            ballRb = ball.GetComponent<Rigidbody>();
        }
    }

    private void SetupUserTeam()
    {
        userTeam = "JMS";

        if (GameDataManager.Instance != null &&
            GameDataManager.Instance.selectedTeam != TeamType.None)
        {
            userTeam = GameDataManager.Instance.selectedTeam.ToString().ToUpper().Trim();
        }

        if (userTeam.Contains("KBC"))
        {
            ourGoalX = fieldMinX;
            enemyGoalX = fieldMaxX;
        }
        else
        {
            ourGoalX = fieldMaxX;
            enemyGoalX = fieldMinX;
        }
    }

    private void SpawnAllPlayers()
    {
        GameObject homeObj = GameObject.Find("Home_Formation");
        GameObject awayObj = GameObject.Find("Away_Formation");

        bool homeIsOurTeam = userTeam.Contains("JMS");
        bool awayIsOurTeam = userTeam.Contains("KBC");

        if (homeObj != null)
            SpawnPlayers(homeObj.transform, homeIsOurTeam, homePlayerPrefabs, "player");

        if (awayObj != null)
            SpawnPlayers(awayObj.transform, awayIsOurTeam, awayPlayerPrefabs, "Rplayer");
    }

    private void SpawnPlayers(Transform parent, bool isOurTeam, List<GameObject> prefabs, string prefix)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            if (i >= prefabs.Count || prefabs[i] == null) continue;

            Transform point = parent.GetChild(i);
            GameObject obj = Instantiate(prefabs[i], point.position, point.rotation);
            obj.name = $"{prefix} ({i + 1})";

            InGamePlayerAI ai = obj.GetComponent<InGamePlayerAI>();
            if (ai == null) continue;

            ai.isOurTeam = isOurTeam;

            bool isMyHero =
                isOurTeam &&
                GameDataManager.Instance != null &&
                i + 1 == GameDataManager.Instance.selectedPlayerNumber;

            ai.InitStats(isMyHero);

            if (isOurTeam) ourPlayers.Add(ai);
            else enemyPlayers.Add(ai);
        }
    }

    private void OpenSequence()
    {
        if (isMatchOver) return;

        Time.timeScale = 0f;
        isSequenceOpen = true;

        ShowCloseUpCamera();
        ShowAreaPanel();
        UpdateUI();
    }

    private void CloseSequence()
    {
        Time.timeScale = 1f;
        isSequenceOpen = false;
        CloseAllPanels();
        ShowBroadcastCamera();
    }

    private void ShowAreaPanel()
    {
        CloseAllPanels();
        actionUIGroup?.SetActive(true);

        if (currentArea == 1 || currentArea == 2)
            panelArea1and2?.SetActive(true);
        else if (currentArea == 3 || currentArea == 4)
            panelArea3and4?.SetActive(true);
        else if (currentArea == 5)
            panelArea5?.SetActive(true);
    }

    private void CloseAllPanels()
    {
        actionUIGroup?.SetActive(false);
        panelArea1and2?.SetActive(false);
        panelArea3and4?.SetActive(false);
        panelArea5?.SetActive(false);
    }

    private bool Roll(int rate)
    {
        return Random.Range(1, 101) <= rate;
    }

    private void StartAction(string actionName, bool success, int nextArea, bool isGoal = false)
    {
        if (!isSequenceOpen || isMatchOver) return;
        StartCoroutine(ActionRoutine(actionName, success, nextArea, isGoal));
    }

    private IEnumerator ActionRoutine(string actionName, bool success, int nextArea, bool isGoal)
    {
        CloseSequence();
        SetResult($"{actionName} 시도!");

        PlayPossessorAnimation(actionName);

        yield return new WaitForSeconds(1.2f);

        if (success)
        {
            if (isGoal)
            {
                ourScore++;
                yield return StartCoroutine(KickOffRoutine("GOAL! 득점 성공!"));
                yield break;
            }
            else
            {
                currentArea = nextArea;
                MovePlayersByArea(currentArea);
                SetResult($"{actionName} 성공! Area {currentArea}");
            }

            yield return new WaitForSeconds(0.8f);
            OpenSequence();
        }
        else
        {
            SetResult($"{actionName} 실패! 상대 역습!");
            yield return StartCoroutine(CounterAttackRoutine());
        }
    }

    private IEnumerator CounterAttackRoutine()
    {
        CloseSequence();

        MovePlayersToCounterAttack();

        yield return new WaitForSeconds(1.5f);

        if (Roll(enemyCounterGoalRate))
        {
            enemyScore++;
            yield return StartCoroutine(KickOffRoutine("상대 역습 성공! 실점했습니다."));
            yield break;
        }
        else
        {
            SetResult("상대 역습을 막았습니다!");
        }

        ResetAttack();
        MovePlayersByArea(1);

        yield return new WaitForSeconds(1f);
        OpenSequence();
    }

    private void ResetAttack()
    {
        currentArea = 1;
        area1SuccessCount = 0;
    }

    private void MovePlayersByArea(int area)
    {
        for (int i = 0; i < ourPlayers.Count; i++)
        {
            Vector3 pos = GetOurAreaPosition(i, area);
            ourPlayers[i].SetTacticalMoveTarget(pos, Quaternion.identity);
        }

        for (int i = 0; i < enemyPlayers.Count; i++)
        {
            Vector3 pos = GetEnemyAreaPosition(i, area);
            enemyPlayers[i].SetTacticalMoveTarget(pos, Quaternion.identity);
        }

        MoveBallToArea(area);
    }

    private void MovePlayersToCounterAttack()
    {
        for (int i = 0; i < ourPlayers.Count; i++)
        {
            Vector3 pos = GetCounterOurPosition(i);
            ourPlayers[i].SetTacticalMoveTarget(pos, Quaternion.identity);
        }

        for (int i = 0; i < enemyPlayers.Count; i++)
        {
            Vector3 pos = GetCounterEnemyPosition(i);
            enemyPlayers[i].SetTacticalMoveTarget(pos, Quaternion.identity);
        }

        MoveBallToCounter();
    }

    private void MoveBallToArea(int area)
    {
        if (ballTransform == null) return;

        float progress = .45f;
        float zOffset = 0f;

        if (area == 1)
        {
            progress = .45f;
            zOffset = 0f;
        }
        else if (area == 2)
        {
            progress = .55f;
            zOffset = 0f;
        }
        else if (area == 3)
        {
            progress = .82f;
            zOffset = 18f;
        }
        else if (area == 4)
        {
            progress = .82f;
            zOffset = -18f;
        }
        else if (area == 5)
        {
            progress = .88f;
            zOffset = -5f;
        }

        Vector3 targetPos = GetPointFromOurGoal(progress, zOffset);
        targetPos.y = ballY;

        StopCoroutine(nameof(SmoothMoveBallRoutine));
        StartCoroutine(SmoothMoveBallRoutine(targetPos));
    }

    private Vector3 GetPointFromOurGoal(float progress, float zOffset)
    {
        float x = Mathf.Lerp(ourGoalX, enemyGoalX, progress);
        float z = Mathf.Clamp(centerZ + zOffset, fieldMinZ + 5f, fieldMaxZ - 5f);
        return new Vector3(x, playerY, z);
    }

    private Vector3 GetPointFromEnemyGoal(float progress, float zOffset)
    {
        float x = Mathf.Lerp(enemyGoalX, ourGoalX, progress);
        float z = Mathf.Clamp(centerZ + zOffset, fieldMinZ + 5f, fieldMaxZ - 5f);
        return new Vector3(x, playerY, z);
    }

    private Vector3 GetOurAreaPosition(int index, int area)
    {
        int i = Mathf.Clamp(index, 0, 10);

        // 0 GK / 1~4 DF / 5~8 MF / 9~10 FW
        float[] z = { 0, -14, -5, 5, 14, -11, -4, 4, 11, -5, 5 };

        float[] a1 = { .06f, .18f, .18f, .18f, .18f, .36f, .40f, .44f, .48f, .58f, .62f };
        float[] a2 = { .06f, .24f, .24f, .24f, .24f, .48f, .52f, .56f, .60f, .70f, .74f };
        float[] a3 = { .06f, .25f, .25f, .25f, .25f, .50f, .56f, .62f, .70f, .80f, .86f };
        float[] a4 = { .06f, .25f, .25f, .25f, .25f, .50f, .56f, .62f, .70f, .80f, .86f };
        float[] a5 = { .08f, .28f, .28f, .28f, .28f, .50f, .56f, .62f, .68f, .78f, .82f };

        float progress = a1[i];
        if (area == 2) progress = a2[i];
        else if (area == 3) progress = a3[i];
        else if (area == 4) progress = a4[i];
        else if (area == 5) progress = a5[i];

        float sideBias = 0f;
        if (area == 3) sideBias = 10f;
        if (area == 4) sideBias = -10f;

        return GetPointFromOurGoal(1f - progress, z[i] + sideBias);
    }

    private Vector3 GetEnemyAreaPosition(int index, int area)
    {
        int i = Mathf.Clamp(index, 0, 10);

        float[] z = { 0, -14, -5, 5, 14, -11, -4, 4, 11, -5, 5 };

        // 상대는 우리 공격이 전진할수록 자기 골대 앞에 더 내려앉아야 함
        float[] e1 = { .94f, .80f, .80f, .80f, .80f, .68f, .64f, .60f, .56f, .48f, .44f };
        float[] e2 = { .94f, .84f, .84f, .84f, .84f, .72f, .68f, .64f, .60f, .52f, .48f };
        float[] e34 = { .96f, .88f, .88f, .88f, .88f, .78f, .74f, .70f, .66f, .58f, .54f };
        float[] e5 = { .94f, .86f, .86f, .86f, .86f, .74f, .70f, .68f, .66f, .58f, .56f };

        float progress = e1[i];
        if (area == 2) progress = e2[i];
        else if (area == 3 || area == 4) progress = e34[i];
        else if (area == 5) progress = e5[i];

        float sideBias = 0f;
        if (area == 3) sideBias = 8f;
        if (area == 4) sideBias = -8f;

        return GetPointFromOurGoal(progress, z[i] + sideBias);
    }

    private Vector3 GetCounterOurPosition(int index)
    {
        float[] z = { 0, -24, -10, 10, 24, -18, -6, 6, 18, -10, 10 };
        float[] p = { .10f, .18f, .18f, .18f, .18f, .30f, .33f, .35f, .35f, .42f, .45f };
        return GetPointFromOurGoal(p[Mathf.Clamp(index, 0, 10)], z[Mathf.Clamp(index, 0, 10)]);
    }

    private Vector3 GetCounterEnemyPosition(int index)
    {
        float[] z = { 0, -24, -10, 10, 24, -18, -6, 6, 18, -10, 10 };
        float[] p = { .08f, .25f, .25f, .25f, .25f, .45f, .55f, .60f, .65f, .75f, .80f };
        return GetPointFromEnemyGoal(p[Mathf.Clamp(index, 0, 10)], z[Mathf.Clamp(index, 0, 10)]);
    }

    private void MoveBallToKickOff()
    {
        if (ballTransform == null) return;

        Vector3 centerPos = new Vector3(
            (fieldMinX + fieldMaxX) * 0.5f,
            ballY,
            (fieldMinZ + fieldMaxZ) * 0.5f
        );

        StopCoroutine(nameof(SmoothMoveBallRoutine));
        StartCoroutine(SmoothMoveBallRoutine(centerPos));
    }

    private IEnumerator KickOffRoutine(string message)
    {
        SetResult(message);

        currentArea = 1;
        area1SuccessCount = 0;

        MovePlayersByArea(1);
        MoveBallToKickOff();

        yield return new WaitForSeconds(2f);

        SetResult("킥오프!");

        yield return new WaitForSeconds(0.8f);

        MoveBallToArea(1);

        yield return new WaitForSeconds(0.8f);

        OpenSequence();
    }

    private IEnumerator SmoothMoveBallRoutine(Vector3 targetPos)
    {
        if (ballTransform == null) yield break;

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
        }

        Vector3 startPos = ballTransform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 2.2f;
            ballTransform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        ballTransform.position = targetPos;
    }

    private void MoveBallToCounter()
    {
        if (ballTransform == null) return;

        Vector3 pos = GetPointFromEnemyGoal(.55f, 0f);
        pos.y = ballY;

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
        }

        ballTransform.position = pos;
    }

    private void PlayPossessorAnimation(string actionName)
    {
        InGamePlayerAI actor = GetMainOurPlayer();
        if (actor == null) return;

        if (actionName.Contains("헤딩 패스")) actor.ExecuteHeadingPass();
        else if (actionName.Contains("헤딩 슈팅")) actor.ExecuteHeadingShoot();
        else if (actionName.Contains("크로스")) actor.ExecuteCross();
        else if (actionName.Contains("패스")) actor.ExecutePass();
        else if (actionName.Contains("드리블")) actor.ExecuteDribble();
        else if (actionName.Contains("슈팅")) actor.ExecuteShoot();
    }

    private InGamePlayerAI GetMainOurPlayer()
    {
        if (ourPlayers.Count == 0) return null;

        int selected = 0;

        if (GameDataManager.Instance != null)
            selected = Mathf.Clamp(GameDataManager.Instance.selectedPlayerNumber - 1, 0, ourPlayers.Count - 1);

        return ourPlayers[selected];
    }

    private void ShowBroadcastCamera()
    {
        /*
        if (camBroadCast != null)
            camBroadCast.SetActive(true);

        if (camCloseUp != null)
            camCloseUp.SetActive(false);
        */
    }

    private void ShowCloseUpCamera()
    {
        /*
        if (camBroadCast != null)
            camBroadCast.SetActive(false);

        if (camCloseUp != null)
            camCloseUp.SetActive(true);
        */

    }

    private void SetResult(string msg)
    {
        if (resultText != null) resultText.text = msg;
        Debug.Log(msg);
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"{ourScore} : {enemyScore}";

        if (areaText != null)
            areaText.text = $"Area {currentArea}";

        if (timerText != null)
        {
            int min = Mathf.FloorToInt(currentTime / 60f);
            int sec = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{min:00}:{sec:00}";
        }
    }

    private void EndMatch()
    {
        isMatchOver = true;
        Time.timeScale = 1f;
        CloseAllPanels();

        if (ourScore > enemyScore) SetResult("경기 종료! 승리!");
        else if (ourScore < enemyScore) SetResult("경기 종료! 패배!");
        else SetResult("경기 종료! 무승부!");
    }

    public void OnClickNormalPass()
    {
        if (currentArea == 1)
        {
            if (Roll(area1PassRate))
            {
                area1SuccessCount++;
                int next = area1SuccessCount >= 2 ? 2 : 1;
                if (next == 2) area1SuccessCount = 0;
                StartAction("패스", true, next);
            }
            else StartAction("패스", false, 1);
        }
        else if (currentArea == 2)
        {
            StartAction("패스", Roll(area2PassRate), 3);
        }
    }

    public void OnClickDribble()
    {
        if (currentArea == 1)
        {
            if (Roll(area1DribbleRate))
            {
                area1SuccessCount++;
                int next = area1SuccessCount >= 2 ? 2 : 1;
                if (next == 2) area1SuccessCount = 0;
                StartAction("드리블", true, next);
            }
            else StartAction("드리블", false, 1);
        }
        else if (currentArea == 2)
        {
            StartAction("드리블", Roll(area2DribbleRate), 4);
        }
        else if (currentArea == 3 || currentArea == 4)
        {
            StartAction("드리블", Roll(area34DribbleRate), currentArea);
        }
    }

    public void OnClickNormalShoot()
    {
        int rate = 0;

        if (currentArea == 1) rate = area1ShootRate;
        else if (currentArea == 2) rate = area2ShootRate;
        else if (currentArea == 3 || currentArea == 4) rate = area34ShootRate;

        StartAction("슈팅", Roll(rate), 1, true);
    }

    public void OnClickCross()
    {
        if (currentArea == 3 || currentArea == 4)
            StartAction("크로스", Roll(area34CrossRate), 5);
    }

    public void OnClickShedding()
    {
        if (currentArea == 5)
            StartAction("흘리기", Roll(area5SheddingRate), 2);
    }

    public void OnClickHeadingPass()
    {
        if (currentArea == 5)
            StartAction("헤딩 패스", Roll(area5HeadingPassRate), 5);
    }

    public void OnClickHeadingShoot()
    {
        if (currentArea == 5)
            StartAction("헤딩 슈팅", Roll(area5HeadingShootRate), 1, true);
    }

    public Vector3 GetTargetDirection(GameObject kicker, string targetType)
    {
        if (kicker == null) return Vector3.forward;

        Vector3 target = new Vector3(enemyGoalX, kicker.transform.position.y, centerZ);
        Vector3 dir = target - kicker.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return kicker.transform.forward;

        return dir.normalized;
    }

    public void ResetChasingFlags()
    {
        foreach (InGamePlayerAI p in ourPlayers)
            if (p != null) p.isChasingBall = false;

        foreach (InGamePlayerAI p in enemyPlayers)
            if (p != null) p.isChasingBall = false;
    }

    public void SetClosestPlayersToChaseBall() { }

    public void TriggerSelectSequence(GameObject player, bool isAirBall = false)
    {
        if (isSequenceOpen || isMatchOver) return;
        OpenSequence();
    }
}
