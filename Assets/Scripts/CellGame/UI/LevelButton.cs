using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private string SceneName;
    private Button btn;
    protected void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(ChangeLevel);
    }

    protected void OnDestroy()
    {
        btn.onClick.RemoveListener(ChangeLevel);
    }

    public void ChangeLevel()
    {
        SceneManager.LoadSceneAsync(SceneName);
    }
}
