using UnityEngine;

/// <summary>
/// 기본 투사체 컴포넌트.
/// ProjectileAbilityData.Execute()에서 Instantiate 후 Initialize()를 호출합니다.
/// XZ 평면에서 직선 이동하며, 적과 Trigger 충돌 시 IDamageable.TakeDamage()를 호출하고 소멸합니다.
///
/// 프리팹 설정:
///  - Rigidbody (자동 설정됨)
///  - Collider (Is Trigger = true, 적 레이어 감지용)
///  - Physics Layer: PlayerProjectile (Enemy 레이어와 충돌 설정)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ProjectileBase : MonoBehaviour
{
    [Header("Auto Destroy")]
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private float maxDistance = 30f;

    private Vector3 direction;
    private float speed;
    private float damage;
    private Vector3 startPosition;
    private Rigidbody rb;
    private bool initialized;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    /// <summary>
    /// 투사체를 초기화합니다. Instantiate 직후 반드시 호출하세요.
    /// </summary>
    public void Initialize(Vector3 dir, float spd, float dmg)
    {
        direction = dir;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f) direction.Normalize();

        speed = spd;
        damage = dmg;
        startPosition = transform.position;
        initialized = true;

        Destroy(gameObject, maxLifetime);
    }

    private void FixedUpdate()
    {
        if (!initialized) return;

        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        if (Vector3.Distance(startPosition, rb.position) >= maxDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized) return;
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;
        if (!damageable.IsAlive) return;

        var info = new DamageInfo(damage, DamageType.Projectile, this);
        info.HitPoint = transform.position;
        info.HitDirection = direction;
        damageable.TakeDamage(info);

        Destroy(gameObject);
    }
}
