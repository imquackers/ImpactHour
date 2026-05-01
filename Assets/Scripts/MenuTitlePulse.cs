using UnityEngine;
using UnityEngine.UI;

// Applies a slow pulse glow effect to the title text.
public class MenuTitlePulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseSpeed = 1.2f;
    public Color colorA = new Color(1f, 0.15f, 0.15f, 1f);
    public Color colorB = new Color(1f, 0.6f, 0.6f, 1f);

    [Header("Scale Pulse")]
    public float minScale = 0.98f;
    public float maxScale = 1.02f;

    private Text titleText;
    private RectTransform rectTransform;

    private void Awake()
    {
        titleText = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        if (titleText != null)
            titleText.color = Color.Lerp(colorA, colorB, t);

        if (rectTransform != null)
        {
            float scale = Mathf.Lerp(minScale, maxScale, t);
            rectTransform.localScale = Vector3.one * scale;
        }
    }
}
