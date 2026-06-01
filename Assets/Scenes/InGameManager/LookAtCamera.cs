using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        // 씬 내의 메인 카메라 트랜스폼을 미리 확보합니다.
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // 이름표 오브젝트의 회전 방향을 카메라 정면과 평행하도록 강제 정렬합니다.
            transform.rotation = Quaternion.LookRotation(transform.position - mainCameraTransform.position);
        }
    }
}