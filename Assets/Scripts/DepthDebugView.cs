using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class DepthDebugView : MonoBehaviour
{
    public AROcclusionManager occlusionManager;
    public RawImage preview;
    public Text info;
    public GameObject infoPanel;

    bool visible;

    void Start()
    {
        preview.uvRect = new Rect(0f, 1f, 1f, -1f);
        Apply();
    }

    public void Toggle()
    {
        visible = !visible;
        Apply();
    }

    void Apply()
    {
        preview.gameObject.SetActive(visible);
        infoPanel.SetActive(visible);
    }

    void Update()
    {
        if (!visible) return;

        bool hasTex = occlusionManager.TryGetEnvironmentDepthTexture(out Texture tex) && tex != null;
        if (hasTex) preview.texture = tex;

        info.text =
            "İstenen: " + occlusionManager.requestedEnvironmentDepthMode +
            "  |  Aktif: " + occlusionManager.currentEnvironmentDepthMode +
            "\nOcclusion: " + occlusionManager.currentOcclusionPreferenceMode +
            "  |  Derinlik dokusu: " + (hasTex ? tex.width + "x" + tex.height : "YOK") +
            "\nYakın = kırmızı, uzak (3m+) = mavi, siyah = veri yok";
    }
}
