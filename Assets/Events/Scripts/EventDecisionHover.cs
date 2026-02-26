using UnityEngine;
using UnityEngine.EventSystems;

// Small helper that forwards pointer enter/exit to EventCanvas to show tooltip
public class EventDecisionHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public EventCanvas parentCanvas;
    [HideInInspector] public EventDataReceiver.OptionData option;
    private bool isHovering = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (parentCanvas == null || option == null) return;
        isHovering = true;
        string text = BuildTooltipText(option);
        parentCanvas.ShowTooltip(text, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (parentCanvas == null) return;
        isHovering = false;
        parentCanvas.HideTooltip();
    }

    void Update()
    {
        if (!isHovering) return;
        if (parentCanvas == null || option == null) return;
        parentCanvas.ShowTooltip(BuildTooltipText(option), Input.mousePosition);
    }

    private string BuildTooltipText(EventDataReceiver.OptionData opt)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (!string.IsNullOrEmpty(opt.OptionEffect))
        {
            sb.AppendLine(opt.OptionEffect);
        }

        // Helper to add age-group stat lines
        void AddStatLine(string label, EventDataReceiver.StatData sd)
        {
            if (sd == null) return;
            sb.Append(label);
            sb.Append(": ");
            sb.Append($"Virality {FormatSigned(sd.Virality)}, ");
            sb.Append($"Impact {FormatSigned(sd.Impact)}, ");
            sb.Append($"Visibility {FormatSigned(sd.Visibility)}");
            sb.AppendLine();
        }

        AddStatLine("Young", opt.OptionEffectYoung);
        AddStatLine("Adult", opt.OptionEffectAdult);
        AddStatLine("Senior", opt.OptionEffectSenior);

        if (opt.OptionSpecialEffect != null)
        {
            var se = opt.OptionSpecialEffect;
            sb.Append("Special: Type=").Append(se.EffectsType);
            if (se.EffectAmmount.HasValue)
                sb.Append($" amt={se.EffectAmmount.Value}");
            sb.AppendLine();
        }

        string outText = sb.ToString().TrimEnd();
        if (string.IsNullOrEmpty(outText)) outText = "No effect.";
        return outText;
    }

    private string FormatSigned(int v)
    {
        return v >= 0 ? $"+{v}" : v.ToString();
    }
}
