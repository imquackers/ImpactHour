using UnityEngine;
using UnityEngine.UI;

/// Scrolls a scanline overlay texture upward for a retro CRT effect on the menu.
public class MenuScanlineEffect : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 0.3f;

    private RawImage rawImage;
    private float offset;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (rawImage == null) return;

        offset += Time.deltaTime * scrollSpeed;
        rawImage.uvRect = new Rect(0f, offset, 1f, 1f);
    }
}
