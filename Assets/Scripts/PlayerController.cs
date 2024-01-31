using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Tower selectedTower; // Reference to the currently selected tower
    [SerializeField] private PathController pathController; // Reference to the PathController component
    private Transform temporaryTarget; // Temporary transform to represent the target position of the path
    public float groundHeight = 0.1f; // Height at which the line will be drawn on the ground

    void Start()
    {
        // Initialize a temporary GameObject to represent the target position for the line
        temporaryTarget = new GameObject("TemporaryTarget").transform;
    }

    void Update()
    {
        // Handle input when the left mouse button is pressed down
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the ray hits an object tagged as "Player"
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    // Get the Tower component from the hit object
                    selectedTower = hit.collider.gameObject.GetComponent<Tower>();
                    if (selectedTower != null)
                    {
                        // Get the PathController component from the selected tower
                        pathController = selectedTower.gameObject.GetComponent<PathController>();
                    }
                }
            }
        }

        // Handle input while the left mouse button is held down
        if (selectedTower != null && Input.GetMouseButton(0))
        {
            // Get the world position of the mouse cursor on the ground
            Vector3 mouseWorldPoint = GetGroundPosition(Input.mousePosition);
            // Set the position of the temporary target to the mouse position
            temporaryTarget.position = mouseWorldPoint;
            if (pathController != null)
            {
                // Update the path to stretch towards the temporary target
                pathController.StretchPathToTarget(temporaryTarget);
            }
        }

        // Handle input when the left mouse button is released
        if (selectedTower != null && Input.GetMouseButtonUp(0))
        {
            // Cast a ray from the camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the ray hits a Tower component
                if (hit.collider.gameObject.GetComponent<Tower>())
                {
                    // Set the target for the selected tower
                    selectedTower.SetTarget(hit.collider.gameObject.transform);
                }
                else
                {
                    // If the ray doesn't hit a Tower, clear the path line
                    if (pathController != null)
                    {
                        pathController.ClearLine();
                    }
                }
            }
            else
            {
                // If the raycast fails, clear the path line
                if (pathController != null)
                {
                    pathController.ClearLine();
                }
            }

            // Reset the selected tower reference
            selectedTower = null;
        }
    }

    // Method to calculate the ground position based on the mouse position
    Vector3 GetGroundPosition(Vector2 mousePosition)
    {
        // Cast a ray from the camera through the mouse position
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        // Create a plane representing the ground at the specified height
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, groundHeight, 0));
        float distance;

        // Check if the ray intersects with the ground plane
        if (groundPlane.Raycast(ray, out distance))
        {
            // Return the point of intersection
            return ray.GetPoint(distance);
        }

        // Default return if no intersection is found
        return new Vector3(mousePosition.x, groundHeight, mousePosition.y);
    }
}
