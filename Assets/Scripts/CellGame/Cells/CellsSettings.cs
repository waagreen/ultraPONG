using UnityEngine;

[CreateAssetMenu(fileName = "CellsSettings", menuName = "Scriptable Objects/CellsSettings")]
public class CellSettings : ScriptableObject
{
    [Header("Perception Values")]
    [Tooltip("How far cells can percieve one another.")] public float perceptionRadius = 2.5f;
    [Tooltip("How close cells can be from one another.")] public float protectedRadius = 1;

    [Header("Movement Internal Settings")]
    public float minSpeed = 2;
    public float maxSpeed = 5;
    [Tooltip("How strongly cells can be steered by external influences.")] public float maxSteerForce = 3;
    
    [Header("Movement External Influences")]
    [Tooltip("Preference to match the velocity of nearby cells.")] public float alignFactor = 1;
    [Tooltip("Preference to move towards the closest group of cells.")] public float cohesionFactor = 1;
    [Tooltip("Preference to avoid colliding with other cells.")] public float avoidFactor = 1;
    [Tooltip("Preference to stay close from where they spawned.")] public float spawnAttractionBias = 0;

    [Header("Obstacle Collisions")]
    [Tooltip("Which layers are considered obstacles.")] public LayerMask obstacleMask;
    [Tooltip("How strongly cells try to avoid colliding with obstacles.")] public float avoidCollisionFactor = 10f;
    [Tooltip("Distance from wich cells percieve obstacles.")] public float collisionAvoidDst = 5;
}
