using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageSpawn : MonoBehaviour
{
    [Header("AR Tracked Image Manager")]
    [Tooltip("Drag in the ARTrackedImageManager component here.")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Image Prefabs")]
    [Tooltip("Assign a prefab for the first reference image (as named in the Reference Image Library).")]
    public GameObject prefabForImageA;
    [Tooltip("Assign a prefab for the second reference image (as named in the Reference Image Library).")]
    public GameObject prefabForImageB;

    // Holds all spawned instances, keyed by the tracked image name
    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();

    private void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Handle newly detected images
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            SpawnForReferenceImage(trackedImage);
        }

        // Handle updates (for tracking state or position changes)
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateSpawnedPrefab(trackedImage);
        }

        // Handle removed images (destroy any associated prefab)
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            if (spawnedPrefabs.TryGetValue(trackedImage.referenceImage.name, out GameObject existing))
            {
                Destroy(existing);
                spawnedPrefabs.Remove(trackedImage.referenceImage.name);
            }
        }
    }

    // Instantiate or enable the correct prefab when an image is detected for the first time
    private void SpawnForReferenceImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        // Decide which prefab to spawn based on the reference image’s name
        GameObject prefabToSpawn = null;
        if (imageName == "ImageA") // Replace with exact name from your Reference Image Library
            prefabToSpawn = prefabForImageA;
        else if (imageName == "ImageB") // Replace with exact name from your Reference Image Library
            prefabToSpawn = prefabForImageB;

        if (prefabToSpawn == null)
            return;

        // Instantiate the prefab at the tracked image’s position and rotation
        GameObject spawned = Instantiate(prefabToSpawn, trackedImage.transform.position, trackedImage.transform.rotation);
        spawned.transform.parent = trackedImage.transform; // Parent it so it moves with the tracked image

        spawnedPrefabs[imageName] = spawned;
    }

    // When a tracked image is updated (e.g. its position changes, or it loses tracking), update or hide the spawned prefab
    private void UpdateSpawnedPrefab(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (!spawnedPrefabs.TryGetValue(imageName, out GameObject spawned))
            return;

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            // Ensure the spawned object is active and follows the image’s pose
            spawned.SetActive(true);
            spawned.transform.position = trackedImage.transform.position;
            spawned.transform.rotation = trackedImage.transform.rotation;
        }
        else
        {
            // If the image is no longer tracked, hide the spawned object
            spawned.SetActive(false);
        }
    }
}
