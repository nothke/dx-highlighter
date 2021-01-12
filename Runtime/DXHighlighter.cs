using System.Collections.Generic;
using UnityEngine;

public class DXHighlighter : MonoBehaviour
{
    public RectTransform image;

    public bool pulsing = true;
    public float pulseSpeed = 80;
    public float pulseDistancePixels = 20;
    public int screenEdgeMargin = 40;

    public bool manualUpdate = false;

    Bounds worldBounds;
    public Bounds screenBounds { get; private set; }

    float offsetPixels;
    Vector3[] boundsCorners = new Vector3[8];
    List<Renderer> renderersList = new List<Renderer>(10);

    bool wasHighlighting = false;

    public void Highlight(GameObject go)
    {
        FindRenderersFor(go);
        CalculateBounds();
    }

    public void ClearAll()
    {
        renderersList.Clear();

        image.gameObject.SetActive(false);
        wasHighlighting = false;
    }

    public void AddRenderer(Renderer renderer)
    {
        renderersList.Add(renderer);
    }

    public void RemoveRenderer(Renderer renderer)
    {
        renderersList.Remove(renderer);
    }

    void FindRenderersFor(GameObject go)
    {
        go.GetComponentsInChildren(renderersList);

        // Remove particle renderers
        for (int i = renderersList.Count - 1; i >= 0; i--)
        {
            if (renderersList[i] is ParticleSystemRenderer)
                renderersList.RemoveAt(i);
        }
    }

    private void Update()
    {
        if (manualUpdate)
            return;

        if (renderersList.Count > 0)
        {
            CalculateBounds();
            UpdateUIRects();
        }
    }

    public void CalculateBounds()
    {
        worldBounds = renderersList[0].bounds;
        for (int i = 1; i < renderersList.Count; i++)
        {
            worldBounds.Encapsulate(renderersList[i].bounds);
        }

        // Find 8 corners of the bounds
        boundsCorners[0] = worldBounds.min;
        boundsCorners[1] = worldBounds.max;
        boundsCorners[2] = new Vector3(boundsCorners[0].x, boundsCorners[0].y, boundsCorners[1].z);
        boundsCorners[3] = new Vector3(boundsCorners[0].x, boundsCorners[1].y, boundsCorners[0].z);
        boundsCorners[4] = new Vector3(boundsCorners[1].x, boundsCorners[0].y, boundsCorners[0].z);
        boundsCorners[5] = new Vector3(boundsCorners[0].x, boundsCorners[1].y, boundsCorners[1].z);
        boundsCorners[6] = new Vector3(boundsCorners[1].x, boundsCorners[0].y, boundsCorners[1].z);
        boundsCorners[7] = new Vector3(boundsCorners[1].x, boundsCorners[1].y, boundsCorners[0].z);

        Camera cam = Camera.main;

        // Transform bounds to screen-space
        boundsCorners[0] = cam.WorldToScreenPoint(boundsCorners[0]);
        boundsCorners[0].z = 10;
        Bounds _screenBounds = new Bounds(boundsCorners[0], Vector3.zero);

        for (int i = 1; i < 8; i++)
        {
            boundsCorners[i] = cam.WorldToScreenPoint(boundsCorners[i]);
            //boundsCorners[i].z = 10;
            _screenBounds.Encapsulate(boundsCorners[i]);
        }

        // Clamp to screen edges
        Vector3 min = _screenBounds.min;
        Vector3 max = _screenBounds.max;
        if (min.x < screenEdgeMargin) min.x = screenEdgeMargin;
        if (min.y < screenEdgeMargin) min.y = screenEdgeMargin;
        float lim = Screen.width - screenEdgeMargin;
        if (max.x > lim) max.x = lim;
        lim = Screen.height - screenEdgeMargin;
        if (max.y > lim) max.y = lim;

        min.z = max.z = 0;
        _screenBounds.SetMinMax(min, max);

        screenBounds = _screenBounds;
    }

    public void UpdateUIRects()
    {
        bool isHighlighting = renderersList.Count > 0;
        if (isHighlighting != wasHighlighting)
        {
            image.gameObject.SetActive(isHighlighting);
            offsetPixels = 0; // Resets timer
        }
        wasHighlighting = isHighlighting;

        if (isHighlighting)
        {
            image.position = screenBounds.center;

            if (pulsing)
            {
                offsetPixels -= Time.deltaTime * pulseSpeed;
                if (offsetPixels < 0)
                    offsetPixels = pulseDistancePixels;
            }
            else
                offsetPixels = 0;

            Vector2 sizeOff = Vector2.one * offsetPixels;
            image.sizeDelta = (Vector2)screenBounds.size + sizeOff;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);

        for (int i = 0; i < 8; i++)
        {
            Gizmos.DrawWireCube(Camera.main.ScreenToWorldPoint(boundsCorners[i]), Vector3.one * 0.02f);
        }
    }
}
