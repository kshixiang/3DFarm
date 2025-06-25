using System;
using UnityEngine;

public enum TileType
{
    Land,
    River,
    Sand
}

public class Tile3D : MonoBehaviour
{
    public TileType type;

    private Outline outline;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float outlineWidth = 2f;
    private Plane groundPlane;
    public float RotY = 0f; // 用于记录��转角度
    private Transform gridRoot; 
    // --- 新增变量 ---
    // 在本地存储网格尺寸，以便在拖拽时频繁使用
    private float gridSize;

    // （可选）拖拽时让地块轻微抬起的高度，以获得更好的视觉反馈
    private const float DragYOffset = .0f;

    void Awake()
    {
    
    //    transform.SetParent(GameObject.Find("Root").transform,false);
        outline = gameObject.GetComponent<Outline>();
        if (outline == null) outline = gameObject.AddComponent<Outline>();
        outline.enabled = false;

        if (GetComponent<Collider>() == null) gameObject.AddComponent<BoxCollider>();

        // --- 新增逻辑：在开始时从 TileManager 获取并存储 gridSize ---
        if (TileManager.Instance != null)
        {
            gridSize = TileManager.Instance.PlanarSize;
            gridRoot = TileManager.Instance.transform; // 获取Grid Root的Transform
        }
        else
        {
            Debug.LogError("Tile3D无法找到TileManager实例！将使用默认gridSize=1。");
            gridSize = 1f; // 设置一个默认值以防万一
        }
        transform.parent = gridRoot;

        groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        
    }
    

    // OnMouseDown 保持不变
    private void OnMouseDown()
    {
        if (TileManager.Instance != null && TileManager.Instance.IsTopTile(this))
        {
            Select = true;
        }
    }

    private bool Select = false;

    // --- 核心修改：在 OnMouseDrag 中实现吸附逻辑 ---
    private void OnMouseDrag()
    {
        if (!Select)
        {
            return;
        }

        // 安全检查
        if (gridRoot == null)
        {
            Debug.LogError("Grid Root 未设定！无法进行正确的拖拽计算。");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // 我们需要一个与Grid Root对齐的虚拟平面来进行射线检测
        Plane dragPlane = new Plane(gridRoot.up, gridRoot.position);

        if (dragPlane.Raycast(ray, out float distance))
        {
            // 1. 获取鼠标在（与Grid Root对齐的）虚拟平面上的世界坐标
            Vector3 mouseWorldPosition = ray.GetPoint(distance);

            // 2. 将这个世界坐标转换为Grid Root的局部坐标
            Vector3 mouseLocalPosition = gridRoot.InverseTransformPoint(mouseWorldPosition);

            // 3. 在局部坐标系下进行网格吸附计算
            float snappedLocalX = Mathf.Floor(mouseLocalPosition.x / gridSize) * gridSize + (gridSize / 2f);
            float snappedLocalZ = Mathf.Floor(mouseLocalPosition.z / gridSize) * gridSize + (gridSize / 2f);
            
            // 4. 构建最终的局部坐标（Y轴可以保持不变或设为偏移）
            Vector3 finalLocalPosition = new Vector3(snappedLocalX, mouseLocalPosition.y, snappedLocalZ);

            // 5. 将计算出的局部坐标转换回世界坐标，来设置地块的位置
            transform.position = gridRoot.TransformPoint(finalLocalPosition);
            
            // 6. （可选但推荐）拖拽时也让地块的旋转与网格一致
            transform.rotation = gridRoot.rotation;
        }
    }

    // OnMouseUp 保持不变，它会将已经对齐好的地块交给Manager处理最终的堆叠（Y轴）位置
    private void OnMouseUp()
    {
        if (!Select)
        {
            return;
        }

        if (outline != null) outline.enabled = false;

        if (TileManager.Instance != null)
        {
            TileManager.Instance.PlaceTile(this);
        }

        Select = false;
    }

    // OnMouseEnter 和 OnMouseExit 保持不变
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