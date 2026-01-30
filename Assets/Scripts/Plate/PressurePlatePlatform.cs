using UnityEngine;

public class PressurePlatePlatform : MonoBehaviour
{
    [Header("平台移动设置")]
    [SerializeField] private float riseHeight = 2f;      // 平台升起的高度
    [SerializeField] private float riseSpeed = 2f;       // 升起速度
    [SerializeField] private float lowerSpeed = 1f;      // 下降速度
    [SerializeField] private bool stayUp = false;        // 玩家离开后是否保持升起

    [Header("状态指示器")]
    [SerializeField] private Material activeMaterial;    // 激活时的材质
    [SerializeField] private Material inactiveMaterial;  // 未激活时的材质

    private Vector3 startPosition;     // 初始位置
    private Vector3 targetPosition;    // 目标位置
    private int playersOnPlatform = 0; // 平台上的玩家数量
    private bool isRising = false;     // 是否正在升起
    private MeshRenderer meshRenderer; // 平台渲染器

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition;
        meshRenderer = GetComponent<MeshRenderer>();

        // 设置初始材质
        if (meshRenderer != null && inactiveMaterial != null)
            meshRenderer.material = inactiveMaterial;
    }

    void Update()
    {
        // 平滑移动到目标位置
        float speed = isRising ? riseSpeed : lowerSpeed;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    void OnCollisionEnter(Collision collision)
    {
        // 检测玩家踩上平台（假设玩家标签为"Player"）
        if (collision.gameObject.CompareTag("Player"))
        {
            playersOnPlatform++;
            CheckPlatformState();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // 检测玩家离开平台
        if (collision.gameObject.CompareTag("Player"))
        {
            playersOnPlatform = Mathf.Max(0, playersOnPlatform - 1);
            CheckPlatformState();
        }
    }

    void CheckPlatformState()
    {
        if (playersOnPlatform > 0)
        {
            // 有玩家在平台上，升起
            targetPosition = startPosition + Vector3.up * riseHeight;
            isRising = true;

            // 切换材质（视觉反馈）
            if (meshRenderer != null && activeMaterial != null)
                meshRenderer.material = activeMaterial;
        }
        else if (!stayUp)
        {
            // 没有玩家且不保持升起，下降
            targetPosition = startPosition;
            isRising = false;

            // 切换材质
            if (meshRenderer != null && inactiveMaterial != null)
                meshRenderer.material = inactiveMaterial;
        }
    }

    // 在Scene视图中显示触发范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            transform.position + Vector3.up * riseHeight / 2,
            new Vector3(transform.localScale.x, riseHeight, transform.localScale.z)
        );
    }
}