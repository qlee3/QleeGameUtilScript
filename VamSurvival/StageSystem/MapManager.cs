using Pathfinding;
using UnityEngine;

/// <summary>
/// 서브스테이지별 맵 프리팹을 로드/언로드합니다.
/// 맵 로드 후 A* Pathfinding 그래프를 재스캔하여 새 장애물을 반영합니다.
/// </summary>
public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    private GameObject currentMapInstance;

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
    }

    /// <summary>
    /// 맵 프리팹을 인스턴스화한 뒤, A* 그래프를 재스캔합니다.
    /// </summary>
    public void LoadMap(GameObject mapPrefab)
    {
        UnloadCurrentMap();

        if (mapPrefab == null)
        {
            Debug.LogWarning("[MapManager] mapPrefab이 null입니다.");
            return;
        }

        currentMapInstance = Instantiate(mapPrefab, transform);
        ScanNavGraph();
    }

    /// <summary>
    /// 현재 로드된 맵을 제거합니다.
    /// </summary>
    public void UnloadCurrentMap()
    {
        if (currentMapInstance != null)
        {
            Destroy(currentMapInstance);
            currentMapInstance = null;
        }
    }

    private void ScanNavGraph()
    {
        if (AstarPath.active == null)
        {
            Debug.LogWarning("[MapManager] AstarPath가 씬에 없어 네비게이션 스캔을 건너뜁니다.");
            return;
        }

        AstarPath.active.Scan();
    }
}
