using UnityEngine;
using UnityEngine.UI;

public class CameraToUI : MonoBehaviour
{
    public Camera renderCam;
    public RenderTexture renderTexture;

    private void Awake()
    {
        renderCam.targetTexture = renderTexture;
        renderCam.enabled = false; // Désactiver pour éviter rendu continu
    }

    public void RenderOnce()
    {
        renderCam.Render(); // Rend une seule frame sur la RenderTexture
    }
}
