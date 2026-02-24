using System;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 경험치 젬 등급 정의.
/// Inspector에서 프리팹과 경험치 범위를 설정합니다.
/// </summary>
[Serializable]
public class ExpGemTier
{
    [Tooltip("이 등급의 젬 프리팹")]
    public ExpGem prefab;

    [Tooltip("이 등급이 부여하는 경험치")]
    public float expValue = 1f;

    [Tooltip("이 등급이 선택되는 최소 경험치량 (스폰 요청 시 비교)")]
    public float minExpThreshold;
}

/// <summary>
/// 경험치 젬 스포너.
/// 씬에 하나만 존재하며, 등급별 프리팹을 오브젝트 풀로 관리합니다.
/// 외부에서 SpawnGems(position, totalExp)를 호출하면
/// 적절한 등급의 젬들을 자동 분배하여 스폰합니다.
/// </summary>
public class ExpGemSpawner : MonoBehaviour
{
    public static ExpGemSpawner Instance { get; private set; }

    [Header("Gem Tiers (높은 등급부터 정렬하세요)")]
    [Tooltip("등급별 젬 설정. expValue가 높은 순서대로 배치하세요.")]
    [SerializeField] private ExpGemTier[] tiers;

    [Header("Pool Settings")]
    [SerializeField] private int poolDefaultCapacity = 50;
    [SerializeField] private int poolMaxSize = 300;

    /// <summary>등급별 오브젝트 풀.</summary>
    private IObjectPool<ExpGem>[] pools;

    private void Awake()
    {
        // 씬 종속 싱글톤 (DontDestroyOnLoad 아님)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializePools();
        ValidateTiers();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 지정 위치에 총 totalExp만큼의 경험치 젬을 스폰합니다.
    /// 큰 등급부터 채워넣고 남은 양은 작은 등급으로 채웁니다.
    /// </summary>
    /// <param name="position">스폰 위치 (적 사망 위치 등).</param>
    /// <param name="totalExp">총 경험치량.</param>
    public void SpawnGems(Vector3 position, float totalExp)
    {
        if (tiers == null || tiers.Length == 0) return;

        float remaining = totalExp;

        // 높은 등급(큰 경험치)부터 분배
        for (int tierIndex = 0; tierIndex < tiers.Length && remaining > 0f; tierIndex++)
        {
            ExpGemTier tier = tiers[tierIndex];

            // 남은 경험치가 이 등급의 경험치 이상일 때만 스폰
            while (remaining >= tier.expValue)
            {
                SpawnSingleGem(tierIndex, position, tier.expValue);
                remaining -= tier.expValue;
            }
        }

        // 남은 찌꺼기가 있으면 가장 작은 등급으로 1개 스폰
        if (remaining > 0f && tiers.Length > 0)
        {
            int lastTier = tiers.Length - 1;
            SpawnSingleGem(lastTier, position, remaining);
        }
    }

    /// <summary>
    /// 특정 등급의 젬 1개를 스폰합니다.
    /// </summary>
    /// <param name="tierIndex">등급 인덱스.</param>
    /// <param name="position">스폰 위치.</param>
    /// <param name="expValue">부여할 경험치.</param>
    public void SpawnSingleGem(int tierIndex, Vector3 position, float expValue)
    {
        tierIndex = Mathf.Clamp(tierIndex, 0, tiers.Length - 1);

        ExpGem gem = pools[tierIndex].Get();
        gem.transform.position = position + Vector3.up * 0.3f;
        gem.Initialize(expValue, pools[tierIndex]);
    }

    private void InitializePools()
    {
        pools = new IObjectPool<ExpGem>[tiers.Length];

        for (int i = 0; i < tiers.Length; i++)
        {
            int index = i; // 클로저 캡처용 로컬 변수
            ExpGemTier tier = tiers[index];

            pools[i] = new ObjectPool<ExpGem>(
                createFunc: () =>
                {
                    ExpGem gem = Instantiate(tier.prefab, transform);
                    gem.gameObject.SetActive(false);
                    return gem;
                },
                actionOnGet: gem => gem.gameObject.SetActive(true),
                actionOnRelease: gem => gem.gameObject.SetActive(false),
                actionOnDestroy: gem => Destroy(gem.gameObject),
                defaultCapacity: poolDefaultCapacity,
                maxSize: poolMaxSize
            );
        }
    }

    /// <summary>등급 배열이 경험치 내림차순으로 정렬되었는지 검증합니다.</summary>
    private void ValidateTiers()
    {
        if (tiers == null || tiers.Length == 0)
        {
            Debug.LogWarning("[ExpGemSpawner] tiers 배열이 비어 있습니다. Inspector에서 설정하세요.");
            return;
        }

        for (int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i].prefab == null)
            {
                Debug.LogError($"[ExpGemSpawner] tiers[{i}]의 프리팹이 null입니다.");
            }
        }

        for (int i = 0; i < tiers.Length - 1; i++)
        {
            if (tiers[i].expValue < tiers[i + 1].expValue)
            {
                Debug.LogWarning(
                    $"[ExpGemSpawner] tiers가 경험치 내림차순이 아닙니다. " +
                    $"tiers[{i}]={tiers[i].expValue}, tiers[{i + 1}]={tiers[i + 1].expValue}. " +
                    $"높은 등급이 먼저 오도록 정렬하세요.");
            }
        }
    }
}
