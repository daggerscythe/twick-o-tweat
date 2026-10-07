using UnityEngine;
using UnityEngine.InputSystem;

// shots a raycast every frame out of the camera to check for interactables

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerBag bag;
    [SerializeField] private float range = 3f; // meters
    [SerializeField] private LayerMask hitMask = ~0; // ~0 = "everything"
    [SerializeField] private bool logInteractions = true; // prints to the console: TODO: remove for HUD

    public IInteractable Current { get; private set; }
    public string CurrentPrompt { get; private set; } = "";

    // Update is called once per frame
    private void Update()
    {
        if (!GameManager.IsPlaying) return; // frozen on Game Over screen

        Current = null;
        CurrentPrompt = "";

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        string hitName = "";

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            // GetComponentInParent searches parents if ray hits a child mesh of an object whose script sits on the parent
            Current = hit.collider.GetComponentInParent<IInteractable>();
            hitName = hit.collider.name;
        }

        if (Current == null) return;

        CurrentPrompt = Current.GetPrompt(bag);

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Current.Interact(bag);

            if (logInteractions)
            {
                string bagState = bag.HasBag
                    ? $"{GameColors.ToName(bag.CurrentColor)} bag, {bag.Contents.Count} candy"
                    : "no bag";
                Debug.Log($"E on '{hitName}' -> {bagState}");
            }
        }
    }

    // draw ray in Scene view for debug
    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * range);
    }
}
