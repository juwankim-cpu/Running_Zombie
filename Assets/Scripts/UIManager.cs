using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Game UI")]
    public Text scoreText; // 점수 표시 UI
    public Image hpBarFill; // 체력 표시 UI
    public Image feverBarFill; // 피버 게이지 표시 UI
    public Image lowHpOverlay; // 저체력 시 붉은 빛 오버레이 (HP 15% 미만)
    public Text countdownText;

    [Header("Low HP Effect Settings")]
    public float lowHpThreshold = 0.15f; // 저체력 임계값 (15%)
    public float lowHpBlinkFrequency = 2f; // 초당 깜빡임 횟수
    public float lowHpOverlayMaxAlpha = 0.3f; // 깜빡일 때 최대 알파

    [Header("Result UI")]
    public ResultUI resultUI; // 결과 UI 참조

    private bool isGameOver = false; // 게임 오버 상태

    [Header("Pause UI")]
    public GameObject pauseUI;
    

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // 게임 오버 상태가 아니면 저체력 효과 업데이트
        if (!isGameOver)
        {
            UpdateLowHpEffect();
        }
    }

    // 점수 업데이트
    public void UpdateScore(float score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Distance: " + Mathf.Floor(score).ToString() + "M";
        }
    }

    // 체력바 업데이트
    public void UpdateHPBar(float currentHp, float maxHp)
    {
        if (hpBarFill != null)
        {
            hpBarFill.fillAmount = currentHp / maxHp;
        }
    }

    // 피버 게이지 업데이트
    public void UpdateFeverBar(float feverGauge, float maxFeverGauge)
    {
        if (feverBarFill != null)
        {
            feverBarFill.fillAmount = feverGauge / maxFeverGauge;
        }
    }

    // 저체력 효과 업데이트
    void UpdateLowHpEffect()
    {
        if (lowHpOverlay != null && GameManager.instance != null)
        {
            float hpRatio = GameManager.instance.currentHp / GameManager.instance.maxHp;
            
            if (hpRatio < lowHpThreshold)
            {
                // HP가 15% 미만일 때 붉은 오버레이가 깜빡이도록 처리 (정사각파 형태)
                bool onPhase = Mathf.FloorToInt(Time.unscaledTime * lowHpBlinkFrequency) % 2 == 0;
                Color color = lowHpOverlay.color;
                color.a = onPhase ? lowHpOverlayMaxAlpha : 0f;
                lowHpOverlay.color = color;
                lowHpOverlay.enabled = true;
            }
            else
            {
                // HP가 15% 이상이면 효과 비활성화
                lowHpOverlay.enabled = false;
            }
        }
    }

    // 결과 UI 표시
    public void ShowResultUI(float finalScore)
    {
        isGameOver = true;
        
        // 저체력 오버레이 비활성화 및 깜빡임 멈춤
        DisableLowHpOverlay();
        
        if (resultUI != null)
        {
            resultUI.ShowResult(finalScore);
        }
        else
        {
            Debug.LogWarning("ResultUI가 설정되지 않았습니다!");
        }
    }

    // 저체력 오버레이 비활성화
    void DisableLowHpOverlay()
    {
        if (lowHpOverlay != null)
        {
            lowHpOverlay.enabled = false;
        }
    }

    // 게임 오버 상태 리셋 (씬 재시작 시 사용)
    public void ResetGameOverState()
    {
        isGameOver = false;
    }

    public void PauseGame()
    {
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
    }
    public void ResumeGame()
    {
       
        
        ResumeGameWithCountDown();
    }

    public void ResumeGameWithCountDown()
    {
        StartCoroutine(CountdownCoroutine());
    }

    IEnumerator CountdownCoroutine()
    {
        // 1. 일시정지 UI 비활성화
        if (pauseUI != null) pauseUI.SetActive(false);

        // 2. 카운트다운 시작
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            
            for (int i = 3; i > 0; i--)
            {
                countdownText.text = i.ToString();
                // 중요: WaitForSecondsRealtime을 써야 timeScale이 0일 때도 시간이 흐릅니다.
                yield return new WaitForSecondsRealtime(1f); 
            }
            
            countdownText.text = "GO!";
            yield return new WaitForSecondsRealtime(0.5f);
            countdownText.gameObject.SetActive(false);
        }

        // 3. 게임 재개 (난이도는 GameManager.instance.gameSpeed에 의해 자동 유지됨)
        Time.timeScale = 1f;
    }

    public void StopGame()
    {
        Time.timeScale = 0f;
        pauseUI.SetActive(false);
        isGameOver = true;
        
        resultUI.ShowResult(GameManager.instance.score);
    }
}
