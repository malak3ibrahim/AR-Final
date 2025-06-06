using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MarkerEngine : MonoBehaviour
{
    [Header("AR Tracked Image Manager")]
    [Tooltip("Assign the ARTrackedImageManager that uses your Reference Image Library.")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Car Prefabs (spawn when markers are detected)")]
    [Tooltip("Prefab to spawn when marker named \"CarAImage\" is detected.")]
    public GameObject carAPrefab;
    [Tooltip("Prefab to spawn when marker named \"CarBImage\" is detected.")]
    public GameObject carBPrefab;

    [Header("External Audio Sources")]
    [Tooltip("AudioSource on an empty GameObject for Car A's sound.")]
    public AudioSource carAAudioSource;
    [Tooltip("AudioSource on an empty GameObject for Car B's sound.")]
    public AudioSource carBAudioSource;

    [Header("Play/Stop Button")]
    [Tooltip("UI Button that toggles the sound for the last spawned car.")]
    public Button playStopButton;

    // Track spawned GameObjects by marker name
    private Dictionary<string, GameObject> spawnedCars = new Dictionary<string, GameObject>();

    // Track which car was last spawned (true = Car A, false = Car B)
    private bool lastWasCarA = false;

    private void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

        if (playStopButton != null)
            playStopButton.onClick.AddListener(ToggleLastCarSound);

        // Ensure both audio sources are stopped initially
        if (carAAudioSource != null) carAAudioSource.Stop();
        if (carBAudioSource != null) carBAudioSource.Stop();
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

        if (playStopButton != null)
            playStopButton.onClick.RemoveAllListeners();
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // Spawn prefabs for newly detected markers
        foreach (var added in args.added)
            SpawnCarForMarker(added);

        // Update position or hide when marker updates
        foreach (var updated in args.updated)
            UpdateCarForMarker(updated);

        // Remove and destroy when marker is removed
        foreach (var removed in args.removed)
            RemoveCarForMarker(removed);
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

        // Instantiate and parent to the tracked image
        GameObject spawned = Instantiate(
            prefabToSpawn,
            trackedImage.transform.position,
            trackedImage.transform.rotation
        );
        spawned.transform.parent = trackedImage.transform;
        spawnedCars[markerName] = spawned;

        // Record which car was spawned
        lastWasCarA = (markerName == "CarAImage");

        // Stop both audio sources so user can toggle fresh
        if (carAAudioSource != null) carAAudioSource.Stop();
        if (carBAudioSource != null) carBAudioSource.Stop();
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

            // Update lastWasCarA if this marker is now active
            lastWasCarA = (markerName == "CarAImage");

            // Stop both audio sources to reset
            if (carAAudioSource != null) carAAudioSource.Stop();
            if (carBAudioSource != null) carBAudioSource.Stop();
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

        // If this removed car was last, clear lastWasCarA
        if ((lastWasCarA && markerName == "CarAImage") ||
            (!lastWasCarA && markerName == "CarBImage"))
        {
            lastWasCarA = false;
            if (carAAudioSource != null) carAAudioSource.Stop();
            if (carBAudioSource != null) carBAudioSource.Stop();
        }
    }

    private void ToggleLastCarSound()
    {
        if (lastWasCarA)
        {
            if (carAAudioSource == null) return;
            if (carAAudioSource.isPlaying) carAAudioSource.Stop();
            else carAAudioSource.Play();
        }
        else
        {
            if (carBAudioSource == null) return;
            if (carBAudioSource.isPlaying) carBAudioSource.Stop();
            else carBAudioSource.Play();
        }
    }
}
