using UnityEngine;

public class PathController : MonoBehaviour
{
    public LineRenderer pathLineRenderer; // The LineRenderer component assigned in the Unity Inspector
    public Material lineMaterial; // Public Material variable to assign in the Inspector
    public float yOffset = -0.9f; // Vertical offset for the starting position of the line

    public float textureScale = 1.0f; // Scale of the texture tiling
    public float scrollSpeed = -1.0f; // Speed at which the texture scrolls


    void Start()
    {
        if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = 2;
            pathLineRenderer.SetPosition(0, transform.position);
            pathLineRenderer.SetPosition(1, transform.position);

            if (lineMaterial != null)
            {
                pathLineRenderer.material = lineMaterial;
            }

            // Set texture mode to Tile and adjust texture scale
            pathLineRenderer.textureMode = LineTextureMode.Tile;
            pathLineRenderer.materials[0].mainTextureScale = new Vector2(textureScale, 1);
        }
    }


    void Update()
    {
        if (pathLineRenderer != null)
        {
            // Update texture offset to create scrolling effect
            float offset = Time.time * scrollSpeed % 1;
            pathLineRenderer.material.mainTextureOffset = new Vector2(offset, 0);
        }
    }

    private void UpdateTextureTiling(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        // Adjust the texture scale based on the line length
        pathLineRenderer.material.mainTextureScale = new Vector2(distance * textureScale, 1);
    }


    public void UpdateLine(Vector3 endPosition)
    {
        if (pathLineRenderer == null) return;

        Vector3 startPosition = transform.position + new Vector3(0, yOffset, 0);
        pathLineRenderer.SetPosition(0, startPosition);
        pathLineRenderer.SetPosition(1, endPosition);

        UpdateTextureTiling(startPosition, endPosition);
    }


    // Method to clear the line renderer
    public void ClearLine()
    {
        // If the line renderer is assigned, reset its position count to 0, effectively hiding the line
        if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = 0;
        }
    }

    // Method to stretch the path to a specified target tower
    public void StretchPathToTarget(Transform targetTower)
    {
        if (targetTower == null || pathLineRenderer == null) return;

        // Ensure the positionCount is set to 2 before setting positions
        pathLineRenderer.positionCount = 2;

        // Set the start position of the line at the current object's position, adjusted by yOffset
        pathLineRenderer.SetPosition(0, transform.position + new Vector3(0, yOffset, 0));
        // Set the end position of the line at the target tower's position, adjusted by yOffset
        pathLineRenderer.SetPosition(1, targetTower.position + new Vector3(0, yOffset, 0));
    }
}



