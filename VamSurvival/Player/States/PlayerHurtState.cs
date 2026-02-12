/// <summary>
/// 플레이어 피격 상태.
/// 2초 동안 무적, 넉백, 피격 시각 효과를 적용합니다.
/// Health.OnDamaged 시 전환되며, duration 경과 후 IdleState로 복귀합니다.
/// </summary>
public class PlayerHurtState : EntityState<PlayerController>
{
    public override int AnimStateId => 2; // Hurt 애니메이션 (Animator에 State 2 추가 시 변경)

    private const float Duration = 0.5f;
    private const float KnockbackForce = 8f;

    private DamageInfo damageInfo;

    /// <summary>
    /// ChangeState 호출 전에 피격 정보를 설정합니다.
    /// </summary>
    public void SetDamageInfo(DamageInfo damage)
    {
        damageInfo = damage;
    }

    protected override void OnEnter(PlayerController entity)
    {
        entity.Movement.Stop();

        // 무적
        entity.Health.SetInvincible(true);

        // 넉백 (피격 방향의 반대 = 밀려나는 방향)
        // DamageInfo.HitDirection = (other - self).normalized → 적에서 플레이어로 향하는 벡터
        // 넉백은 그 반대 = 플레이어가 밀려나는 방향
        if (damageInfo.HitDirection.sqrMagnitude > 0.001f)
        {
            entity.Movement.ApplyKnockback(damageInfo.HitDirection, KnockbackForce);
        }

        // 피격 시각 효과
        var hurtEffect = entity.GetComponentInChildren<PlayerHurtEffect>();
        hurtEffect?.PlayEffect(0.1f);

        if (entity.Input.MoveInput.sqrMagnitude > 0.01f)
        {
            entity.Movement.Move(entity.Input.MoveInput);
        }
    }

    protected override void OnStep(PlayerController entity)
    {
        if (TimeSinceEntered >= Duration)
        {
            entity.ChangeState(entity.IdleState);
        }
    }

    protected override void OnExit(PlayerController entity)
    {
        entity.Health.SetInvincible(false);

        var hurtEffect = entity.GetComponentInChildren<PlayerHurtEffect>();
        hurtEffect?.StopEffect();

        entity.Movement.Stop();
    }
}
