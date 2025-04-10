using UnityEngine;

[CreateAssetMenu(fileName = "CellsSettings", menuName = "Scriptable Objects/CellsSettings")]
public class CellSettings : ScriptableObject
{
    [Header("Movement Internal Settings")]
    public float minSpeed = 2;
    public float maxSpeed = 5;
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 1;
    public float maxSteerForce = 3;
    
    [Header("Movement External Influences")]
    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float seperateWeight = 1;
    public float targetWeight = 1;

    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float avoidCollisionWeight = 10f;
    public float collisionAvoidDst = 5;
}
