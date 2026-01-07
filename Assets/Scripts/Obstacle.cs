using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed = 8f;

    
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
}
