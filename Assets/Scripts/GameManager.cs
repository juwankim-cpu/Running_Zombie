using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Score Settings")]
    public float score = 0f;
    public Text scoreText; // 점수 표시 UI

    [Header("Health Settings")]
    public float maxHp = 200f;
    public float currentHp;
    public float hpDecreaseRate = 3f; // 초당 체력 감소량
    public Image hpBarFill; // 체력 표시 UI
    public Image lowHpOverlay; // 저체력 시 붉은 빛 오버레이 (HP 15% 미만)
    public float lowHpThreshold = 0.15f; // 저체력 임계값 (15%)
	public float lowHpBlinkFrequency = 2f; // 초당 깜빡임 횟수
	public float lowHpOverlayMaxAlpha = 0.3f; // 깜빡일 때 최대 알파

    [Header("Difficulty Settings")]
    public float gameSpeed = 1f; // 게임 속도 배율
    public float speedMultiplier = 0.2f; // 난이도 상승 시 증가할 속도
    public float distanceToLevelUp = 100f; // 난이도 상승 거리
    private float nextLevelDistance = 100f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        
        
    }

    void Start()
    {
        // 체력 초기화
        currentHp = maxHp;
    }
    void Update()
    {
        // 점수(거리) 계산: 게임 속도에 비례해서 증가
        score += Time.deltaTime * gameSpeed * 5f;
        scoreText.text = "Distance: " + Mathf.Floor(score).ToString() + "M";

        // 2. 시간에 따른 체력 감소
        UpdateHP(-hpDecreaseRate * Time.deltaTime);

        // 3. 체력바 UI 업데이트
        if (hpBarFill != null)
        {
            hpBarFill.fillAmount = currentHp / maxHp;
        }

        // 4. 저체력 효과 (HP 15% 미만일 때 붉은 빛)
        UpdateLowHpEffect();

        // 5. 게임 오버 체크
        if (currentHp <= 0)
        {
            GameOver();
        }

        // 난이도 상승 로직
        if (score >= nextLevelDistance)
            LevelUp();
    }

    void LevelUp()
    {
        gameSpeed += speedMultiplier;
        nextLevelDistance += distanceToLevelUp;
    }

     // 체력을 변화시키는 공용 함수 (회복/데미지 모두 사용)
    public void UpdateHP(float amount)
    {
        currentHp += amount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp); // 0~200 사이로 고정
    }

    void UpdateLowHpEffect()
    {
        if (lowHpOverlay != null)
        {
            float hpRatio = currentHp / maxHp;
            
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

    void GameOver()
    {
        Debug.Log("Game Over");
        // 여기에 게임 오버 로직 추가 가능 (예: 씬 재시작, UI 표시 등)
        Time.timeScale = 0f; // 게임 일시정지
    }
}
