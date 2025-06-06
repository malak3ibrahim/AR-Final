using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MarkerRotator : MonoBehaviour
{
    [Header("AR Tracked Image Manager")]
    [Tooltip("Assign the ARTrackedImageManager that uses your Reference Image Library.")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Car Prefabs (spawn when markers are detected)")]
    [Tooltip("Prefab to spawn when marker named \"CarAImage\" is detected.")]
    public GameObject carAPrefab;
    [Tooltip("Prefab to spawn when marker named \"CarBImage\" is detected.")]
    public GameObject carBPrefab;

    [Header("Rotate Last Button")]
    [Tooltip("Click to toggle rotation on the last spawned car.")]
    public Button rotateButton;

    // Cache the spawned GameObject for each reference image name
    private Dictionary<string, GameObject> spawnedCars = new Dictionary<string, GameObject>();

    // Reference to the Rotation component on the most recently spawned car
    private Rotation lastRotationComponent;

    private void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

        if (rotateButton != null)
            rotateButton.onClick.AddListener(ToggleLastCarRotation);
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

        if (rotateButton != null)
            rotateButton.onClick.RemoveAllListeners();
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // Handle newly detected images
        foreach (var trackedImage in args.added)
            SpawnOrEnableCar(trackedImage);

        // Handle updated images (position changes or tracking state)
        foreach (var trackedImage in args.updated)
            UpdateCarForMarker(trackedImage);

        // Handle removed images (destroy their car instances)
        foreach (var trackedImage in args.removed)
            RemoveCarForMarker(trackedImage);
    }

    private void SpawnOrEnableCar(ARTrackedImage trackedImage)
    {
        string markerName = trackedImage.referenceImage.name;
        GameObject prefabToSpawn = null;

        if (markerName == "CarAImage")
            prefabToSpawn = carAPrefab;
        else if (markerName == "CarBImage")
            prefabToSpawn = carBPrefab;

        if (prefabToSpawn == null)
            return;

        // If not already spawned, instantiate and parent to the tracked image
        if (!spawnedCars.ContainsKey(markerName))
        {
            GameObject spawned = Instantiate(
                prefabToSpawn,
                trackedImage.transform.position,
                trackedImage.transform.rotation
            );
            spawned.transform.parent = trackedImage.transform;
            spawnedCars[markerName] = spawned;

            CacheRotationComponent(spawned);
        }
        else
        {
            // Already spawned—just re-enable and reposition
            GameObject spawned = spawnedCars[markerName];
            spawned.SetActive(true);
            spawned.transform.position = trackedImage.transform.position;
            spawned.transform.rotation = trackedImage.transform.rotation;
            CacheRotationComponent(spawned);
        }
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
            CacheRotationComponent(spawned);
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

        // If the removed car was the last one, clear the cached rotation
        if (lastRotationComponent != null && lastRotationComponent.gameObject == spawned)
            lastRotationComponent = null;
    }

    private void CacheRotationComponent(GameObject car)
    {
        // Cache the Rotation component for the most recently spawned/enabled car
        lastRotationComponent = car.GetComponent<Rotation>();
        if (lastRotationComponent == null)
            Debug.LogWarning("ImageTrackingRotator: spawned car lacks a Rotation component.");
    }

    private void ToggleLastCarRotation()
    {
        if (lastRotationComponent != null)
            lastRotationComponent.ToggleRotation();
        else
            Debug.LogWarning("ImageTrackingRotator: no car available to rotate.");
    }
}
