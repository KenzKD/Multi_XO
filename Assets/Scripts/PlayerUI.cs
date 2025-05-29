using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject CrossText;
    [SerializeField] private GameObject CrossArrow;
    [SerializeField] private TextMeshProUGUI CrossScoreText;

    [SerializeField] private GameObject CircleText;
    [SerializeField] private GameObject CircleArrow;
    [SerializeField] private TextMeshProUGUI CircleScoreText;

    private void Awake()
    {
        CrossText.SetActive(false);
        CrossArrow.SetActive(false);
        CrossScoreText.text = "";

        CircleText.SetActive(false);
        CircleArrow.SetActive(false);
        CircleScoreText.text = "";
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;
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

        CrossScoreText.text = "0";
        CircleScoreText.text = "0";
    }

    private void GameManager_OnScoreChanged(object sender, System.EventArgs e)
    {
        GameManager.Instance.GetScores(out int playerCrossScore, out int playerCircleScore);

        CrossScoreText.text = playerCrossScore.ToString();
        CircleScoreText.text = playerCircleScore.ToString();    
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
