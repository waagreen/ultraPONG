using System.Collections.Generic;
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
    private Ball ballInstance;
    private Paddle paddleInstance;
    
    public List<Goalkeeper> goalKeepers;

    private int goals;

    private void Awake()
    {
        screenBounds = GetComponent<ScreenBounds>();
    }

    private void Start()
    {
        ballInstance = Instantiate(ballPrefab, ballSpawn);
        paddleInstance = Instantiate(paddlePrefab, paddleSpawn);

        ballInstance.OnGoal += Goal;
        screenBounds.OnExitBounds += TeleportOutOfBoundsPaddle;
        foreach (var keeper in goalKeepers) keeper.InjectTarget(ballInstance.gameObject);
    }

    private async void Goal()
    {
        if (ballInstance == null) return;
        
        ballInstance.ResetMovement();
        ballInstance.gameObject.SetActive(false);
        ballInstance.transform.position = ballSpawn.position;
        goals++;
        goalsDisplay.SetText($"<color=#FFFFFF>Gols:</color> <color=#22DD70>{goals}</color>");
        
        await Task.Delay(2000);
        if (ballInstance == null) return;

        ballInstance.gameObject.SetActive(true);
    }

    private void TeleportOutOfBoundsPaddle()
    {
        if (paddleInstance == null) return;
        
        Vector3 newPosition = screenBounds.CalculateWrappedPosition(paddleInstance.transform.position);
        paddleInstance.transform.position = newPosition;
    }

    void OnDestroy()
    {
        ballInstance.OnGoal -= Goal;
        screenBounds.OnExitBounds -= TeleportOutOfBoundsPaddle;
    }
}
