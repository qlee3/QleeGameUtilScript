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

    private readonly Dictionary<ProjectileData, IObjectPool<ProjectileBase>> pools = new();

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
        float overrideDamage = -1f
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
        projectile.Initialize(data, owner, direction, homingTarget, pool, overrideDamage);
        projectile.gameObject.SetActive(true);
        projectile.OnSpawnedFromPool();
        return projectile;
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
}
