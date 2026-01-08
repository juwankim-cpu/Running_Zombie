using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ResultUI : MonoBehaviour
{
    [Header("UI References")]
    public Text scoreText; // 점수 표시 텍스트
    public Text deathReasonText; // 사망 이유 텍스트
    public Button retryButton; // 다시하기 버튼
    public Button mainMenuButton; // 메인 메뉴 버튼

    [Header("Animation Settings")]
    public float slideDownDuration = 0.5f; // 아래로 내려오는 애니메이션 시간
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 애니메이션 커브

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Vector2 targetPosition;
    private bool isAnimating = false;

    // 랜덤 사망 이유 텍스트 배열
    private string[] deathReasons = new string[]
    {
        "체력이 모두 소진되었습니다...",
        "너무 빨리 달렸습니다...",
        "인간에게 걸렸습니다...",
        "체력 회복을 받지 못했습니다...",
        "지쳤습니다...",
        "무리해서 달렸습니다...",
        "안전하게 달리지 못했습니다...",
        "피버타임을 활용하지 못했습니다...",
        "장애물을 제때 피하지 못했습니다..."
    };

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Canvas 찾기
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
        
        // 초기 위치 저장 (화면 위쪽 밖)
        originalPosition = rectTransform.anchoredPosition;
        
        // 타겟 위치는 원래 위치 (화면 중앙)
        // 만약 원래 위치가 중앙이 아니라면, 중앙으로 설정
        if (rectTransform.anchorMin == new Vector2(0.5f, 0.5f) && 
            rectTransform.anchorMax == new Vector2(0.5f, 0.5f))
        {
            // 앵커가 중앙이면 타겟 위치를 중앙으로 설정
            targetPosition = Vector2.zero;
        }
        else
        {
            targetPosition = originalPosition;
        }
    }

    void Start()
    {
        // 시작 시 비활성화 상태로 설정
        gameObject.SetActive(false);
        
        // 버튼 이벤트 연결
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
    }

    // 결과 UI 표시
    public void ShowResult(float finalScore)
    {
        // UI 활성화
        gameObject.SetActive(true);
        
        // 점수 표시
        if (scoreText != null)
        {
            scoreText.text = "최종 거리: " + Mathf.Floor(finalScore).ToString() + "M";
        }
        
        // 랜덤 사망 이유 표시
        if (deathReasonText != null)
        {
            string randomReason = deathReasons[Random.Range(0, deathReasons.Length)];
            deathReasonText.text = randomReason;
        }
        
        // 애니메이션 시작
        StartCoroutine(SlideDownAnimation());
    }

    // 위에서 아래로 내려오는 애니메이션
    IEnumerator SlideDownAnimation()
    {
        isAnimating = true;
        
        // 시작 위치: 화면 위쪽 밖 (Canvas 높이만큼 위로 올림)
        float screenHeight = Screen.height;
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            // Canvas가 Camera 모드인 경우 Canvas의 RectTransform 높이 사용
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                screenHeight = canvasRect.rect.height;
            }
        }
        
        Vector2 startPosition = targetPosition + new Vector2(0, screenHeight);
        rectTransform.anchoredPosition = startPosition;
        
        float elapsed = 0f;
        
        while (elapsed < slideDownDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Time.timeScale이 0이어도 작동하도록
            float t = elapsed / slideDownDuration;
            float curveValue = slideCurve.Evaluate(t);
            
            // 시작 위치에서 타겟 위치로 보간
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
            
            yield return null;
        }
        
        // 정확한 위치로 설정
        rectTransform.anchoredPosition = targetPosition;
        isAnimating = false;
    }

    // 다시하기 버튼 클릭 이벤트
    public void OnRetryButtonClicked()
    {
        if (isAnimating) return; // 애니메이션 중이면 무시
        
        // Time.timeScale 복구
        Time.timeScale = 1f;
        
        // UIManager의 게임 오버 상태 리셋
        if (UIManager.instance != null)
        {
            UIManager.instance.ResetGameOverState();
        }
        
        // 현재 씬 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 메인 메뉴 버튼 클릭 이벤트
    public void OnMainMenuButtonClicked()
    {
        if (isAnimating) return; // 애니메이션 중이면 무시
        
        // Time.timeScale 복구
        Time.timeScale = 1f;
        
        // 메인 메뉴 씬으로 이동 (씬 이름은 Unity 에디터에서 설정 필요)
        // MainScene이 메인 게임 씬이므로, 메인 메뉴 씬이 따로 있다면 그 씬의 이름으로 변경
        // 현재는 MainScene의 첫 번째 씬(인덱스 0)으로 이동한다고 가정
        if (SceneManager.sceneCountInBuildSettings > 0)
        {
            SceneManager.LoadScene(0); // 첫 번째 씬으로 이동
        }
        else
        {
            // 메인 메뉴 씬이 없다면 현재 씬을 다시 로드
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // 외부에서 메인 메뉴 씬 이름 설정
    public void SetMainMenuSceneName(string sceneName)
    {
        // 필요시 사용할 수 있는 메서드
    }
}
