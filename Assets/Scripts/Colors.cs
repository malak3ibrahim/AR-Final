using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class Colors : MonoBehaviour
{
    [Header("AR Placement Interactables (same as Spawn script)")]
    public ARPlacementInteractable carAPlacer;   // Placer that spawns Car A
    public ARPlacementInteractable carBPlacer;   // Placer that spawns Car B

    [Header("Color Buttons")]
    public Button redButton;
    public Button greenButton;
    public Button blueButton;

    // Cache the most recently placed car GameObject
    private GameObject lastPlacedCar;

    // Cache Renderers + their fresh Material instances under the last placed car
    private List<Renderer> cachedRenderers = new List<Renderer>();
    private List<Material> cachedMaterials = new List<Material>();

    private void OnEnable()
    {
        // Subscribe to both placers' objectPlaced events
        if (carAPlacer != null)
            carAPlacer.objectPlaced.AddListener(args => OnCarPlaced(args.placementObject));
        if (carBPlacer != null)
            carBPlacer.objectPlaced.AddListener(args => OnCarPlaced(args.placementObject));

        // Hook up color buttons
        if (redButton != null)
            redButton.onClick.AddListener(() => ChangeColor(Color.red));
        if (greenButton != null)
            greenButton.onClick.AddListener(() => ChangeColor(Color.green));
        if (blueButton != null)
            blueButton.onClick.AddListener(() => ChangeColor(Color.blue));
    }

    private void OnDisable()
    {
        // Unsubscribe from placement events
        if (carAPlacer != null)
            carAPlacer.objectPlaced.RemoveAllListeners();
        if (carBPlacer != null)
            carBPlacer.objectPlaced.RemoveAllListeners();

        // Remove color button listeners
        if (redButton != null)
            redButton.onClick.RemoveAllListeners();
        if (greenButton != null)
            greenButton.onClick.RemoveAllListeners();
        if (blueButton != null)
            blueButton.onClick.RemoveAllListeners();
    }

    private void OnCarPlaced(GameObject placedObject)
    {
        if (placedObject == null)
            return;

        lastPlacedCar = placedObject;

        // Clear any previous caches
        cachedRenderers.Clear();
        cachedMaterials.Clear();

        // Find every Renderer under placedObject
        Renderer[] allRenderers = placedObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in allRenderers)
        {
            // Create a new Material instance from the renderer's current material
            // so that tinting does not affect the original asset or other instances
            Material newMat = new Material(rend.material);
            rend.material = newMat;

            // Cache for later color changes
            cachedRenderers.Add(rend);
            cachedMaterials.Add(newMat);
        }
    }

    private void ChangeColor(Color tint)
    {
        if (cachedMaterials.Count == 0)
            return; // Nothing to tint

        foreach (Material mat in cachedMaterials)
            mat.color = tint;
    }
}
