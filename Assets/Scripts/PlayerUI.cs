using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject CrossText;
    [SerializeField] private GameObject CrossArrow;

    [SerializeField] private GameObject CircleText;
    [SerializeField] private GameObject CircleArrow;

    private void Awake()
    {
        CrossText.SetActive(false);
        CrossArrow.SetActive(false);
        CircleText.SetActive(false);
        CircleArrow.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, System.EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            CrossText.SetActive(true);
        }
        else
        {
            CircleText.SetActive(true);
        }
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            CrossArrow.SetActive(true);
            CircleArrow.SetActive(false);
        }
        else
        {
            CrossArrow.SetActive(false);
            CircleArrow.SetActive(true);
        }
    }
}
