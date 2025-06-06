using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ColorMarker : MonoBehaviour
{
    [Header("AR Tracked Image Manager")]
    [Tooltip("Assign the ARTrackedImageManager that is using your Reference Image Library.")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Car Prefabs (to spawn when markers are detected)")]
    [Tooltip("Prefab to spawn when marker named \"CarAImage\" is detected.")]
    public GameObject carAPrefab;
    [Tooltip("Prefab to spawn when marker named \"CarBImage\" is detected.")]
    public GameObject carBPrefab;

    [Header("Color Buttons")]
    public Button redButton;
    public Button greenButton;
    public Button blueButton;

    // Keep track of the spawned car GameObject for each reference image
    private Dictionary<string, GameObject> spawnedCars = new Dictionary<string, GameObject>();

    // Cache the most recently “placed” car’s renderers/materials for coloring
    private List<Renderer> cachedRenderers = new List<Renderer>();
    private List<Material> cachedMaterials = new List<Material>();

    private void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

        if (redButton != null)
            redButton.onClick.AddListener(() => ChangeColor(Color.red));
        if (greenButton != null)
            greenButton.onClick.AddListener(() => ChangeColor(Color.green));
        if (blueButton != null)
            blueButton.onClick.AddListener(() => ChangeColor(Color.blue));
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

        if (redButton != null) redButton.onClick.RemoveAllListeners();
        if (greenButton != null) greenButton.onClick.RemoveAllListeners();
        if (blueButton != null) blueButton.onClick.RemoveAllListeners();
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // For each newly detected marker, spawn the corresponding car prefab
        foreach (var tracked in args.added)
            SpawnCarForMarker(tracked);

        // For each updated marker, update position/visibility
        foreach (var tracked in args.updated)
            UpdateCarForMarker(tracked);

        // For each removed marker, destroy the car and clear caches
        foreach (var tracked in args.removed)
            RemoveCarForMarker(tracked);
    }

    private void SpawnCarForMarker(ARTrackedImage trackedImage)
    {
        string markerName = trackedImage.referenceImage.name;
        GameObject prefabToSpawn = null;

        if (markerName == "CarAImage")
            prefabToSpawn = carAPrefab;
        else if (markerName == "CarBImage")
            prefabToSpawn = carBPrefab;

        if (prefabToSpawn == null)
            return;

        // Instantiate the car at the marker’s pose and parent it to follow the marker
        GameObject spawned = Instantiate(prefabToSpawn,
                                         trackedImage.transform.position,
                                         trackedImage.transform.rotation);
        spawned.transform.parent = trackedImage.transform;
        spawnedCars[markerName] = spawned;

        CacheRenderers(spawned);
    }

    private void UpdateCarForMarker(ARTrackedImage trackedImage)
    {
        string markerName = trackedImage.referenceImage.name;
        if (!spawnedCars.TryGetValue(markerName, out GameObject spawned))
            return;

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            spawned.SetActive(true);
            spawned.transform.position = trackedImage.transform.position;
            spawned.transform.rotation = trackedImage.transform.rotation;
            CacheRenderers(spawned);
        }
        else
        {
            spawned.SetActive(false);
        }
    }

    private void RemoveCarForMarker(ARTrackedImage trackedImage)
    {
        string markerName = trackedImage.referenceImage.name;
        if (spawnedCars.TryGetValue(markerName, out GameObject spawned))
        {
            Destroy(spawned);
            spawnedCars.Remove(markerName);
        }

        cachedRenderers.Clear();
        cachedMaterials.Clear();
    }

    private void CacheRenderers(GameObject car)
    {
        cachedRenderers.Clear();
        cachedMaterials.Clear();

        // Find all Renderers under the spawned car and create fresh material instances
        foreach (var rend in car.GetComponentsInChildren<Renderer>())
        {
            Material newMat = new Material(rend.material);
            rend.material = newMat;
            cachedRenderers.Add(rend);
            cachedMaterials.Add(newMat);
        }
    }

    private void ChangeColor(Color tint)
    {
        if (cachedMaterials.Count == 0)
            return;

        foreach (var mat in cachedMaterials)
            mat.color = tint;
    }
}
