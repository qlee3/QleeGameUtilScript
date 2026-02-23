using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 공용 투사체 런타임 본체.
/// 이동/충돌/피해/수명 관리와 풀 반환을 담당합니다.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ProjectileBase : MonoBehaviour
{
    [Header("Optional References")]
    [SerializeField] private Collider hitCollider;

    [Header("Effect (ETFX-style)")]
    [SerializeField] private GameObject muzzleEffectPrefab;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private float muzzleEffectLifetime = 1.5f;
    [SerializeField] private float impactEffectLifetime = 3f;

    private Rigidbody rb;
    private ProjectileData data;
    private IProjectileMovement movement;
    private IObjectPool<ProjectileBase> pool;
    private object owner;

    private float lifeRemaining;
    private int hitCount;
    private float runtimeDamage;
    private int runtimeTargetLayerMask;
    private Vector3 travelDirection = Vector3.forward;
    private bool isInitialized;

    private readonly HashSet<IDamageable> hitTargets = new();
    private ParticleSystem[] particleSystems;
    private TrailRenderer[] trailRenderers;

    private void Awake()
    {
        if (hitCollider == null)
        {
            hitCollider = GetComponent<Collider>();
        }
        rb = GetComponent<Rigidbody>();

        if (hitCollider != null)
        {
            hitCollider.isTrigger = true;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
    }

    /// <summary>
    /// 투사체를 초기화합니다.
    /// </summary>
    public void Initialize(
        ProjectileData projectileData,
        object sourceOwner,
        Vector3 direction,
        Transform homingTarget = null,
        IObjectPool<ProjectileBase> objectPool = null,
        float overrideDamage = -1f,
        int overrideTargetLayerMask = -1
    )
    {
        data = projectileData;
        owner = sourceOwner;
        pool = objectPool;

        lifeRemaining = data != null ? data.lifeTime : 0f;
        hitCount = 0;
        hitTargets.Clear();
        isInitialized = data != null;

        if (!isInitialized)
        {
            Release();
            return;
        }

        runtimeDamage = overrideDamage >= 0f ? overrideDamage : data.damage;
        runtimeTargetLayerMask = overrideTargetLayerMask >= 0
            ? overrideTargetLayerMask
            : data.targetLayers.value;

        travelDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        movement = ProjectileMovementFactory.Create(data.movementType);
        movement.Initialize(this, data, travelDirection, homingTarget);

        transform.forward = travelDirection;
    }

    private void Update()
    {
        if (!isInitialized) return;

        lifeRemaining -= Time.deltaTime;
        if (lifeRemaining <= 0f)
        {
            Release();
            return;
        }

        movement?.Tick(Time.deltaTime);
        if (travelDirection.sqrMagnitude > 0.001f)
        {
            transform.forward = travelDirection;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized || data == null) return;
        if (!CanHitCollider(other)) return;
        if (!TryResolveDamageable(other, out IDamageable target)) return;
        if (!target.IsAlive) return;

        if (!data.allowMultiHitSameTarget && hitTargets.Contains(target))
        {
            return;
        }

        hitTargets.Add(target);
        hitCount++;

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 hitDirection = travelDirection.sqrMagnitude > 0.001f
            ? travelDirection
            : (other.transform.position - transform.position).normalized;
        Vector3 hitNormal = (transform.position - hitPoint).sqrMagnitude > 0.001f
            ? (transform.position - hitPoint).normalized
            : -hitDirection;

        SpawnImpactEffect(hitPoint, hitNormal);

        target.TakeDamage(new DamageInfo
        {
            Amount = runtimeDamage,
            Type = DamageType.Projectile,
            Source = owner ?? this,
            HitPoint = hitPoint,
            HitDirection = hitDirection,
        });

        int maxHits = 1 + Mathf.Max(0, data.penetrationCount);
        if (hitCount >= maxHits)
        {
            Release();
        }
    }

    public void SetTravelDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f) return;
        travelDirection = direction.normalized;
    }

    private bool CanHitCollider(Collider other)
    {
        if (other == null) return false;
        if (other.transform == transform) return false;

        if (owner is Component ownerComponent)
        {
            if (other.transform.IsChildOf(ownerComponent.transform))
            {
                return false;
            }
        }
        else if (owner is GameObject ownerGameObject)
        {
            if (other.transform.IsChildOf(ownerGameObject.transform))
            {
                return false;
            }
        }
        else if (owner is Transform ownerTransform)
        {
            if (other.transform.IsChildOf(ownerTransform))
            {
                return false;
            }
        }

        if (runtimeTargetLayerMask != 0)
        {
            int layerBit = 1 << other.gameObject.layer;
            if ((runtimeTargetLayerMask & layerBit) == 0)
            {
                return false;
            }
        }

        return true;
    }

    private bool TryResolveDamageable(Collider other, out IDamageable target)
    {
        if (other.TryGetComponent(out target))
        {
            return true;
        }

        if (other.GetComponentInParent<IDamageable>() is IDamageable parentDamageable)
        {
            target = parentDamageable;
            return true;
        }

        return false;
    }

    private void Release()
    {
        isInitialized = false;
        movement = null;
        hitTargets.Clear();

        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 풀에서 꺼낸 직후 시각 요소를 초기화합니다.
    /// 위치/회전이 먼저 잡힌 뒤 호출해야 합니다.
    /// </summary>
    public void OnSpawnedFromPool()
    {
        ResetTrails();
        RestartPlayOnAwakeParticles();
    }

    /// <summary>
    /// 풀로 반환하기 직전 시각 요소를 정리합니다.
    /// </summary>
    public void OnReleasedToPool()
    {
        StopAndClearAllParticles();
        ResetTrails();
    }

    private void RestartPlayOnAwakeParticles()
    {
        if (particleSystems == null) return;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem ps = particleSystems[i];
            if (ps == null) continue;

            if (ps.main.playOnAwake)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play(true);
            }
        }
    }

    private void StopAndClearAllParticles()
    {
        if (particleSystems == null) return;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem ps = particleSystems[i];
            if (ps == null) continue;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void ResetTrails()
    {
        if (trailRenderers == null) return;

        for (int i = 0; i < trailRenderers.Length; i++)
        {
            TrailRenderer tr = trailRenderers[i];
            if (tr == null) continue;
            tr.Clear();
        }
    }

    private void SpawnImpactEffect(Vector3 position, Vector3 normal)
    {
        if (impactEffectPrefab == null) return;

        Quaternion rotation = normal.sqrMagnitude > 0.001f
            ? Quaternion.FromToRotation(Vector3.up, normal.normalized)
            : Quaternion.identity;

        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.SpawnEffect(
                impactEffectPrefab,
                position,
                rotation,
                impactEffectLifetime
            );
        }
        else
        {
            GameObject impact = Instantiate(impactEffectPrefab, position, rotation);
            Destroy(impact, impactEffectLifetime);
        }
    }

    public void SpawnMuzzleEffect(Vector3 position, Quaternion rotation)
    {
        if (muzzleEffectPrefab == null) return;

        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.SpawnEffect(
                muzzleEffectPrefab,
                position,
                rotation,
                muzzleEffectLifetime
            );
        }
        else
        {
            GameObject muzzle = Instantiate(muzzleEffectPrefab, position, rotation);
            Destroy(muzzle, muzzleEffectLifetime);
        }
    }
}
