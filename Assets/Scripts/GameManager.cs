using UnityEngine;

public enum enemystate
{
    idle,
    alert,
    explore,
    patrol,
    follow,
    fury,
}

public class GameManager : MonoBehaviour
{
    public Transform player;

    [Header("Slime IA)")]
    public float slimeIdleWaitTime;
    public Transform[] slimeWaypoints; // Pontos de patrulha para os slimes
    public float slimeDistancetoAttack = 2.3f;

    void Start()
    {

    }

    void Update()
    {

    }
}
