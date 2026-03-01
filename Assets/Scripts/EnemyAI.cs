using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI_NoNavMesh : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }
    public State state = State.Patrol;

    [Header("References")]
    public Transform player;
    public Transform attackPoint; // Enemy'nin önünde boş obje

    [Header("Detection")]
    public float detectRange = 10f;
    public float loseRange = 14f;
    public float attackRange = 1.8f;

    [Header("Patrol Area")]
    public float patrolRadius = 8f;
    public float patrolPointTolerance = 0.7f;
    public float waitAtPointMin = 0.5f;
    public float waitAtPointMax = 1.5f;

    [Header("Movement")]
    public float patrolSpeed = 2.2f;
    public float chaseSpeed = 3.5f;
    public float rotateSpeed = 12f;
    public float gravity = -20f;

    [Header("Attack")]
    public bool enableAttack = true;
    public float attackCooldown = 1.2f;
    public int damage = 10;
    public float attackRadius = 1.0f;
    public LayerMask playerLayer;

    [Header("Animator Params (aynı isimler)")]
    public string speedParam = "Speed";      // Float
    public string combatParam = "InCombat";  // Bool
    public string attackTrigger = "Attack";  // Trigger

    CharacterController cc;
    Animator anim;

    Vector3 velocity;
    Vector3 originPos;
    Vector3 patrolTarget;
    float waitTimer;
    float cd;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        originPos = transform.position;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (attackPoint == null) attackPoint = transform;

        PickNewPatrolPoint();
    }

    void Update()
    {
        cd -= Time.deltaTime;

        // Player yoksa sadece patrol
        if (player == null)
        {
            SetCombat(false);
            PatrolTick();
            ApplyGravity();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        // State geçişleri
        if (state == State.Patrol)
        {
            if (dist <= detectRange) state = State.Chase;
        }
        else // Chase veya Attack
        {
            if (dist >= loseRange) state = State.Patrol;
        }

        if (enableAttack)
        {
            if (state == State.Chase && dist <= attackRange) state = State.Attack;
            if (state == State.Attack && dist > attackRange + 0.3f) state = State.Chase;
        }

        // Çalıştır
        switch (state)
        {
            case State.Patrol:
                SetCombat(false);
                PatrolTick();
                break;

            case State.Chase:
                SetCombat(true);
                ChaseTick();
                break;

            case State.Attack:
                SetCombat(true);
                AttackTick();
                break;
        }

        ApplyGravity();
    }

    // ---------------- PATROL ----------------
    void PatrolTick()
    {
        float d = Vector3.Distance(transform.position, patrolTarget);

        if (d <= patrolPointTolerance)
        {
            // varınca bekle
            waitTimer -= Time.deltaTime;
            SetSpeedAnim(0f);

            if (waitTimer <= 0f)
                PickNewPatrolPoint();

            return;
        }

        MoveTowards(patrolTarget, patrolSpeed);
        SetSpeedAnim(1f);
    }

    void PickNewPatrolPoint()
    {
        Vector2 r = Random.insideUnitCircle * patrolRadius;
        patrolTarget = new Vector3(originPos.x + r.x, originPos.y, originPos.z + r.y);
        waitTimer = Random.Range(waitAtPointMin, waitAtPointMax);
    }

    // ---------------- CHASE ----------------
    void ChaseTick()
    {
        MoveTowards(player.position, chaseSpeed);
        SetSpeedAnim(1f);
    }

    // ---------------- ATTACK ----------------
    void AttackTick()
    {
        Face(player.position);
        SetSpeedAnim(0f);

        if (cd <= 0f)
        {
            cd = attackCooldown;
            if (anim != null) anim.SetTrigger(attackTrigger);
            // Hasar için en iyisi: Attack animasyonuna Animation Event -> DealDamage()
        }
    }

    // Attack animasyonunda vurduğu frame'e event koy: DealDamage()
    public void DealDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            attackPoint.position,
            attackRadius,
            playerLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (var h in hits)
        {
            var ph = h.GetComponentInParent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
                break;
            }
        }
    }

    // ---------------- HELPERS ----------------
    void MoveTowards(Vector3 targetPos, float speed)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
        {
            SetSpeedAnim(0f);
            return;
        }

        dir.Normalize();
        Face(transform.position + dir);

        cc.Move(dir * speed * Time.deltaTime);
    }

    void Face(Vector3 lookAtPos)
    {
        Vector3 dir = lookAtPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotateSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (cc.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        cc.Move(Vector3.up * velocity.y * Time.deltaTime);
    }

    void SetSpeedAnim(float v01)
    {
        if (anim != null) anim.SetFloat(speedParam, v01);
    }

    void SetCombat(bool v)
    {
        if (anim != null) anim.SetBool(combatParam, v);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? originPos : transform.position, patrolRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        if (attackPoint != null) Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}