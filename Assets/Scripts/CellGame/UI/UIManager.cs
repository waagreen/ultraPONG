using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [SerializeField] private BaseUiScreen successScreen;
    [SerializeField] private BaseUiScreen failScreen;

    private EventSystem eventSystem;
    private CellsManager cellsManager;

    private void Awake()
    {
        cellsManager = FindFirstObjectByType<CellsManager>();
        eventSystem = GetComponent<EventSystem>();

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
        // eventSystem.firstSelectedGameObject = failScreen.FirstSelected;
        // eventSystem.UpdateModules();
        failScreen.gameObject.SetActive(true);
    }

    private void ShowSuccessScreen()
    {
        // eventSystem.firstSelectedGameObject = successScreen.FirstSelected;
        // eventSystem.UpdateModules();
        successScreen.gameObject.SetActive(true);
    }
}