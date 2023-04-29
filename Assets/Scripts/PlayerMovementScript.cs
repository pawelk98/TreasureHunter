using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public Rigidbody playerRb;
    public Animator animator;
    public PlayerCombat playerCombat;

    public float speed = 10;
    public float dodgeSpeed = 30;
    public float dodgeDuration = 0.1f;
    public float dodgeCooldown = 2;
    public float attackSpeed = 1f;
    public float maxAttackDirectionInputTime = 0.2f;

    float attackStart;
    float dodgeStart;
    float dodgeEnd;
    Vector3 dodgeDirection;

    public bool isMoving;
    public bool isDodging;
    public bool isAttacking;
    public bool hadEffect;
    GameObject chestInRange;

    void Update()
    {
        Vector3 moveInput = Vector3.zero;
        if (Input.GetKey("w"))
            moveInput.z += 1;
        if (Input.GetKey("s"))
            moveInput.z -= 1;
        if ( Input.GetKey("d"))
            moveInput.x += 1;
        if (Input.GetKey("a"))
            moveInput.x -= 1;

        Vector3 attackInput = Vector3.zero;
        if (Input.GetKey("up"))
            attackInput.z += 1;
        if (Input.GetKey("down"))
            attackInput.z -= 1;
        if (Input.GetKey("right"))
            attackInput.x += 1;
        if (Input.GetKey("left"))
            attackInput.x -= 1;

        Move(moveInput);
        Dodge(moveInput);
        Attack(attackInput);
        Interact();
    }

    void Move(Vector3 inputDirection)
    {
        if (inputDirection.magnitude == 0 || isAttacking)
        {
            playerRb.velocity = Vector3.zero;
            animator.SetFloat("Moving", 0);
            isMoving = false;
        }
        else if (!isDodging && !isAttacking)
        {
            Vector3 movementDirection = inputDirection.normalized * speed;
            playerRb.velocity = movementDirection;
            transform.LookAt(playerRb.position + movementDirection);
            animator.SetFloat("Moving", movementDirection.magnitude);
            isMoving = true;
        }
    }

    void Dodge(Vector3 inputDirection)
    {
        if (Input.GetKeyDown("z") && isMoving && !isDodging && Time.time - dodgeEnd >= dodgeCooldown)
        {
            dodgeStart = Time.time;
            dodgeDirection = inputDirection.normalized * dodgeSpeed;
            animator.SetFloat("Dodging", 1 / dodgeDuration);
            isDodging = true;
        }

        if (isDodging)
        {
            playerRb.velocity = dodgeDirection;
            transform.LookAt(playerRb.position + dodgeDirection);

            if (Time.time >= dodgeStart + dodgeDuration)
            {
                dodgeEnd = Time.time;
                isDodging = false;
                animator.SetFloat("Dodging", 0);
            }
        }
    }

    void Attack(Vector3 inputDirection)
    {
        if (inputDirection.magnitude > 0 && !isDodging && !isAttacking)
        {
            attackStart = Time.time;
            animator.SetFloat("Attack_Melee", attackSpeed);
            isAttacking = true;
        }
        else if (isAttacking && Time.time - attackStart >= 1 / attackSpeed)
        {
            animator.SetFloat("Attack_Melee", 0);
            isAttacking = false;
            hadEffect = false;
        }
        else if (isAttacking && !hadEffect && Time.time - attackStart >= 1 / attackSpeed / 2)
        {
            playerCombat.Attack();
            hadEffect = true;
        }
        else if (isAttacking && Time.time - attackStart <= maxAttackDirectionInputTime)
            transform.LookAt(playerRb.position + inputDirection);

    }

    void Interact()
    {
        if(Input.GetKeyDown("v"))
        {
            if(chestInRange != null)
            {
                chestInRange.GetComponent<ChestController>().Open();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Chest")
            chestInRange = other.gameObject;
        else if (isAttacking && other.gameObject.tag == "Enemy")
            Debug.Log("You hit an enemy");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == chestInRange)
            chestInRange = null;
    }
}
