using UnityEngine;

public class DeathSceneSetup : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;  // unlock mouse
        Cursor.visible   = true;                 // show mouse
    }
}