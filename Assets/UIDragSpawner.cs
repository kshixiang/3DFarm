using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragSpawner : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public TileType typeToSpawn;
    public GameObject previewPrefab;

    private GameObject currentPreviewInstance;
    private float gridSize;
    private Transform gridRoot; 

    // --- 【关键修正】在这里声明 dragPlane 变量 ---
    // 将其声明为类的成员变量，以便所有方法都可以访问
    private Plane dragPlane;

    void Start()
    {
        if (TileManager.Instance != null)
        {
            gridSize = TileManager.Instance.PlanarSize;
            gridRoot = TileManager.Instance.transform;
            
            // --- 【关键修正】在这里初始化 dragPlane ---
            // 初始时，让它与GridRoot的朝向和位置对齐
            dragPlane = new Plane(gridRoot.up, gridRoot.position);
        }
        else
        {
            Debug.LogError("UIDragSpawner无法找到TileManager实例！");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (previewPrefab == null || gridSize <= 0 || gridRoot == null) return;

        currentPreviewInstance = Instantiate(previewPrefab);
        currentPreviewInstance.transform.rotation = gridRoot.rotation;
        
        // 立即更新一次位置，让它出现在鼠标下面并对齐网格
        UpdatePreviewPosition();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentPreviewInstance != null)
        {
            UpdatePreviewPosition();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentPreviewInstance != null)
        {
            TileManager.Instance.PlaceTile(currentPreviewInstance.transform.GetComponent<Tile3D>());
            currentPreviewInstance = null;
        }
    }
    
    private void UpdatePreviewPosition()
    {
        // 实时更新虚拟平面的法线和位置，以防GridRoot在运行时也发生变化
        dragPlane.SetNormalAndPosition(gridRoot.up, gridRoot.position);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 现在可以正常使用 dragPlane 了
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPosition = ray.GetPoint(distance);
            
            Vector3 mouseLocalPosition = gridRoot.InverseTransformPoint(mouseWorldPosition);

            float snappedLocalX = Mathf.Floor(mouseLocalPosition.x / gridSize) * gridSize + (gridSize / 2f);
            float snappedLocalZ = Mathf.Floor(mouseLocalPosition.z / gridSize) * gridSize + (gridSize / 2f);
            
            Vector3 finalLocalPosition = new Vector3(snappedLocalX, 0.1f, snappedLocalZ);

            currentPreviewInstance.transform.position = gridRoot.TransformPoint(finalLocalPosition);
            currentPreviewInstance.transform.rotation = gridRoot.rotation;
        }
    }
}