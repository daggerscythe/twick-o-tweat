using TMPro;
using UnityEngine;
using UnityEngine.UI;

// one order card on the HUD
// lives on OrderCard and OrderCard_2

public class OrderCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text kidText;
    [SerializeField] private Transform entryContainer;
    [SerializeField] private OrderEntryUI entryPrefab;
    [SerializeField] private Image timerFill; // same bar as the speech bubble (Image Type = Filled, Horizontal)

    private Kid kid;

    // kid == null hides the card
    public void Show(Kid newKid, int doorNumber)
    {
        kid = newKid;
        gameObject.SetActive(kid != null);
        if (kid == null) return;

        foreach (Transform child in entryContainer) Destroy(child.gameObject);

        kidText.text = $"Door {doorNumber}: {GameColors.ToName(kid.KidColor)} kid";
        foreach (var pair in kid.Order.Items)
        {
            OrderEntryUI entry = Instantiate(entryPrefab, entryContainer);
            entry.Set(pair.Key.icon, pair.Value);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (kid == null) return;

        // while the timer is off (tutorial step 1) TimeLeft stays full, so the bar stays full like the bubble
        float t = kid.TimeLeft / kid.TotalTime;
        timerFill.fillAmount = t;
        timerFill.color = SpeechBubble.TimerColor(t);
    }
}
