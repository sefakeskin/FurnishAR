using UnityEngine;

public class PlacedFurniture : MonoBehaviour
{
    public GameObject prefab;
    public BoxCollider box;

    GameObject model;
    GameObject indicator;

    public void Init(Material selectionMaterial)
    {
        box = gameObject.AddComponent<BoxCollider>();

        indicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
        indicator.name = "Selection";
        DestroyImmediate(indicator.GetComponent<Collider>());
        indicator.GetComponent<Renderer>().sharedMaterial = selectionMaterial;
        indicator.transform.SetParent(transform, false);
        indicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        indicator.SetActive(false);
    }

    public void SetModel(GameObject newPrefab, float frontYaw)
    {
        prefab = newPrefab;
        Vector3 p = transform.position;
        Quaternion r = transform.rotation;
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        if (model) Destroy(model);
        model = Instantiate(newPrefab, transform);
        model.name = "Model";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(0f, frontYaw, 0f) * newPrefab.transform.rotation;
        model.transform.localScale = newPrefab.transform.localScale;

        // Collider boyutunu modelin renderer sınırlarına göre hesapla
        var rs = model.GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds(Vector3.zero, Vector3.zero);
        for (int i = 0; i < rs.Length; i++)
        {
            if (i == 0) b = rs[i].bounds; else b.Encapsulate(rs[i].bounds);
        }
        box.center = b.center;
        box.size = b.size;

        indicator.transform.localPosition = new Vector3(b.center.x, b.min.y + 0.005f, b.center.z);
        indicator.transform.localScale = new Vector3(b.size.x + 0.12f, b.size.z + 0.12f, 1f);

        transform.SetPositionAndRotation(p, r);
    }

    public void SetSelected(bool selected) => indicator.SetActive(selected);
}
