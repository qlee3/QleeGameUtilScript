using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 투사체(ProjectileBase)와 파티클 VFX(PooledParticle)를 프리팹별로 풀링하는 씬 종속 싱글톤.
/// GamePlay 씬에 빈 GameObject + 이 컴포넌트를 배치하세요.
/// </summary>
public class CombatPoolManager : MonoBehaviour
{
    public static CombatPoolManager Instance { get; private set; }

    [Header("Pool Capacity")]
    [SerializeField] private int projectilePoolDefaultCapacity = 32;
    [SerializeField] private int projectilePoolMaxSize = 128;
    [SerializeField] private int particlePoolDefaultCapacity = 16;
    [SerializeField] private int particlePoolMaxSize = 64;

    private Dictionary<GameObject, IObjectPool<ProjectileBase>> projectilePools;
    private Dictionary<GameObject, IObjectPool<PooledParticle>> particlePools;
    private Transform projectileRoot;
    private Transform particleRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        projectilePools = new Dictionary<GameObject, IObjectPool<ProjectileBase>>();
        particlePools = new Dictionary<GameObject, IObjectPool<PooledParticle>>();

        projectileRoot = new GameObject("ProjectilePool").transform;
        projectileRoot.SetParent(transform);

        particleRoot = new GameObject("ParticlePool").transform;
        particleRoot.SetParent(transform);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// 투사체를 풀에서 꺼내 반환합니다. 위치/회전 설정 후 Initialize()를 호출하세요.
    /// </summary>
    public ProjectileBase GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        var pool = GetOrCreateProjectilePool(prefab);
        var proj = pool.Get();
        proj.transform.SetPositionAndRotation(position, rotation);
        proj.gameObject.SetActive(true);
        proj.SetPool(pool);
        return proj;
    }

    /// <summary>
    /// 투사체를 풀에 반환합니다. ProjectileBase 내부에서 호출합니다.
    /// </summary>
    public void ReleaseProjectile(ProjectileBase proj)
    {
        proj?.ReleaseToPool();
    }

    /// <summary>
    /// 파티클을 풀에서 꺼내 지정 위치/회전에서 재생합니다. 재생 완료 시 자동 반환됩니다.
    /// </summary>
    public void GetParticle(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return;

        var pool = GetOrCreateParticlePool(prefab);
        var vfx = pool.Get();
        vfx.transform.SetPositionAndRotation(position, rotation);
        vfx.gameObject.SetActive(true);
        vfx.SetPool(pool);
        vfx.Play();
    }

    private IObjectPool<ProjectileBase> GetOrCreateProjectilePool(GameObject prefab)
    {
        if (!projectilePools.TryGetValue(prefab, out var pool))
        {
            GameObject key = prefab;
            pool = new ObjectPool<ProjectileBase>(
                createFunc: () =>
                {
                    var go = Instantiate(key, projectileRoot);
                    go.SetActive(false);
                    var p = go.GetComponent<ProjectileBase>();
                    if (p == null)
                        p = go.AddComponent<ProjectileBase>();
                    return p;
                },
                actionOnGet: null,
                actionOnRelease: p => p.gameObject.SetActive(false),
                actionOnDestroy: p => Destroy(p.gameObject),
                defaultCapacity: projectilePoolDefaultCapacity,
                maxSize: projectilePoolMaxSize
            );
            projectilePools[key] = pool;
        }
        return pool;
    }

    private IObjectPool<PooledParticle> GetOrCreateParticlePool(GameObject prefab)
    {
        if (!particlePools.TryGetValue(prefab, out var pool))
        {
            GameObject key = prefab;
            pool = new ObjectPool<PooledParticle>(
                createFunc: () =>
                {
                    var go = Instantiate(key, particleRoot);
                    go.SetActive(false);
                    var p = go.GetComponent<PooledParticle>();
                    if (p == null)
                        p = go.AddComponent<PooledParticle>();
                    return p;
                },
                actionOnGet: null,
                actionOnRelease: p => p.gameObject.SetActive(false),
                actionOnDestroy: p => Destroy(p.gameObject),
                defaultCapacity: particlePoolDefaultCapacity,
                maxSize: particlePoolMaxSize
            );
            particlePools[key] = pool;
        }
        return pool;
    }
}
