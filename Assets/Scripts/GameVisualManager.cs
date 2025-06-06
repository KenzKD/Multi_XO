using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;

    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform lineCompletePrefab;

    private Transform lineCompleteTransform;


    private List<GameObject> visualGameObjectList;

    private void Awake()
    {
        visualGameObjectList = new();
    }

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        float eulerZ;
        switch (e.line.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal: eulerZ = 0f; break;
            case GameManager.Orientation.Vertical: eulerZ = 90f; break;
            case GameManager.Orientation.DiagonalA: eulerZ = 45f; break;
            case GameManager.Orientation.DiagonalB: eulerZ = -45f; break;

        }
        lineCompleteTransform = Instantiate
        (
            lineCompletePrefab,
            GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y),
            Quaternion.Euler(0f, 0f, eulerZ)
        );
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        Debug.Log("GM Clicked");
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    private void GameManager_OnRematch(object sender, System.EventArgs e)
    {
        foreach (GameObject visualGameObject in visualGameObjectList)
        {
            Destroy(visualGameObject);
        }
        visualGameObjectList.Clear();
        if (lineCompleteTransform != null)
        {
            Destroy(lineCompleteTransform.gameObject);
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Debug.Log("Spawned Obj");
        Transform prefab;
        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                prefab = crossPrefab;
                break;
            case GameManager.PlayerType.Circle:
                prefab = circlePrefab;
                break;
        }
        Transform spawnedCross = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        spawnedCross.GetComponent<NetworkObject>().Spawn(true);

        visualGameObjectList.Add(spawnedCross.gameObject);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2((x - 1) * GRID_SIZE, (y - 1) * GRID_SIZE);
    }
}
