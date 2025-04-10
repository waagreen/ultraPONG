using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CellsManager : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] private float integrityThreshold;
    [SerializeField][Range(0f, 1f)] private float contaminationThreshold;

    [Header("Spawn Settings")]
    [SerializeField] private BasicCell prefab;
    [SerializeField] private float spawnRadius = 10;
    [SerializeField] private int spawnCount = 10;
    [SerializeField] List<Vector3> spawnPoints = new();

    [Header("Cell Group Behaviour")]
    [SerializeField] private CellSettings settings;
    [SerializeField] private ComputeShader compute;

    [Header("Display Info")]
    [SerializeField] private TMP_Text integrityDisplay;
    [SerializeField] private TMP_Text contaminationDisplay;

    private const int threadGroupSize = 1024;
    private readonly List<BasicCell> cells = new();
    private bool finishedSpawning = false;
    private int goodCells;
    private int badCells;

    public System.Action OnFail;
    public System.Action OnSucceed;

    public float ContaminationLevel
    {
        get => 1f - (spawnCount - badCells) / (float)spawnCount;
    }

    public float IntegrityLevel
    {
        get => 1f - (spawnCount - goodCells) / (float)spawnCount;
    }


    private void UpdateLevels()
    {
        integrityDisplay.SetText($"Integridade\n<color=#28DECA>{string.Format("{0:p}", IntegrityLevel)}</color>");
        contaminationDisplay.SetText($"Contaminação\n<color=#E00053>{string.Format("{0:p}",ContaminationLevel)}</color>");
    }

    private void EliminateCell(BasicCell cell)
    {
        cells.Remove(cell);
        cell.gameObject.SetActive(false);
        cell.OnDeath -= EliminateCell;

        if (cell.IsContaminated) badCells--;
        else goodCells--;
    
        UpdateLevels();
        
        if (IntegrityLevel < integrityThreshold) OnFail.Invoke();
        if (ContaminationLevel < contaminationThreshold) OnSucceed.Invoke();
    }

    private Color GetCellColor(bool isContaminated)
    {
        if (isContaminated) return new(1f, Random.Range(0.4f, 0.6f), Random.Range(0.3f,  0.4f));
        else return new(Random.Range(0.3f,  0.4f), Random.Range(0.4f, 0.6f), 1f);
    }

    private void SpawnCells()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            int spawnPointIndex = Random.Range(0, spawnPoints.Count);
            Vector2 pos = (Vector2)spawnPoints[spawnPointIndex] + Random.insideUnitCircle * spawnRadius;
            BasicCell cell = Instantiate(prefab);
            
            cell.transform.position = pos;
            cell.transform.up = Random.insideUnitCircle.normalized;

            bool isContaminated = Random.Range(1, 7) == 6;
            
            if (isContaminated) badCells++;
            else goodCells++;

            cell.Initialize(settings, isContaminated);
            cell.SetColour(GetCellColor(isContaminated));
            cells.Add(cell);

            cell.OnDeath += EliminateCell;
        }

        UpdateLevels();

        finishedSpawning = true;
    }

    private void Start()
    {
        SpawnCells();
    }

    private void FixedUpdate()
    {
        if (!finishedSpawning) return;
        if (cells.Count < 1) return;

        int numCells = cells.Count;
        CellData[] cellData = new CellData[numCells];

        // Inject cell instances position and direction into the data array
        for (int i = 0; i < cells.Count; i++)
        {
            cellData[i].position = cells[i].position;
            cellData[i].direction = cells[i].up;
        }

        // Initialized buffer and set the data array
        ComputeBuffer cellBuffer = new(numCells, CellData.Size);
        cellBuffer.SetData(cellData);
        
        // Inject variables into the compute shader
        compute.SetBuffer(0, "cells", cellBuffer);
        compute.SetInt("numCells", cells.Count);
        compute.SetFloat("viewRadius", settings.perceptionRadius);
        compute.SetFloat("avoidRadius", settings.avoidanceRadius);

        // Dispatch compute shader to GPU
        int threadGroups = Mathf.CeilToInt(numCells / (float)threadGroupSize);
        compute.Dispatch(0, threadGroups, 1, 1);

        // Update cell instances based on the updated data
        cellBuffer.GetData(cellData);
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].avgFlockHeading = cellData[i].flockHeading;
            cells[i].centreOfFlockmates = cellData[i].flockCentre;
            cells[i].avgAvoidanceHeading = cellData[i].avoidanceHeading;
            cells[i].numPerceivedFlockmates = cellData[i].numFlockmates;

            cells[i].UpdateCell();
        }

        cellBuffer.Release();
    }

    private void OnDrawGizmos()
    {
        foreach(Vector3 pos in spawnPoints)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pos, spawnRadius);
        }
    }

    public struct CellData
    {
        public Vector2 position;
        public Vector2 direction;

        public Vector2 flockHeading;
        public Vector2 flockCentre;
        public Vector2 avoidanceHeading;
        public int numFlockmates;

        public static int Size 
        {
            get
            {
                return sizeof(float) * 2 * 5 + sizeof(int);
            }
        }
    }
}