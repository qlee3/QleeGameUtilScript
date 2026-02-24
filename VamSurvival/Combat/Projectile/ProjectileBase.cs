using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 기본 투사체 컴포넌트.
/// CombatPoolManager를 통해 풀에서 꺼낸 뒤 Initialize()를 호출합니다.
/// Muzzle은 발사 위치에서, Explosion은 충돌 위치에서 풀링된 파티클로 재생됩니다.
///
/// 프리팹 설정:
///  - Rigidbody (Kinematic), Collider (Is Trigger)
///  - Physics Layer: PlayerProjectile
///  - Muzzle/Explosion: PooledParticle 컴포넌트가 붙은 파티클 프리팹 할당
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ProjectileBase : MonoBehaviour
{
    [Header("Auto Destroy")]
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private float maxDistance = 30f;

    [Header("VFX")]
    [Tooltip("발사 위치(firePoint)에서 재생할 파티클. PooledParticle 컴포넌트 필요.")]
    [SerializeField] private GameObject muzzlePrefab;

    [Tooltip("충돌 위치에서 재생할 파티클. PooledParticle 컴포넌트 필요.")]
    [SerializeField] private GameObject explosionPrefab;

    private Vector3 direction;
    private float speed;
    private float damage;
    private Vector3 startPosition;
    private Rigidbody rb;
    private bool initialized;
    private float lifetimeRemaining;
    private IObjectPool<ProjectileBase> pool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    /// <summary>
    /// 풀 참조. CombatPoolManager.GetProjectile()에서 설정합니다.
    /// </summary>
    public void SetPool(IObjectPool<ProjectileBase> objectPool)
    {
        pool = objectPool;
    }

    /// <summary>
    /// 투사체를 초기화합니다. 풀에서 꺼낸 직후 반드시 호출하세요.
    /// </summary>
    /// <param name="firePointPos">발사 위치. Muzzle 파티클 스폰 위치로 사용됩니다.</param>
    public void Initialize(Vector3 dir, float spd, float dmg, Vector3 firePointPos)
    {
        direction = dir;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f) direction.Normalize();

        speed = spd;
        damage = dmg;
        startPosition = transform.position;
        lifetimeRemaining = maxLifetime;
        initialized = true;

        if (muzzlePrefab != null && CombatPoolManager.Instance != null)
            CombatPoolManager.Instance.GetParticle(muzzlePrefab, firePointPos, Quaternion.LookRotation(direction));
    }

    private void FixedUpdate()
    {
        if (!initialized) return;

        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        lifetimeRemaining -= Time.fixedDeltaTime;
        if (lifetimeRemaining <= 0f || Vector3.Distance(startPosition, rb.position) >= maxDistance)
        {
            ReleaseToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized) return;
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;
        if (!damageable.IsAlive) return;

        Vector3 hitPos = transform.position;

        var info = new DamageInfo(damage, DamageType.Projectile, this);
        info.HitPoint = hitPos;
        info.HitDirection = direction;
        damageable.TakeDamage(info);

        if (explosionPrefab != null && CombatPoolManager.Instance != null)
            CombatPoolManager.Instance.GetParticle(explosionPrefab, hitPos, Quaternion.LookRotation(direction));

        ReleaseToPool();
    }

    /// <summary>
    /// 풀에 반환합니다. 풀이 없으면 Destroy (폴백).
    /// </summary>
    public void ReleaseToPool()
    {
        initialized = false;
        if (pool != null)
            pool.Release(this);
        else
            Destroy(gameObject);
    }

    private void OnDisable()
    {
        initialized = false;
    }
}
