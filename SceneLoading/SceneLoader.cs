using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QGU.SceneLoading
{
    /// <summary>
    /// 비동기 씬 로딩 유틸리티
    /// 로딩 진행률 콜백과 페이드 전환을 지원합니다.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader instance;
        public static SceneLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("[SceneLoader]");
                    instance = go.AddComponent<SceneLoader>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        /// <summary>로딩 진행률 (0 ~ 1)</summary>
        public float Progress { get; private set; }

        /// <summary>현재 로딩 중인지 여부</summary>
        public bool IsLoading { get; private set; }

        /// <summary>로딩 진행률이 변경될 때 호출 (0~1)</summary>
        public event Action<float> OnProgressChanged;

        /// <summary>씬 로딩이 완료되었을 때 호출</summary>
        public event Action OnLoadComplete;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 씬을 비동기로 로드합니다.
        /// </summary>
        /// <param name="sceneName">로드할 씬 이름</param>
        /// <param name="minLoadTime">최소 로딩 시간 (초) - 로딩 화면을 보여주기 위한 최소 대기 시간</param>
        public void LoadScene(string sceneName, float minLoadTime = 0.5f)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneLoader] 이미 씬을 로딩 중입니다. 요청 무시: {sceneName}");
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneName, minLoadTime));
        }

        /// <summary>
        /// 씬을 비동기로 로드하는 코루틴
        /// </summary>
        private IEnumerator LoadSceneAsync(string sceneName, float minLoadTime)
        {
            IsLoading = true;
            Progress = 0f;
            OnProgressChanged?.Invoke(0f);

            float elapsed = 0f;

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOperation == null)
            {
                Debug.LogError($"[SceneLoader] 씬을 찾을 수 없습니다: {sceneName}");
                IsLoading = false;
                yield break;
            }

            // 씬 전환을 수동으로 제어
            asyncOperation.allowSceneActivation = false;

            // 로딩 진행
            while (!asyncOperation.isDone)
            {
                elapsed += Time.unscaledDeltaTime;

                // Unity의 비동기 로딩은 0.9까지만 진행 (allowSceneActivation이 false일 때)
                float realProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                float timeProgress = minLoadTime > 0f ? Mathf.Clamp01(elapsed / minLoadTime) : 1f;

                // 실제 로딩과 최소 시간 중 작은 값을 사용
                Progress = Mathf.Min(realProgress, timeProgress);
                OnProgressChanged?.Invoke(Progress);

                // 로딩 완료 + 최소 시간 경과 시 씬 활성화
                if (asyncOperation.progress >= 0.9f && elapsed >= minLoadTime)
                {
                    Progress = 1f;
                    OnProgressChanged?.Invoke(1f);
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }

            IsLoading = false;
            OnLoadComplete?.Invoke();
        }

        /// <summary>
        /// 씬을 즉시 (동기) 로드합니다.
        /// </summary>
        /// <param name="sceneName">로드할 씬 이름</param>
        public void LoadSceneImmediate(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
