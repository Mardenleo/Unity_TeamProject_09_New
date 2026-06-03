using UnityEngine;

public class InGamePlayerAI : MonoBehaviour
{
    public bool isOurTeam; 
    
    [Header("--- 실시간 반영될 스텟 ---")]
    public float moveSpeed = 5f; 
    public float shootPower = 15f; 
    public float passAccuracy = 40f;

    private Animator anim;
    private Rigidbody ballRb;
    private Transform ballTransform;
    private Rigidbody playerRb;
    private Vector3 originFormationPos;

    [HideInInspector] public bool isChasingBall = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        playerRb = GetComponent<Rigidbody>();
        
        GameObject ball = GameObject.FindWithTag("Ball");
        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody>();
            ballTransform = ball.transform;
        }

        originFormationPos = (transform.parent != null) ? transform.parent.position : transform.position;
    }

    public void InitStats(bool isMyHero)
    {
        if (isMyHero && GameDataManager.Instance != null)
        {
            moveSpeed = 5f + (GameDataManager.Instance.speedStat * 0.05f);       
            shootPower = 15f + (GameDataManager.Instance.attackStat * 0.15f);    
            passAccuracy = 40f + (GameDataManager.Instance.passStat * 0.5f);     
        }
        else
        {
            moveSpeed = 5.5f;       
            shootPower = 16f;       
            passAccuracy = 45f;     
        }
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f || ballTransform == null)
        {
            if (playerRb != null) playerRb.linearVelocity = Vector3.zero;
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        if (isChasingBall)
        {
            Vector3 targetDir = (ballTransform.position - transform.position);
            targetDir.y = 0f; 

            if (targetDir.magnitude > 0.5f) 
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);

                playerRb.linearVelocity = targetDir.normalized * moveSpeed;
                if (anim != null) anim.SetFloat("Speed", playerRb.linearVelocity.magnitude);
            }
            return;
        }

        Vector3 ballPos = ballTransform.position;
        Vector3 targetPositioning = Vector3.Lerp(originFormationPos, ballPos, 0.25f); 
        targetPositioning.y = transform.position.y;

        Vector3 moveDir = (targetPositioning - transform.position);
        moveDir.y = 0f;

        if (moveDir.magnitude > 0.3f)
        {
            Vector3 lookBallDir = (ballPos - transform.position);
            lookBallDir.y = 0f;
            if (lookBallDir.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookBallDir), Time.fixedDeltaTime * 5f);
            }

            playerRb.linearVelocity = moveDir.normalized * (moveSpeed * 0.4f);
            if (anim != null) anim.SetFloat("Speed", playerRb.linearVelocity.magnitude);
        }
        else
        {
            playerRb.linearVelocity = Vector3.zero;
            if (anim != null) anim.SetFloat("Speed", 0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            isChasingBall = false;
            if (playerRb != null) playerRb.linearVelocity = Vector3.zero;

            if (InGameMatchDirector.Instance != null)
            {
                if (this.isOurTeam)
                {
                    InGameMatchDirector.Instance.TriggerSelectSequence(this.gameObject, false);
                }
                else
                {
                    Debug.Log($"[상대팀 확보] {gameObject.name}가 공을 잡았습니다.");
                    Invoke("EnemySimpleAIAction", 0.4f);
                }
            }
        }
    }

    void EnemySimpleAIAction()
    {
        if (Time.timeScale == 0f) return;
        ExecutePass();
        // 적군은 반대 조건이므로 정면으로 패스하게 처리
        OnBallKickImpact("Pass");
    }

    public void ExecutePass() { if(anim != null) anim.SetTrigger("doPass"); }
    public void ExecuteShoot() { if(anim != null) anim.SetTrigger("doShoot"); }
    public void ExecuteCross() { if(anim != null) anim.SetTrigger("doCross"); }
    public void ExecuteDribble() { if(anim != null) anim.SetTrigger("doDribble"); }
    public void ExecuteHeadingShoot() { if(anim != null) anim.SetTrigger("doHeadingShoot"); }
    public void ExecuteHeadingPass() { if(anim != null) anim.SetTrigger("doHeadingPass"); }

    // 💡 [방향 교정의 핵심] 킥 이팩트 이벤트 발생 시 유도리 있는 타겟팅 계산 작동
    public void OnBallKickImpact(string actionType)
    {
        if (ballRb == null) return;

        // 기본 방향은 몸 앞쪽이지만, 아군 플레이어라면 디렉터에게 영리한 타겟 방향을 물어봅니다.
        Vector3 targetDirection = transform.forward;
        if (isOurTeam && InGameMatchDirector.Instance != null)
        {
            targetDirection = InGameMatchDirector.Instance.GetTargetDirection(this.gameObject, actionType);
            
            // 공을 차기 직전, 목표 지점을 확실하게 쳐다보도록 강제 회전시켜 자연스러운 모션을 만듭니다.
            if (targetDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(targetDirection);
            }
        }

        float power = 12f; float upForce = 0f;

        if (actionType == "Shoot") { power = shootPower; upForce = 2.5f; }
        else if (actionType == "Cross") { power = 18f; upForce = 12f; }

        ballRb.linearVelocity = Vector3.zero;
        ballRb.AddForce((targetDirection * power) + (Vector3.up * upForce), ForceMode.Impulse);
        
        if (InGameMatchDirector.Instance != null) InGameMatchDirector.Instance.ResetChasingFlags();
    }

    public void OnDribbleImpact()
    {
        if (ballRb == null) return;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.AddForce((transform.forward * 4f) + (Vector3.up * 0.5f), ForceMode.Impulse);
    }

    public void OnBallHeaderImpact(string headerType)
    {
        if (ballRb == null) return;
        
        Vector3 targetDirection = transform.forward;
        if (isOurTeam && InGameMatchDirector.Instance != null)
        {
            string actionType = (headerType == "HeaderShoot") ? "Shoot" : "Pass";
            targetDirection = InGameMatchDirector.Instance.GetTargetDirection(this.gameObject, actionType);
            if (targetDirection != Vector3.zero) transform.rotation = Quaternion.LookRotation(targetDirection);
        }

        float power = (headerType == "HeaderShoot") ? 15f : 8f;
        float upForce = (headerType == "HeaderShoot") ? -4f : 2f;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.AddForce((targetDirection * power) + (Vector3.up * upForce), ForceMode.Impulse);
        
        if (InGameMatchDirector.Instance != null) InGameMatchDirector.Instance.ResetChasingFlags();
    }
}