using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundDrag_multiplier = 1f;
    
    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    
    [Header("Camera")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 90f;
    private float xRotation = 0f;
    private Camera mainCamera;
    
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 20f;
    [SerializeField] private float staminaDrainRate = 30f;
    private float currentStamina;
    private bool isMoving;
    
    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private Vector3 moveDirection;
    private Vector3 velocity;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        currentStamina = maxStamina;
        
        // Инициализация Input System
        var inputMap = InputSystem.actions;
        moveAction = inputMap.FindAction("Move");
        lookAction = inputMap.FindAction("Look");
        jumpAction = inputMap.FindAction("Jump");
        
        if (moveAction != null) moveAction.Enable();
        if (lookAction != null) lookAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
    }
    
    void Update()
    {
        HandleCamera();
        HandleGroundCheck();
        HandleStamina();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
    }
    
    void HandleMovement()
    {
        Vector2 moveInput = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        
        isMoving = moveInput.magnitude > 0.1f;
        
        // Направление движения относительно камеры
        moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.y = 0; // Не влияет на Y ось
        moveDirection = moveDirection.normalized;
        
        // Плавное ускорение
        velocity = Vector3.Lerp(velocity, moveDirection * moveSpeed, Time.fixedDeltaTime * acceleration);
        
        // Применение драга
        if (isGrounded)
            velocity.y = -2f; // Небольшое давление на землю
        else
            velocity.y += Physics.gravity.y * Time.fixedDeltaTime;
        
        rb.velocity = velocity;
    }
    
    void HandleCamera()
    {
        Vector2 lookInput = lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        
        // Горизонтальное вращение (Y ось)
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
        
        // Вертикальное вращение (X ось)
        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        
        if (mainCamera != null)
            mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    void HandleGroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
    }
    
    void HandleStamina()
    {
        if (isMoving && isGrounded)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime * 0.3f; // Небольшой расход
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }
        
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }
    
    public float GetStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
    public bool CanAct() => currentStamina > 10f;
    public void DrainStamina(float amount) => currentStamina = Mathf.Max(0, currentStamina - amount);
}