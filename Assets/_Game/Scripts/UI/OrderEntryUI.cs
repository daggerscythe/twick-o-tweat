using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One "[icon] x2" chip
// Used in the speech bubble & in HUD order panel

public class OrderEntryUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text countText;

    public void Set(Sprite sprite, int count)
    {
        icon.sprite = sprite;
        countText.text = "x" + count;
    }
}
