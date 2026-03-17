using System;
using UnityEngine;

/// <summary>
/// 스테이지/서브스테이지 진행을 관리합니다.
/// MapManager로 맵을 로드하고, StageWaveSpawner로 스폰 레이아웃을 실행하며,
/// 서브스테이지 클리어 시 다음 서브스테이지로 자동 진행합니다.
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private MapManager mapManager;
    [SerializeField] private StageWaveSpawner waveSpawner;

    // ── 상태 ──

    private StageData currentStage;
    private int currentSubStageIndex;

    // ── 프로퍼티 ──

    public StageData CurrentStage => currentStage;
    public int CurrentSubStageIndex => currentSubStageIndex;
    public int TotalSubStageCount => currentStage != null ? currentStage.SubStageCount : 0;

    public SubStageConfig CurrentSubStageConfig =>
        currentStage != null && currentSubStageIndex < currentStage.SubStageCount
            ? currentStage.GetSubStage(currentSubStageIndex)
            : null;

    // ── 이벤트 ──

    /// <summary>서브스테이지가 클리어되었을 때 발생. (아직 남은 서브스테이지가 있음)</summary>
    public event Action<int> OnSubStageClear;

    /// <summary>스테이지의 모든 서브스테이지가 클리어되었을 때 발생.</summary>
    public event Action OnStageClear;

    /// <summary>새 서브스테이지가 시작될 때 발생. (서브스테이지 인덱스, 총 서브스테이지 수)</summary>
    public event Action<int, int> OnSubStageStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (waveSpawner != null)
            waveSpawner.OnAllWavesCleared -= HandleAllWavesCleared;
    }

    /// <summary>
    /// 스테이지를 로드하고 첫 번째 서브스테이지를 시작합니다.
    /// </summary>
    public void LoadStage(StageData stage)
    {
        if (stage == null || stage.SubStageCount == 0)
        {
            Debug.LogWarning("[StageManager] stage 데이터가 비어 있습니다.");
            return;
        }

        if (waveSpawner != null)
            waveSpawner.OnAllWavesCleared -= HandleAllWavesCleared;

        currentStage = stage;
        currentSubStageIndex = 0;

        ResolveReferences();

        waveSpawner.OnAllWavesCleared += HandleAllWavesCleared;

        LoadSubStage(currentSubStageIndex);
    }

    /// <summary>
    /// 다음 서브스테이지를 강제로 시작합니다.
    /// </summary>
    public void StartNextSubStage()
    {
        if (currentStage == null) return;

        int next = currentSubStageIndex + 1;
        if (next >= currentStage.SubStageCount) return;

        currentSubStageIndex = next;
        LoadSubStage(currentSubStageIndex);
    }

    /// <summary>
    /// 현재 스테이지를 정리합니다.
    /// </summary>
    public void Cleanup()
    {
        if (waveSpawner != null)
        {
            waveSpawner.OnAllWavesCleared -= HandleAllWavesCleared;
            waveSpawner.StopAndClearAll();
        }

        if (mapManager != null)
            mapManager.UnloadCurrentMap();

        currentStage = null;
    }

    private void LoadSubStage(int index)
    {
        SubStageConfig config = currentStage.GetSubStage(index);
        if (config == null)
        {
            Debug.LogWarning($"[StageManager] subStage[{index}]가 유효하지 않습니다.");
            return;
        }

        waveSpawner.StopAndClearAll();
        mapManager.LoadMap(config.mapPrefab);

        OnSubStageStarted?.Invoke(index, currentStage.SubStageCount);

        if (config.spawnLayout != null)
        {
            waveSpawner.StartWaves(config.spawnLayout);
        }
        else
        {
            Debug.LogWarning($"[StageManager] subStage[{index}]에 spawnLayout이 없습니다.");
        }
    }

    private void HandleAllWavesCleared()
    {
        int nextIndex = currentSubStageIndex + 1;

        if (nextIndex < currentStage.SubStageCount)
        {
            OnSubStageClear?.Invoke(currentSubStageIndex);
            currentSubStageIndex = nextIndex;
            LoadSubStage(currentSubStageIndex);
        }
        else
        {
            OnSubStageClear?.Invoke(currentSubStageIndex);
            OnStageClear?.Invoke();
        }
    }

    private void ResolveReferences()
    {
        if (mapManager == null)
            mapManager = MapManager.Instance;
        if (waveSpawner == null)
            waveSpawner = StageWaveSpawner.Instance;

        if (mapManager == null)
            Debug.LogError("[StageManager] MapManager를 찾을 수 없습니다.");
        if (waveSpawner == null)
            Debug.LogError("[StageManager] StageWaveSpawner를 찾을 수 없습니다.");
    }
}
