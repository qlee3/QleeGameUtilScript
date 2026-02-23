using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 투사체 스킬 발동 허브.
/// 트리거에서 TryCast를 호출하면 내부 쿨다운을 검사한 뒤 ProjectileEmitter를 통해 발사합니다.
/// </summary>
[RequireComponent(typeof(ProjectileEmitter))]
public class ProjectileSkillController : MonoBehaviour
{
    private ProjectileEmitter emitter;
    private readonly Dictionary<ProjectileSkillDefinition, float> nextCastTimeBySkill = new();

    private void Awake()
    {
        emitter = GetComponent<ProjectileEmitter>();
    }

    public bool TryCast(ProjectileSkillDefinition skill)
    {
        if (skill == null || emitter == null) return false;

        if (nextCastTimeBySkill.TryGetValue(skill, out float nextCastTime) && Time.time < nextCastTime)
        {
            return false;
        }

        bool fired = emitter.FireSlot(skill.slotId);
        if (!fired) return false;

        nextCastTimeBySkill[skill] = Time.time + Mathf.Max(0f, skill.cooldown);
        return true;
    }
}
