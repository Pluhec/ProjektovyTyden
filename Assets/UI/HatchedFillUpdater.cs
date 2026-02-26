using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Forces the UI Canvas to refresh the hatched fill material every frame,
/// so changes in the Inspector are visible in real-time.
/// Attach this to the same GameObject as the Image with the HatchedFill material.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class HatchedFillUpdater : MonoBehaviour
{
    private Graphic graphic;

    void OnEnable()
    {
        graphic = GetComponent<Graphic>();
    }

    void Update()
    {
        if (graphic != null)
            graphic.SetMaterialDirty();
    }
}
