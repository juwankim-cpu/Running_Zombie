using UnityEngine;

public class HealthItem : MonoBehaviour
{
    public float speed = 8f;
    public float healAmount = 20f; // 체력 회복량

    void Update()
    {
        // 게임 속도에 비례해서 이동 속도 증가
        float currentSpeed = speed;
        if (GameManager.instance != null)
        {
            currentSpeed *= GameManager.instance.gameSpeed;
        }
        
        transform.Translate(Vector2.left * currentSpeed * Time.deltaTime);

        // 왼쪽 끝(-15f)로 이동하면 비활성화
        if (transform.position.x < -15f)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어와 충돌 시
        if (collision.CompareTag("Player"))
        {
            // 체력 회복
            if (GameManager.instance != null)
            {
                GameManager.instance.UpdateHP(healAmount);
                Debug.Log($"체력 회복! HP +{healAmount}");
            }

            // 아이템 비활성화
            gameObject.SetActive(false);
        }
    }
}
