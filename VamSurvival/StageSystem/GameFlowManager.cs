using System;
using UnityEngine;

/// <summary>
/// 최상위 게임 플로우 상태 머신.
/// 스테이지 진행, 승리/패배 조건을 통합하여 게임 흐름을 제어합니다.
///
/// Loading → Playing → SubStageClear → Playing → ... → StageClear → (다음 스테이지 or GameWin)
/// Playing → GameOver (플레이어 사망 시)
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private StageDatabase stageDatabase;

    [Header("References")]
    [SerializeField] private StageManager stageManager;

    [Header("Auto Start")]
    [Tooltip("씬 로드 시 자동으로 게임을 시작합니다.")]
    [SerializeField] private bool autoStartOnEnable = true;
    [SerializeField] private int startStageIndex = 0;

    // ── 상태 ──

    private GameState currentState = GameState.Loading;
    private int currentStageIndex;
    private PlayerHealth playerHealth;

    // ── 프로퍼티 ──

    public GameState CurrentState => currentState;
    public int CurrentStageIndex => currentStageIndex;
    public StageDatabase Database => stageDatabase;

    // ── 이벤트 ──

    /// <summary>게임 상태가 변경될 때 발생. (이전 상태, 새 상태)</summary>
    public event Action<GameState, GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (autoStartOnEnable)
            StartGame(startStageIndex);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        UnbindEvents();
    }

    /// <summary>
    /// 지정한 스테이지 인덱스로 게임을 시작합니다.
    /// </summary>
    public void StartGame(int stageIndex)
    {
        if (stageDatabase == null || stageIndex < 0 || stageIndex >= stageDatabase.StageCount)
        {
            Debug.LogError($"[GameFlowManager] 유효하지 않은 stageIndex: {stageIndex}");
            return;
        }

        ResolveReferences();
        BindEvents();

        currentStageIndex = stageIndex;
        ChangeState(GameState.Loading);

        StageData stage = stageDatabase.GetStage(currentStageIndex);
        stageManager.LoadStage(stage);

        ChangeState(GameState.Playing);
    }

    /// <summary>
    /// 현재 스테이지를 다시 시도합니다.
    /// </summary>
    public void RetryStage()
    {
        StartGame(currentStageIndex);
    }

    /// <summary>
    /// 다음 스테이지로 진행합니다.
    /// </summary>
    public void NextStage()
    {
        int next = currentStageIndex + 1;

        if (next >= stageDatabase.StageCount)
        {
            ChangeState(GameState.GameWin);
            return;
        }

        StartGame(next);
    }

    // ── 이벤트 핸들러 ──

    private void HandleSubStageClear(int subStageIndex)
    {
        if (currentState != GameState.Playing) return;

        bool isLastSubStage = stageManager.CurrentSubStageIndex >= stageManager.TotalSubStageCount;
        if (!isLastSubStage)
        {
            ChangeState(GameState.SubStageClear);
            ChangeState(GameState.Playing);
        }
    }

    private void HandleStageClear()
    {
        if (currentState == GameState.GameOver) return;

        ChangeState(GameState.StageClear);
        NextStage();
    }

    private void HandlePlayerDeath()
    {
        if (currentState == GameState.GameOver || currentState == GameState.GameWin) return;

        ChangeState(GameState.GameOver);
    }

    // ── 상태 전환 ──

    private void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        GameState prev = currentState;
        currentState = newState;

        OnStateExit(prev);
        OnStateEnter(newState);
        OnGameStateChanged?.Invoke(prev, newState);
    }

    private void OnStateEnter(GameState state)
    {
        switch (state)
        {
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;

            case GameState.GameWin:
                Time.timeScale = 0f;
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                break;
        }
    }

    private void OnStateExit(GameState state)
    {
        // 추후 전환 연출 등에 사용
    }

    // ── 바인딩 ──

    private void BindEvents()
    {
        UnbindEvents();

        if (stageManager != null)
        {
            stageManager.OnSubStageClear += HandleSubStageClear;
            stageManager.OnStageClear += HandleStageClear;
        }

        ResolvePlayerHealth();
        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }
    }

    private void UnbindEvents()
    {
        if (stageManager != null)
        {
            stageManager.OnSubStageClear -= HandleSubStageClear;
            stageManager.OnStageClear -= HandleStageClear;
        }

        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }

    private void ResolveReferences()
    {
        if (stageManager == null)
            stageManager = StageManager.Instance;

        if (stageManager == null)
            Debug.LogError("[GameFlowManager] StageManager를 찾을 수 없습니다.");
    }

    private void ResolvePlayerHealth()
    {
        if (playerHealth != null) return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            playerHealth = player.Health;
    }
}
