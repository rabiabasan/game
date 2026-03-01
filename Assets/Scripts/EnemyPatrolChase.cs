using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyPatrolChase : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }
    public State state = State.Patrol;

    [Header("Target")]
    public Transform player;

    [Header("Ranges")]
    public float detectRange = 10f;   // bu mesafede seni fark eder
    public float loseRange = 14f;     // bu mesafeden sonra bırakır
    public float attackRange = 1.8f;  // yakınsa saldırır (istersen kapat)

    [Header("Patrol Area")]
    public float patrolRadius = 8f;     // devriye alanı
    public float patrolPointTolerance = 0.6f;
    public float patrolWaitMin = 0.5f;
    public float patrolWaitMax = 1.5f;

    [Header("Movement")]
    public float patrolSpeed = 2.2f;
    public float chaseSpeed = 3.5f;
    public float rotateSpeed = 12f;
    public float gravity = -20f;

    [Header("Attack (optional)")]
    public bool enableAttack = true;
    public float attackCooldown = 1.2f;
    public int damage = 10;
    public Transform attackPoint;
    public float attackRadius = 1.0f;
    public LayerMask playerLayer;

    [Header("Animator Params (aynı isimler)")]
    public string speedParam = "Speed";       // Float
    public string combatParam = "InCombat";   // Bool
    public string attackTrigger = "Attack";   // Trigger

    CharacterController cc;
    Animator anim;
    Vector3 velocity;

    Vector3 spawnPos;
    Vector3 patrolTarget;
    float patrolWaitTimer;
    float cd;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        spawnPos = transform.position;

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
        if (player == null)
        {
            // player yoksa sadece patrol
            SetCombat(false);
            PatrolTick();
            ApplyGravity();
            return;
        }

        cd -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        // State geçişleri
        if (state != State.Chase && state != State.Attack)
        {
            if (dist <= detectRange) state = State.Chase;
        }
        else
        {
            if (dist >= loseRange) state = State.Patrol;
        }

        if (enableAttack && state == State.Chase && dist <= attackRange) state = State.Attack;
        if (state == State.Attack && dist > attackRange + 0.3f) state = State.Chase;

        // State çalıştır
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

    // ------------------ PATROL ------------------
    void PatrolTick()
    {
        float distToPoint = Vector3.Distance(transform.position, patrolTarget);

        if (distToPoint <= patrolPointTolerance)
        {
            // hedefe vardık: biraz bekle, sonra yeni nokta seç
            patrolWaitTimer -= Time.deltaTime;
            SetSpeedAnim(0f);

            if (patrolWaitTimer <= 0f)
                PickNewPatrolPoint();

            return;
        }

        MoveTowards(patrolTarget, patrolSpeed);
        SetSpeedAnim(1f); // 0..1, istersen daha doğru yaparız
    }

    void PickNewPatrolPoint()
    {
        // spawn etrafında rastgele bir nokta seç
        Vector2 r = Random.insideUnitCircle * patrolRadius;
        patrolTarget = new Vector3(spawnPos.x + r.x, spawnPos.y, spawnPos.z + r.y);

        patrolWaitTimer = Random.Range(patrolWaitMin, patrolWaitMax);

        // Basit engel kontrolü: önünde duvar varsa yeni nokta seç (çok temel)
        // İstersen geliştiririz.
    }

    // ------------------ CHASE ------------------
    void ChaseTick()
    {
        if (player == null) return;
        MoveTowards(player.position, chaseSpeed);

        // Anim speed 0..1
        SetSpeedAnim(1f);
    }

    // ------------------ ATTACK ------------------
    void AttackTick()
    {
        if (player == null) return;

        Face(player.position);
        SetSpeedAnim(0f);

        if (cd <= 0f)
        {
            cd = attackCooldown;
            if (anim != null) anim.SetTrigger(attackTrigger);

            // Hasarı en temiz şekilde anim event ile ver: DealDamage()
            // İstersen direkt burada da vur: DealDamage();
        }
    }

    // Attack animasyonunun vurduğu frame'e Animation Event olarak bağla
    public void DealDamage()
    {
        if (!enableAttack) return;

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayer, QueryTriggerInteraction.Ignore);
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

    // ------------------ MOVE HELPERS ------------------
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

        Vector3 move = dir * speed;
        cc.Move(move * Time.deltaTime);
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
        Gizmos.DrawWireSphere(Application.isPlaying ? spawnPos : transform.position, patrolRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        if (attackPoint != null) Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}