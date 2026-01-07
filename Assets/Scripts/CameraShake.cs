using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private Vector3 originalPosition;
    private float shakeDuration = 0f;
    private float shakeIntensity = 0f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            // 랜덤한 방향으로 카메라 이동
            transform.localPosition = originalPosition + Random.insideUnitSphere * shakeIntensity;
            
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            // 흔들림 종료 후 원래 위치로 복구
            shakeDuration = 0f;
            transform.localPosition = originalPosition;
        }
    }

    // 화면 흔들림 시작
    public void Shake(float duration, float intensity)
    {
        shakeDuration = duration;
        shakeIntensity = intensity;
    }
}
