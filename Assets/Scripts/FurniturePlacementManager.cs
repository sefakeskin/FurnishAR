using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[DefaultExecutionOrder(100)]
public class FurniturePlacementManager : MonoBehaviour
{
    public GameObject spawnableFurniture;
    public XROrigin sessionOrigin;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public Camera arCamera;

    [Header("Mobilya Butonları")]
    public Image[] furnitureButtons;
    public GameObject[] buttonFurnitures;
    public float[] frontYaw;

    [Header("Menü ve Seçim")]
    public Material selectionMaterial;
    public GameObject menuPanel;
    public GameObject selectionBar;
    public Text planesButtonText;

    const float Gap = 0.02f;

    readonly List<ARRaycastHit> arHits = new List<ARRaycastHit>();
    readonly RaycastHit[] physHits = new RaycastHit[16];
    readonly List<PlacedFurniture> placed = new List<PlacedFurniture>();

    PlacedFurniture selected;
    bool dragging;
    Vector3 dragOffset;
    bool showPlanes = true;
    bool planesAutoHidden;

    void Start()
    {
        if (menuPanel) menuPanel.SetActive(false);
        HighlightSelected();
        RefreshUI();
    }

    void Update()
    {
        if (Input.touchCount == 1) HandleTouch(Input.GetTouch(0));
        else dragging = false;

        foreach (var plane in planeManager.trackables)
        {
            bool visible = showPlanes && plane.subsumedBy == null;
            if (plane.gameObject.activeSelf != visible) plane.gameObject.SetActive(visible);
        }
    }

    void HandleTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                dragging = false;
                if (IsButtonPressed(touch)) return;
                CloseMenu();

                // Sahnedeki bir mobilyaya dokunulduysa seç ve sürüklemeyi başlat
                var hitFurniture = RaycastFurniture(touch.position);
                if (hitFurniture != null)
                {
                    Select(hitFurniture);
                    if (RaycastPlane(touch.position, TrackableType.PlaneWithinInfinity, out Pose p))
                    {
                        dragOffset = hitFurniture.transform.position - p.position;
                        dragOffset.y = 0f;
                        dragging = true;
                    }
                    return;
                }

                // Seçili mobilya varken boş alana dokunulursa seçimi kaldır
                if (selected != null)
                {
                    Select(null);
                    return;
                }

                // Zemine dokunulduğunda yeni mobilya yerleştir
                if (RaycastPlane(touch.position, TrackableType.PlaneWithinPolygon, out Pose pose))
                    Spawn(pose.position);
                break;

            case TouchPhase.Moved:
                if (dragging && selected != null &&
                    RaycastPlane(touch.position, TrackableType.PlaneWithinInfinity, out Pose mp))
                {
                    selected.transform.position = mp.position + dragOffset;
                    ResolveOverlap(selected);
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                dragging = false;
                break;
        }
    }

    void Spawn(Vector3 position)
    {
        var go = new GameObject(spawnableFurniture.name);
        var f = go.AddComponent<PlacedFurniture>();
        f.Init(selectionMaterial);
        go.transform.SetPositionAndRotation(position, FacingCamera(position));
        f.SetModel(spawnableFurniture, YawFor(spawnableFurniture));
        placed.Add(f);
        ResolveOverlap(f);

        if (!planesAutoHidden)
        {
            planesAutoHidden = true;
            showPlanes = false;
            RefreshUI();
        }
    }

    Quaternion FacingCamera(Vector3 position)
    {
        Vector3 d = arCamera.transform.position - position;
        d.y = 0f;
        return d.sqrMagnitude < 1e-6f ? Quaternion.identity : Quaternion.LookRotation(d.normalized, Vector3.up);
    }

    float YawFor(GameObject prefab)
    {
        int i = System.Array.IndexOf(buttonFurnitures, prefab);
        return (frontYaw != null && i >= 0 && i < frontYaw.Length) ? frontYaw[i] : 0f;
    }

    // Mobilyaların üst üste binmesini önlemek için çarpışma kontrolü
    void ResolveOverlap(PlacedFurniture f)
    {
        for (int iter = 0; iter < 10; iter++)
        {
            bool moved = false;
            foreach (var other in placed)
            {
                if (other == f) continue;
                if (!Physics.ComputePenetration(
                        f.box, f.transform.position, f.transform.rotation,
                        other.box, other.transform.position, other.transform.rotation,
                        out Vector3 dir, out float dist))
                    continue;

                Vector3 push = new Vector3(dir.x, 0f, dir.z);
                if (push.sqrMagnitude < 1e-4f)
                {
                    push = f.transform.position - other.transform.position;
                    push.y = 0f;
                    if (push.sqrMagnitude < 1e-6f) push = arCamera.transform.right;
                }
                f.transform.position += push.normalized * (dist + Gap);
                moved = true;
            }
            if (!moved) break;
        }
        Physics.SyncTransforms();
    }

    bool RaycastPlane(Vector2 screenPos, TrackableType type, out Pose pose)
    {
        if (raycastManager.Raycast(screenPos, arHits, type))
        {
            foreach (var hit in arHits)
            {
                if (hit.trackable is ARPlane plane && plane.alignment != PlaneAlignment.HorizontalUp) continue;
                if (Vector3.Dot(hit.pose.up, Vector3.up) < 0.9f) continue;
                pose = hit.pose;
                return true;
            }
        }
        pose = default;
        return false;
    }

    PlacedFurniture RaycastFurniture(Vector2 screenPos)
    {
        Physics.SyncTransforms();
        var ray = arCamera.ScreenPointToRay(screenPos);
        int n = Physics.RaycastNonAlloc(ray, physHits, 30f, ~0, QueryTriggerInteraction.Collide);
        PlacedFurniture best = null;
        float bestDist = float.MaxValue;
        for (int i = 0; i < n; i++)
        {
            var f = physHits[i].collider.GetComponentInParent<PlacedFurniture>();
            if (f != null && physHits[i].distance < bestDist)
            {
                best = f;
                bestDist = physHits[i].distance;
            }
        }
        return best;
    }

    public bool IsButtonPressed(Touch touch)
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
    }

    void Select(PlacedFurniture f)
    {
        if (selected != null) selected.SetSelected(false);
        selected = f;
        if (selected != null) selected.SetSelected(true);
        HighlightSelected();
        RefreshUI();
    }

    public void SwitchFurniture(GameObject furniture)
    {
        spawnableFurniture = furniture;
        if (selected != null)
        {
            selected.SetModel(furniture, YawFor(furniture));
            selected.SetSelected(true);
            ResolveOverlap(selected);
        }
        HighlightSelected();
    }

    public void RotateSelected()
    {
        if (selected == null) return;
        selected.transform.Rotate(0f, 45f, 0f, Space.World);
        ResolveOverlap(selected);
    }

    public void DeleteSelected()
    {
        if (selected == null) return;
        placed.Remove(selected);
        Destroy(selected.gameObject);
        selected = null;
        HighlightSelected();
        RefreshUI();
    }

    public void Deselect() => Select(null);

    public void ClearAll()
    {
        foreach (var f in placed) Destroy(f.gameObject);
        placed.Clear();
        selected = null;
        CloseMenu();
        HighlightSelected();
        RefreshUI();
    }

    public void ToggleMenu()
    {
        if (menuPanel) menuPanel.SetActive(!menuPanel.activeSelf);
    }

    public void CloseMenu()
    {
        if (menuPanel) menuPanel.SetActive(false);
    }

    public void TogglePlanes()
    {
        showPlanes = !showPlanes;
        planesAutoHidden = true;
        RefreshUI();
        CloseMenu();
    }

    void RefreshUI()
    {
        if (selectionBar) selectionBar.SetActive(selected != null);
        if (planesButtonText) planesButtonText.text = showPlanes ? "Zemini Gizle" : "Zemini Göster";
    }

    void HighlightSelected()
    {
        if (furnitureButtons == null || buttonFurnitures == null) return;
        var current = selected != null ? selected.prefab : spawnableFurniture;
        for (int i = 0; i < furnitureButtons.Length && i < buttonFurnitures.Length; i++)
        {
            bool on = buttonFurnitures[i] == current;
            furnitureButtons[i].color = on ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            furnitureButtons[i].transform.localScale = Vector3.one * (on ? 1.12f : 1f);
        }
    }
}
