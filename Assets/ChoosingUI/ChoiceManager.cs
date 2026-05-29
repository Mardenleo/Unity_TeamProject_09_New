using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChoiceManager : MonoBehaviour
{
    public Transform ball; public Transform shootTarget; public float ballMoveSpeed = 15f; public Transform missTarget;
    public Transform passTargetCenter;
    public Transform passTargetWing;
    public Transform dribbleTarget;

    public Transform crossTarget;
    public Transform headerPassTarget;
    public Transform flickTarget;
    public Transform headerShootTarget;

    private int homeScore = 0;
    private int awayScore = 0;

    private float matchTime = 0f;
    private float maxMatchTime = 120f;

    private bool isMatchEnded = false;
    private bool isResolving = false;
    private bool isGameStarted = false;

    private Coroutine ballMoveCoroutine;
    private Vector3 ballStartPosition;
    private Transform ballStartParent;
    private Vector3 ballStartLocalPosition;

    public TMP_Text resultText;
    public enum ChoiceState
    {
        BuildUp,
        AttackWing,
        AttackCenter,
        AirBall,
        PenaltyKick
    }

    public ChoiceState currentState = ChoiceState.BuildUp;

    public Animator playerAnimator;

    public Image choice1Image;
    public Image choice2Image;
    public Image choice3Image;

    public Sprite buildUp1Pass;
    public Sprite buildUp2Shoot;
    public Sprite buildUp3Dribble;

    public Sprite wing1Cross;
    public Sprite wing2Dribble;
    public Sprite wing3Shoot;

    public Sprite center1Pass;
    public Sprite center2Shoot;
    public Sprite center3Dribble;

    public Sprite air1HeadingPass;
    public Sprite air2Dummy;
    public Sprite air3HeadingShoot;

    public Sprite pkLeft;
    public Sprite pkCenter;
    public Sprite pkRight;

    void Start()
    {
        ballStartPosition = ball.position;
        ballStartParent = ball.parent;
        ballStartLocalPosition = ball.localPosition;

        UpdateChoiceImages();


        ShowResult("PRESS SPACE TO START");
    }


    void ShowResult(string message)
    {
        resultText.text = message;

        CancelInvoke(nameof(ClearResult));
        Invoke(nameof(ClearResult), 2f);

    }

    void SetChoicesVisible(bool visible)
    {
        choice1Image.gameObject.SetActive(visible);
        choice2Image.gameObject.SetActive(visible);
        choice3Image.gameObject.SetActive(visible);
    }

    void ClearResult()
    {
        resultText.text = "";
    }

    void Update()
    {
        if (!isGameStarted)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                isGameStarted = true;
                ShowResult("KICK OFF!");
            }

            return;
        }
        if (isMatchEnded)
            return;

        matchTime += Time.deltaTime;

        if (matchTime >= maxMatchTime)
        {
            EndMatch();
            return;
        }

        if (isResolving)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SelectChoice(1);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SelectChoice(2);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SelectChoice(3);
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            ChangeState(ChoiceState.BuildUp);
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            ChangeState(ChoiceState.AttackWing);
        }

        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            ChangeState(ChoiceState.AttackCenter);
        }

        if (Keyboard.current.f4Key.wasPressedThisFrame)
        {
            ChangeState(ChoiceState.AirBall);
        }

        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            ChangeState(ChoiceState.PenaltyKick);
        }
    }

    void AddHomeGoal()
    {
        homeScore++;

        CancelInvoke(nameof(EndResolve));

        Debug.Log("SCORE: HOME " + homeScore + " : " + awayScore + " AWAY");

        Invoke(nameof(RestartAfterGoal), 3f);
    }

    void SelectChoice(int choice)
    {
        isResolving = true;
        SetChoicesVisible(false);
        Invoke(nameof(EndResolve), 2f);

        switch (currentState)
        {
            case ChoiceState.BuildUp:
                HandleBuildUp(choice);
                break;

            case ChoiceState.AttackWing:
                HandleAttackWing(choice);
                break;

            case ChoiceState.AttackCenter:
                HandleAttackCenter(choice);
                break;

            case ChoiceState.AirBall:
                HandleAirBall(choice);
                break;

            case ChoiceState.PenaltyKick:
                HandlePenaltyKick(choice);
                break;
        }
    }

    void HandleBuildUp(int choice)
    {
        if (choice == 1)
        {
            Debug.Log("빌드업: 패스하기");
            playerAnimator.SetTrigger("Pass");

            if (CheckSuccess(90))
            {
                ShootBallTo(passTargetCenter);

                Invoke(nameof(ResetBall), 1.5f);

                Debug.Log("PASS SUCCESS!");
                ShowResult("PASS SUCCESS!");
                ChangeState(ChoiceState.AttackCenter);
            }
            else
            {
                Debug.Log("PASS MISSED!");
                ShowResult("PASS MISSED!");
                OpponentAttack();
            }
        }
        else if (choice == 2)
        {
            Debug.Log("빌드업: 슈팅하기");
            playerAnimator.SetTrigger("Shoot");

            if (CheckSuccess(10))
            {
                ShootBallTo(shootTarget);
                Invoke(nameof(PlayCelebrate), 0.8f);

                Debug.Log("GOAL!");
                ShowResult("GOAL!");
                AddHomeGoal();
            }
            else
            {
                ShootBallTo(missTarget);

                Debug.Log("SHOOT MISSED!");
                ShowResult("SHOOT MISSED!");
                OpponentAttack();
            }
        }
        else if (choice == 3)
        {
            Debug.Log("빌드업: 드리블하기");
            playerAnimator.SetTrigger("Dribble");

            if (CheckSuccess(75))
            {
                ShootBallTo(dribbleTarget);
                Invoke(nameof(ResetBall), 1.5f);
                Debug.Log("DRIBBLE SUCCESS!");
                ShowResult("DRIBBLE SUCCESS!");
                ChangeState(ChoiceState.AttackWing);
            }
            else
            {
                Debug.Log("DRIBBLE FAILED!");
                ShowResult("DRIBBLE FAILED!");
                OpponentAttack();
            }
        }
    }

    void HandleAttackWing(int choice)
    {
        if (choice == 1)
        {
            Debug.Log("측면: 크로스하기");
            playerAnimator.SetTrigger("Cross");

            if (CheckSuccess(70))
            {
                ShootBallTo(crossTarget);
                Invoke(nameof(ResetBall), 1.5f);
                Debug.Log("CROSS SUCCESS!");
                ShowResult("CROSS SUCCESS!");
                ChangeState(ChoiceState.AirBall);
            }
            else
            {
                Debug.Log("CROSS FAILED!");
                ShowResult("CROSS FAILED!");
                OpponentAttack();
            }
        }
        else if (choice == 2)
        {
            Debug.Log("측면: 드리블하기");
            playerAnimator.SetTrigger("Dribble");

            if (CheckSuccess(55))
            {
                ShootBallTo(dribbleTarget);
                Invoke(nameof(ResetBall), 1.5f);
                Debug.Log("DRIBBLE SUCCESS!");
                ShowResult("DRIBBLE SUCCESS!");
                ChangeState(ChoiceState.AttackCenter);
            }
            else
            {
                Debug.Log("DRIBBLE FAILED!");
                ShowResult("DRIBBLE FAILED!");
                OpponentAttack();
            }
        }
        else if (choice == 3)
        {
            Debug.Log("측면: 슈팅하기");
            playerAnimator.SetTrigger("Shoot");


            if (CheckSuccess(25))

            {
                ShootBallTo(shootTarget);
                Invoke(nameof(PlayCelebrate), 0.8f);
                Debug.Log("GOAL!");
                ShowResult("GOAL!");
                AddHomeGoal();
            }
            else
            {
                ShootBallTo(missTarget);
                Debug.Log("SHOT MISSED!");
                ShowResult("SHOT MISSED!");
                OpponentAttack();
            }
        }
    }

    void HandleAttackCenter(int choice)
    {
        if (choice == 1)
        {
            Debug.Log("중앙: 패스하기");
            playerAnimator.SetTrigger("Pass");

            if (CheckSuccess(80))
            {
                ShootBallTo(passTargetWing);
                Invoke(nameof(ResetBall), 1.5f);

                Debug.Log("PASS SUCCESS!");
                ShowResult("PASS SUCCESS!");
                ChangeState(ChoiceState.AttackWing);
            }
            else
            {
                Debug.Log("PASS MISSED!");
                ShowResult("PASS MISSED!");
                OpponentAttack();
            }
        }
        else if (choice == 2)
        {
            Debug.Log("중앙: 슈팅하기");
            playerAnimator.SetTrigger("Shoot");


            if (CheckSuccess(20))
            {
                ShootBallTo(shootTarget);
                Invoke(nameof(PlayCelebrate), 0.8f);
                Debug.Log("GOAL!");
                ShowResult("GOAL!");
                AddHomeGoal();
            }
            else
            {
                ShootBallTo(missTarget);
                Debug.Log("SHOT MISSED!");
                ShowResult("SHOT MISSED!");
                OpponentAttack();
            }
        }
        else if (choice == 3)
        {
            Debug.Log("중앙: 드리블하기");
            playerAnimator.SetTrigger("Dribble");

            if (CheckSuccess(60))
            {
                ShootBallTo(dribbleTarget);
                Invoke(nameof(ResetBall), 1.5f);
                Debug.Log("DRIBBLE SUCCESS!");
                ShowResult("DRIBBLE SUCCESS!");
                ChangeState(ChoiceState.AirBall);
            }
            else
            {
                Debug.Log("DRIBBLE FAILED!");
                ShowResult("DRIBBLE FAILED!");
                OpponentAttack();
            }
        }
    }

    void HandleAirBall(int choice)
    {
        if (choice == 1)
        {
            Debug.Log("공중볼: 헤딩으로 패스하기");
            playerAnimator.SetTrigger("HeaderPass");

            if (CheckSuccess(80))
            {
                ShootBallTo(headerPassTarget);
                Invoke(nameof(ResetBall), 1.5f);

                Debug.Log("PASS SUCCESS!");
                ShowResult("PASS SUCCESS!");
                ChangeState(ChoiceState.AttackCenter);
            }
            else
            {
                Debug.Log("PASS MISSED!");
                ShowResult("PASS MISSED!");
                OpponentAttack();
            }
        }
        else if (choice == 2)
        {
            Debug.Log("공중볼: 흘리기");
            playerAnimator.SetTrigger("Flick");

            if (CheckSuccess(75))
            {
                ShootBallTo(flickTarget);
                Invoke(nameof(ResetBall), 1.5f);

                Debug.Log("FLICK SUCCESS!");
                ShowResult("FLICK SUCCESS!");
                ChangeState(ChoiceState.AttackCenter);
            }
            else
            {
                ShootBallTo(missTarget);
                Debug.Log("HEADER MISSED!");
                ShowResult("HEADER MISSED!");
                OpponentAttack();
            }
        }
        else if (choice == 3)
        {
            Debug.Log("공중볼: 헤딩으로 슈팅하기");
            playerAnimator.SetTrigger("HeaderShoot");


            if (CheckSuccess(40))
            {
                ShootBallTo(headerShootTarget);
                Invoke(nameof(PlayCelebrate), 0.8f);
                Debug.Log("HEADER GOAL!");
                ShowResult("HEADER GOAL!");
                AddHomeGoal();
            }
            else
            {
                ShootBallTo(missTarget);
                Debug.Log("HEADER MISSED!");
                ShowResult("HEADER MISSED!");
                OpponentAttack();
            }
        }
    }

    void HandlePenaltyKick(int choice)
    {
        if (choice == 1)
        {
            Debug.Log("PK: 왼쪽");
            playerAnimator.SetTrigger("Shoot");


            if (CheckSuccess(75))
            {
                ShootBallTo(shootTarget);
                Invoke(nameof(PlayCelebrate), 0.8f);
                Debug.Log("PK GOAL!");
                ShowResult("PK GOAL!");
                AddHomeGoal();
            }
            else
            {
                ShootBallTo(missTarget);
                Debug.Log("PK MISSED!");
                ShowResult("PK MISSED!");
                OpponentAttack();
            }
        }
        else if (choice == 2)
        {
            Debug.Log("PK: 중앙");
            playerAnimator.SetTrigger("Shoot");


            if (CheckSuccess(75))
            {
                ShootBallTo(shootTarget);
                Invoke(nameof(PlayCelebrate), 0.8f);
                Debug.Log("PK GOAL!");
                ShowResult("PK GOAL!");
                AddHomeGoal();
            }
            else
            {
                ShootBallTo(missTarget);
                Debug.Log("PK MISSED!");
                ShowResult("PK MISSED!");
                OpponentAttack();
            }
        }
        else if (choice == 3)
        {
            Debug.Log("PK: 오른쪽");
            playerAnimator.SetTrigger("Shoot");


            if (CheckSuccess(75))
            {
                ShootBallTo(shootTarget);
                Invoke(nameof(PlayCelebrate), 0.8f);
                Debug.Log("PK GOAL!");
                ShowResult("PK GOAL!");
                AddHomeGoal();
            }
            else
            {
                ShootBallTo(missTarget);
                Debug.Log("PK MISSED!");
                ShowResult("PK MISSED!");
                OpponentAttack();
            }
        }
    }

    public void ChangeState(ChoiceState newState)
    {
        currentState = newState;
        UpdateChoiceImages();
    }

    void ChangeStateDelayed()
    {
        CancelInvoke(nameof(ApplyDelayedBuildUp));
        Invoke(nameof(ApplyDelayedBuildUp), 2f);
    }

    void ApplyDelayedBuildUp()
    {
        ChangeState(ChoiceState.BuildUp);
    }

    void EndMatch()
    {
        isMatchEnded = true;

        Debug.Log("FULL TIME");
        Debug.Log("FINAL SCORE: HOME " + homeScore + " : " + awayScore + " AWAY");

        if (homeScore > awayScore)
        {
            ShowResult("YOU WIN!");
        }
        else if (homeScore < awayScore)
        {
            ShowResult("YOU LOSE!");
        }
        else
        {
            ShowResult("DRAW!");
        }
    }

    void EndResolve()
    {
        isResolving = false;
        SetChoicesVisible(true);
    }

    void AddAwayGoal()
    {
        awayScore++;

        Debug.Log("SCORE: HOME " + homeScore + " : " + awayScore + " AWAY");

        Invoke(nameof(RestartAfterGoal), 2f);
    }

    void RestartAfterGoal()
    {
        ResetBall();

        ShowResult("KICK OFF!");
        ChangeState(ChoiceState.BuildUp);

        isResolving = false;
        SetChoicesVisible(true);
    }

    void RestartAfterMiss()
    {
        ResetBall();

        ShowResult("PLAY ON!");
        ChangeState(ChoiceState.BuildUp);
    }

    void OpponentAttack()
    {
        Debug.Log("OPPONENT ATTACK!");

        if (CheckSuccess(18))
        {
            Debug.Log("OPPONENT GOAL!");
            ShowResult("OPPONENT GOAL!");
            AddAwayGoal();
        }
        else
        {
            Debug.Log("OPPONENT MISSED!");
            ShowResult("OPPONENT MISSED!");

            Invoke(nameof(RestartAfterMiss), 2f);
        }
    }

    void OnValidate()
    {
        if (choice1Image != null && choice2Image != null && choice3Image != null)
        {
            UpdateChoiceImages();
        }
    }

    void ShootBallTo(Transform target)
    {
        if (ballMoveCoroutine != null)
        {
            StopCoroutine(ballMoveCoroutine);
        }

        ballMoveCoroutine = StartCoroutine(MoveBallToTarget(target));
    }

    IEnumerator MoveBallToTarget(Transform target)
    {
        yield return new WaitForSeconds(0.8f);

        ball.SetParent(null);

        while (Vector3.Distance(ball.position, target.position) > 0.1f)
        {
            ball.position = Vector3.MoveTowards(
                ball.position,
                target.position,
                ballMoveSpeed * Time.deltaTime
            );

            yield return null;
        }
    }

    void PlayCelebrate()
    {
        playerAnimator.SetTrigger("Celebrate");
    }

    void ResetBall()
    {
        if (ballMoveCoroutine != null)
        {
            StopCoroutine(ballMoveCoroutine);
            ballMoveCoroutine = null;
        }

        ball.SetParent(ballStartParent);
        ball.localPosition = ballStartLocalPosition;
    }

    bool CheckSuccess(int successRate)
    {
        int roll = Random.Range(1, 101);

        Debug.Log("주사위 : " + roll);

        return roll <= successRate;
    }

    void UpdateChoiceImages()
    {
        if (currentState == ChoiceState.BuildUp)
        {
            choice1Image.sprite = buildUp1Pass;
            choice2Image.sprite = buildUp2Shoot;
            choice3Image.sprite = buildUp3Dribble;
        }
        else if (currentState == ChoiceState.AttackWing)
        {
            choice1Image.sprite = wing1Cross;
            choice2Image.sprite = wing2Dribble;
            choice3Image.sprite = wing3Shoot;
        }
        else if (currentState == ChoiceState.AttackCenter)
        {
            choice1Image.sprite = center1Pass;
            choice2Image.sprite = center2Shoot;
            choice3Image.sprite = center3Dribble;
        }
        else if (currentState == ChoiceState.AirBall)
        {
            choice1Image.sprite = air1HeadingPass;
            choice2Image.sprite = air2Dummy;
            choice3Image.sprite = air3HeadingShoot;
        }
        else if (currentState == ChoiceState.PenaltyKick)
        {
            choice1Image.sprite = pkLeft;
            choice2Image.sprite = pkCenter;
            choice3Image.sprite = pkRight;
        }
    }

}