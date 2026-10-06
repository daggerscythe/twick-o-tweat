using UnityEngine;
using UnityEngine.UI;

// white bubble above a kid's head: one OrderEntryUI per candy type, plus a timer bar.

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private Transform entryContainer; // has a Horizontal Layout Group
    [SerializeField] private OrderEntryUI entryPrefab;
    [SerializeField] private Image timerFill; // Image Type = Filled

    public void Show(Order order)
    {
        // remove old entries
        foreach (Transform child in entryContainer) Destroy(child.gameObject);

        foreach (var pair in order.Items)
        {
            // creates it as a child of the container
            OrderEntryUI entry = Instantiate(entryPrefab, entryContainer);
            entry.Set(pair.Key.icon, pair.Value);
        }
    }

    // t01 = fraction of time left, from 1 (full) to 0 (out of time).
    public void SetTimer(float t01)
    {
        if (timerFill == null) return;
        timerFill.fillAmount = t01;
        timerFill.color = Color.Lerp(Color.red, Color.green, t01); // green -> red as time runs out
    }
}
