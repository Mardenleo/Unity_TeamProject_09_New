using UnityEngine;
using UnityEngine.EventSystems;

public class RotatePreview : MonoBehaviour, IDragHandler
{
    public Transform characterTransform; // 여기에 player(4) 드래그
    public float rotateSpeed = 5.0f; // 속도를 크게 설정

    public void OnDrag(PointerEventData eventData)
    {
        if (characterTransform != null)
        {
            // delta.x는 마우스의 좌우 이동량입니다.
            float rotX = eventData.delta.x * rotateSpeed;
            // 캐릭터를 Y축 기준으로 회전시킵니다.
            characterTransform.Rotate(Vector3.up, -rotX);
        }
    }
}