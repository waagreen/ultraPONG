using UnityEngine;
using UnityEngine.UI;

public class BaseUiScreen : MonoBehaviour
{
    [SerializeField] private Button firstSelected;
    public GameObject FirstSelected => firstSelected.gameObject;

    private void OnEnable()
    {
        firstSelected.Select();
    }

    private void OnDisable()
    {
        firstSelected.Select();
    }
}
