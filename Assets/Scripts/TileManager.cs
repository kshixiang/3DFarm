using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public float rote;
    public GameObject tilePrefab; // 地块预制件，在Inspector中拖入
    public int maxStackHeight = 10; // 最大叠放层数

    // --- 修改部分 ---
    // gridSize 不再需要手动设置，将由代码自动计算
    // [SerializeField] 使得我们可以在Inspector中看到自动计算出的值，方便调试
    [SerializeField] private float planarSize; // XZ平面的网格尺寸
    [SerializeField] private float stackHeight; // Y轴的堆叠高度
    private readonly Dictionary<Vector2Int, List<Tile3D>> grid = new();
    public static TileManager Instance { get; private set; }

    // 使用公开属性，让其他脚本可以读取尺寸，但不能修改，更安全。
    public float PlanarSize { get; private set; }
    public float StackHeight { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 在Awake中执行尺寸计算
            CalculateGridSizeFromPrefab();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 新增方法 ---
    /// <summary>
    ///     根据 tilePrefab 自动计算网格尺寸和堆叠高度
    /// </summary>
    private void CalculateGridSizeFromPrefab()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("TileManager: tilePrefab 未在Inspector中指定！");
            return;
        }

        // --- 修改后的诊断代码 ---

        // 1. 获取 Renderer 组件，它包含了最终的世界尺寸信息
        var prefabRenderer = tilePrefab.GetComponentInChildren<Renderer>();
        if (prefabRenderer == null)
        {
            Debug.LogError("TileManager: tilePrefab 上没有找到 Renderer 组件！");
            return;
        }

        // 2. 获取 MeshFilter 组件，它包含了未经缩放的原始模型信息
        var prefabMeshFilter = tilePrefab.GetComponentInChildren<MeshFilter>();
        if (prefabMeshFilter == null || prefabMeshFilter.sharedMesh == null)
        {
            Debug.LogError("TileManager: tilePrefab 上没有找到 MeshFilter 或有效的 Mesh！");
            return;
        }

        // 3. 获取各个尺寸和缩放值
        var finalWorldSize = prefabRenderer.bounds.size;
        var originalMeshSize = prefabMeshFilter.sharedMesh.bounds.size;
        var prefabScale = tilePrefab.transform.localScale;

        // 4. 打印所有信息到控制台
        Debug.Log("--- TILE SIZE DIAGNOSTICS ---");
        Debug.Log($"预制件 Transform Scale: {prefabScale}");
        Debug.Log($"原始模型网格尺寸 (Local Size): {originalMeshSize}");
        Debug.Log($"计算出的最终世界尺寸 (World Size): {finalWorldSize}");
        Debug.Log($"验证: 原始X尺寸 * X缩放 = {originalMeshSize.x} * {prefabScale.x} = {originalMeshSize.x * prefabScale.x}");
        Debug.Log("------------------------------");

        // 修改后的赋值:
        PlanarSize = finalWorldSize.x;
        StackHeight = finalWorldSize.y;


        // 5. 使用最终的世界尺寸进行赋值（这部分逻辑不变）
        planarSize = finalWorldSize.x;
        stackHeight = finalWorldSize.y;


        if (planarSize <= 0.001f) Debug.LogError("计算出的平面尺寸过小或为0！请检查模型的导入设置和预制件的Scale。");
    }

    // 获取位置的堆叠高度
    public int GetStackHeight(Vector3 position)
    {
        var gridPos = WorldToGridPos(position);
        if (grid.TryGetValue(gridPos, out var tileStack)) return tileStack.Count;

        return 0;
    }

    public void RevemoveTile(Vector3 position)
    {
        // --- 修改部分：使用 planarSize 和 stackHeight ---
        var gridPos = new Vector2Int(
            Mathf.FloorToInt(position.x / planarSize),
            Mathf.FloorToInt(position.z / planarSize)
        );

        grid.Remove(gridPos);
    }

    // --- 新增 IsTopTile 方法 ---
    /// <summary>
    ///     检查一个地块实例是否是其所在堆叠的最顶层地块
    /// </summary>
    public bool IsTopTile(Tile3D tile)
    {
        var gridPos = WorldToGridPos(tile.transform.position);
        if (grid.TryGetValue(gridPos, out var tileStack) && tileStack.Count > 0)
            if (tileStack.Contains(tile))
                return tileStack.Last() == tile;

        return true;
    }

    /// <summary>
    ///     旋转需要更新对应的位置
    /// </summary>
    public void Rotation()
    {
        var valueCollection = grid?.Values.ToList();
        if (valueCollection != null)
            foreach (var tile3Dse in valueCollection)
                for (var index = 0; index < tile3Dse.Count; index++)
                {
                    var tile3D = tile3Dse[index];
                    PlaceTile(tile3D, false);
                }
    }


    // --- PlaceTile 使用“对象复用”版本 ---
    public bool PlaceTile(Tile3D tileToPlace, bool updatePos = true)
    {
        RemoveFromGrid(tileToPlace);

        // 1. 获取地块在世界中的目标位置
        var worldPosition = tileToPlace.transform.position;
        // 2. 将世界位置转换为基于TileManager的局部网格坐标
        var gridPos = WorldToGridPos(worldPosition);
        // 3. 在这个局部网格坐标上获取当前高度
        var currentHeight = GetStackHeight(worldPosition);

        if (currentHeight < maxStackHeight)
        {
            // 4. 计算出在TileManager局部坐标系下的、对齐网格的最终位置
            var finalLocalPosition = new Vector3(
                gridPos.x * PlanarSize + PlanarSize / 2f,
                currentHeight * StackHeight,
                gridPos.y * PlanarSize + PlanarSize / 2f
            );

            if (updatePos)
            {
                // 5. 将计算出的局部位置，转换回世界位置，来设置地块的最终transform
                tileToPlace.transform.position = transform.TransformPoint(finalLocalPosition);
                // 6. （关键）同时，让地块的旋转与TileManager的旋转保持一致
                tileToPlace.transform.rotation = transform.rotation;
            }


            // 7. 将地块添加到数据结构中
            if (!grid.ContainsKey(gridPos)) grid[gridPos] = new List<Tile3D>();

            grid[gridPos].Add(tileToPlace);
            return true;
        }

        Destroy(tileToPlace.gameObject);
        return false;
    }

    // --- 新增一个辅助方法，用于从任何列表中移除一个地块 ---
    public void RemoveFromGrid(Tile3D tile)
    {
        var valueCollection = grid?.Values;
        if (tile == null || valueCollection == null) return;

        foreach (var tile3Dse in valueCollection) tile3Dse.Remove(tile);
    }

    // --- 辅助方法，将任何世界坐标点转换为本对象的局部网格坐标 ---
    private Vector2Int WorldToGridPos(Vector3 worldPosition)
    {
        // 1. 将世界坐标点转换为TileManager的局部坐标点
        var localPosition = transform.InverseTransformPoint(worldPosition);
        // 2. 在局部坐标系下进行网格计算
        return new Vector2Int(
            Mathf.FloorToInt(localPosition.x / PlanarSize),
            Mathf.FloorToInt(localPosition.z / PlanarSize)
        );
    }
}