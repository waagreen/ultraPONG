using UnityEngine;

[CreateAssetMenu(fileName = "CellsSettings", menuName = "Scriptable Objects/CellsSettings")]
public class CellSettings : ScriptableObject
{
    [Header("Movement")]
    public float minSpeed = 1;
    public float maxSpeed = 4;
    public float perceptionRadius = 2f;
    public float avoidanceRadius = 1;
    public float maxSteerForce = 2;
    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float seperateWeight = 1;
    public float targetWeight = 1;

    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float boundsRadius = 0.27f;
    public float avoidCollisionWeight = 10f;
    public float collisionAvoidDst = 5;
}
