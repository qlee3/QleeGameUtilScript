using UnityEngine;

/// <summary>
/// 투사체 이동 전략 인터페이스.
/// ProjectileBase가 생성/업데이트 라이프사이클을 관리하고,
/// 실제 이동 계산은 구현체가 담당합니다.
/// </summary>
public interface IProjectileMovement
{
    void Initialize(ProjectileBase projectile, ProjectileData data, Vector3 direction, Transform target);
    void Tick(float deltaTime);
}

public static class ProjectileMovementFactory
{
    public static IProjectileMovement Create(ProjectileMovementType type)
    {
        switch (type)
        {
            case ProjectileMovementType.Homing:
                return new HomingProjectileMovement();
            case ProjectileMovementType.Straight:
            default:
                return new StraightProjectileMovement();
        }
    }
}

public sealed class StraightProjectileMovement : IProjectileMovement
{
    private ProjectileBase projectile;
    private ProjectileData data;
    private Vector3 direction;

    public void Initialize(ProjectileBase projectile, ProjectileData data, Vector3 direction, Transform target)
    {
        this.projectile = projectile;
        this.data = data;
        this.direction = direction.sqrMagnitude > 0.001f ? direction.normalized : projectile.transform.forward;
    }

    public void Tick(float deltaTime)
    {
        projectile.transform.position += direction * data.speed * deltaTime;
        projectile.SetTravelDirection(direction);
    }
}

public sealed class HomingProjectileMovement : IProjectileMovement
{
    private ProjectileBase projectile;
    private ProjectileData data;
    private Transform target;
    private Vector3 direction;

    public void Initialize(ProjectileBase projectile, ProjectileData data, Vector3 direction, Transform target)
    {
        this.projectile = projectile;
        this.data = data;
        this.target = target;
        this.direction = direction.sqrMagnitude > 0.001f ? direction.normalized : projectile.transform.forward;
    }

    public void Tick(float deltaTime)
    {
        if (target != null)
        {
            Vector3 toTarget = target.position - projectile.transform.position;
            if (toTarget.sqrMagnitude > 0.001f)
            {
                Vector3 targetDirection = toTarget.normalized;
                float maxRadiansDelta = Mathf.Deg2Rad * data.homingTurnRate * deltaTime;
                direction = Vector3.RotateTowards(direction, targetDirection, maxRadiansDelta, 0f).normalized;
            }
        }

        projectile.transform.position += direction * data.speed * deltaTime;
        projectile.SetTravelDirection(direction);
    }
}
