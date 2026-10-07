using UnityEngine;
using UnityEngine.InputSystem;

// first person movement and mouse look

// [RequireComponent] makes Unity automatically add a CharacterController with  this script and stops from removing it by accident
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform; // the Main Camera

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f; // m/s
    [SerializeField] private float gravity = -20f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 0.1f;

    // public so bonus effects can change it
    public float CurrentSpeed { get; set; }

    private CharacterController controller;
    private float pitch; // up/down camera angle
    private float verticalVelocity; // falling speed

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        CurrentSpeed = walkSpeed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        SetCursorLocked(true);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!GameManager.IsPlaying) return; // frozen on Game Over screen

        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (kb == null || mouse == null) return; // no keyboard/mouse plugged in

        // Cursor lock TODO: pause menu will appear later
        if (kb.escapeKey.wasPressedThisFrame) SetCursorLocked(false);
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (mouse.leftButton.wasPressedThisFrame) SetCursorLocked(true);
            return; // dont move or look while the cursor is free
        }

        // Mouse look
        // mouse delta = how many pixels the mouse moved THIS frame
        Vector2 look = mouse.delta.ReadValue() * mouseSensitivity;
        
        transform.Rotate(0f, look.x, 0f); // left/right turns the whole body
        pitch = Mathf.Clamp(pitch - look.y, -85f, 85f); // up/down tilts only camera
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);

        // movement
        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed) input.y += 1;
        if (kb.sKey.isPressed) input.y -= 1;
        if (kb.dKey.isPressed) input.x += 1;
        if (kb.aKey.isPressed) input.x -= 1;
        input = Vector2.ClampMagnitude(input, 1f); // diagonal isnt faster than straight
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        // gravity
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f; // keep snapped to floor
        verticalVelocity += gravity * Time.deltaTime;

        // apply delta time to movement
        Vector3 velocity = move * CurrentSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
