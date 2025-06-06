using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class Engine : MonoBehaviour
{
    [Header("AR Placement Interactables")]
    public ARPlacementInteractable carAPlacer;   // Spawns Car A
    public ARPlacementInteractable carBPlacer;   // Spawns Car B

    [Header("External Audio Sources")]
    [Tooltip("An empty GameObject in the scene that has Car A’s AudioSource.")]
    public AudioSource carAAudioSource;
    [Tooltip("An empty GameObject in the scene that has Car B’s AudioSource.")]
    public AudioSource carBAudioSource;

    [Header("Control Buttons")]
    public Button playStopButton;    // Calls ToggleSound() on whichever car is last spawned

    // Tracks which car was placed last (true = Car A, false = Car B)
    private bool lastPlacedWasCarA = false;

    private void OnEnable()
    {
        if (carAPlacer != null)
            carAPlacer.objectPlaced.AddListener(args => OnCarPlaced(true));
        if (carBPlacer != null)
            carBPlacer.objectPlaced.AddListener(args => OnCarPlaced(false));

        if (playStopButton != null)
            playStopButton.onClick.AddListener(ToggleLastCarSound);

        // Ensure both AudioSources start stopped
        if (carAAudioSource != null) carAAudioSource.Stop();
        if (carBAudioSource != null) carBAudioSource.Stop();
    }

    private void OnDisable()
    {
        if (carAPlacer != null)
            carAPlacer.objectPlaced.RemoveAllListeners();
        if (carBPlacer != null)
            carBPlacer.objectPlaced.RemoveAllListeners();

        if (playStopButton != null)
            playStopButton.onClick.RemoveAllListeners();
    }

    // Called when either placer finishes spawning a car
    private void OnCarPlaced(bool isCarA)
    {
        lastPlacedWasCarA = isCarA;
        // Whenever a new car is placed, stop both audio sources—
        // you only want the user to toggle the audio for the latest car.
        if (carAAudioSource != null) carAAudioSource.Stop();
        if (carBAudioSource != null) carBAudioSource.Stop();
    }

    // Called by the UI button to play/stop the correct AudioSource
    private void ToggleLastCarSound()
    {
        AudioSource chosen = lastPlacedWasCarA ? carAAudioSource : carBAudioSource;
        if (chosen == null)
            return;

        if (chosen.isPlaying)
            chosen.Stop();
        else
            chosen.Play();
    }
}
