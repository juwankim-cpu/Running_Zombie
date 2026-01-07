using UnityEngine;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public int poolSize = 5;
    public float spawnRate = 2f;

    private List<GameObject> obstaclePool = new List<GameObject>();
    private float timer;

    void Start()
    {
        // 미리 장애물 풀을 생성해서 비활성화
        for (int i = 0; i < poolSize; i++)    
        {
            GameObject obstacle = Instantiate(obstaclePrefab);
            obstacle.SetActive(false);
            obstaclePool.Add(obstacle);
        }
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
            timer = 0f;
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
}
