using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] private BaseUiScreen successScreen;
    [SerializeField] private BaseUiScreen failScreen;
    [SerializeField] private CanvasGroup uiContent;

    private CellsManager cellsManager;
    private Sequence showContentSequence;

    private void Awake()
    {
        uiContent.alpha = 0f;
        cellsManager = FindFirstObjectByType<CellsManager>();

        cellsManager.OnFail += ShowDeathScreen;
        cellsManager.OnSucceed += ShowSuccessScreen;

        showContentSequence = DOTween.Sequence();
        showContentSequence.AppendInterval(CellsManager.kLevelIntroductionDuration - 0.2f * CellsManager.kLevelIntroductionDuration);
        showContentSequence.Append(uiContent.DOFade(1f, 0.3f).SetEase(Ease.InCubic));
    }

    private void OnDestroy()
    {
        cellsManager.OnFail -= ShowDeathScreen;
        cellsManager.OnSucceed -= ShowSuccessScreen;
    }

    private void ShowDeathScreen()
    {
        failScreen.gameObject.SetActive(true);
    }

    private void ShowSuccessScreen()
    {
        successScreen.gameObject.SetActive(true);
    }
}