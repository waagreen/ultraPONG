using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CourtMaster : MonoBehaviour
{
    [Header("Spawns")]
    [SerializeField] private Transform paddleSpawn;
    [SerializeField] private Transform ballSpawn;
    
    [Header("Prefabs")]
    [SerializeField] private Ball ballPrefab;
    [SerializeField] private Paddle paddlePrefab;

    [Header("Display")]
    [SerializeField] private TMP_Text goalsDisplay;

    private ScreenBounds screenBounds;
    private Ball ballInstace;
    private Paddle paddleInstance;

    private int goals;

    private void Awake()
    {
        screenBounds = GetComponent<ScreenBounds>();
    }

    private void Start()
    {
        ballInstace = Instantiate(ballPrefab, ballSpawn);
        paddleInstance = Instantiate(paddlePrefab, paddleSpawn);

        ballInstace.OnGoal += Goal;
        screenBounds.OnExitBounds += TeleportOutOfBoundsPaddle;
    }

    private async void Goal()
    {
        ballInstace.ResetVelocity();
        ballInstace.gameObject.SetActive(false);
        ballInstace.transform.position = ballSpawn.position;
        goals++;
        goalsDisplay.SetText($"Gols: -{goals}");
        
        await Task.Delay(2000);
        ballInstace.gameObject.SetActive(true);
        ballInstace.ApplyRandomDirection();
    }

    private void TeleportOutOfBoundsPaddle()
    {
        Vector3 newPosition = screenBounds.CalculateWrappedPosition(paddleInstance.transform.position);
        paddleInstance.transform.position = newPosition;
    }

    void OnDestroy()
    {
        ballInstace.OnGoal -= Goal;
        screenBounds.OnExitBounds -= TeleportOutOfBoundsPaddle;
    }
}
