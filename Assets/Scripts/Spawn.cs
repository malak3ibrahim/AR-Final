using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class Spawn : MonoBehaviour
{
    [Header("AR Placement Interactables")]
    [Tooltip("Assign the ARPlacementInteractable that spawns Car A.")]
    public ARPlacementInteractable carAPlacer;
    [Tooltip("Assign the ARPlacementInteractable that spawns Car B.")]
    public ARPlacementInteractable carBPlacer;

    [Header("UI Buttons")]
    [Tooltip("Click to select Car A for spawning.")]
    public Button carAButton;
    [Tooltip("Click to select Car B for spawning.")]
    public Button carBButton;

    private void OnEnable()
    {
        // Subscribe to placement events
        if (carAPlacer != null)
            carAPlacer.objectPlaced.AddListener(args => OnCarPlaced(args.placementObject, "Car A"));
        if (carBPlacer != null)
            carBPlacer.objectPlaced.AddListener(args => OnCarPlaced(args.placementObject, "Car B"));

        // Hook up UI buttons
        if (carAButton != null)
            carAButton.onClick.AddListener(ActivateCarA);
        if (carBButton != null)
            carBButton.onClick.AddListener(ActivateCarB);

        // Default to Car A
        ActivateCarA();
    }

    private void OnDisable()
    {
        // Unsubscribe from placement events
        if (carAPlacer != null)
            carAPlacer.objectPlaced.RemoveAllListeners();
        if (carBPlacer != null)
            carBPlacer.objectPlaced.RemoveAllListeners();

        // Remove UI listeners
        if (carAButton != null)
            carAButton.onClick.RemoveAllListeners();
        if (carBButton != null)
            carBButton.onClick.RemoveAllListeners();
    }

    private void ActivateCarA()
    {
        if (carAPlacer != null)
            carAPlacer.enabled = true;
        if (carBPlacer != null)
            carBPlacer.enabled = false;

        Debug.Log("Switched to Car A placement.");
    }

    private void ActivateCarB()
    {
        if (carAPlacer != null)
            carAPlacer.enabled = false;
        if (carBPlacer != null)
            carBPlacer.enabled = true;

        Debug.Log("Switched to Car B placement.");
    }

    private void OnCarPlaced(GameObject placedObject, string carName)
    {
        if (placedObject == null)
            return;

        Debug.Log($"Placed {carName} at position: {placedObject.transform.position}");
        // Additional logic (e.g., caching for color changes) can go here
    }
}
