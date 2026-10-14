using System.Collections;
using UnityEngine;

// swings a door open when a kid shows up at its door slot

public class DoorSwing : MonoBehaviour
{
    [SerializeField] private KidSpawner kids;
    [SerializeField] private int slot = 1; // 0 = Door 1, 1 = Door 2
    [SerializeField] private float closedAngle = 90f; // hinge Y rotation while closed
    [SerializeField] private float openAngle = 0f; // hinge Y rotation when fully open
    [SerializeField] private float openTime = 1f; // seconds the swing takes

    private bool opened;

    private void Awake()
    {
        SetAngle(closedAngle); // always start closed
        kids.OnKidsChanged += HandleKidsChanged; // Awake runs before the spawner's Start, so we never miss a kid
    }

    private void OnDestroy()
    {
        if (kids != null) kids.OnKidsChanged -= HandleKidsChanged;
    }

    private void HandleKidsChanged()
    {
        if (opened || kids.GetKid(slot) == null) return; // already open, or nobody at this door yet
        opened = true;
        StartCoroutine(Swing());
    }

    // a coroutine can pause (yield) and continue next frame, so the door moves a bit every frame
    private IEnumerator Swing()
    {
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(1f, t + Time.deltaTime / openTime); // deltaTime is 0 while paused, so it waits behind the pop-up
            float eased = 1f - (1f - t) * (1f - t); // fast start, soft stop
            SetAngle(Mathf.LerpAngle(closedAngle, openAngle, eased));
            yield return null; // wait one frame
        }
    }

    private void SetAngle(float y)
    {
        transform.localRotation = Quaternion.Euler(0f, y, 0f);
    }
}