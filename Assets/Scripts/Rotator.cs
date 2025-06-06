using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class Rotator : MonoBehaviour
{
    [Header("AR Placement Interactables")]
    [Tooltip("Drag in the ARPlacementInteractable that spawns Car A.")]
    public ARPlacementInteractable carAPlacer;
    [Tooltip("Drag in the ARPlacementInteractable that spawns Car B.")]
    public ARPlacementInteractable carBPlacer;

    [Header("Spawn Selection Buttons")]
    [Tooltip("Click to enable Car A placement.")]
    public Button carAButton;
    [Tooltip("Click to enable Car B placement.")]
    public Button carBButton;

    [Header("Rotate Last Button")]
    [Tooltip("Click to toggle rotation on the last spawned car.")]
    public Button rotateButton;

    // Tracks whether Car A or Car B is active for placement
    private bool placingCarA = true;

    // Reference to the last spawned car's CarRotator component
    private Rotation rotation;

    private void OnEnable()
    {
        // Subscribe to placement events
        if (carAPlacer != null)
            carAPlacer.objectPlaced.AddListener(args => OnCarPlaced(args.placementObject));
        if (carBPlacer != null)
            carBPlacer.objectPlaced.AddListener(args => OnCarPlaced(args.placementObject));

        // Hook up spawn selection buttons
        if (carAButton != null)
            carAButton.onClick.AddListener(() => SetActivePlacer(true));
        if (carBButton != null)
            carBButton.onClick.AddListener(() => SetActivePlacer(false));

        // Hook up rotate button
        if (rotateButton != null)
            rotateButton.onClick.AddListener(ToggleLastCarRotation);

        // Default: enable Car A
        SetActivePlacer(true);
    }

    private void OnDisable()
    {
        // Unsubscribe placement events
        if (carAPlacer != null)
            carAPlacer.objectPlaced.RemoveAllListeners();
        if (carBPlacer != null)
            carBPlacer.objectPlaced.RemoveAllListeners();

        // Remove UI listeners
        if (carAButton != null) carAButton.onClick.RemoveAllListeners();
        if (carBButton != null) carBButton.onClick.RemoveAllListeners();
        if (rotateButton != null) rotateButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Enable one ARPlacementInteractable and disable the other.
    /// </summary>
    private void SetActivePlacer(bool useCarA)
    {
        placingCarA = useCarA;
        if (carAPlacer != null) carAPlacer.enabled = useCarA;
        if (carBPlacer != null) carBPlacer.enabled = !useCarA;
    }

    /// <summary>
    /// Called when either Car A or Car B is placed. Caches that instance's CarRotator.
    /// </summary>
    private void OnCarPlaced(GameObject spawnedCar)
    {
        if (spawnedCar == null)
            return;

        // Find CarRotator component on the spawned car
        rotation = spawnedCar.GetComponent<Rotation>();
        if (rotation == null)
        {
            Debug.LogWarning("Spawned car does not have a CarRotator component.");
        }
    }

    private void ToggleLastCarRotation()
    {
        if (rotation != null)
        {
            rotation.ToggleRotation();
        }
        else
        {
            Debug.LogWarning("No car has been spawned yet. Rotate button does nothing.");
        }
    }
}
