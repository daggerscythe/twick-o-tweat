using TMPro;
using UnityEngine;

// one order card on the HUD
// lives on OrderCard and OrderCard_2

public class OrderCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text kidText;
    [SerializeField] private Transform entryContainer;
    [SerializeField] private OrderEntryUI entryPrefab;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Color timerNormalColor = new Color(0.13f, 0.13f, 0.13f); // dark
    [SerializeField] private Color timerUrgentColor = Color.red; // last 5 seconds

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

        if (!kid.TimerRunning)
        {
            timerText.text = "No rush";
            timerText.color = timerNormalColor;
            return;
        }

        int seconds = Mathf.CeilToInt(kid.TimeLeft);
        timerText.text = $"Time left: {seconds}s";
        timerText.color = seconds <= 5 ? timerUrgentColor : timerNormalColor;
    }
}
