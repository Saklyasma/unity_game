using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class HoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [TextArea]
    [SerializeField] private string message = "Listen to the question in voice";

    private void Awake()
    {
        HideTooltip();
        RefreshText();
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        RefreshText();
        if (tooltipRoot != null)
            tooltipRoot.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    public void SetMessage(string value)
    {
        message = value;
        RefreshText();
    }

    private void RefreshText()
    {
        if (tooltipText != null)
            tooltipText.text = message;
    }

    private void HideTooltip()
    {
        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);
    }
}
