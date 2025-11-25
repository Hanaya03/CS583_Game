using UnityEngine;

public class MenuCameraRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f; // degrees per second
    [SerializeField] private Vector3 rotationCenter = Vector3.zero; // center point to rotate around
    [SerializeField] private float rotationRadius = 5f; // distance from center
    [SerializeField] private float cameraHeight = 1.5f; // height of camera
    
    private float currentAngle = 0f;
    private Camera cam;
    
    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        // Calculate initial position based on angle
        UpdateCameraPosition();
    }
    
    private void Update()
    {
        // Increment angle based on rotation speed
        currentAngle += rotationSpeed * Time.deltaTime;
        
        // Keep angle between 0 and 360
        if (currentAngle >= 360f)
        {
            currentAngle -= 360f;
        }
        
        // Update camera position and rotation
        UpdateCameraPosition();
    }
    
    private void UpdateCameraPosition()
    {
        // Calculate position in a circle around the center
        float radians = currentAngle * Mathf.Deg2Rad;
        float x = rotationCenter.x + rotationRadius * Mathf.Sin(radians);
        float z = rotationCenter.z + rotationRadius * Mathf.Cos(radians);
        
        // Set camera position
        transform.position = new Vector3(x, rotationCenter.y + cameraHeight, z);
        
        // Make camera look at the center at eye level (same height as camera)
        // This makes it look horizontally at the walls, not down at the floor
        Vector3 lookAtPoint = new Vector3(rotationCenter.x, rotationCenter.y + cameraHeight, rotationCenter.z);
        transform.LookAt(lookAtPoint);
    }
}

