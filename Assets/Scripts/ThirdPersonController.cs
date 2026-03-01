using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Tooltip("Speed ​​at which the character moves. It is not affected by gravity or jumping.")]
    public float velocity = 5f;

    [Tooltip("This value is added to the speed value while the character is sprinting.")]
    public float sprintAdittion = 3.5f;

    [Tooltip("The higher the value, the higher the character will jump.")]
    public float jumpForce = 6f; // (ÖNERİ) 18 çok yüksek; CC ile genelde 5-8 arası

    [Tooltip("Force that pulls the player down. MUST be negative. Example: -20")]
    public float gravity = -20f; // <-- NEGATIF olmalı

    [Space]
    public float groundedStickVelocity = -2f; // zemine yapıştırma (gömülmeyi değil, havada kalmayı düzeltir)

    // Player states
    bool isSprinting = false;
    bool isCrouching = false;

    // Inputs
    float inputHorizontal;
    float inputVertical;
    bool inputJump;
    bool inputCrouch;
    bool inputSprint;

    Animator animator;
    CharacterController cc;

    // FIX: gerçek dikey hız
    float verticalVelocity;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogWarning("Animator yok. Animasyonlar çalışmaz.");

        // CharacterController ile Root Motion genelde kapalı olmalı
        if (animator != null)
            animator.applyRootMotion = false;
    }

    void Update()
    {
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        inputJump = Input.GetAxis("Jump") == 1f;
        inputSprint = Input.GetAxis("Fire3") == 1f;
        inputCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);

        if (inputCrouch)
            isCrouching = !isCrouching;

        if (animator != null)
        {
            animator.SetBool("air", cc.isGrounded == false);

            if (cc.isGrounded)
            {
                animator.SetBool("crouch", isCrouching);

                float minimumSpeed = 0.9f;
                animator.SetBool("run", cc.velocity.magnitude > minimumSpeed);

                isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;
                animator.SetBool("sprint", isSprinting);
            }
        }

        // Jump isteğini burada yakala, uygulamasını FixedUpdate'te yapacağız
        // (Input kaçmasın diye Update'te almak doğru)
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Sprinting velocity boost or crounching desacelerate
        float velocityAdittion = 0f;
        if (isSprinting) velocityAdittion = sprintAdittion;
        if (isCrouching) velocityAdittion = -(velocity * 0.50f); // -50%

        // Kamera yönüne göre hareket
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 inputDir = (forward * inputVertical + right * inputHorizontal).normalized;

        float moveSpeed = (velocity + velocityAdittion);
        Vector3 horizontalMove = inputDir * moveSpeed;

        // --- ROTATION ---
        if (inputDir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }

        // --- VERTICAL / JUMP FIX ---
        if (cc.isGrounded)
        {
            // Yerdeyken küçük negatifte tut (zemine yapış)
            if (verticalVelocity < 0f)
                verticalVelocity = groundedStickVelocity;

            if (inputJump)
            {
                verticalVelocity = jumpForce; // zıplama başlangıç hızı
            }
        }
        else
        {
            // Havada kafa çarpması varsa zıplamayı kes
            if (HeadHittingDetect())
            {
                if (verticalVelocity > 0f) verticalVelocity = 0f;
            }
        }

        // Gravity hız olarak birikir
        verticalVelocity += gravity * dt;

        Vector3 move = (horizontalMove + Vector3.up * verticalVelocity) * dt;
        cc.Move(move);
    }

    // Head hit: true döndürsün (FixedUpdate'te kullanıyoruz)
    bool HeadHittingDetect()
    {
        float headHitDistanceMultiplier = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistanceMultiplier;

        return Physics.Raycast(ccCenter, Vector3.up, hitCalc);
    }
}