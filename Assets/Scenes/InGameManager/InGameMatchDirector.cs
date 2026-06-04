using UnityEngine;
using System.Collections.Generic;

public class InGameMatchDirector : MonoBehaviour
{
    public static InGameMatchDirector Instance { get; private set; }

    [Header("팀별 선수 프리팹")]
    public List<GameObject> homePlayerPrefabs = new List<GameObject>();
    public List<GameObject> awayPlayerPrefabs = new List<GameObject>();

    [Header("UI")]
    public GameObject actionUIGroup;
    public GameObject panelArea1and2;
    public GameObject panelArea3and4;
    public GameObject panelArea5;

    [Header("Camera")]
    public Camera mainGameCamera;
    private Camera closeUpCamera;

    [Header("Blue Out Line Bounds")]
    public float outMinX = -25f;
    public float outMaxX = 95.6f;
    public float outMinZ = -40f;
    public float outMaxZ = 28f;

    [Header("Goal X")]
    public float leftGoalX = -25.0f;
    public float rightGoalX = 95.62f;

    [Header("Out Wall")]
    public bool useOutBounce = true;
    public bool createOutWalls = true;
    public bool showOutWalls = true;
    public float wallHeight = 20f;
    public float wallThickness = 2f;
    public float outBouncePower = 18f;
    public PhysicsMaterial ballBounceMaterial;

    [Header("Sequence")]
    public float sequenceCooldown = 1.2f;

    private string userTeam = "JMS";
    private float ourGoalX;
    private float enemyGoalX;

    private GameObject currentPossessor;
    private Transform ballTransform;
    private Rigidbody ballRb;

    private readonly List<InGamePlayerAI> allPlayersInGame = new List<InGamePlayerAI>();

    private bool isSequencePlaying = false;
    private float lastSequenceTime = -999f;

    private float fieldCenterX;
    private float fieldCenterZ;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        fieldCenterX = (outMinX + outMaxX) * 0.5f;
        fieldCenterZ = (outMinZ + outMaxZ) * 0.5f;
    }

    private void Start()
    {
        CloseAllSequencePanels();

        if (mainGameCamera == null)
            mainGameCamera = Camera.main;

        CreateCloseUpCamera();
        FindBall();
        SetupUserTeamDirection();

        if (createOutWalls)
            CreateOutWalls();

        SpawnAllPlayers();
    }

    private void FixedUpdate()
    {
        if (useOutBounce)
            CheckBallOutAndBounce();
    }

    private void Update()
    {
        if (ballTransform == null) return;

        if (!isSequencePlaying && Time.timeScale > 0f)
            SetClosestPlayersToChaseBall();
    }

    private void FindBall()
    {
        GameObject ball = GameObject.FindWithTag("Ball");

        if (ball == null)
        {
            Debug.LogError("Ball 태그가 붙은 공을 찾지 못했습니다.");
            return;
        }

        ballTransform = ball.transform;
        ballRb = ball.GetComponent<Rigidbody>();

        if (ballRb == null)
            Debug.LogError("Ball에 Rigidbody가 없습니다.");
    }

    private void SetupUserTeamDirection()
    {
        userTeam = "JMS";

        if (GameDataManager.Instance != null &&
            GameDataManager.Instance.selectedTeam != TeamType.None)
        {
            userTeam = GameDataManager.Instance.selectedTeam.ToString().ToUpper().Trim();
        }

        if (userTeam.Contains("KBC"))
        {
            ourGoalX = leftGoalX;
            enemyGoalX = rightGoalX;
            Debug.Log("KBC 선택: 왼쪽 우리 골대 → 오른쪽 공격");
        }
        else
        {
            ourGoalX = rightGoalX;
            enemyGoalX = leftGoalX;
            Debug.Log("JMS 선택: 오른쪽 우리 골대 → 왼쪽 공격");
        }
    }

    private void SpawnAllPlayers()
    {
        GameObject homeObj = GameObject.Find("Home_Formation");
        GameObject awayObj = GameObject.Find("Away_Formation");

        bool homeIsOurTeam = userTeam.Contains("JMS");
        bool awayIsOurTeam = userTeam.Contains("KBC");

        if (homeObj != null)
            SpawnPlayersAtFormation(homeObj.transform, homeIsOurTeam, homePlayerPrefabs, "player");

        if (awayObj != null)
            SpawnPlayersAtFormation(awayObj.transform, awayIsOurTeam, awayPlayerPrefabs, "Rplayer");
    }

    private void SpawnPlayersAtFormation(
        Transform formationParent,
        bool isOurTeam,
        List<GameObject> prefabList,
        string namePrefix)
    {
        for (int i = 0; i < formationParent.childCount; i++)
        {
            if (i >= prefabList.Count || prefabList[i] == null) continue;

            Transform pos = formationParent.GetChild(i);

            GameObject obj = Instantiate(prefabList[i], pos.position, pos.rotation);
            obj.transform.SetParent(pos);
            obj.name = $"{namePrefix} ({i + 1})";

            InGamePlayerAI ai = obj.GetComponent<InGamePlayerAI>();
            if (ai == null) continue;

            ai.isOurTeam = isOurTeam;
            allPlayersInGame.Add(ai);

            bool isMyHero =
                isOurTeam &&
                GameDataManager.Instance != null &&
                i + 1 == GameDataManager.Instance.selectedPlayerNumber;

            ai.InitStats(isMyHero);
        }
    }

    public void TriggerSelectSequence(GameObject player, bool isAirBall = false)
    {
        if (isSequencePlaying) return;
        if (Time.unscaledTime - lastSequenceTime < sequenceCooldown) return;
        if (player == null) return;

        InGamePlayerAI ai = player.GetComponent<InGamePlayerAI>();
        if (ai == null || !ai.isOurTeam) return;

        currentPossessor = player;
        isSequencePlaying = true;
        lastSequenceTime = Time.unscaledTime;

        Time.timeScale = 0f;

        ResetChasingFlags();
        CloseAllSequencePanels();

        ShowCloseUp(player);

        actionUIGroup?.SetActive(true);

        int area = GetAreaNumber(player.transform.position);
        Debug.Log($"현재 구역: {area}");

        if (isAirBall || area == 5)
            panelArea5?.SetActive(true);
        else if (area == 3 || area == 4)
            panelArea3and4?.SetActive(true);
        else
            panelArea1and2?.SetActive(true);
    }

    private int GetAreaNumber(Vector3 worldPos)
    {
        fieldCenterX = (outMinX + outMaxX) * 0.5f;
        fieldCenterZ = (outMinZ + outMaxZ) * 0.5f;

        if (Mathf.Abs(worldPos.x - fieldCenterX) < 15f)
            return 1;

        float totalAttackDistance = Mathf.Abs(enemyGoalX - ourGoalX);
        float progressed = Mathf.Abs(worldPos.x - ourGoalX) / totalAttackDistance;

        float topWingZ = fieldCenterZ + 23f;
        float bottomWingZ = fieldCenterZ - 23f;

        if (progressed < 0.55f)
            return 1;

        if (progressed < 0.82f)
        {
            if (worldPos.z > topWingZ) return 3;
            if (worldPos.z < bottomWingZ) return 4;
            return 2;
        }

        if (worldPos.z > topWingZ) return 3;
        if (worldPos.z < bottomWingZ) return 4;

        return 5;
    }

    public Vector3 GetTargetDirection(GameObject kicker, string targetType)
    {
        fieldCenterZ = (outMinZ + outMaxZ) * 0.5f;

        if (kicker == null)
            return Vector3.forward;

        if (targetType == "Shoot")
        {
            Vector3 target = new Vector3(enemyGoalX, 0f, fieldCenterZ + Random.Range(-5.5f, 5.5f));
            return GetFlatDirection(kicker.transform.position, target);
        }

        if (targetType == "Cross")
        {
            float boxX = enemyGoalX > ourGoalX ? enemyGoalX - 12f : enemyGoalX + 12f;
            Vector3 target = new Vector3(boxX, 0f, fieldCenterZ + Random.Range(-8f, 8f));

            InGamePlayerAI best = FindBestOurTeammateNear(target, kicker);

            if (best != null)
                target = best.transform.position;

            return GetFlatDirection(kicker.transform.position, target);
        }

        if (targetType == "Pass")
        {
            InGamePlayerAI teammate = FindClosestOurTeammate(kicker);

            if (teammate != null)
                return GetFlatDirection(kicker.transform.position, teammate.transform.position);
        }

        return kicker.transform.forward;
    }

    private Vector3 GetFlatDirection(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return Vector3.forward;

        return dir.normalized;
    }

    private InGamePlayerAI FindClosestOurTeammate(GameObject kicker)
    {
        InGamePlayerAI closest = null;
        float minDist = float.MaxValue;

        foreach (var p in allPlayersInGame)
        {
            if (p == null || p.gameObject == kicker || !p.isOurTeam) continue;

            float dist = Vector3.Distance(kicker.transform.position, p.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = p;
            }
        }

        return closest;
    }

    private InGamePlayerAI FindBestOurTeammateNear(Vector3 targetPos, GameObject kicker)
    {
        InGamePlayerAI best = null;
        float minDist = float.MaxValue;

        foreach (var p in allPlayersInGame)
        {
            if (p == null || p.gameObject == kicker || !p.isOurTeam) continue;

            float dist = Vector3.Distance(p.transform.position, targetPos);

            if (dist < minDist)
            {
                minDist = dist;
                best = p;
            }
        }

        return best;
    }

    private void CreateCloseUpCamera()
    {
        GameObject oldCam = GameObject.Find("Runtime_CloseUp_Camera");

        if (oldCam != null)
            Destroy(oldCam);

        GameObject camObj = new GameObject("Runtime_CloseUp_Camera");
        closeUpCamera = camObj.AddComponent<Camera>();

        if (mainGameCamera != null)
            closeUpCamera.CopyFrom(mainGameCamera);

        closeUpCamera.enabled = false;
        closeUpCamera.depth = 100;
    }

    private void ShowCloseUp(GameObject player)
    {
        if (closeUpCamera == null)
            CreateCloseUpCamera();

        if (mainGameCamera != null)
            mainGameCamera.enabled = false;

        Vector3 camPos =
            player.transform.position
            - player.transform.forward * 4.5f
            + Vector3.up * 2.8f;

        closeUpCamera.transform.position = camPos;
        closeUpCamera.transform.LookAt(player.transform.position + Vector3.up * 1.3f);
        closeUpCamera.fieldOfView = 28f;
        closeUpCamera.enabled = true;

        Debug.Log("클로즈업 전용 카메라 ON");
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;

        isSequencePlaying = false;
        lastSequenceTime = Time.unscaledTime;

        ResetChasingFlags();
        CloseAllSequencePanels();

        if (closeUpCamera != null)
            closeUpCamera.enabled = false;

        if (mainGameCamera != null)
            mainGameCamera.enabled = true;
    }

    private void CheckBallOutAndBounce()
    {
        if (ballTransform == null || ballRb == null) return;

        Vector3 pos = ballTransform.position;
        Vector3 vel = ballRb.linearVelocity;

        bool bounced = false;

        if (pos.x < outMinX)
        {
            pos.x = outMinX + 1f;
            vel.x = Mathf.Abs(vel.x) + outBouncePower;
            bounced = true;
        }
        else if (pos.x > outMaxX)
        {
            pos.x = outMaxX - 1f;
            vel.x = -Mathf.Abs(vel.x) - outBouncePower;
            bounced = true;
        }

        if (pos.z < outMinZ)
        {
            pos.z = outMinZ + 1f;
            vel.z = Mathf.Abs(vel.z) + outBouncePower;
            bounced = true;
        }
        else if (pos.z > outMaxZ)
        {
            pos.z = outMaxZ - 1f;
            vel.z = -Mathf.Abs(vel.z) - outBouncePower;
            bounced = true;
        }

        if (bounced)
        {
            ballTransform.position = pos;
            ballRb.linearVelocity = vel;
            ballRb.angularVelocity = Vector3.zero;

            ResetChasingFlags();

            Debug.Log("공이 파란 아웃라인 밖으로 나감 → 안쪽으로 반사");
        }
    }

    private void CreateOutWalls()
    {
        GameObject oldGroup = GameObject.Find("OutWall_Group");

        if (oldGroup != null)
            Destroy(oldGroup);

        GameObject parent = new GameObject("OutWall_Group");
        parent.transform.SetParent(transform);

        float centerX = (outMinX + outMaxX) * 0.5f;
        float centerZ = (outMinZ + outMaxZ) * 0.5f;

        float lengthX = Mathf.Abs(outMaxX - outMinX);
        float lengthZ = Mathf.Abs(outMaxZ - outMinZ);

        CreateWall(
            parent.transform,
            "OutWall_Right",
            new Vector3(outMaxX, wallHeight * 0.5f, centerZ),
            new Vector3(wallThickness, wallHeight, lengthZ)
        );

        CreateWall(
            parent.transform,
            "OutWall_Left",
            new Vector3(outMinX, wallHeight * 0.5f, centerZ),
            new Vector3(wallThickness, wallHeight, lengthZ)
        );

        CreateWall(
            parent.transform,
            "OutWall_Top",
            new Vector3(centerX, wallHeight * 0.5f, outMaxZ),
            new Vector3(lengthX, wallHeight, wallThickness)
        );

        CreateWall(
            parent.transform,
            "OutWall_Bottom",
            new Vector3(centerX, wallHeight * 0.5f, outMinZ),
            new Vector3(lengthX, wallHeight, wallThickness)
        );

        Debug.Log($"파란 아웃라인 벽 생성 완료 X:{outMinX}~{outMaxX}, Z:{outMinZ}~{outMaxZ}");
    }

    private void CreateWall(Transform parent, string wallName, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);

        wall.name = wallName;
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;

        Collider col = wall.GetComponent<Collider>();
        col.isTrigger = false;

        if (ballBounceMaterial != null)
            col.sharedMaterial = ballBounceMaterial;

        Rigidbody rb = wall.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        MeshRenderer mr = wall.GetComponent<MeshRenderer>();

        if (mr != null)
            mr.enabled = showOutWalls;
    }

    public void SetClosestPlayersToChaseBall()
    {
        if (allPlayersInGame.Count == 0 || ballTransform == null) return;

        InGamePlayerAI closestOur = null;
        InGamePlayerAI closestEnemy = null;

        float ourDist = float.MaxValue;
        float enemyDist = float.MaxValue;

        foreach (var p in allPlayersInGame)
        {
            if (p == null) continue;

            float d = Vector3.Distance(p.transform.position, ballTransform.position);

            if (p.isOurTeam)
            {
                if (d < ourDist)
                {
                    ourDist = d;
                    closestOur = p;
                }
            }
            else
            {
                if (d < enemyDist)
                {
                    enemyDist = d;
                    closestEnemy = p;
                }
            }
        }

        foreach (var p in allPlayersInGame)
        {
            if (p != null)
                p.isChasingBall = false;
        }

        if (closestOur != null)
            closestOur.isChasingBall = true;

        if (closestEnemy != null)
            closestEnemy.isChasingBall = true;
    }

    public void ResetChasingFlags()
    {
        foreach (var p in allPlayersInGame)
        {
            if (p != null)
                p.isChasingBall = false;
        }
    }

    private void CloseAllSequencePanels()
    {
        actionUIGroup?.SetActive(false);
        panelArea1and2?.SetActive(false);
        panelArea3and4?.SetActive(false);
        panelArea5?.SetActive(false);
    }

    public void OnClickNormalPass()
    {
        ResumeGame();
        currentPossessor?.GetComponent<InGamePlayerAI>()?.ExecutePass();
    }

    public void OnClickNormalShoot()
    {
        ResumeGame();
        currentPossessor?.GetComponent<InGamePlayerAI>()?.ExecuteShoot();
    }

    public void OnClickDribble()
    {
        ResumeGame();
        currentPossessor?.GetComponent<InGamePlayerAI>()?.ExecuteDribble();
    }

    public void OnClickCross()
    {
        ResumeGame();
        currentPossessor?.GetComponent<InGamePlayerAI>()?.ExecuteCross();
    }

    public void OnClickHeadingPass()
    {
        ResumeGame();
        currentPossessor?.GetComponent<InGamePlayerAI>()?.ExecuteHeadingPass();
    }

    public void OnClickHeadingShoot()
    {
        ResumeGame();
        currentPossessor?.GetComponent<InGamePlayerAI>()?.ExecuteHeadingShoot();
    }

    public void OnClickShedding()
    {
        ResumeGame();
    }
}