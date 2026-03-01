using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public int maxHP = 100;
    public int hp;

    [Header("Death")]
    public GameObject deathVFXPrefab;   // BossDeathVFX buraya sürüklenecek
    public AudioClip deathSfx;
    public float destroyDelay = 0.1f;   // hemen yok et, efekt ayrı çalışsın

    private AudioSource audioSource;

    void Awake()
    {
        hp = maxHP;
        audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
            Die();
    }

    void Die()
    {
        // 1) Efekti bossun olduğu yerde oluştur
        if (deathVFXPrefab != null)
        {
            Instantiate(deathVFXPrefab, transform.position, transform.rotation);
        }

        // 2) Ölüm sesi (boss yok olacağı için OneShot daha iyi)
        if (deathSfx != null)
        {
            AudioSource.PlayClipAtPoint(deathSfx, transform.position);
        }

        // 3) Bossu yok et
        Destroy(gameObject, destroyDelay);
    }
}