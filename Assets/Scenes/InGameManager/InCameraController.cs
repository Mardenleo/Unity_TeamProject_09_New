using UnityEngine;
// 유니티 6 전용 시네머신 네임스페이스입니다.
using Unity.Cinemachine; 

public class InGameCameraController : MonoBehaviour
{
    public static InGameCameraController Instance;

    [Header("--- 시네머신 카메라 연동 ---")]
    public CinemachineCamera camBroadCast; // 옛날 Virtual Camera 대신 유니티6는 CinemachineCamera를 씁니다.
    public CinemachineCamera camCloseUp;   

    void Awake()
    {
        Instance = this;
    }

    public void SwitchCameraView(bool isOurTeamPossession)
    {
        // 유니티 6에서도 Priority 수치가 높은 카메라가 화면을 차지하는 룰은 똑같습니다!
        if (isOurTeamPossession)
        {
            camBroadCast.Priority = 10;
            camCloseUp.Priority = 20;
        }
        else
        {
            camBroadCast.Priority = 20;
            camCloseUp.Priority = 10;
        }
    }
}