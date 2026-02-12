using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 피격 시 셰이더 키워드 기반 시각 효과를 처리하는 컴포넌트.
/// GetComponentsInChildren&lt;Renderer&gt;로 materials를 캐싱하고,
/// PlayEffect(duration) 호출 시 지정된 시간 동안 효과를 표시합니다.
/// 셰이더에 "_HIT_ON" 키워드가 정의되어 있어야 합니다.
/// </summary>
public class PlayerHurtEffect : MonoBehaviour
{
    private const string HitEffectKeyword = "_HIT_ON";

    private Renderer[] renderers;
    private Material[] materials;
    private Coroutine effectCoroutine;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);

        var matList = new System.Collections.Generic.List<Material>();
        foreach (var r in renderers)
        {
            foreach (var m in r.materials)
            {
                matList.Add(m);
            }
        }
        materials = matList.ToArray();
    }

    /// <summary>
    /// 지정된 시간 동안 피격 효과를 표시합니다.
    /// 이미 재생 중이면 기존 코루틴을 중단하고 새로 시작합니다.
    /// </summary>
    public void PlayEffect(float duration)
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }

        effectCoroutine = StartCoroutine(EffectCoroutine(duration));
    }

    /// <summary>
    /// 효과를 즉시 중단합니다.
    /// </summary>
    public void StopEffect()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }
        SetHitEffect(false);
    }

    private IEnumerator EffectCoroutine(float duration)
    {
        SetHitEffect(true);
        yield return new WaitForSeconds(duration);
        SetHitEffect(false);
        effectCoroutine = null;
    }

    private void SetHitEffect(bool isOn)
    {
        if (materials == null) return;

        foreach (var mat in materials)
        {
            if (mat == null) continue;

            if (isOn)
                mat.EnableKeyword(HitEffectKeyword);
            else
                mat.DisableKeyword(HitEffectKeyword);
        }
    }

    private void OnDestroy()
    {
        SetHitEffect(false);
    }
}
