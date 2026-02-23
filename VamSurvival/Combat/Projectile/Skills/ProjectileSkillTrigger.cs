using UnityEngine;

/// <summary>
/// 투사체 스킬 트리거 베이스.
/// 파생 클래스가 조건이 만족될 때 TriggerCast()를 호출합니다.
/// </summary>
public abstract class ProjectileSkillTrigger : MonoBehaviour
{
    [SerializeField] protected ProjectileSkillController skillController;
    [SerializeField] protected ProjectileSkillDefinition skill;

    protected virtual void Awake()
    {
        if (skillController == null)
        {
            skillController = GetComponentInParent<ProjectileSkillController>();
        }
    }

    protected bool TriggerCast()
    {
        if (skillController == null || skill == null) return false;
        return skillController.TryCast(skill);
    }
}
