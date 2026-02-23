using UnityEngine;

/// <summary>
/// N초마다 스킬 발동을 시도하는 트리거.
/// </summary>
public class IntervalSkillTrigger : ProjectileSkillTrigger
{
    [SerializeField] private float interval = 1f;
    [SerializeField] private bool castOnEnable = true;

    private float nextTriggerTime;

    private void OnEnable()
    {
        if (castOnEnable)
        {
            TriggerCast();
        }

        nextTriggerTime = Time.time + Mathf.Max(0.01f, interval);
    }

    private void Update()
    {
        if (Time.time < nextTriggerTime) return;

        TriggerCast();
        nextTriggerTime = Time.time + Mathf.Max(0.01f, interval);
    }
}
