using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager instance;

    public GameObject obstaclePrefab;
    public int poolSize = 10; // 풀 크기 증가 (난이도가 올라가면 여러 개 생성되므로)
    public float baseSpawnRate = 2f; // 기본 생성 간격
    public float spawnRateDecreasePerLevel = 0.1f; // 레벨당 생성 간격 감소량
    public float minSpawnRate = 0.5f; // 최소 생성 간격

    private List<GameObject> obstaclePool = new List<GameObject>();
    private float timer;
    private float lastSpawnTime = 0f; // 마지막 생성 시간 (타이머 기준)
    private float nextSpawnTime = 0f; // 다음 생성 예정 시간
    public float lastSpawnRealTime = -10f; // 마지막 생성 실제 시간 (ItemManager와의 충돌 방지용)
    private float currentSpawnRate; // 현재 생성 간격
    private int currentLevel = 1; // 현재 난이도 레벨
    private int spawnCount = 1; // 한 번에 생성할 장애물 개수

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        // 미리 장애물 풀을 생성해서 비활성화
        for (int i = 0; i < poolSize; i++)    
        {
            GameObject obstacle = Instantiate(obstaclePrefab);
            obstacle.SetActive(false);
            obstaclePool.Add(obstacle);
        }
        
        // 초기 생성 시간 설정
        currentSpawnRate = baseSpawnRate;
        lastSpawnTime = 0f;
        nextSpawnTime = currentSpawnRate;
    }

   
    void Update()
    {
        // 게임 속도에 비례해서 타이머 증가 (게임 속도가 빠를수록 더 빠르게 증가)
        if (GameManager.instance != null)
        {
            timer += Time.deltaTime * GameManager.instance.gameSpeed;
        }
        else
        {
            timer += Time.deltaTime;
        }

        // 스폰 주기 체크 (게임 속도와 무관하게 일정한 간격으로 생성)
        if (timer >= currentSpawnRate)
        {
            // 난이도에 따라 여러 개 생성 (약간의 간격을 두고 생성)
            StartCoroutine(SpawnMultipleObstacles(spawnCount));
            
            lastSpawnTime = timer;
            lastSpawnRealTime = Time.time; // 실제 생성 시간 기록
            nextSpawnTime = timer + currentSpawnRate;
            timer = 0f;
        }
        else
        {
            // 다음 생성 예정 시간 업데이트
            nextSpawnTime = lastSpawnTime + currentSpawnRate;
        }
    }

    void SpawnObstacle()
    {
        // 풀에서 비활성화된 장애물을 찾아서 활성화
        foreach (GameObject obstacle in obstaclePool)
        {
            if (!obstacle.activeInHierarchy)
            {
                // 위치 초기화 및 활성화
                float randomY = Random.Range(0, 2) == 0 ? -3.5f : -1.5f; // 바닥 또는 공중
                obstacle.transform.position = new Vector3(12f, randomY, 0f);
                obstacle.SetActive(true);
                return;
            }
        }
    }

    // 여러 개의 장애물을 겹치지 않게 거리 간격을 두고 생성
IEnumerator SpawnMultipleObstacles(int count)
{
    for (int i = 0; i < count; i++)
    {
        GameObject newObstacle = GetObstacleFromPool();
        
        if (newObstacle != null)
        {
            // 1. 생성 위치 설정
            newObstacle.transform.position = new Vector3(12f, -3.5f, 0f);
            newObstacle.SetActive(true);

            // 2. 다음 장애물 생성 전 대기 (최소 거리 보장)
            if (i < count - 1)
            {
                // 앞 장애물이 최소 3만큼 이동할 때까지 대기
                // (이 수치는 장애물 크기에 따라 2.5f ~ 4.0f 사이로 조절하세요)
                float minDistance = 3.5f; 
                Vector3 lastPos = newObstacle.transform.position;

                // 앞의 장애물이 12f 위치에서 (12f - minDistance) 위치까지 갈 때까지 루프
                while (newObstacle.activeInHierarchy && newObstacle.transform.position.x > (12f - minDistance))
                {
                    yield return null; // 다음 프레임까지 대기
                }
            }
        }
    }
}

// 풀에서 비활성 오브젝트를 찾는 로직을 별도 함수로 분리
GameObject GetObstacleFromPool()
{
    foreach (GameObject obstacle in obstaclePool)
    {
        if (!obstacle.activeInHierarchy)
        {
            return obstacle;
        }
    }
    return null; // 풀이 모자랄 경우
}

    // 난이도 변경 시 호출되는 함수
    public void OnDifficultyChanged(int level)
    {
        currentLevel = level;
        
        
        
        // 레벨에 따라 생성 간격 감소
        currentSpawnRate = baseSpawnRate - (spawnRateDecreasePerLevel * (level - 1));
        currentSpawnRate = Mathf.Max(currentSpawnRate, minSpawnRate); // 최소 간격 보장
        
        Debug.Log($"난이도 레벨 {level}: 장애물 생성 개수 {spawnCount}개, 생성 간격 {currentSpawnRate}초");
    }

    // 다음 장애물 생성 예정 시간 반환 (아이템 생성기와의 충돌 방지용)
    public float GetNextSpawnTime()
    {
        return nextSpawnTime;
    }

    // 현재 타이머 값 반환
    public float GetCurrentTimer()
    {
        return timer;
    }

    // 장애물 생성 주기 반환
    public float GetSpawnRate()
    {
        return currentSpawnRate;
    }

    // 현재 활성화된 장애물 목록 반환 (아이템 생성기에서 위치 확인용)
    public List<GameObject> GetActiveObstacles()
    {
        List<GameObject> activeObstacles = new List<GameObject>();
        foreach (GameObject obstacle in obstaclePool)
        {
            if (obstacle != null && obstacle.activeInHierarchy)
            {
                activeObstacles.Add(obstacle);
            }
        }
        return activeObstacles;
    }
}
