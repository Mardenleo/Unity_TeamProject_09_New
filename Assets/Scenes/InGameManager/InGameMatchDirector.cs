using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class InGameMatchDirector : MonoBehaviour
{
    public static InGameMatchDirector Instance { get; private set; }

    private const string ActionPass = "PASS";
    private const string ActionDribble = "DRIBBLE";
    private const string ActionShoot = "SHOOT";
    private const string ActionCross = "CROSS";
    private const string ActionShedding = "SHEDDING";
    private const string ActionHeadingPass = "HEADING_PASS";
    private const string ActionHeadingShoot = "HEADING_SHOOT";

    [Header("Player Prefabs")]
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

    [Header("Camera")]
    public GameObject camBroadCast;
    public GameObject camCloseUp;

    [Header("Field")]
    public float fieldMinX = -35f;
    public float fieldMaxX = 105f;
    public float fieldMinZ = -115f;
    public float fieldMaxZ = -45f;
    public float playerY = 0f;
    public float ballY = 0.3f;

    [Header("Field Direction")]
    public bool ourTeamDefendsLeftGoal = true;

    [Header("Match Time")]
    public float realMatchDuration = 120f;
    public float displayMatchMinutes = 90f;

    [Header("Scene")]
    public string matchResultSceneName = "MatchResult";

    [Header("Match State")]
    public int currentArea = 1;
    public int area1SuccessCount = 0;
    public int ourScore = 0;
    public int enemyScore = 0;

    [Header("Rates")]
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

    private float currentTime = 0f;
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

    private struct AreaBallProfile
    {
        public float progress;
        public float lane;

        public AreaBallProfile(float progress, float lane)
        {
            this.progress = progress;
            this.lane = lane;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Time.timeScale = 1f;
        currentTime = 0f;
        centerZ = (fieldMinZ + fieldMaxZ) * 0.5f;

        FindBall();
        SetupUserTeam();
        SpawnAllPlayers();
        MovePlayersByArea(1);

        CloseAllPanels();
        ShowBroadcastCamera();
        UpdateUI();

        StartCoroutine(KickOffRoutine("Match Start"));
    }

    private void Update()
    {
        if (isMatchOver) return;

        currentTime += Time.unscaledDeltaTime;

        if (currentTime >= realMatchDuration)
        {
            currentTime = realMatchDuration;
            EndMatch();
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

        if (ourTeamDefendsLeftGoal)
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
        SetResult($"{actionName} Try");

        PlayPossessorAnimation(actionName);

        yield return new WaitForSeconds(1.2f);

        if (success)
        {
            if (isGoal)
            {
                ourScore++;
                yield return StartCoroutine(KickOffRoutine("GOAL"));
                yield break;
            }

            currentArea = nextArea;
            MovePlayersByArea(currentArea);
            SetResult($"{actionName} Success - Area {currentArea}");

            yield return new WaitForSeconds(0.8f);
            OpenSequence();
        }
        else
        {
            SetResult($"{actionName} Failed - Counter Attack");
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
            yield return StartCoroutine(KickOffRoutine("Enemy Counter Goal"));
            yield break;
        }

        SetResult("Counter Attack Stopped");

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

    private AreaBallProfile GetAreaBallProfile(int area)
    {
        switch (area)
        {
            case 1: return new AreaBallProfile(0.45f, 0f);
            case 2: return new AreaBallProfile(0.58f, 0f);
            case 3: return new AreaBallProfile(0.82f, 18f);
            case 4: return new AreaBallProfile(0.82f, -18f);
            case 5: return new AreaBallProfile(0.90f, 0f);
            default: return new AreaBallProfile(0.45f, 0f);
        }
    }

    private Vector3 GetAreaBallPosition(int area)
    {
        AreaBallProfile profile = GetAreaBallProfile(area);
        Vector3 pos = GetPointFromOurGoal(profile.progress, profile.lane);
        pos.y = ballY;
        return pos;
    }

    private void MoveBallToArea(int area)
    {
        if (ballTransform == null) return;

        Vector3 targetPos = GetAreaBallPosition(area);

        StopCoroutine(nameof(SmoothMoveBallRoutine));
        StartCoroutine(SmoothMoveBallRoutine(targetPos));
    }

    private Vector3 GetOurAreaPosition(int index, int area)
    {
        int i = Mathf.Clamp(index, 0, 10);
        AreaBallProfile ball = GetAreaBallProfile(area);

        float side = ball.lane >= 0f ? 1f : -1f;
        float progress = ball.progress;
        float lane = 0f;

        switch (i)
        {
            case 0:
                progress = 0.08f;
                lane = 0f;
                break;

            case 1:
                progress = ball.progress - 0.32f;
                lane = -22f;
                break;
            case 2:
                progress = ball.progress - 0.34f;
                lane = -7f;
                break;
            case 3:
                progress = ball.progress - 0.34f;
                lane = 7f;
                break;
            case 4:
                progress = ball.progress - 0.32f;
                lane = 22f;
                break;

            case 5:
                progress = ball.progress - 0.05f;
                lane = side * 24f;
                break;
            case 6:
                progress = ball.progress - 0.16f;
                lane = -7f;
                break;
            case 7:
                progress = ball.progress - 0.14f;
                lane = 7f;
                break;
            case 8:
                progress = ball.progress - 0.10f;
                lane = -side * 20f;
                break;

            case 9:
                progress = ball.progress + 0.07f;
                lane = side * 5f;
                break;
            case 10:
                progress = ball.progress + 0.09f;
                lane = -side * 6f;
                break;
        }

        if (area == 5)
        {
            if (i == 9)
            {
                progress = 0.90f;
                lane = -5f;
            }
            else if (i == 10)
            {
                progress = 0.92f;
                lane = 6f;
            }
            else if (i >= 5 && i <= 8)
            {
                progress = Mathf.Min(progress, 0.76f);
            }
        }

        progress = ClampProgressForOurRole(i, progress);
        return GetPointFromOurGoal(progress, lane);
    }

    private Vector3 GetEnemyAreaPosition(int index, int area)
    {
        int i = Mathf.Clamp(index, 0, 10);
        AreaBallProfile ball = GetAreaBallProfile(area);

        float side = ball.lane >= 0f ? 1f : -1f;
        float progress = ball.progress;
        float lane = 0f;

        switch (i)
        {
            case 0:
                progress = 0.94f;
                lane = 0f;
                break;

            case 1:
                progress = ball.progress + 0.10f;
                lane = -22f;
                break;
            case 2:
                progress = ball.progress + 0.08f;
                lane = -7f;
                break;
            case 3:
                progress = ball.progress + 0.08f;
                lane = 7f;
                break;
            case 4:
                progress = ball.progress + 0.10f;
                lane = 22f;
                break;

            case 5:
                progress = ball.progress - 0.03f;
                lane = side * 22f;
                break;
            case 6:
                progress = ball.progress - 0.05f;
                lane = -7f;
                break;
            case 7:
                progress = ball.progress - 0.05f;
                lane = 7f;
                break;
            case 8:
                progress = ball.progress - 0.03f;
                lane = -side * 20f;
                break;

            case 9:
                progress = ball.progress - 0.28f;
                lane = -8f;
                break;
            case 10:
                progress = ball.progress - 0.26f;
                lane = 8f;
                break;
        }

        if (area == 5)
        {
            if (i >= 1 && i <= 4)
                progress = 0.84f;

            if (i >= 5 && i <= 8)
                progress = 0.76f;
        }

        progress = ClampProgressForEnemyRole(i, progress);
        return GetPointFromOurGoal(progress, lane);
    }

    private float ClampProgressForOurRole(int index, float progress)
    {
        if (index == 0) return Mathf.Clamp(progress, 0.06f, 0.10f);
        if (index <= 4) return Mathf.Clamp(progress, 0.16f, 0.42f);
        if (index <= 8) return Mathf.Clamp(progress, 0.30f, 0.76f);
        return Mathf.Clamp(progress, 0.52f, 0.94f);
    }

    private float ClampProgressForEnemyRole(int index, float progress)
    {
        if (index == 0) return Mathf.Clamp(progress, 0.92f, 0.96f);
        if (index <= 4) return Mathf.Clamp(progress, 0.62f, 0.92f);
        if (index <= 8) return Mathf.Clamp(progress, 0.38f, 0.82f);
        return Mathf.Clamp(progress, 0.18f, 0.62f);
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

    private Vector3 GetCounterOurPosition(int index)
    {
        int i = Mathf.Clamp(index, 0, 10);
        float[] z = { 0, -24, -10, 10, 24, -18, -6, 6, 18, -10, 10 };
        float[] p = { .10f, .18f, .18f, .18f, .18f, .30f, .33f, .35f, .35f, .42f, .45f };
        return GetPointFromOurGoal(p[i], z[i]);
    }

    private Vector3 GetCounterEnemyPosition(int index)
    {
        int i = Mathf.Clamp(index, 0, 10);
        float[] z = { 0, -24, -10, 10, 24, -18, -6, 6, 18, -10, 10 };
        float[] p = { .08f, .25f, .25f, .25f, .25f, .45f, .55f, .60f, .65f, .75f, .80f };
        return GetPointFromEnemyGoal(p[i], z[i]);
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

        SetResult("Kick Off");

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

        if (actionName == ActionHeadingPass) actor.ExecuteHeadingPass();
        else if (actionName == ActionHeadingShoot) actor.ExecuteHeadingShoot();
        else if (actionName == ActionCross) actor.ExecuteCross();
        else if (actionName == ActionPass) actor.ExecutePass();
        else if (actionName == ActionDribble) actor.ExecuteDribble();
        else if (actionName == ActionShoot) actor.ExecuteShoot();
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
        if (camBroadCast != null) camBroadCast.SetActive(true);
        if (camCloseUp != null) camCloseUp.SetActive(false);
    }

    private void ShowCloseUpCamera()
    {
        if (camBroadCast != null) camBroadCast.SetActive(false);
        if (camCloseUp != null) camCloseUp.SetActive(true);
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
            float progress = realMatchDuration <= 0f ? 0f : currentTime / realMatchDuration;
            progress = Mathf.Clamp01(progress);

            float displayTime = progress * displayMatchMinutes * 60f;

            int min = Mathf.FloorToInt(displayTime / 60f);
            int sec = Mathf.FloorToInt(displayTime % 60f);

            timerText.text = $"{min:00}:{sec:00}";
        }
    }

    private void EndMatch()
    {
        if (isMatchOver) return;

        isMatchOver = true;
        Time.timeScale = 1f;
        CloseAllPanels();

        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SaveMatchResult(ourScore, enemyScore);

        SceneManager.LoadScene(matchResultSceneName);
    }

    public void OnClickNormalPass()
    {
        if (currentArea == 1)
        {
            if (Roll(area1PassRate))
            {
                area1SuccessCount++;
                int next = area1SuccessCount >= 2 ? 2 : 1;

                if (next == 2)
                    area1SuccessCount = 0;

                StartAction(ActionPass, true, next);
            }
            else
            {
                StartAction(ActionPass, false, 1);
            }
        }
        else if (currentArea == 2)
        {
            StartAction(ActionPass, Roll(area2PassRate), 3);
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

                if (next == 2)
                    area1SuccessCount = 0;

                StartAction(ActionDribble, true, next);
            }
            else
            {
                StartAction(ActionDribble, false, 1);
            }
        }
        else if (currentArea == 2)
        {
            StartAction(ActionDribble, Roll(area2DribbleRate), 4);
        }
        else if (currentArea == 3 || currentArea == 4)
        {
            StartAction(ActionDribble, Roll(area34DribbleRate), currentArea);
        }
    }

    public void OnClickNormalShoot()
    {
        int rate = 0;

        if (currentArea == 1) rate = area1ShootRate;
        else if (currentArea == 2) rate = area2ShootRate;
        else if (currentArea == 3 || currentArea == 4) rate = area34ShootRate;

        StartAction(ActionShoot, Roll(rate), 1, true);
    }

    public void OnClickCross()
    {
        if (currentArea == 3 || currentArea == 4)
            StartAction(ActionCross, Roll(area34CrossRate), 5);
    }

    public void OnClickShedding()
    {
        if (currentArea == 5)
            StartAction(ActionShedding, Roll(area5SheddingRate), 2);
    }

    public void OnClickHeadingPass()
    {
        if (currentArea == 5)
            StartAction(ActionHeadingPass, Roll(area5HeadingPassRate), 5);
    }

    public void OnClickHeadingShoot()
    {
        if (currentArea == 5)
            StartAction(ActionHeadingShoot, Roll(area5HeadingShootRate), 1, true);
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

    public void SetClosestPlayersToChaseBall()
    {
    }

    public void TriggerSelectSequence(GameObject player, bool isAirBall = false)
    {
        if (isSequenceOpen || isMatchOver) return;
        OpenSequence();
    }
}