using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DXHighlighter : MonoBehaviour
{
    public bool pulsing = true;
    public float pulseSpeed = 40;
    public float pulseDistancePixels = 20;
    float offsetPixels;

    Bounds worldBounds;
    public Bounds screenBounds { get; private set; }

    //GameObject target;

    public RectTransform image;

    public bool manualUpdate = false;

    Vector3[] boundPoint = new Vector3[8];

    List<Renderer> renderersList = new List<Renderer>(10);

    public void Highlight(GameObject go)
    {
        //target = go;
        FindRenderersFor(go);
        image.gameObject.SetActive(true);
        CalculateBounds();
    }

    public void ClearAll()
    {
        //target = null;
        renderersList.Clear();
        image.gameObject.SetActive(false);
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

        // Find 8 points of the bounds
        boundPoint[0] = worldBounds.min;
        boundPoint[1] = worldBounds.max;
        boundPoint[2] = new Vector3(boundPoint[0].x, boundPoint[0].y, boundPoint[1].z);
        boundPoint[3] = new Vector3(boundPoint[0].x, boundPoint[1].y, boundPoint[0].z);
        boundPoint[4] = new Vector3(boundPoint[1].x, boundPoint[0].y, boundPoint[0].z);
        boundPoint[5] = new Vector3(boundPoint[0].x, boundPoint[1].y, boundPoint[1].z);
        boundPoint[6] = new Vector3(boundPoint[1].x, boundPoint[0].y, boundPoint[1].z);
        boundPoint[7] = new Vector3(boundPoint[1].x, boundPoint[1].y, boundPoint[0].z);

        Camera cam = Camera.main;

        boundPoint[0] = cam.WorldToScreenPoint(boundPoint[0]);
        boundPoint[0].z = 10;
        Bounds _screenBounds = new Bounds(boundPoint[0], Vector3.zero);

        for (int i = 1; i < 8; i++)
        {
            boundPoint[i] = cam.WorldToScreenPoint(boundPoint[i]);
            boundPoint[i].z = 10;
            _screenBounds.Encapsulate(boundPoint[i]);
        }

        screenBounds = _screenBounds;
    }

    void UpdateUIRects()
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);

        for (int i = 0; i < 8; i++)
        {
            Gizmos.DrawWireCube(Camera.main.ScreenToWorldPoint(boundPoint[i]), Vector3.one * 0.02f);
        }
    }
}
