using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 풀링된 이펙트 오브젝트의 자동 반환과 시각 상태 리셋을 담당합니다.
/// </summary>
public class PooledEffectAutoRelease : MonoBehaviour
{
    private IObjectPool<GameObject> pool;
    private ParticleSystem[] particleSystems;
    private TrailRenderer[] trailRenderers;
    private int playToken;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
    }

    public void SetPool(IObjectPool<GameObject> effectPool)
    {
        pool = effectPool;
    }

    public void OnSpawned(float lifetime)
    {
        playToken++;

        ResetTrails();
        RestartPlayOnAwakeParticles();

        if (lifetime > 0f)
        {
            StartCoroutine(ReleaseAfterLifetime(lifetime, playToken));
        }
    }

    public void OnReleased()
    {
        StopAllCoroutines();
        StopAndClearAllParticles();
        ResetTrails();
    }

    private IEnumerator ReleaseAfterLifetime(float lifetime, int tokenAtStart)
    {
        yield return new WaitForSeconds(lifetime);

        if (tokenAtStart != playToken) yield break;
        if (pool == null) yield break;

        pool.Release(gameObject);
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
}
