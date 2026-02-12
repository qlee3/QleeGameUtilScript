using UnityEngine;

/// <summary>
/// 적의 기본 데이터를 정의하는 ScriptableObject.
/// Inspector에서 적 유형별 스탯, 보상, 타입별 파라미터를 설정합니다.
/// 동일 프리팹이 다른 EnemyData를 받아 다양한 적으로 동작할 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "VamSurvival/Enemy Data")]
public class EnemyData : ScriptableObject
{
    // ── 기본 정보 ──

    [Header("기본 정보")]
    [Tooltip("적 이름 (UI 표시용)")]
    public string enemyName;

    [Tooltip("행동 유형")]
    public EnemyType enemyType;

    // ── 기본 스탯 ──

    [Header("기본 스탯")]
    [Tooltip("최대 체력")]
    public float maxHp = 50f;

    [Tooltip("이동 속도")]
    public float moveSpeed = 3f;

    [Tooltip("접촉 피해량")]
    public float contactDamage = 10f;

    [Tooltip("접촉 피해 간격 (초)")]
    public float contactDamageInterval = 0.5f;

    // ── 보상 ──

    [Header("보상")]
    [Tooltip("사망 시 드롭할 총 경험치량")]
    public float expReward = 10f;

    // ── 추격형 파라미터 ──

    [Header("추격형 (Chaser)")]
    [Tooltip("추격 중 간헐적 정지 시간 (초)")]
    public float chasePauseDuration = 0.5f;

    [Tooltip("정지 전 최소 추격 시간 (초)")]
    public float chaseMinDuration = 2f;

    [Tooltip("정지 전 최대 추격 시간 (초)")]
    public float chaseMaxDuration = 5f;

    // ── 대쉬형 파라미터 ──

    [Header("대쉬형 (Dasher)")]
    [Tooltip("대쉬 준비(텔레그래프) 시간")]
    public float dashPrepareTime = 0.8f;

    [Tooltip("대쉬 속도")]
    public float dashSpeed = 15f;

    [Tooltip("대쉬 거리")]
    public float dashDistance = 8f;

    [Tooltip("대쉬 시작 거리 (이 거리 이내 진입 시 대쉬 준비)")]
    public float dashTriggerRange = 6f;

    [Tooltip("대쉬 후 쿨다운 시간 (초)")]
    public float dashCooldown = 2f;

    [Tooltip("대쉬 중 접촉 피해 배율")]
    public float dashDamageMultiplier = 2f;

    // ── 원거리형 파라미터 ──

    [Header("원거리형 (Ranged)")]
    [Tooltip("공격 사거리 (이 거리 이내에서 정지 후 공격)")]
    public float attackRange = 8f;

    [Tooltip("공격 준비 시간 (조준)")]
    public float attackPrepareTime = 0.5f;

    [Tooltip("공격 쿨다운")]
    public float attackCooldown = 2f;

    // ── 스킬 공용 파라미터 ──

    [Header("스킬 공용 (Mindless / Summoner / Passive)")]
    [Tooltip("스킬 사용 간격 (초)")]
    public float skillInterval = 3f;

    // ── 스폰 ──

    [Header("스폰")]
    [Tooltip("스폰 무적 시간 (초). 스폰 연출 동안 무적.")]
    public float spawnInvincibleTime = 0.5f;
}
