using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowInNoFogOnly : MonoBehaviour
{
    public Camera cam; //The Camera using the masked render texture
    private MeshRenderer myRenderer; // reference to the render you want toggled based on the position of this transform
    private MeshRenderer[] childRenderers; // references to any children renderers

    [Range(0f, 1f)]
    public float threshold = 0.1f; //the threshold for when this script considers myRenderer should render

    // made so all instances share the same texture, reducing texture reads
    private static Texture2D myT2D;
    private static Rect r_rect;
    private static bool isDirty = true; // used so that only one instance will update the RenderTexture per frame

    public void Start()
    {
        myRenderer = GetComponent<MeshRenderer>();
        childRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    private Color GetColorAtPosition()
    {
        if (!cam)
        {
            // if no camera is referenced script assumes there no fog and will return white (which should show the entity)
            return Color.white;
        }

        RenderTexture renderTexture = cam.targetTexture;
        if (!renderTexture)
        {
            // fallback to Camera's Color
            return cam.backgroundColor;
        }

        // If fog of war texture to consider doesnt exist or doesn't match fog of war camera's dimensions, make a placeholder texture
        if (myT2D is null || renderTexture.width != r_rect.width || renderTexture.height != r_rect.height)
        {
            r_rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            myT2D = new Texture2D((int) r_rect.width, (int) r_rect.height, TextureFormat.RGB24, false);
        }

        // Does the fog of war RenderTexture need to be fetched from the Fog camera?
        // This is effectively instantiating the fog of war render texture
        if (isDirty)
        {
            // Change active Render Texture momentarily to read the fog/no-fog pixels
            RenderTexture current = RenderTexture.active;
            RenderTexture.active = renderTexture;
            myT2D.ReadPixels(r_rect, 0, 0);

            // Change active RenderTexture back to what it as
            RenderTexture.active = current;
            // Remaining objects do not need to load the new RenderTexture anymore
            isDirty = false;
        }

        // Where on the screen is this object?
        Vector3 pixel = cam.WorldToScreenPoint(transform.position);
        // Return the color of the fog of war at that location on the screen
        return myT2D.GetPixel((int) pixel.x, (int) pixel.y);
    }

    private void Update()
    {
        isDirty = true;
    }

    void LateUpdate()
    {
        // If no renderer is available, this whole game object disables?
        // TODO: What is this if statement for?
        if (!myRenderer)
        {
            enabled = false;
            return;
        }

        // Enable/disable mesh renderers depending on color value
        myRenderer.enabled = GetColorAtPosition().grayscale >= threshold;
        foreach(MeshRenderer renderer in childRenderers)
            renderer.enabled = GetColorAtPosition().grayscale >= threshold;
    }
}