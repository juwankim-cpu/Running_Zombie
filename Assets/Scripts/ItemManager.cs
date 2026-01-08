using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    [Header("Health Item Settings")]
    public GameObject healthItemPrefab;
    public int healthItemPoolSize = 3;
    public float healthItemSpawnRate = 5f; // 체력 회복 아이템 생성 주기

    [Header("Fever Item Settings")]
    public GameObject feverItemPrefab;
    public int feverItemPoolSize = 3;
    public float feverItemSpawnRate = 7f; // 피버 타임 아이템 생성 주기

    [Header("Spawn Settings")]
    public float spawnX = 12f; // 장애물 생성기와 같은 x값
    public Transform playerTransform; // 플레이어 Transform 참조

    private List<GameObject> healthItemPool = new List<GameObject>();
    private List<GameObject> feverItemPool = new List<GameObject>();
    private float healthItemTimer;
    private float feverItemTimer;
    private float groundY = -3.5f; // 바닥 y 위치 (ObstacleManager와 동일)
    private const float minSpawnInterval = 0.3f; // 최소 생성 간격 (다른 오브젝트와 겹치지 않도록)
    private float lastHealthItemSpawnTime = -10f; // 마지막 체력 아이템 생성 시간
    private float lastFeverItemSpawnTime = -10f; // 마지막 피버 아이템 생성 시간
    private float lastObstacleSpawnTime = -10f; // 마지막 장애물 생성 시간
    private const float obstacleY = -3.5f; // 장애물 Y 위치
    private const float obstacleHeight = 1f; // 장애물 높이 (콜라이더 크기)
    private const float minItemY = -2.0f; // 아이템 최소 Y값 (장애물과 겹치지 않도록)
    private const float maxItemY = 0.77f; // 아이템 최대 Y값

    void Start()
    {
        // 체력 회복 아이템 풀 생성
        if (healthItemPrefab != null)
        {
            for (int i = 0; i < healthItemPoolSize; i++)
            {
                GameObject item = Instantiate(healthItemPrefab);
                item.SetActive(false);
                healthItemPool.Add(item);
            }
        }

        // 피버 타임 아이템 풀 생성
        if (feverItemPrefab != null)
        {
            for (int i = 0; i < feverItemPoolSize; i++)
            {
                GameObject item = Instantiate(feverItemPrefab);
                item.SetActive(false);
                feverItemPool.Add(item);
            }
        }

        // 플레이어 Transform 찾기
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // 각 아이템 생성 타이머를 서로 다른 오프셋으로 시작 (겹침 방지)
        healthItemTimer = 0.3f; // 체력 아이템은 0.3초 오프셋
        feverItemTimer = 0.6f; // 피버 아이템은 0.6초 오프셋
    }

    void Update()
    {
        // ObstacleManager의 마지막 생성 시간 업데이트
        if (ObstacleManager.instance != null)
        {
            float obstacleLastSpawn = ObstacleManager.instance.lastSpawnRealTime;
            if (obstacleLastSpawn > lastObstacleSpawnTime)
            {
                lastObstacleSpawnTime = obstacleLastSpawn;
            }
        }

        // 게임 속도에 비례해서 타이머 증가
        float timeDelta = GameManager.instance != null 
            ? Time.deltaTime * GameManager.instance.gameSpeed 
            : Time.deltaTime;

        // 체력 회복 아이템 생성 타이머
        healthItemTimer += timeDelta;
        if (healthItemTimer >= healthItemSpawnRate)
        {
            // 다른 생성기와 겹치지 않는지 확인
            if (CanSpawnItem(true)) // true = 체력 아이템
            {
                SpawnHealthItem();
                lastHealthItemSpawnTime = Time.time;
                healthItemTimer = 0f;
            }
            else
            {
                // 겹치면 조금 늦춰서 생성 (다음 프레임으로 미룸)
                healthItemTimer = healthItemSpawnRate - 0.05f;
            }
        }

        // 피버 타임 아이템 생성 타이머
        feverItemTimer += timeDelta;
        if (feverItemTimer >= feverItemSpawnRate)
        {
            // 다른 생성기와 겹치지 않는지 확인
            if (CanSpawnItem(false)) // false = 피버 아이템
            {
                SpawnFeverItem();
                lastFeverItemSpawnTime = Time.time;
                feverItemTimer = 0f;
            }
            else
            {
                // 겹치면 조금 늦춰서 생성 (다음 프레임으로 미룸)
                feverItemTimer = feverItemSpawnRate - 0.05f;
            }
        }
    }

    void SpawnHealthItem()
    {
        if (healthItemPrefab == null) return;

        // 풀에서 비활성화된 아이템 찾아서 활성화
        foreach (GameObject item in healthItemPool)
        {
            if (!item.activeInHierarchy)
            {
                float randomY = GetRandomSpawnY();
                item.transform.position = new Vector3(spawnX, randomY, 0f);
                item.SetActive(true);
                return;
            }
        }
    }

    void SpawnFeverItem()
    {
        if (feverItemPrefab == null) return;

        // 풀에서 비활성화된 아이템 찾아서 활성화
        foreach (GameObject item in feverItemPool)
        {
            if (!item.activeInHierarchy)
            {
                float randomY = GetRandomSpawnY();
                item.transform.position = new Vector3(spawnX, randomY, 0f);
                item.SetActive(true);
                return;
            }
        }
    }

    // 아이템 생성 Y값 랜덤 생성 (장애물과 겹치지 않도록)
    float GetRandomSpawnY()
    {
        float y;
        int attempts = 0;
        const int maxAttempts = 10; // 최대 시도 횟수
        
        do
        {
            // minItemY ~ maxItemY 사이의 랜덤 Y값 생성
            y = Random.Range(minItemY, maxItemY);
            attempts++;
            
            // 장애물과 겹치지 않는지 확인
            if (!IsPositionOverlappingWithObstacle(y))
            {
                return y;
            }
        } 
        while (attempts < maxAttempts);
        
        // 여러 번 시도해도 겹치면 장애물 위쪽에 강제로 배치
        return Random.Range(-2.0f, maxItemY);
    }

    // 지정된 Y 위치가 장애물과 겹치는지 확인
    bool IsPositionOverlappingWithObstacle(float itemY)
    {
        // ObstacleManager를 통해 현재 활성화된 장애물 확인
        if (ObstacleManager.instance != null)
        {
            List<GameObject> activeObstacles = ObstacleManager.instance.GetActiveObstacles();
            
            foreach (GameObject obstacle in activeObstacles)
            {
                if (obstacle != null && obstacle.activeInHierarchy)
                {
                    float obstaclePosY = obstacle.transform.position.y;
                    float obstacleTop = obstaclePosY + (obstacleHeight * 0.5f);
                    float obstacleBottom = obstaclePosY - (obstacleHeight * 0.5f);
                    
                    // 아이템과 장애물의 Y 범위가 겹치는지 확인
                    // 장애물 범위: obstacleBottom ~ obstacleTop
                    // 안전 거리 0.5 추가하여 완전히 겹치지 않도록 함
                    if (itemY >= obstacleBottom - 0.5f && itemY <= obstacleTop + 0.5f)
                    {
                        return true; // 겹침
                    }
                }
            }
        }
        
        // 기본 장애물 위치와도 겹치지 않는지 확인 (아이템 생성 시점에 장애물이 없을 수도 있으므로)
        float defaultObstacleTop = obstacleY + (obstacleHeight * 0.5f);
        float defaultObstacleBottom = obstacleY - (obstacleHeight * 0.5f);
        if (itemY >= defaultObstacleBottom - 0.5f && itemY <= defaultObstacleTop + 0.5f)
        {
            return true; // 겹침
        }
        
        return false; // 겹치지 않음
    }

    // 아이템 생성 가능 여부 확인 (다른 생성기와 겹치지 않는지 체크)
    // isHealthItem: true = 체력 아이템, false = 피버 아이템
    bool CanSpawnItem(bool isHealthItem)
    {
        float currentTime = Time.time;

        // 장애물 생성 시간과의 차이 확인
        if (Mathf.Abs(currentTime - lastObstacleSpawnTime) < minSpawnInterval)
        {
            return false;
        }

        // 체력 아이템과 피버 아이템 간의 겹침 확인
        if (isHealthItem)
        {
            // 체력 아이템 생성 시 피버 아이템 생성 시간과의 차이 확인
            if (Mathf.Abs(currentTime - lastFeverItemSpawnTime) < minSpawnInterval)
            {
                return false;
            }
        }
        else
        {
            // 피버 아이템 생성 시 체력 아이템 생성 시간과의 차이 확인
            if (Mathf.Abs(currentTime - lastHealthItemSpawnTime) < minSpawnInterval)
            {
                return false;
            }
        }

        return true;
    }
}