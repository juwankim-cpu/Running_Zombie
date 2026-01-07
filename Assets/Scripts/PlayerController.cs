using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
   [Header("Movement Settings")]
    public float jumpForce = 12f;

    [Header("Status")]
    public bool isGrounded;
    public int jumpCount = 0;
    private bool isSliding = false;

    [Header("Hit Effect")]
    public float blinkDuration = 0.5f; // 깜빡임 지속 시간
    public float blinkInterval = 0.1f; // 깜빡임 간격

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Keyboard keyboard;
    private SpriteRenderer spriteRenderer;
    private bool isBlinking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 슬라이딩 시 콜라이더 크기 조절을 위해 초기값 저장
        originalColliderSize = col.size;
        originalColliderOffset = col.offset;
    }

    void Update()
    {
        // 키보드 입력 가져오기
        keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 1. 점프 입력 (Space)
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }

        // 2. 슬라이딩 입력 (S 또는 Shift) - 키를 누르고 있는 동안만 슬라이드 유지
        bool slideKeyPressed = keyboard.sKey.isPressed || keyboard.leftShiftKey.isPressed;
        
        if (slideKeyPressed && isGrounded && !isSliding)
        {
            // 슬라이드 시작
            StartSliding();
        }
        else if ((!slideKeyPressed || !isGrounded) && isSliding)
        {
            // 키를 떼거나 공중에 떠있으면 슬라이드 종료
            StopSliding();
        }
    }

    void Jump()
    {
        if (isGrounded || jumpCount < 2) // 2단 점프까지 허용
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            isGrounded = false;
            
            // 점프하면 슬라이딩 강제 종료
            if (isSliding) StopSliding();
        }
    }

    void StartSliding()
    {
        isSliding = true;
        // 콜라이더 높이를 절반으로 줄임 (낮은 장애물 통과용)
        col.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f);
        col.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y * 0.5f);
    }

    void StopSliding()
    {
        isSliding = false;
        col.size = originalColliderSize;
        col.offset = originalColliderOffset;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 'Ground' 태그가 붙은 바닥에 닿았을 때 초기화
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            Debug.Log("장애물 충돌! HP -15");
            
            // GameManager에 접근하여 체력 감소
            GameManager.instance.UpdateHP(-15f);

            // 피격 연출: 깜빡임 효과
            if (!isBlinking)
            {
                StartCoroutine(BlinkEffect());
            }

            // 피격 연출: 화면 흔들림
            if (CameraShake.instance != null)
            {
                CameraShake.instance.Shake(0.2f, 0.3f);
            }

            // 피격 시 장애물을 비활성화 (중복 충돌 방지)
            collision.gameObject.SetActive(false);
        }
    }

    // 깜빡임 효과 코루틴
    IEnumerator BlinkEffect()
    {
        isBlinking = true;
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < blinkDuration)
        {
            if (spriteRenderer != null)
            {
                // 알파값을 토글하여 깜빡임 효과
                Color color = spriteRenderer.color;
                color.a = visible ? 0.3f : 1f;
                spriteRenderer.color = color;
                visible = !visible;
            }
            
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // 깜빡임 종료 후 원래 상태로 복구
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        isBlinking = false;
    }
}
