using System.Collections;
using UnityEngine;
using UnityEngine.AI;

//Comportamentos da IA do Slime
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

    public const float patrolWaitTime = 5f;
    private bool isWalk;
    private bool isAlert;
    private bool isAttack;
    private bool isPlayerVisible;
    private int idWaypoint;
    private Vector3 destination;

    void Start()
    {
        // Inicializa referências e define o estado inicial
        _GameManager = FindFirstObjectByType<GameManager>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ChangeState(state);
    }

    void Update()
    {
        if (isDead) return;

        StateManager(); // Gerencia o comportamento de acordo com o estado

        // Atualiza parâmetros de animação
        isWalk = agent.desiredVelocity.magnitude >= 0.1f;
        anim.SetBool("isWalk", isWalk);
        anim.SetBool("isAlert", isAlert);
    }

    private IEnumerator Dead()
    {
        // Para o movimento e aguarda antes de destruir o objeto
        agent.isStopped = true;
        yield return new WaitForSeconds(2.5f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detecta se o player entrou no trigger de visão
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerVisible = true;
            if (state != EnemyState.FURY)
            {
                ChangeState(EnemyState.ALERT);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Detecta se o player saiu do trigger de visão
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerVisible = false;
        }
    }

    #region Meus Métodos

    // Recebe dano do player ou outro atacante
    public void GetHit(int amount)
    {
        if (isDead) return;

        health -= amount;

        if (health > 0)
        {
            ChangeState(EnemyState.FURY); // Fica furioso ao ser atingido
            anim.SetTrigger("GetHit");
        }
        else
        {
            isDead = true;
            anim.SetTrigger("Die");
            StartCoroutine(Dead()); // Inicia a rotina de destruição
        }
    }

    // Gerencia o comportamento de acordo com o estado atual
    void StateManager()
    {
        switch (state)
        {
            case EnemyState.FOLLOW:
                // Persegue o jogador
                destination = _GameManager.player.position;
                agent.destination = destination;
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    Attack();
                }

                break;
            case EnemyState.FURY:
                // Persegue o jogador com distância de ataque reduzida
                destination = _GameManager.player.position;
                agent.destination = destination;

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    Attack();
                }

                break;
        }
    }

    // Troca o estado do slime e executa a lógica do novo estado
    void ChangeState(EnemyState newState)
    {
        if (isDead) return;

        StopAllCoroutines();
        isAlert = false;

        switch (newState)
        {
            case EnemyState.IDLE:
                // Fica parado no lugar
                agent.stoppingDistance = 0;
                destination = transform.position;
                agent.destination = destination;
                StartCoroutine(IdleCoroutine());
                break;

            case EnemyState.ALERT:
                // Fica alerta por um tempo
                agent.stoppingDistance = 0;
                destination = transform.position;
                agent.destination = destination;
                isAlert = true;
                StartCoroutine(AlertCoroutine());
                break;

            case EnemyState.PATROL:
                // Vai até um waypoint aleatório
                agent.stoppingDistance = 0;
                idWaypoint = Random.Range(0, _GameManager.slimeWaypoints.Length);
                destination = _GameManager.slimeWaypoints[idWaypoint].position;
                agent.destination = destination;
                StartCoroutine(PatrolCoroutine());
                break;

            case EnemyState.FOLLOW:
                // Apenas ajusta a distância de parada para ataque
                agent.stoppingDistance = _GameManager.slimeDistancetoAttack;
                StartCoroutine("FOLLOW");
                StartCoroutine(AttackDelay());
                break;

            case EnemyState.FURY:
                // Para no lugar, mas pode ser expandido para comportamento agressivo
                destination = transform.position;
                agent.destination = destination;
                break;
        }
        state = newState;
    }

    // Rotina para o estado IDLE
    IEnumerator IdleCoroutine()
    {
        yield return new WaitForSeconds(_GameManager.slimeIdleWaitTime);
        if (!isDead)
            StayStill(50);
    }

    // Rotina para o estado ALERT
    IEnumerator AlertCoroutine()
    {
        yield return new WaitForSeconds(_GameManager.slimeAlertTime);
        isAlert = true;
        if (isPlayerVisible)
        {
            ChangeState(EnemyState.FOLLOW);
        }
        else
        {
            StayStill(10);
        }
    }

    // Rotina para o estado PATROL
    IEnumerator PatrolCoroutine()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= 0.1f);
        if (!isDead)
            StayStill(30);
    }

    // Decide se permanece parado ou patrulha novamente
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

    IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(_GameManager.slimeAttackDelay);
        isAttack = false;
    }

    void Attack()
    {
        StartCoroutine(AttackDelay());
        isAttack = true;
        anim.SetTrigger("Attack");   
    }

    #endregion
}
