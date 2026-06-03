using UnityEngine;

public class InGameBall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. 공과 부딪힌 오브젝트에서 실제 선수 AI 스크립트를 가져옵니다.
        InGamePlayerAI playerAIScript = other.GetComponent<InGamePlayerAI>();

        // 2. 부딪힌 대상이 선수가 맞는지 확인
        if (playerAIScript != null)
        {
            // 🔵 3. 유저가 선택해서 플레이 중인 '우리 팀' 선수가 공을 잡았을 때
            if (playerAIScript.isOurTeam == true) 
            {
                Debug.Log($"[아군 소유] {other.name} 선수가 공을 획득했습니다! -> 클로즈업 뷰 전환");

                // 🎥 유니티 6 시네머신 카메라를 우리 팀 클로즈업(CloseUp) 뷰로 스위칭
                if (InGameCameraController.Instance != null)
                {
                    InGameCameraController.Instance.SwitchCameraView(true);
                }

                // 💻 구역별 UI 선택창(Pass, Shoot 등)을 화면에 팝업
                if (InGameMatchDirector.Instance != null)
                {
                    // 기본 지상볼(false) 상태로 매치 디렉터에게 신호를 보냅니다.
                    InGameMatchDirector.Instance.TriggerSelectSequence(other.gameObject, false);
                }
            }
            
            // 🔴 4. 유저가 선택하지 않은 '상대 팀(적군)' 선수가 공을 뺏어갔을 때
            else if (playerAIScript.isOurTeam == false)
            {
                Debug.Log($"[적군 소유] {other.name} 선수가 공을 가로챘습니다! -> 중계 와이드 뷰 전환");

                // 🎥 카메라를 경기장 전체를 넓게 비추는 중계(BroadCast) 뷰로 스위칭
                if (InGameCameraController.Instance != null)
                {
                    InGameCameraController.Instance.SwitchCameraView(false);
                }
            }
        }
    }
}