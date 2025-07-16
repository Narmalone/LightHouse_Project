using UnityEngine;

public class ToggleRayTracing : ToggleParameter
{
    new void True() => Debug.Log("RayTracing ON");
    new void False() => Debug.Log("RayTracing OFF");
}
