using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    [Header("Switch Visuals")]
    public Image targetImage;       // The Image component to change
    public Sprite switchUpSprite;   // The "Up" position image
    public Sprite switchDownSprite; // The "Down" position image

    [Header("Settings")]
    public bool isUp = true;        // Tracks if switch is currently Up

    void Start()
    {
        // Ensure the visual matches the starting state
        UpdateVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. Flip the state
        isUp = !isUp;

        // 2. Update the image
        UpdateVisual();

        // 3. (Optional) Print to console for testing
        Debug.Log(gameObject.name + " is now " + (isUp ? "UP" : "DOWN"));
    }

    void UpdateVisual()
    {
        if (targetImage != null && switchUpSprite != null && switchDownSprite != null)
        {
            targetImage.sprite = isUp ? switchUpSprite : switchDownSprite;
        }
    }
}