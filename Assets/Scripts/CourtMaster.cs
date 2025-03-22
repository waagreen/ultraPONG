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

    private Ball ballInstace;
    private Paddle paddleInstance;

    private int goals;

    private void Start()
    {
        ballInstace = Instantiate(ballPrefab, ballSpawn);
        paddleInstance = Instantiate(paddlePrefab, paddleSpawn);

        ballInstace.OnGoal += Goal;
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
        ballInstace.DoRandomDirectionImpulse();
    }

    void OnDestroy()
    {
        ballInstace.OnGoal -= Goal;
    }
}
