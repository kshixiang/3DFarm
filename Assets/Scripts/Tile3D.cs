using UnityEngine;

// enum TileType 保持不变
public enum TileType
{
    Land,
    River,
    Sand
}

public class Tile3D : MonoBehaviour
{
    public TileType type; // 这个保持不变，用于区分地块类型

    // --- 【新增】物品类型，用于资源管理器 ---
    [Tooltip("此地块在资源管理器中的类型")] public PlaceableType itemType;

    // --- 【新增】对种植其上的农作物的引用 ---
    public Crop plantedCrop { get; private set; }

    private Outline outline;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float outlineWidth = 2f;
    private Plane groundPlane;
    public float RotY;
    private Transform gridRoot;
    private float gridSize;
    private const float DragYOffset = .0f;
    private float lastClickTime = -1f;
    private const float DoubleClickTimeThreshold = 0.3f;

    private void Awake()
    {
        outline = gameObject.GetComponent<Outline>();
        if (outline == null) outline = gameObject.AddComponent<Outline>();
        outline.enabled = false;

        if (GetComponent<Collider>() == null) gameObject.AddComponent<BoxCollider>();

        if (TileManager.Instance != null)
        {
            gridSize = TileManager.Instance.PlanarSize;
            gridRoot = TileManager.Instance.transform;
        }
        else
        {
            Debug.LogError("Tile3D无法找到TileManager实例！");
            gridSize = 1f;
        }

        transform.parent = gridRoot;
        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }

    private void OnMouseDown()
    {
        if (GameUIHandler.Instance.Preview) return;

        // --- 【修改】在回收前，检查地块上是否有农作物 ---
        if (plantedCrop != null)
        {
            Debug.Log("地块上有农作物，不能回收！");
            return; // 直接返回，不执行后续逻辑
        }

        if (TileManager.Instance != null && TileManager.Instance.IsTopTile(this))
        {
            if (Time.time - lastClickTime <= DoubleClickTimeThreshold)
            {
                RecycleTile();
                lastClickTime = -1f;
            }
            else
            {
                Select = true;
                lastClickTime = Time.time;
            }
        }
    }

    private bool Select;

    private void RecycleTile()
    {
        if (TileManager.Instance != null) TileManager.Instance.RemoveFromGrid(this);

        // --- 【修改】回收时，将物品数量返还给资源管理器 ---
        if (ResourceManager.Instance != null) ResourceManager.Instance.ReturnItem(itemType);

        Destroy(gameObject);
    }

    // --- 【新增】尝试种植作物的方法 ---
    public bool TryPlantCrop(GameObject cropPrefab)
    {
        // 只有Land类型且没有作物的地块才能种植
        if (type != TileType.Land || plantedCrop != null) return false;

        var transformPosition = transform.position;
        transformPosition.y += 0.1f;
        // 实例化作物，并设置其父对象为当前地块，以保持相对位置
        var cropInstance = Instantiate(cropPrefab, transformPosition, transform.rotation, transform);

        var cropComponent = cropInstance.GetComponent<Crop>();
        if (cropComponent != null)
        {
            // 建立双向引用
            plantedCrop = cropComponent;
            cropComponent.SetParentTile(this);
            return true;
        }

        // 如果预制件没有Crop脚本，则销毁实例并返回失败
        Destroy(cropInstance);
        return false;
    }

    // --- 【新增】当作物被收割时，由作物调用以清除引用 ---
    public void ClearPlantedCrop()
    {
        plantedCrop = null;
    }

    // OnMouseDrag, OnMouseUp, OnMouseEnter, OnMouseExit 方法保持不变...
    // ...
    private void OnMouseDrag()
    {
        if (!Select) return;

        if (gridRoot == null)
        {
            Debug.LogError("Grid Root 未设定！无法进行正确的拖拽计算。");
            return;
        }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var dragPlane = new Plane(gridRoot.up, gridRoot.position);

        if (dragPlane.Raycast(ray, out var distance))
        {
            var mouseWorldPosition = ray.GetPoint(distance);
            var mouseLocalPosition = gridRoot.InverseTransformPoint(mouseWorldPosition);

            var snappedLocalX = Mathf.Floor(mouseLocalPosition.x / gridSize) * gridSize + gridSize / 2f;
            var snappedLocalZ = Mathf.Floor(mouseLocalPosition.z / gridSize) * gridSize + gridSize / 2f;

            var finalLocalPosition = new Vector3(snappedLocalX, mouseLocalPosition.y, snappedLocalZ);

            transform.position = gridRoot.TransformPoint(finalLocalPosition);
            transform.rotation = gridRoot.rotation;
        }
    }

    private void OnMouseUp()
    {
        if (!Select) return;

        if (outline != null) outline.enabled = false;

        if (TileManager.Instance != null) TileManager.Instance.PlaceTile(this);

        Select = false;
    }

    private void OnMouseEnter()
    {
        if (outline != null)
        {
            outline.enabled = true;
            outline.OutlineColor = highlightColor;
            outline.OutlineWidth = outlineWidth;
        }
    }

    private void OnMouseExit()
    {
        if (outline != null) outline.enabled = false;
    }
}