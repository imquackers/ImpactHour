using UnityEngine;
using UnityEngine.UI;

// displays a gif by using images and playing them in a sequence.
// Import the gif frames as individual PNG sprites and assign them to the frames array.
// If only a single texture is assigned it shows as a static background.
[RequireComponent(typeof(RawImage))]
public class GifBackground : MonoBehaviour
{
    [Header("Gif Frames")]
    [Tooltip("Assign the gif's individual frame textures here (exported from the gif).")]
    public Texture2D[] frames;

    [Tooltip("Frames per second for playback.")]
    public float fps = 15f;

    private RawImage rawImage;
    private int currentFrame;
    private float timer;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (frames == null || frames.Length <= 1) return;

        timer += Time.deltaTime;

        if (timer >= 1f / fps)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % frames.Length;
            rawImage.texture = frames[currentFrame];
        }
    }
}
