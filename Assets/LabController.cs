using UnityEngine;
using TMPro;                // Needed for TextMeshPro
using UnityEngine.EventSystems; // Needed for dragging the knob

public class LabController : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform knobTransform;  // Drag your Knob Image here
    public TextMeshProUGUI voltmeterText; // Drag Voltmeter Text here
    public TextMeshProUGUI ammeterText;   // Drag Ammeter Text here

    [Header("Circuit Physics")]
    public float maxVoltage = 10.0f;     // Maximum voltage (e.g. 10V)
    public float seriesResistance = 100f;// Resistor in the circuit (Ohms)
    public float diodeCutIn = 0.7f;      // Silicon Diode turns on at 0.7V

    private float currentVoltage = 0f;
    private bool isDraggingKnob = false;

    void Update()
    {
        // Check if we are clicking/dragging the knob
        HandleKnobInput();
    }

    void HandleKnobInput()
    {
        // Simple logic: If mouse is near knob and clicked, rotate it
        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(knobTransform, Input.mousePosition))
            {
                isDraggingKnob = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDraggingKnob = false;
        }

        // If dragging, calculate angle
        if (isDraggingKnob)
        {
            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(knobTransform.parent as RectTransform, Input.mousePosition, null, out localMousePos);
            
            Vector2 knobPos = knobTransform.anchoredPosition;
            Vector2 direction = localMousePos - knobPos;
            
            // Calculate angle (Atan2 returns radians, convert to degrees)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Offset logic to make the knob feel right (depends on your image rotation)
            // Let's constrain it between -45 (Min) and 225 (Max) degrees approx
            knobTransform.rotation = Quaternion.Euler(0, 0, angle - 90);

            // Convert Angle to Voltage (0 to 1)
            // This is a simple mapper. Adjust 'angle' math based on your specific image start point.
            // For now, let's just use mouse X movement for simpler testing if rotation is tricky:
            
            // ALTERNATIVE SIMPLER INPUT: Drag Right to increase, Left to decrease
            float rotationSpeed = -Input.GetAxis("Mouse X") * 5f;
            knobTransform.Rotate(0, 0, rotationSpeed);
        }

        // 1. GET VALUE: Normalize knob rotation to a 0-1 value
        // We assume the knob rotates from 0 to -180 degrees for full power
        float zRotation = knobTransform.localEulerAngles.z;
        if (zRotation > 180) zRotation -= 360; // Convert to -180 to 180 range
        
        // Clamp rotation to useful range (e.g., 0 is OFF, -180 is MAX)
        float normalizedVal = Mathf.InverseLerp(0, -180, zRotation);
        
        // 2. CALCULATE PHYSICS
        UpdateCircuit(normalizedVal);
    }

    void UpdateCircuit(float inputPercent)
    {
        // A. Calculate Voltage (Knob % * Max Voltage)
        currentVoltage = inputPercent * maxVoltage;
        if (currentVoltage < 0) currentVoltage = 0; // Safety

        // B. Calculate Current (Ohm's Law for Diode)
        // Logic: No current flows until Voltage > 0.7 (Cut-in voltage)
        float currentAmps = 0f;

        if (currentVoltage > diodeCutIn)
        {
            // I = (V - 0.7) / R
            currentAmps = (currentVoltage - diodeCutIn) / seriesResistance;
        }
        
        // Convert Amps to MilliAmps for display
        float displayMA = currentAmps * 1000f;

        // C. Update Screens
        voltmeterText.text = currentVoltage.ToString("F2") + " V";
        ammeterText.text = displayMA.ToString("F1") + " mA";
    }
}