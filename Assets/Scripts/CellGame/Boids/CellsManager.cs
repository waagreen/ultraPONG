using System.Collections.Generic;
using UnityEngine;

public class CellsManager : MonoBehaviour
{
    private const int threadGroupSize = 1024;
    
    public BasicCell prefab;
    public float spawnRadius = 10;
    public int spawnCount = 10;
    public Color colour;

    public CellSettings settings;
    public ComputeShader compute;

    private readonly List<BasicCell> cells = new();
    private bool finishedSpawning = false;

    private void SpawnCells()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
            BasicCell cell = Instantiate(prefab);
            cell.transform.position = pos;
            cell.transform.up = Random.insideUnitCircle.normalized;

            cell.Initialize(settings, null);
            cell.SetColour(colour);
            cells.Add(cell);
        }

        finishedSpawning = true;
    }

    private void Start()
    {
        SpawnCells();
    }

    private void Update()
    {
        if (finishedSpawning)
        {
            int numCells = cells.Count;
            CellData[] cellData = new CellData[numCells];

            for (int i = 0; i < cells.Count; i++)
            {
                cellData[i].position = cells[i].position;
                cellData[i].direction = cells[i].forward;
            }

            var cellBuffer = new ComputeBuffer(numCells, CellData.Size);
            cellBuffer.SetData(cellData);

            compute.SetBuffer(0, "cells", cellBuffer);
            compute.SetInt("numCells", cells.Count);
            compute.SetFloat("viewRadius", settings.perceptionRadius);
            compute.SetFloat("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt (numCells / (float) threadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);

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
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(colour.r, colour.g, colour.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
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
                return sizeof (float) * 2 * 5 + sizeof (int);
            }
        }
    }
}