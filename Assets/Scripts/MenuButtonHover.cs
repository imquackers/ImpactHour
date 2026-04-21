using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// Adds hover scale, color glow, and a shimmer sweep effect to menu buttons.
/// Does NOT implement IPointerClickHandler so Button.onClick fires normally.
[RequireComponent(typeof(Image))]
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale")]
    public float hoverScale = 1.08f;
    public float animationSpeed = 10f;

    [Header("Colors")]
    public Color normalColor = new Color(0.85f, 0.1f, 0.1f, 1f);
    public Color hoverColor = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("Shimmer")]
    public Color shimmerColor = new Color(1f, 1f, 1f, 0.25f);
    public float shimmerDuration = 0.4f;

    private Vector3 targetScale;
    private Color targetColor;
    private Image buttonImage;
    private RectTransform rectTransform;
    private Image shimmerImage;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
        targetScale = Vector3.one;
        targetColor = normalColor;
        buttonImage.color = normalColor;

        CreateShimmerOverlay();
    }

    private void CreateShimmerOverlay()
    {
        GameObject shimmerObj = new GameObject("Shimmer");
        shimmerObj.transform.SetParent(transform, false);

        RectTransform rt = shimmerObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        shimmerImage = shimmerObj.AddComponent<Image>();
        shimmerImage.color = new Color(shimmerColor.r, shimmerColor.g, shimmerColor.b, 0f);
        shimmerImage.raycastTarget = false;
    }

    private void Update()
    {
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * animationSpeed);
        buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
        targetColor = hoverColor;
        StartCoroutine(ShimmerSweep());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one;
        targetColor = normalColor;
    }

    private IEnumerator ShimmerSweep()
    {
        if (shimmerImage == null) yield break;

        float elapsed = 0f;
        while (elapsed < shimmerDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shimmerDuration;

            // Arc: fade in then out across the hover duration
            float alpha = shimmerColor.a * Mathf.Sin(t * Mathf.PI);
            shimmerImage.color = new Color(shimmerColor.r, shimmerColor.g, shimmerColor.b, alpha);

            yield return null;
        }

        shimmerImage.color = new Color(shimmerColor.r, shimmerColor.g, shimmerColor.b, 0f);
    }
}
