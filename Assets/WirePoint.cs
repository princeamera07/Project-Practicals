using UnityEngine;
using UnityEngine.EventSystems;

public class WirePoint : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector] public RectTransform rectTransform;

    void Awake() => rectTransform = GetComponent<RectTransform>();

    public void OnPointerDown(PointerEventData eventData)
    {
        // RULE 2: Disable drawing if user clicks empty space
        // Since StartWire is ONLY called here, clicking empty space does nothing automatically!
        WireManager_UI.Instance.StartWire(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Check if we landed on a VALID hole
        GameObject hovered = eventData.pointerCurrentRaycast.gameObject;
        
        if (hovered != null && hovered.TryGetComponent<WirePoint>(out WirePoint endHole))
        {
            WireManager_UI.Instance.EndWire(endHole);
        }
        // If hovered is null (empty space), WireManager.Update() handles the cleanup automatically
    }
}