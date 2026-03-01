using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Speed at which the character moves. It is not affected by gravity or jumping.")]
    public float velocity = 5f;

    [Tooltip("This value is added to the speed value while the character is sprinting.")]
    public float sprintAdittion = 3.5f;

    [Tooltip("Jump initial velocity (CharacterController).")]
    public float jumpForce = 6f;

    [Tooltip("Gravity MUST be negative. Example: -20")]
    public float gravity = -20f;

    [Tooltip("Keeps the character glued to ground when grounded.")]
    public float groundedStickVelocity = -2f;

    [Header("Checks")]
    [Tooltip("Only these layers will be considered as ground for head hit checks.")]
    public LayerMask environmentMask = ~0; // her şey (istersen sadece Ground/Default seç)

    // Player states
    bool isSprinting = false;
    bool isCrouching = false;

    // Inputs (Update'te alınır)
    float inputHorizontal;
    float inputVertical;
    bool inputJumpDown;
    bool inputCrouchDown;
    bool inputSprint;

    Animator animator;
    CharacterController cc;

    float verticalVelocity;

    Transform camT;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogWarning("Animator yok. Animasyonlar çalışmaz.");
        else
            animator.applyRootMotion = false;

        if (Camera.main != null) camT = Camera.main.transform;
    }

    void Update()
    {
        // Kamera sonradan oluşuyorsa tekrar yakala
        if (camT == null && Camera.main != null)
            camT = Camera.main.transform;

        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");

        // ÖNEMLİ: Jump = ButtonDown
        inputJumpDown = Input.GetButtonDown("Jump");

        // Sprint (Fire3 default: Left Shift)
        inputSprint = Input.GetButton("Fire3");

        // Crouch toggle
        inputCrouchDown = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.JoystickButton1);
        if (inputCrouchDown)
            isCrouching = !isCrouching;

        // Animator parametreleri (Update’te)
        if (animator != null)
        {
            bool grounded = cc.isGrounded;
            animator.SetBool("air", !grounded);

            // koşma/sprint’i input’a göre daha stabil verelim
            float inputMag = new Vector2(inputHorizontal, inputVertical).magnitude;
            bool isMoving = inputMag > 0.1f;

            animator.SetBool("crouch", isCrouching);
            animator.SetBool("run", isMoving && !isCrouching);

            isSprinting = isMoving && inputSprint && !isCrouching;
            animator.SetBool("sprint", isSprinting);
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Kamera yoksa, world yönünde hareket eder
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (camT != null)
        {
            forward = camT.forward;
            right = camT.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }

        Vector3 inputDir = (forward * inputVertical + right * inputHorizontal);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        // speed
        float velocityAdd = 0f;
        if (isSprinting) velocityAdd = sprintAdittion;
        if (isCrouching) velocityAdd = -(velocity * 0.50f);

        float moveSpeed = velocity + velocityAdd;
        Vector3 horizontalMove = inputDir * moveSpeed;

        // rotation (yalnızca hareket varsa)
        if (inputDir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.15f);
        }

        // vertical
        if (cc.isGrounded)
        {
            // Yere basınca küçük negatifte tut
            if (verticalVelocity < 0f)
                verticalVelocity = groundedStickVelocity;

            // zıplama (sadece buttondown ile)
            if (inputJumpDown && !isCrouching)
                verticalVelocity = jumpForce;
        }
        else
        {
            // kafayı tavana vurduysa zıplama hızını kes
            if (HeadHittingDetect() && verticalVelocity > 0f)
                verticalVelocity = 0f;
        }

        verticalVelocity += gravity * dt;

        Vector3 move = (horizontalMove + Vector3.up * verticalVelocity) * dt;
        cc.Move(move);
    }

    bool HeadHittingDetect()
    {
        // CharacterController center world
        Vector3 ccCenter = transform.TransformPoint(cc.center);

        // Kafa mesafesi: cc.height/2 + biraz pay
        float hitDistance = (cc.height * 0.5f) + 0.05f;

        // Sadece environmentMask’e çarpsın (istersen Ground/Default seç)
        return Physics.Raycast(ccCenter, Vector3.up, hitDistance, environmentMask, QueryTriggerInteraction.Ignore);
    }
}