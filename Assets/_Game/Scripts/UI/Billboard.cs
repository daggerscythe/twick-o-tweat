using UnityEngine;

// makes an object always face the camera (for speech bubbles, floating candy icons)

public class Billboard : MonoBehaviour
{
    private Camera cam;

    // LateUpdate is called after every update
    private void LateUpdate()
    {
        if (cam == null) cam = Camera.main; // the camera tagged "MainCamera"
        if (cam == null) return;

        // point the same way the camera points
        transform.forward = cam.transform.forward;
    }
}
