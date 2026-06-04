using UnityEngine;

public class InGamePlayerAI : MonoBehaviour
{
    public bool isOurTeam;

    [Header("실시간 반영될 스텟")]
    public float moveSpeed = 5f;
    public float shootPower = 15f;
    public float passAccuracy = 40f;

    private Animator anim;
    private Rigidbody ballRb;
    private Transform ballTransform;
    private Rigidbody playerRb;
    private Vector3 originFormationPos;

    [HideInInspector] public bool isChasingBall = false;

    private bool isCollisionCooldown = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        playerRb = GetComponent<Rigidbody>();

        GameObject ball = GameObject.FindWithTag("Ball");

        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody>();
            ballTransform = ball.transform;
        }

        originFormationPos = transform.parent != null ? transform.parent.position : transform.position;
    }

    public void InitStats(bool isMyHero)
    {
        if (isMyHero && GameDataManager.Instance != null)
        {
            moveSpeed = 5f + GameDataManager.Instance.speedStat * 0.05f;
            shootPower = 15f + GameDataManager.Instance.attackStat * 0.15f;
            passAccuracy = 40f + GameDataManager.Instance.passStat * 0.5f;
        }
        else
        {
            moveSpeed = 5.5f;
            shootPower = 16f;
            passAccuracy = 45f;
        }
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0f || ballTransform == null)
        {
            StopMove();
            return;
        }

        if (isChasingBall)
        {
            MoveToBall();
            return;
        }

        KeepFormation();
    }

    private void MoveToBall()
    {
        Vector3 targetDir = ballTransform.position - transform.position;
        targetDir.y = 0f;

        if (targetDir.magnitude > 0.3f)
        {
            RotateTo(targetDir);

            if (playerRb != null)
                playerRb.linearVelocity = targetDir.normalized * moveSpeed;

            if (anim != null)
                anim.SetFloat("Speed", playerRb != null ? playerRb.linearVelocity.magnitude : moveSpeed);
        }
        else
        {
            StopMove();
        }
    }

    private void KeepFormation()
    {
        Vector3 ballPos = ballTransform.position;
        Vector3 targetPositioning = Vector3.Lerp(originFormationPos, ballPos, 0.25f);
        targetPositioning.y = transform.position.y;

        Vector3 moveDir = targetPositioning - transform.position;
        moveDir.y = 0f;

        if (moveDir.magnitude > 0.4f)
        {
            Vector3 lookBallDir = ballPos - transform.position;
            lookBallDir.y = 0f;

            if (lookBallDir.magnitude > 0.1f)
                RotateTo(lookBallDir, 5f);

            if (playerRb != null)
                playerRb.linearVelocity = moveDir.normalized * moveSpeed * 0.4f;

            if (anim != null)
                anim.SetFloat("Speed", playerRb != null ? playerRb.linearVelocity.magnitude : moveSpeed);
        }
        else
        {
            StopMove();
        }
    }

    private void RotateTo(Vector3 dir, float speed = 12f)
    {
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * speed);
    }

    private void StopMove()
    {
        if (playerRb != null)
            playerRb.linearVelocity = Vector3.zero;

        if (anim != null)
            anim.SetFloat("Speed", 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isCollisionCooldown) return;
        if (!collision.gameObject.CompareTag("Ball")) return;

        isChasingBall = false;
        StopMove();

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
        }

        if (ballTransform != null)
            ballTransform.position = transform.position + transform.forward * 1.2f + Vector3.up * 0.25f;

        StartCooldown();

        Collider playerCol = GetComponent<Collider>();
        Collider ballCol = ballRb != null ? ballRb.GetComponent<Collider>() : null;

        if (playerCol != null && ballCol != null)
        {
            Physics.IgnoreCollision(playerCol, ballCol, true);
            StartCoroutine(ResetCollisionRoutine(playerCol, ballCol, 1.0f));
        }

        if (InGameMatchDirector.Instance == null) return;

        if (isOurTeam)
        {
            InGameMatchDirector.Instance.TriggerSelectSequence(gameObject, false);
        }
        else
        {
            Invoke(nameof(EnemySimpleAIAction), 0.25f);
        }
    }

    private void EnemySimpleAIAction()
    {
        if (Time.timeScale == 0f) return;

        int random = Random.Range(0, 3);

        if (random == 0) ExecutePass();
        else if (random == 1) ExecuteDribble();
        else ExecuteShoot();
    }

    public void ExecutePass()
    {
        StartCooldown();
        if (anim != null) anim.SetTrigger("doPass");
    }

    public void ExecuteShoot()
    {
        StartCooldown();
        if (anim != null) anim.SetTrigger("doShoot");
    }

    public void ExecuteCross()
    {
        StartCooldown();
        if (anim != null) anim.SetTrigger("doCross");
    }

    public void ExecuteDribble()
    {
        StartCooldown();
        if (anim != null) anim.SetTrigger("doDribble");
    }

    public void ExecuteHeadingShoot()
    {
        StartCooldown();
        if (anim != null) anim.SetTrigger("doHeadingShoot");
    }

    public void ExecuteHeadingPass()
    {
        StartCooldown();
        if (anim != null) anim.SetTrigger("doHeadingPass");
    }

    public void StartCooldown()
    {
        isCollisionCooldown = true;
        CancelInvoke(nameof(ResetCooldown));
        Invoke(nameof(ResetCooldown), 1.0f);
    }

    private void ResetCooldown()
    {
        isCollisionCooldown = false;
    }

    public void OnBallKickImpact(string actionType)
    {
        if (ballRb == null) return;

        Vector3 targetDirection = transform.forward;

        if (InGameMatchDirector.Instance != null)
        {
            targetDirection = InGameMatchDirector.Instance.GetTargetDirection(gameObject, actionType);
        }

        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude > 0.01f)
        {
            targetDirection.Normalize();
            transform.rotation = Quaternion.LookRotation(targetDirection);
        }

        float power = 12f;
        float upForce = 0f;

        if (actionType == "Shoot")
        {
            power = shootPower * 1.3f;
            upForce = Random.Range(1.0f, 3.5f);
        }
        else if (actionType == "Cross")
        {
            power = 20f;
            upForce = 12f;
        }
        else if (actionType == "Pass")
        {
            power = passAccuracy * 0.4f + 9f;
            upForce = 0.2f;
        }

        IgnoreBallCollisionTemporarily(0.4f);

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        ballRb.AddForce(targetDirection * power + Vector3.up * upForce, ForceMode.Impulse);

        if (InGameMatchDirector.Instance != null)
            InGameMatchDirector.Instance.ResetChasingFlags();
    }

    public void OnDribbleImpact()
    {
        if (ballRb == null) return;

        IgnoreBallCollisionTemporarily(0.35f);

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        ballRb.AddForce(transform.forward * 6.5f + Vector3.up * 0.5f, ForceMode.Impulse);

        if (InGameMatchDirector.Instance != null)
            InGameMatchDirector.Instance.ResetChasingFlags();
    }

    public void OnBallHeaderImpact(string headerType)
    {
        if (ballRb == null) return;

        string actionType = headerType == "HeaderShoot" ? "Shoot" : "Pass";

        Vector3 targetDirection = transform.forward;

        if (InGameMatchDirector.Instance != null)
            targetDirection = InGameMatchDirector.Instance.GetTargetDirection(gameObject, actionType);

        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude > 0.01f)
        {
            targetDirection.Normalize();
            transform.rotation = Quaternion.LookRotation(targetDirection);
        }

        float power = headerType == "HeaderShoot" ? shootPower * 1.2f : 9f;
        float upForce = headerType == "HeaderShoot" ? -1.5f : 2.5f;

        IgnoreBallCollisionTemporarily(0.4f);

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        ballRb.AddForce(targetDirection * power + Vector3.up * upForce, ForceMode.Impulse);

        if (InGameMatchDirector.Instance != null)
            InGameMatchDirector.Instance.ResetChasingFlags();
    }

    private void IgnoreBallCollisionTemporarily(float delay)
    {
        Collider playerCol = GetComponent<Collider>();
        Collider ballCol = ballRb != null ? ballRb.GetComponent<Collider>() : null;

        if (playerCol != null && ballCol != null)
        {
            Physics.IgnoreCollision(playerCol, ballCol, true);
            StartCoroutine(ResetCollisionRoutine(playerCol, ballCol, delay));
        }
    }

    private System.Collections.IEnumerator ResetCollisionRoutine(Collider pCol, Collider bCol, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (pCol != null && bCol != null)
            Physics.IgnoreCollision(pCol, bCol, false);
    }
}