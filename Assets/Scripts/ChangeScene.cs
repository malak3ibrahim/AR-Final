using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void LoadPlaneDetection()
    {
        SceneManager.LoadScene("Plane Detection");
    }

    public void LoadImageTracking()
    {
        SceneManager.LoadScene("Image Tracking");
    }
}
