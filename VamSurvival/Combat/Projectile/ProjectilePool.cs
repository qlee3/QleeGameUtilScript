using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// ProjectileData 기준 오브젝트 풀 매니저.
/// 공용 씬 오브젝트로 두고 플레이어/적 발사체를 함께 관리합니다.
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [SerializeField] private ProjectileData[] preconfiguredProjectiles;
    [SerializeField] private int defaultCapacity = 16;
    [SerializeField] private int maxSize = 128;
    [SerializeField] private int effectDefaultCapacity = 8;
    [SerializeField] private int effectMaxSize = 64;

    private readonly Dictionary<ProjectileData, IObjectPool<ProjectileBase>> pools = new();
    private readonly Dictionary<GameObject, IObjectPool<GameObject>> effectPools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildPreconfiguredPools();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public ProjectileBase Spawn(
        ProjectileData data,
        Vector3 position,
        Quaternion rotation,
        object owner,
        Vector3 direction,
        Transform homingTarget = null,
        float overrideDamage = -1f,
        int overrideTargetLayerMask = -1
    )
    {
        if (data == null || data.projectilePrefab == null)
        {
            Debug.LogWarning("[ProjectilePool] ProjectileData 또는 prefab이 비어 있습니다.");
            return null;
        }

        IObjectPool<ProjectileBase> pool = GetOrCreatePool(data);
        ProjectileBase projectile = pool.Get();

        projectile.transform.SetPositionAndRotation(position, rotation);
        projectile.Initialize(data, owner, direction, homingTarget, pool, overrideDamage, overrideTargetLayerMask);
        projectile.gameObject.SetActive(true);
        projectile.OnSpawnedFromPool();
        return projectile;
    }

    public GameObject SpawnEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation, float lifeTime)
    {
        if (effectPrefab == null) return null;

        IObjectPool<GameObject> pool = GetOrCreateEffectPool(effectPrefab);
        GameObject effect = pool.Get();
        effect.transform.SetPositionAndRotation(position, rotation);
        effect.SetActive(true);

        if (effect.TryGetComponent(out PooledEffectAutoRelease autoRelease))
        {
            autoRelease.OnSpawned(lifeTime);
        }
        else
        {
            // 방어 코드: 컴포넌트가 없으면 비풀링 fallback
            Destroy(effect, lifeTime);
        }

        return effect;
    }

    private void BuildPreconfiguredPools()
    {
        if (preconfiguredProjectiles == null) return;

        for (int i = 0; i < preconfiguredProjectiles.Length; i++)
        {
            ProjectileData data = preconfiguredProjectiles[i];
            if (data == null || data.projectilePrefab == null) continue;
            GetOrCreatePool(data);
        }
    }

    private IObjectPool<ProjectileBase> GetOrCreatePool(ProjectileData data)
    {
        if (pools.TryGetValue(data, out IObjectPool<ProjectileBase> existing))
        {
            return existing;
        }

        ProjectileBase prefab = data.projectilePrefab;
        var pool = new ObjectPool<ProjectileBase>(
            createFunc: () =>
            {
                ProjectileBase instance = Instantiate(prefab, transform);
                instance.gameObject.SetActive(false);
                return instance;
            },
            actionOnGet: _ => { },
            actionOnRelease: instance =>
            {
                instance.OnReleasedToPool();
                instance.gameObject.SetActive(false);
            },
            actionOnDestroy: instance =>
            {
                if (instance != null)
                {
                    Destroy(instance.gameObject);
                }
            },
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        pools.Add(data, pool);
        return pool;
    }

    private IObjectPool<GameObject> GetOrCreateEffectPool(GameObject effectPrefab)
    {
        if (effectPools.TryGetValue(effectPrefab, out IObjectPool<GameObject> existing))
        {
            return existing;
        }

        IObjectPool<GameObject> effectPool = null;
        effectPool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject instance = Instantiate(effectPrefab, transform);
                instance.SetActive(false);

                PooledEffectAutoRelease autoRelease = instance.GetComponent<PooledEffectAutoRelease>();
                if (autoRelease == null)
                {
                    autoRelease = instance.AddComponent<PooledEffectAutoRelease>();
                }
                autoRelease.SetPool(effectPool);

                return instance;
            },
            actionOnGet: _ => { },
            actionOnRelease: instance =>
            {
                if (instance.TryGetComponent(out PooledEffectAutoRelease autoRelease))
                {
                    autoRelease.OnReleased();
                }
                instance.SetActive(false);
            },
            actionOnDestroy: instance =>
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            },
            collectionCheck: false,
            defaultCapacity: effectDefaultCapacity,
            maxSize: effectMaxSize
        );

        effectPools.Add(effectPrefab, effectPool);
        return effectPool;
    }
}
