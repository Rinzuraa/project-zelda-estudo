using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator anim;

    [Header("Configurações do Jogador")]
    public float movementSpeed = 3f;
    public float runningSpeed = 6f; // Velocidade de corrida

    private Vector3 direction;
    private bool isWalk;
    private bool isRunning; // Verifica se o jogador está correndo

    // Inputs
    private float horizontal;
    private float vertical;

    [Header("Configurações de Ataque")]
    public ParticleSystem fxAttack;
    public Transform Hitbox;
    [Range(0.2f, 1f)]
    public float Hitrange = 0.5f;
    public LayerMask hitMask; // Máscara de camada para detectar inimigos
    private bool isAttacking;
    public Collider[] hitInfo;
    public int amountDmg;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        INPUTS();           // Captura os inputs do jogador
        MOVECHARACTER();    // Move o personagem com base nos inputs
        UPDATEANIMATION();  // Atualiza as animações do personagem
    }

    #region MEUS MÉTODOS

    // CAPTURA OS INPUTS DO JOGADOR
    void INPUTS()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // Verifica se o jogador pressionou o botão de ataque
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            Attack();
        }

        // Normaliza a direção do jogador
        direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Verifica se o jogador está segurando a tecla Shift para correr
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    // REALIZA O ATAQUE DO JOGADOR
    void Attack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack");
        fxAttack.Emit(1);

        // Detecta os inimigos dentro da área de ataque
        hitInfo = Physics.OverlapSphere(Hitbox.position, Hitrange, hitMask);

        foreach (Collider c in hitInfo)
        {
            // Envia a mensagem para o objeto atingido
            c.gameObject.SendMessage("GetHit", amountDmg, SendMessageOptions.DontRequireReceiver);
        }
    }

    // MOVE O PERSONAGEM COM BASE NOS INPUTS CAPTURADOS
    void MOVECHARACTER()
    {
        float currentSpeed = isRunning ? runningSpeed : movementSpeed;

        if (direction.magnitude > 0.1f)
        {
            // Calcula o ângulo de rotação do personagem
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }

        // Move o personagem
        controller.Move(direction * currentSpeed * Time.deltaTime);
    }

    // ATUALIZA AS ANIMAÇÕES DO PERSONAGEM
    void UPDATEANIMATION()
    {
        anim.SetBool("isWalk", isWalk);
        anim.SetBool("isRunning", isRunning);
    }

    // CHAMADO PELO ANIMATION EVENT PARA FINALIZAR O ATAQUE
    void attackisOver()
    {
        isAttacking = false;
    }

    #endregion

    // DESENHA A ÁREA DE ATAQUE NO INSPECTOR
    void OnDrawGizmosSelected()
    {
        if (Hitbox != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Hitbox.position, Hitrange);
        }
    }
}