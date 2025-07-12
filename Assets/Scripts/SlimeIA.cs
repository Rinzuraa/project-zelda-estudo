using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    IDLE,
    ALERT,
    EXPLORE,
    PATROL,
    FOLLOW,
    FURY
}

public class SlimeIA : MonoBehaviour
{
    private GameManager _GameManager;
    private Animator anim;
    private NavMeshAgent agent;

    public int health = 2;
    private bool isDead;

    public EnemyState state = EnemyState.IDLE;

    public const float patrolWaitTime = 5f; // Tempo de espera no estado de patrulha

    // IA
    private bool isWalk;
    private bool isAlert;
    private int idWaypoint;
    private Vector3 destination;

    void Start()
    {
        _GameManager = FindFirstObjectByType<GameManager>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        ChangeState(state); // Inicia o slime no estado inicial
    }

    void Update()
    {
        if (isDead) return; // Não faz nada se estiver morto

        StateManager();

        if (agent.desiredVelocity.magnitude >= 0.1f)
            isWalk = true;
        else
            isWalk = false;

        anim.SetBool("isWalk", isWalk);
    }

    // Coroutine chamada quando o slime morre
    private IEnumerator Dead()
    {
        Debug.Log("Coroutine Dead chamada!");
        agent.isStopped = true; // Para o movimento do NavMeshAgent
        yield return new WaitForSeconds(2.5f); // Espera a animação de morte terminar
        Destroy(gameObject); // Destroi o slime
        StopAllCoroutines(); // Para todas as corrotinas de IA
    }

    #region Meus Métodos

    /// <summary>
    /// Recebe dano do player ou outro atacante.
    /// </summary>
    /// <param name="amount">Quantidade de dano recebido</param>
    public void GetHit(int amount)
    {
        if (isDead) return;

        health -= amount;

        if (health > 0)
        {
            ChangeState(EnemyState.FURY);
            anim.SetTrigger("GetHit");
        }
        else
        {
            isDead = true;
            anim.SetTrigger("Die");
            StartCoroutine(Dead());
        }
    }

    // Gerencia o estado atual do slime
    void StateManager()
    {
        switch (state)
        {
            case EnemyState.FOLLOW:
                // Adicione lógica de perseguição se desejar
                break;
            case EnemyState.FURY:
                destination = _GameManager.player.position;
                agent.stoppingDistance = _GameManager.slimeDistancetoAttack;
                agent.destination = destination;
                break;
            // Adicione outros estados conforme necessário
        }
    }

    // Troca o estado do slime e executa a lógica do novo estado
    void ChangeState(EnemyState newState)
    {
        if (isDead) return;

        StopAllCoroutines();
        Debug.Log("Novo estado: " + newState);
        state = newState;

        switch (state)
        {
            case EnemyState.IDLE:
                agent.stoppingDistance = 0;
                destination = transform.position;
                agent.SetDestination(destination);
                StartCoroutine(IdleCoroutine());
                break;

            case EnemyState.ALERT:
                // Adicione lógica para ALERT se necessário
                break;

            case EnemyState.PATROL:
                agent.stoppingDistance = 0;
                idWaypoint = Random.Range(0, _GameManager.slimeWaypoints.Length);
                destination = _GameManager.slimeWaypoints[idWaypoint].position;
                agent.destination = destination;
                StartCoroutine(PatrolCoroutine());
                break;

            case EnemyState.FURY:
                destination = transform.position;
                agent.destination = destination;
                break;

            // Adicione outros estados conforme necessário
        }
    }

    // Coroutine para o estado IDLE
    IEnumerator IdleCoroutine()
    {
        yield return new WaitForSeconds(_GameManager.slimeIdleWaitTime);
        if (!isDead)
            StayStill(50);
    }

    // Coroutine para o estado PATROL
    IEnumerator PatrolCoroutine()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= 0.1f);
        if (!isDead)
            StayStill(30);
    }

    // Decide se fica parado ou patrulha novamente
    void StayStill(int yes)
    {
        if (isDead) return;

        if (Rand() <= yes)
            ChangeState(EnemyState.IDLE);
        else
            ChangeState(EnemyState.PATROL);
    }

    // Retorna um número aleatório entre 0 e 99
    int Rand()
    {
        return Random.Range(0, 100);
    }

    #endregion
}
