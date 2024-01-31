using UnityEngine;

public class TextureScroll : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    private Material material;

    void Start()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            material = lineRenderer.material;
        }
    }

    void Update()
    {
        if (material != null)
        {
            float offset = Time.time * scrollSpeed;
            material.mainTextureOffset = new Vector2(offset, 0);
        }
    }
}
