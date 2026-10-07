using UnityEngine;
using UnityEngine.InputSystem;

// left click fires an instant raycast bullet from center of screen
// the gun lowers while crosshair is on the door zone
// lives on the Player object

public class Gun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform gunModel;
    [SerializeField] private Transform muzzle;
    [SerializeField] private LineRenderer tracer;

    [Header("Shooting")]
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private float range = 40f;
    [SerializeField] private float fireCooldown = 0.15f; // min seconds between shots
    [SerializeField] private LayerMask shootMask = ~0; // ~0 = everything
    [SerializeField] private float tracerTime = 0.05f;

    [Header("Lowered pose (facing the door)")]
    [SerializeField] private Vector3 loweredOffset = new Vector3(0f, -0.2f, -0.05f);
    [SerializeField] private float loweredTilt = 45f; // degrees
    [SerializeField] private float poseSpeed = 10f; // how fast it moves between poses

    public int Damage { get; set; } // Yellow effect raises this
    public bool IsLowered { get; private set; }

    private Vector3 raisedPos;
    private Quaternion raisedRot;
    private float nextFireTime;
    private float tracerHideTime;

    private void Awake()
    {
        Damage = baseDamage;
        raisedPos = gunModel.localPosition;
        raisedRot = gunModel.localRotation;
        tracer.enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!GameManager.IsPlaying) return;

        // ray from the middle of the screen, straight forward
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        IsLowered = AimingAtDoor(ray);
        UpdatePose();

        if (tracer.enabled && Time.time > tracerHideTime) tracer.enabled = false;

        Mouse mouse = Mouse.current;
        if (mouse == null || Cursor.lockState != CursorLockMode.Locked) return;
        if (IsLowered) return;

        if (mouse.leftButton.wasPressedThisFrame && Time.time >= nextFireTime) Shoot(ray);
    }

    // true if the FIRST thing the crosshair touches is the door zone
    private bool AimingAtDoor(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Collide))
        {
            return hit.collider.GetComponent<NoShootZone>() != null;
        }
        return false;
    }

    private void Shoot(Ray ray)
    {
        nextFireTime = Time.time + fireCooldown;

        Vector3 end = ray.origin + ray.direction * range; // where the tracer ends if nothing hit

        // QueryTriggerInteraction.Ignore = bullets fly through triggers
        if (Physics.Raycast(ray, out RaycastHit hit, range, shootMask, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;

            Ghost ghost = hit.collider.GetComponentInParent<Ghost>();
            if (ghost != null) ghost.TakeHit(Damage);
        }

        tracer.SetPosition(0, muzzle.position);
        tracer.SetPosition(1, end);
        tracer.enabled = true;
        tracerHideTime = Time.time + tracerTime;
    }

    // smoothly move the model toward the raised or lowered pose
    private void UpdatePose()
    {
        Vector3 targetPos = IsLowered ? raisedPos + loweredOffset : raisedPos;
        Quaternion targetRot = IsLowered ? raisedRot * Quaternion.Euler(loweredTilt, 0f, 0f) : raisedRot;

        float t = poseSpeed * Time.deltaTime;
        gunModel.localPosition = Vector3.Lerp(gunModel.localPosition, targetPos, t);
        gunModel.localRotation = Quaternion.Slerp(gunModel.localRotation, targetRot, t);
    }
}
