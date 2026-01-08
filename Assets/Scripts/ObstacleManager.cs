using UnityEngine;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager instance;

    public GameObject obstaclePrefab;
    public int poolSize = 5;
    public float spawnRate = 2f;

    private List<GameObject> obstaclePool = new List<GameObject>();
    private float timer;
    private float lastSpawnTime = 0f; // 마지막 생성 시간 (타이머 기준)
    private float nextSpawnTime = 0f; // 다음 생성 예정 시간
    public float lastSpawnRealTime = -10f; // 마지막 생성 실제 시간 (ItemManager와의 충돌 방지용)

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
        lastSpawnTime = 0f;
        nextSpawnTime = spawnRate;
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
        if (timer >= spawnRate)
        {
            SpawnObstacle();
            lastSpawnTime = timer;
            lastSpawnRealTime = Time.time; // 실제 생성 시간 기록
            nextSpawnTime = timer + spawnRate;
            timer = 0f;
        }
        else
        {
            // 다음 생성 예정 시간 업데이트
            nextSpawnTime = lastSpawnTime + spawnRate;
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
                obstacle.transform.position = new Vector3(12f, -3.5f, 0f);
                obstacle.SetActive(true);
                return;
            }
        }
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
        return spawnRate;
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
