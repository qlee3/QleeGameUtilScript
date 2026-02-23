using UnityEngine;

/// <summary>
/// 투사체 스킬 정의 데이터.
/// 아이템/패시브/트리거가 이 정의를 참조해 발사를 요청합니다.
/// </summary>
[CreateAssetMenu(fileName = "ProjectileSkillDefinition", menuName = "VamSurvival/Combat/Projectile Skill")]
public class ProjectileSkillDefinition : ScriptableObject
{
    [Tooltip("스킬 식별자")]
    public string skillId = "Skill";

    [Tooltip("ProjectileEmitter 슬롯 ID")]
    public string slotId = "Primary";

    [Tooltip("스킬 내부 쿨다운(초)")]
    [Min(0f)] public float cooldown = 1f;
}
