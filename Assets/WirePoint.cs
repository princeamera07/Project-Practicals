using UnityEngine;
using UnityEngine.EventSystems;

public class WirePoint : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector] public RectTransform rectTransform;

    [Header("Connection Set 1")]
    public string pointID_1;   
    public string targetID_1; 

    [Header("Connection Set 2 (Optional)")]
    public string pointID_2;   
    public string targetID_2; 

    void Awake() => rectTransform = GetComponent<RectTransform>();

    public void OnPointerDown(PointerEventData eventData)
    {
        WireManager_UI.Instance.StartWire(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameObject hovered = eventData.pointerCurrentRaycast.gameObject;
        if (hovered != null && hovered.TryGetComponent<WirePoint>(out WirePoint endHole))
        {
            WireManager_UI.Instance.EndWire(endHole);
        }
    }
}