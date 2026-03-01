using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator anim;
    public float cooldown = 0.4f;
    float cd;

    void Awake()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        cd -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && cd <= 0f)
        {
            cd = cooldown;
            anim.SetTrigger("Attack"); // ✅ bu satır var
        }
    }
}