using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject successScreen;
    [SerializeField] private GameObject failScreen;

    private CellsManager cellsManager;

    private void Awake()
    {
        cellsManager = FindFirstObjectByType<CellsManager>();

        cellsManager.OnFail += ShowDeathScreen;
        cellsManager.OnSucceed += ShowSuccessScreen;
    }

    private void OnDestroy()
    {
        cellsManager.OnFail -= ShowDeathScreen;
        cellsManager.OnSucceed -= ShowSuccessScreen;
    }

    private void ShowDeathScreen()
    {
        failScreen.SetActive(true);
    }

    private void ShowSuccessScreen()
    {
        successScreen.SetActive(true);
    }
}
