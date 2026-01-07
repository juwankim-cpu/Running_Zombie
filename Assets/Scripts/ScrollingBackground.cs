using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [Tooltip("배경이 흐르는 속도")]
    public float speed = 0.5f;
    
    private MeshRenderer meshRenderer;

    void Awake()
    {
        // 텍스처를 조작하기 위해 MeshRenderer를 가져옵니다.
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        // 시간에 따라 X축 오프셋을 계산합니다.
        // GameManager의 GameSpeed를 곱합니다
        Vector2 offset = new Vector2(Time.time * speed 
        * GameManager.instance.gameSpeed, 0);
        
        // 메인 텍스처의 오프셋에 적용하여 이미지를 움직입니다.
        meshRenderer.material.mainTextureOffset = offset;
    }
}
