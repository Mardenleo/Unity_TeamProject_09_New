using UnityEngine;

public class BallKick : MonoBehaviour
{
   public float power = 5f; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // [중요] 바닥 마찰력을 무시하기 위해 공을 아주 살짝(0.1m) 위로 띄웁니다.
                other.transform.position += Vector3.up * 0.05f;

                // 물리값 초기화
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // [수정] 위로 뜨는 힘을 조금 더 섞었습니다. (Up 값을 0.8로 상향)
                Vector3 shootDir = transform.root.forward + (Vector3.up * 0.1f);

        
                rb.AddForce(shootDir.normalized * power, ForceMode.Impulse);
            }
        }
    }
}
