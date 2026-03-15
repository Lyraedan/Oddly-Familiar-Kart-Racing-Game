using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    public RacerConfig config;
    public RectTransform playerInMap;

    // The root object that contains the level meshes
    public Transform trackRoot => RaceManager.Instance.TrackRoot;

    // The minimap UI image
    public RectTransform minimapUI => IngameUIHolder.Instance.MiniMapUI;

    public Image MiniMapImage => IngameUIHolder.Instance.MinimapBackground;

    private Vector2 worldMin;
    private Vector2 worldMax;

    void Start()
    {
        CalculateBounds();
        UpdateMinimapBackground();
    }

    void Update()
    {
        // Bounds not calculated yet
        if (worldMin == Vector2.zero && worldMax == Vector2.zero)
            return;

        UpdatePlayerPosition(RaceManager.Instance.MinimapOrientation);
    }

    void CalculateBounds()
    {
        Renderer[] renderers = trackRoot.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        worldMin = new Vector2(bounds.min.x, bounds.min.z);
        worldMax = new Vector2(bounds.max.x, bounds.max.z);
        Debug.Log("Minimap bounds calculated: " + worldMin + " to " + worldMax);
    }

    void UpdateMinimapBackground()
    {
        if (MiniMapImage == null) return;

        // Fixed square minimap
        MiniMapImage.rectTransform.sizeDelta = new Vector2(256f, 256f);

        // Center the image
        MiniMapImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        MiniMapImage.rectTransform.anchoredPosition = Vector2.zero;
        MiniMapImage.rectTransform.localRotation = Quaternion.identity;
    }

    void UpdatePlayerPosition(Vector2 orientation)
    {
        Vector3 playerPos = transform.position;

        float nx = Mathf.InverseLerp(worldMin.x, worldMax.x, playerPos.x);
        float ny = Mathf.InverseLerp(worldMin.y, worldMax.y, playerPos.z);

        Vector2 centered = new Vector2(nx - 0.5f, ny - 0.5f);

        // orientation = direction in world space that points "up" on the minimap
        float cos = orientation.x;
        float sin = orientation.y;

        Vector2 rotated;
        rotated.x = centered.x * cos - centered.y * sin;
        rotated.y = centered.x * sin + centered.y * cos;

        // This shit needs fucking fixing
        float mapX = rotated.x * MiniMapImage.rectTransform.sizeDelta.x;
        float mapY = rotated.y * MiniMapImage.rectTransform.sizeDelta.y;

        playerInMap.localPosition = new Vector3(mapX, mapY, 0f);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (trackRoot == null)
            return;

        // If bounds haven't been calculated yet (editor case)
        if (worldMin == Vector2.zero && worldMax == Vector2.zero)
        {
            Renderer[] renderers = trackRoot.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
                bounds.Encapsulate(r.bounds);

            worldMin = new Vector2(bounds.min.x, bounds.min.z);
            worldMax = new Vector2(bounds.max.x, bounds.max.z);
        }

        Gizmos.color = Color.yellow;

        Vector3 p1 = new Vector3(worldMin.x, 0, worldMin.y);
        Vector3 p2 = new Vector3(worldMax.x, 0, worldMin.y);
        Vector3 p3 = new Vector3(worldMax.x, 0, worldMax.y);
        Vector3 p4 = new Vector3(worldMin.x, 0, worldMax.y);

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);

        // This shit tracks perfectly here
        Gizmos.color = Color.red;
        Vector3 mapPos = new Vector3(
            Mathf.Lerp(worldMin.x, worldMax.x, Mathf.InverseLerp(worldMin.x, worldMax.x, transform.position.x)),
            0,
            Mathf.Lerp(worldMin.y, worldMax.y, Mathf.InverseLerp(worldMin.y, worldMax.y, transform.position.z))
        );
        Gizmos.DrawSphere(mapPos, 1f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(mapPos, mapPos + Vector3.up * 500f);
    }
#endif
}