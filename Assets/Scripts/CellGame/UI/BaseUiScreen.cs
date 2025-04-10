using UnityEngine;
using UnityEngine.UI;

public class BaseUiScreen : MonoBehaviour
{
    [SerializeField] private Button firstSelected;
    public GameObject FirstSelected => firstSelected.gameObject;

    private void Start()
    {
        firstSelected.Select();
    }
}
