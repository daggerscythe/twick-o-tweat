using TMPro;
using UnityEngine;
using UnityEngine.UI;

// one line in the HUD effect list: [color square] Speed Boost  3.2s
// lives on EffectRow_LightBlue / _Yellow / _Red / _Green under EffectPanel

public class EffectRowUI : MonoBehaviour
{
    [SerializeField] private BagColor color;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        icon.color = GameColors.ToColor(color);
    }

    public void Refresh(ColorEffects effects)
    {
        bool on = effects.IsActive(color);
        if (gameObject.activeSelf != on) gameObject.SetActive(on);
        if (on) text.text = $"{ColorEffects.EffectName(color)}  {effects.GetTimeLeft(color):0.0}s";
    }
}