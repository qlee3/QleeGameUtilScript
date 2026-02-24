using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 풀에서 꺼내 재생 후, ParticleSystem 재생이 끝나면 자동으로 풀에 반환되는 컴포넌트.
/// Muzzle / Explosion 등 단발 파티클 프리팹에 부착합니다.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class PooledParticle : MonoBehaviour
{
    private ParticleSystem ps;
    private IObjectPool<PooledParticle> pool;
    private bool hasBeenAlive;

    private void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        if (ps == null)
            ps = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// 풀 참조를 설정합니다. CombatPoolManager.GetParticle()에서 호출합니다.
    /// </summary>
    public void SetPool(IObjectPool<PooledParticle> objectPool)
    {
        pool = objectPool;
    }

    /// <summary>
    /// 파티클을 재생합니다. 위치/회전은 풀에서 꺼낼 때 미리 설정됩니다.
    /// </summary>
    public void Play()
    {
        ps.Clear();
        ps.Play();
        hasBeenAlive = false;
    }

    private void Update()
    {
        if (pool == null) return;
        if (ps.IsAlive())
            hasBeenAlive = true;
        else if (hasBeenAlive)
            pool.Release(this);
    }

    private void OnDisable()
    {
        ps?.Clear();
    }
}
