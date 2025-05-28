using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum PlayerType
    {
        None,
        Cross,
        Circle,
    }
    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB,
    }
    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new();
    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;
    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one instance of GameManager found!");
        }
        Instance = this;
        playerTypeArray = new PlayerType[3, 3];
        lineList = new()
        {
            // Horizontal Lines
            new Line
            {
                gridVector2IntList =new List<Vector2Int> { new(0,0), new(1,0), new(2,0) },
                centerGridPosition = new(1,0),
                orientation = Orientation.Horizontal,
            },
            new Line
            {
                gridVector2IntList =new List<Vector2Int> { new(0,1), new(1,1), new(2,1) },
                centerGridPosition = new(1,1),
                orientation = Orientation.Horizontal,
            },
            new Line
            {
                gridVector2IntList =new List<Vector2Int> { new(0,2), new(1,2), new(2,2) },
                centerGridPosition = new(1,2),
                orientation = Orientation.Horizontal,
            },

            // Vertical Lines
            new Line
            {
                gridVector2IntList =new List<Vector2Int> { new(0,0), new(0,1), new(0,2) },
                centerGridPosition = new(0,1),
                orientation = Orientation.Vertical,
            },
            new Line
            {
                gridVector2IntList =new List<Vector2Int> { new(1,0), new(1,1), new(1,2) },
                centerGridPosition = new(1,1),
                orientation = Orientation.Vertical,
            },
            new Line
            {
                gridVector2IntList =new List<Vector2Int> { new(2,0), new(2,1), new(2,2) },
                centerGridPosition = new(2,1),
                orientation = Orientation.Vertical,
            },

            // Diagonal Lines
            new Line
            {
                gridVector2IntList =new List<Vector2Int> { new(0,0), new(1,1), new(2,2) },
                centerGridPosition = new(1,1),
                orientation = Orientation.DiagonalA,
            },
            new Line
            {
                gridVector2IntList =new List<Vector2Int> { new(0,2), new(1,1), new(2,0) },
                centerGridPosition = new(1,1),
                orientation = Orientation.DiagonalB,
            },

        };
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (oldPlayerType, newPlayerType) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };

    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        Debug.Log("GM Clicked: " + x + ", " + y);
        if (playerType != currentPlayablePlayerType.Value)
        {
            return;
        }

        if (playerTypeArray[x, y] != PlayerType.None)
        {
            // Already Occupied
            return;
        }

        playerTypeArray[x, y] = playerType;

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType,
        });

        switch (currentPlayablePlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }

        TestWinner();
    }

    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return aPlayerType != PlayerType.None &&
            aPlayerType == bPlayerType &&
            bPlayerType == cPlayerType;
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine
        (
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
        );
    }

    private void TestWinner()
    {
        for (int i = 0; i < lineList.Count; i++)
        {
            Line line = lineList[i];
            if (TestWinnerLine(line))
            {
                Debug.Log("Winner!");
                currentPlayablePlayerType.Value = PlayerType.None;
                TriggerOnGameWinRpc(i, playerTypeArray[line.centerGridPosition.x, line.centerGridPosition.y]);
                break;
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
    {
        Line line = lineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            line = line,
            winPlayerType = winPlayerType,
        });
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }

}
