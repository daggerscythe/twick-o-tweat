using System.Collections;
using UnityEngine;

// pops the trash can lid open when a bag is dumped
// lives on the TrashCan object

public class TrashCanLid : MonoBehaviour
{
    [SerializeField] private Transform lidHinge;
    [SerializeField] private Transform pedalPivot;

    [Header("Angles (flip the sign if it moves the wrong way)")]
    [SerializeField] private float lidOpenAngle = -70f; // X rotation of the lid when fully open
    [SerializeField] private float pedalPressAngle = 15f; // X rotation of the pedal when pressed

    [Header("Timing (seconds)")]
    [SerializeField] private float openTime = 0.15f;
    [SerializeField] private float holdTime = 0.4f;
    [SerializeField] private float closeTime = 0.3f;

    private Quaternion lidClosed;
    private Quaternion pedalUp;
    private float openAmount; // 0 = closed, 1 = fully open
    private Coroutine running;

    private void Awake()
    {
        // remember the starting (closed) rotations
        lidClosed = lidHinge.localRotation;
        pedalUp = pedalPivot.localRotation;
    }

    public void Play()
    {
        if (running != null) StopCoroutine(running); // dumped again mid-animation: carry on from where the lid is
        running = StartCoroutine(OpenAndClose());
    }

    private IEnumerator OpenAndClose()
    {
        yield return MoveTo(1f, openTime); // pop open
        yield return new WaitForSeconds(holdTime); // stay open a moment
        yield return MoveTo(0f, closeTime); // fall shut
        running = null;
    }

    // smoothly changes openAmount from where it is now to target
    private IEnumerator MoveTo(float target, float time)
    {
        float start = openAmount;
        for (float t = 0f; t < 1f; t += Time.deltaTime / time)
        {
            SetOpen(Mathf.Lerp(start, target, t));
            yield return null; // wait one frame
        }
        SetOpen(target);
    }

    private void SetOpen(float amount)
    {
        openAmount = amount;
        lidHinge.localRotation = lidClosed * Quaternion.Euler(lidOpenAngle * amount, 0f, 0f);
        pedalPivot.localRotation = pedalUp * Quaternion.Euler(pedalPressAngle * amount, 0f, 0f);
    }
}