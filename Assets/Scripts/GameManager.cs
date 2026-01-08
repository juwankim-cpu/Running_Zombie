using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Score Settings")]
    public float score = 0f;

    [Header("Health Settings")]
    public float maxHp = 200f;
    public float currentHp;
    public float hpDecreaseRate = 3f; // 초당 체력 감소량

    [Header("Difficulty Settings")]
    public float gameSpeed = 1f; // 게임 속도 배율
    public float speedMultiplier = 0.2f; // 난이도 상승 시 증가할 속도
    public float distanceToLevelUp = 100f; // 난이도 상승 거리
    private float nextLevelDistance = 100f;

    [Header("Fever Time Settings")]
    public float feverGauge = 0f; // 피버 게이지 (0~100)
    public float maxFeverGauge = 100f; // 피버 게이지 최대값
    public float feverDuration = 5f; // 피버 타임 지속 시간 (초)
    public float feverSpeedMultiplier = 2f; // 피버 타임 시 게임 속도 배율
    public bool isFeverTime = false; // 피버 타임 활성화 여부
    private float baseGameSpeed = 1f; // 피버 타임 전 기본 게임 속도

    private bool isGameOver = false; // 게임 오버 상태

    void Awake()
    {
        if (instance == null)
            instance = this;
        
        
    }

    void Start()
    {
        // 체력 초기화
        currentHp = maxHp;
        
        // 기본 게임 속도 초기화
        baseGameSpeed = gameSpeed;
    }
    void Update()
    {
        // 점수(거리) 계산: 게임 속도에 비례해서 증가
        score += Time.deltaTime * gameSpeed * 5f;
        
        // UI 업데이트
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateScore(score);
            UIManager.instance.UpdateHPBar(currentHp, maxHp);
            UIManager.instance.UpdateFeverBar(feverGauge, maxFeverGauge);
        }

        // 시간에 따른 체력 감소
        UpdateHP(-hpDecreaseRate * Time.deltaTime);

        // 게임 오버 체크
        if (currentHp <= 0 && !isGameOver)
        {
            GameOver();
        }

        // 난이도 상승 로직
        if (score >= nextLevelDistance)
            LevelUp();
    }

    void LevelUp()
    {
        baseGameSpeed += speedMultiplier;
        // 피버 타임 중이 아니면 즉시 적용, 피버 타임 중이면 피버 타임 종료 후 적용됨
        if (!isFeverTime)
        {
            gameSpeed = baseGameSpeed;
        }
        nextLevelDistance += distanceToLevelUp;
    }

     // 체력을 변화시키는 공용 함수 (회복/데미지 모두 사용)
    public void UpdateHP(float amount)
    {
        currentHp += amount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp); // 0~200 사이로 고정
    }

    // 피버 게이지 증가 함수
    public void AddFeverGauge(float amount)
    {
        feverGauge += amount;
        feverGauge = Mathf.Clamp(feverGauge, 0f, maxFeverGauge);

        // 피버 게이지가 최대값에 도달하면 피버 타임 시작
        if (feverGauge >= maxFeverGauge && !isFeverTime)
        {
            StartFeverTime();
        }
    }

    // 피버 타임 시작
    void StartFeverTime()
    {
        if (isFeverTime) return; // 이미 피버 타임 중이면 무시

        isFeverTime = true;
        // 현재 baseGameSpeed를 저장 (피버 타임 시작 전의 기본 속도)
        // 피버 타임 중에는 baseGameSpeed는 계속 추적되지만 gameSpeed만 2배로 증가
        gameSpeed = baseGameSpeed * feverSpeedMultiplier; // 게임 속도 2배
        feverGauge = maxFeverGauge; // 게이지 최대값으로 고정

        // 피버 타임 종료 코루틴 시작
        StartCoroutine(EndFeverTimeCoroutine());
    }

    // 피버 타임 종료 코루틴
    IEnumerator EndFeverTimeCoroutine()
    {
        yield return new WaitForSeconds(feverDuration);

        // 피버 타임 종료
        isFeverTime = false;
        feverGauge = 0f;
        gameSpeed = baseGameSpeed; // 현재 점수에 맞는 게임 속도로 복귀
    }

    void GameOver()
    {
        if (isGameOver) return; // 이미 게임 오버 상태면 무시
        
        isGameOver = true;
        Debug.Log("Game Over");
        
        // 게임 일시정지
        Time.timeScale = 0f;
        
        // 결과 UI 표시
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowResultUI(score);
        }
    }
}
