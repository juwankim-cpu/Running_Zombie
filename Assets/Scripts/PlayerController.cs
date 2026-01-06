using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
   [Header("Movement Settings")]
    public float jumpForce = 12f;

    [Header("Status")]
    public bool isGrounded;
    public int jumpCount = 0;
    private bool isSliding = false;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Keyboard keyboard;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        
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
}
