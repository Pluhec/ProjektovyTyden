using UnityEngine;

/// <summary>
/// Drives the _CloudTime property on the cloud material.
/// Toggle 'stopped' to freeze clouds in place without any jump.
/// </summary>
public class CloudTimeController : MonoBehaviour
{
    [Tooltip("When true, clouds stop moving. When set back to false they resume from where they were.")]
    public bool stopped = false;

    private float accumulatedTime;
    private Renderer rend;
    private MaterialPropertyBlock mpb;
    private static readonly int CloudTimeID = Shader.PropertyToID("_CloudTime");

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning($"[CloudTimeController] No Renderer found on '{gameObject.name}'.");
        }
        mpb = new MaterialPropertyBlock();
        accumulatedTime = Time.time;
        ApplyTime();
    }

    void Update()
    {
        if (!stopped)
        {
            accumulatedTime += Time.deltaTime;
        }
        ApplyTime();
    }

    private void ApplyTime()
    {
        if (rend == null) return;
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(CloudTimeID, accumulatedTime);
        rend.SetPropertyBlock(mpb);
    }
}
