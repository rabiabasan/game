using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public Transform attackPoint;           // AttackPoint buraya
    public float attackRange = 1.2f;        // yakınlık
    public int damage = 25;
    public float attackCooldown = 0.6f;     // kaç saniyede bir vurabilir
    public LayerMask enemyLayer;            // Enemy layer seçilecek

    [Header("Animation (Optional)")]
    public Animator animator;
    public string attackTriggerName = "Attack";

    private float nextAttackTime = 0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        // animasyonu oynat
        if (animator != null)
            animator.SetTrigger(attackTriggerName);

        // vurma kontrolü (yakındaki düşmanlar)
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);

        foreach (var hit in hits)
        {
            // düşmanda BossHealth / EnemyHealth varsa hasar ver
            var bossHealth = hit.GetComponentInParent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.TakeDamage(damage);
                continue;
            }

            var enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                continue;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}