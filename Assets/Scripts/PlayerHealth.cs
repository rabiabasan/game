using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth == 0)
        {
            Debug.Log("Player died!");
            // burada respawn / game over yaparsın
        }
    }
}