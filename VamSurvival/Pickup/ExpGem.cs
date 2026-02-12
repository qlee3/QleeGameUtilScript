using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

/// <summary>
/// 경험치 젬 아이템.
/// 적 사망 시 스폰되며, 플레이어에게 자석처럼 끌려가 경험치를 부여합니다.
/// DOTween을 사용한 스폰 scatter 연출 + Transform 기반 자석 이동.
/// </summary>
public class ExpGem : MonoBehaviour
{
    [Header("Magnet")]
    [Tooltip("자석 흡수가 시작되는 거리")]
    [SerializeField] private float magnetRadius = 3f;

    [Tooltip("자석 이동 기본 속도")]
    [SerializeField] private float magnetSpeed = 8f;

    [Tooltip("자석 가속 배율 (가까울수록 빨라짐)")]
    [SerializeField] private float magnetAcceleration = 2f;

    [Tooltip("실제 획득 판정 거리")]
    [SerializeField] private float pickupRadius = 0.5f;

    [Header("Scatter")]
    [Tooltip("스폰 시 퍼지는 최대 거리")]
    [SerializeField] private float scatterDistance = 1.5f;

    [Tooltip("스폰 scatter 연출 시간")]
    [SerializeField] private float scatterDuration = 0.4f;

    [Tooltip("스폰 시 위로 튀는 높이")]
    [SerializeField] private float scatterJumpHeight = 1f;

    // ── 런타임 상태 ──
    private float expValue;
    private Transform playerTransform;
    private PlayerLevel playerLevel;
    private bool isMagneted;
    private bool isScattering;      // scatter 연출 중에는 자석 비활성
    private IObjectPool<ExpGem> pool;
    private Tween scatterTween;

    /// <summary>
    /// 젬을 초기화합니다. ExpGemSpawner에서 풀로부터 꺼낸 직후 호출하세요.
    /// </summary>
    /// <param name="exp">이 젬의 경험치 값.</param>
    /// <param name="objectPool">회수할 오브젝트 풀 참조. null이면 Destroy로 제거.</param>
    public void Initialize(float exp, IObjectPool<ExpGem> objectPool = null)
    {
        expValue = exp;
        pool = objectPool;
        isMagneted = false;
        isScattering = false;

        // 플레이어 캐싱
        if (playerTransform == null)
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
                playerLevel = player.Level;
            }
        }

        ScatterOnSpawn();
    }

    private void Update()
    {
        if (playerTransform == null || isScattering) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 자석 범위 진입
        if (!isMagneted && distance <= magnetRadius)
        {
            isMagneted = true;
        }

        // 자석 이동
        if (isMagneted)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;

            // 가까울수록 가속: baseSpeed + acceleration * (1 - distance/magnetRadius)
            float speedMultiplier = 1f + magnetAcceleration * Mathf.Clamp01(1f - distance / magnetRadius);
            float speed = magnetSpeed * speedMultiplier;

            transform.position += direction * speed * Time.deltaTime;
        }

        // 획득 판정
        if (distance <= pickupRadius)
        {
            Collect();
        }
    }

    /// <summary>
    /// DOTween으로 스폰 scatter 연출.
    /// 랜덤 방향으로 포물선을 그리며 튀어나감.
    /// </summary>
    private void ScatterOnSpawn()
    {
        isScattering = true;

        // 랜덤 XZ 방향
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(0.5f, 1f);
        Vector3 scatterTarget = transform.position + new Vector3(
            randomCircle.x * scatterDistance,
            0f,
            randomCircle.y * scatterDistance
        );

        // DOTween Jump: 포물선 이동 (Y축 점프 + XZ 이동)
        scatterTween = transform.DOJump(scatterTarget, scatterJumpHeight, 1, scatterDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                isScattering = false;
            });
    }

    private void Collect()
    {
        if (playerLevel != null)
        {
            playerLevel.AddExp(expValue);
        }

        Release();
    }

    /// <summary>풀로 회수하거나, 풀이 없으면 Destroy.</summary>
    private void Release()
    {
        // 진행 중인 트윈 정리
        scatterTween?.Kill();
        scatterTween = null;
        isMagneted = false;
        isScattering = false;

        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        // 풀로 회수될 때 트윈 정리
        scatterTween?.Kill();
        scatterTween = null;
    }
}
