using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

// Adds hover scale, color glow, and shimmer effect to buttons
// IMPORTANT: Does NOT handle clicks 
[RequireComponent(typeof(Image))]
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // scale settings
    [Header("Scale")]
    public float hoverScale = 1.08f;     // How big the button gets on hover
    public float animationSpeed = 10f;   // How fast the animation is

    // colour settings
    [Header("Colours")]
    public Color normalColor = new Color(0.85f, 0.1f, 0.1f, 1f); // Default color
    public Color hoverColor = new Color(1f, 0.35f, 0.35f, 1f);   // Hover color

    // shimmer effect
    [Header("Shimmer")]
    public Color shimmerColor = new Color(1f, 1f, 1f, 0.25f); // White glow
    public float shimmerDuration = 0.4f; // How long shimmer lasts

    // internal variuables
    private Vector3 targetScale;  // What scale we're moving toward
    private Color targetColor;    // What color we're moving toward
    private Image buttonImage;    // Button image component
    private RectTransform rectTransform; // For scaling
    private Image shimmerImage;   // Overlay used for shimmer

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();

        // Set starting values
        targetScale = Vector3.one;
        targetColor = normalColor;
        buttonImage.color = normalColor;

        CreateShimmerOverlay(); // Create shimmer effect object
    }

    // Creates a child object that acts as the shimmer overlay
    private void CreateShimmerOverlay()
    {
        GameObject shimmerObj = new GameObject("Shimmer");
        shimmerObj.transform.SetParent(transform, false);

        // Stretch to match button size
        RectTransform rt = shimmerObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        // Add image for shimmer
        shimmerImage = shimmerObj.AddComponent<Image>();
        shimmerImage.color = new Color(shimmerColor.r, shimmerColor.g, shimmerColor.b, 0f); // Start invisible
        shimmerImage.raycastTarget = false; // Doesn't block clicks
    }

    private void Update()
    {
        // Smoothly scale button
        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            Time.deltaTime * animationSpeed
        );

        // Smoothly change color
        buttonImage.color = Color.Lerp(
            buttonImage.color,
            targetColor,
            Time.deltaTime * animationSpeed
        );
    }

    // Called when mouse enters button
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale; // Grow button
        targetColor = hoverColor;               // Change color

        StartCoroutine(ShimmerSweep());         // Play shimmer effect
    }

    // Called when mouse leaves button
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one;   // Return to normal size
        targetColor = normalColor;   // Return to normal color
    }

    // Creates a quick shimmer (fade in/out)
    private IEnumerator ShimmerSweep()
    {
        if (shimmerImage == null) yield break;

        float elapsed = 0f;

        while (elapsed < shimmerDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / shimmerDuration;

            // Creates a smooth fade in and out (like a pulse)
            float alpha = shimmerColor.a * Mathf.Sin(t * Mathf.PI);

            shimmerImage.color = new Color(
                shimmerColor.r,
                shimmerColor.g,
                shimmerColor.b,
                alpha
            );

            yield return null;
        }

        // Ensure its fully invisible at the end
        shimmerImage.color = new Color(shimmerColor.r, shimmerColor.g, shimmerColor.b, 0f);
    }
}