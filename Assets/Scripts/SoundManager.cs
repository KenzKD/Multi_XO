using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSfxPrefab;
    [SerializeField] private Transform winSfxPrefab;
    [SerializeField] private Transform loseSfxPrefab;

    private const float SFX_DESTROY_DELAY = 5;

    private void Start()
    {
        GameManager.Instance.OnPlacedObject += GameManager_OnPlacedObject;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == e.winPlayerType)
        {
            Transform winSfxTransform = Instantiate(winSfxPrefab);
            Destroy(winSfxTransform.gameObject, SFX_DESTROY_DELAY);
        }
        else
        {
            Transform loseSfxTransform = Instantiate(loseSfxPrefab);
            Destroy(loseSfxTransform.gameObject, SFX_DESTROY_DELAY);
        }

    }

    private void GameManager_OnPlacedObject(object sender, System.EventArgs e)
    {
        Transform sfxTransform = Instantiate(placeSfxPrefab);
        Destroy(sfxTransform.gameObject, SFX_DESTROY_DELAY);
    }



}
