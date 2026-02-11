using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QGU.PauseSystem
{
    /// <summary>
    /// 게임 일시정지를 관리하는 매니저.
    /// ESC 키를 직접 처리하며, Time.timeScale을 제어합니다.
    /// 씬 종속 싱글톤으로 동작합니다.
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        public static PauseManager Instance { get; private set; }

        /// <summary>현재 일시정지 상태인지 여부.</summary>
        public bool IsPaused { get; private set; }

        /// <summary>일시정지될 때 발행됩니다.</summary>
        public event Action OnPaused;

        /// <summary>재개될 때 발행됩니다.</summary>
        public event Action OnResumed;

        private InputAction pauseAction;

        private void Awake()
        {
            // 씬 종속 싱글톤 (DontDestroyOnLoad 없음)
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // ESC 키 독립 바인딩 (Player 액션맵과 무관하게 항상 동작)
            pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");
            pauseAction.performed += _ => Toggle();
        }

        private void OnEnable()
        {
            pauseAction?.Enable();
        }

        private void OnDisable()
        {
            pauseAction?.Disable();
        }

        private void OnDestroy()
        {
            pauseAction?.Dispose();

            if (Instance == this)
                Instance = null;
        }

        /// <summary>일시정지와 재개를 토글합니다.</summary>
        public void Toggle()
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }

        /// <summary>게임을 일시정지합니다.</summary>
        public void Pause()
        {
            if (IsPaused) return;

            IsPaused = true;
            Time.timeScale = 0f;
            OnPaused?.Invoke();
        }

        /// <summary>게임을 재개합니다.</summary>
        public void Resume()
        {
            if (!IsPaused) return;

            IsPaused = false;
            Time.timeScale = 1f;
            OnResumed?.Invoke();
        }

        /// <summary>
        /// timeScale을 복원하고 일시정지 상태를 초기화합니다.
        /// 씬 전환 전에 호출하여 다음 씬이 멈추지 않도록 합니다.
        /// </summary>
        public void ForceReset()
        {
            IsPaused = false;
            Time.timeScale = 1f;
        }
    }
}
