using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using LargeNumbers;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Item _itemPrefab;
    [SerializeField] private Transform _gridParent;

    [SerializeField] private Canvas _draggingCanvas;
    private static Canvas s_draggingCanvas;

    private static int _baseWidth;
    private static int _baseHeight;

    private const int _minWidth = 6;
    private const int _maxWidth = 12;
    private const int _minHeight = 4;
    private const int _maxHeight = 10;

    
    private static Tile s_tilePrefab;
    private static Item s_itemPrefab;
    private static Transform s_gridParent;

    // Grid offset is basically a magic number, used to align the center the grid accounting for the UI on the left.
    private const int _gridParentXOffset = -153;

    private static Dictionary<Vector2, Tile> _tiles = new Dictionary<Vector2, Tile>();

    private static List<SpecialItemType> _mergeableSpecialTypes = new List<SpecialItemType> { SpecialItemType.GeneratorSpeedup, SpecialItemType.AutoMergeSpeedup, SpecialItemType.CurrencyBonus };

    private void Awake()
    {
        s_tilePrefab = _tilePrefab;
        s_itemPrefab = _itemPrefab;
        s_gridParent = _gridParent;
        s_draggingCanvas = _draggingCanvas;
    }

    void Start()
    {
        // base values for smallest grid
        _baseWidth = 6;
        _baseHeight = 4;
        //_baseWidth = 12;
        //_baseHeight = 10;

        GenerateGrid();
    }

    public static AlphabeticNotation TotalItemOutput()
    {
        AlphabeticNotation total = new AlphabeticNotation();

        foreach (var tile in _tiles.Values)
        {
            if (tile.ItemTenent != null && tile.ItemTenent.SpecialType == SpecialItemType.None)
            {
                // each tier outputs 50% more than the two items that were merged to create it
                total += Math.Pow(3, tile.ItemTenent.Tier - 1);
            }
        }

        return total;
    }

    public static Dictionary<SerializableVector2, SerializableTile> GetSerializedGridData()
    {
        var serializedGrid = new Dictionary<SerializableVector2, SerializableTile>();

        foreach (KeyValuePair<Vector2, Tile> tile in _tiles)
        {
            serializedGrid[new SerializableVector2(tile.Key)] = new SerializableTile(tile.Value);
        }

        return serializedGrid;
    }

    public static void GenerateGridFromData(Dictionary<SerializableVector2, SerializableTile> gridData)
    {

        int width = (int)gridData.Max(a => a.Key.x) + 1;
        int height = (int)gridData.Max(a => a.Key.y) + 1;

        // Sanity check for saved grid size
        if (width < _minWidth || width > _maxWidth || height < _minHeight || height > _maxHeight)
        {
            throw new Exception($"Failed to load grid from data. Grid size of {width}, {height} is not valid.");
        }

        // Generate the grid of our current size.
        GenerateGrid();

        // Apply the items to the grid from the serialized data
        foreach (SerializableTile savedTile in gridData.Values)
        {
            if (savedTile.ItemTenent != null && savedTile.ItemTenent.Tier != 0)
            {
                Tile tile = _tiles.FirstOrDefault(x => x.Value.GridX == savedTile.GridX && x.Value.GridY == savedTile.GridY).Value;

                // In the case that the user had items outside of the size of our unlocked grid, those items will be lost.
                if (tile == null)
                {
                    continue;
                }

                Item spawnedItem = Instantiate(s_itemPrefab, s_gridParent);
                spawnedItem.Deserialize(savedTile.ItemTenent, tile);
                spawnedItem.transform.position = tile.transform.position;
                spawnedItem.UpdateItem();
                spawnedItem.transform.localScale = new Vector3(1, 1, 1);
                spawnedItem.SetCanvasRefs(s_draggingCanvas);

                tile.ItemTenent = spawnedItem;
            }
        }

    }

    public static void GenerateGrid()
    {
        GenerateGrid(_baseWidth + UpgradeManager.GridSizeBonusX + PrestiegeSkillTreeManager.Instance.AdditionalColumn.GetBonusFlat(),
            _baseHeight + UpgradeManager.GridSizeBonusY + PrestiegeSkillTreeManager.Instance.AdditionalRow.GetBonusFlat());
    }

    public static void GenerateGrid(int width, int height)
    {
        // Place grid parent based on tile count, uses a magic number for the X offset.
        s_gridParent.position = new Vector3(_gridParentXOffset - (width - 10f) * 36, height * 36, 0);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Tile spawnedTile = Instantiate(s_tilePrefab, new Vector3(s_gridParent.position.x + x*72 + 36f, s_gridParent.position.y - y*72 - 36f, 0), Quaternion.identity);
                spawnedTile.transform.SetParent(s_gridParent, true);
                spawnedTile.name = $"tile.{x},{y}";
                spawnedTile.GridX = x;
                spawnedTile.GridY = y;

                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }
    }

    // Useful for when new rows or columns are added
    public static void RegenerateGrid()
    {
        // To be on the safe side, automatic events that care about what's on the grid are disabled while the grid is being regenerated.
        CurrencyManager.Collecting = false;
        Generator.Running = false;
        AutoMerger.Running = false;

        var gridSnapshot = GetSerializedGridData();
        DestroyGrid();
        GenerateGridFromData(gridSnapshot);

        Generator.Running = true;
        AutoMerger.Running = true;
        CurrencyManager.Collecting = true;
    }

    public Tile GetTileAtPosition(Vector2 position)
    {
        if (_tiles.TryGetValue(position, out Tile tile))
        {
            return tile;
        }

        return null;
    }

    public static Tile GetOpenTile()
    {
        foreach (var tile in _tiles.Values)
        {
            if (tile.ItemTenent == null)
            {
                return tile;
            }
        }

        return null;
    }

    public static List<ItemPair> GetItemsToMerge(int pairsToFind, int maxItemTier)
    {
        List<ItemPair> pairsFound = new List<ItemPair>();

        List<Item> bonusItems = _tiles.Values.Where(x => x.ItemTenent != null &&
                                               !x.ItemTenent.IsDragging &&
                                               x.ItemTenent.SpecialType != SpecialItemType.None &&
                                               x.ItemTenent.Tier <= PrestiegeSkillTreeManager.Instance.AutoMergeBonusItems.GetBonusPercent())
                                        .Select(x => x.ItemTenent).ToList();

        while (bonusItems.Count > 1 && pairsFound.Count < pairsToFind)
        {
            
            foreach (SpecialItemType bonusType in _mergeableSpecialTypes)
            {
                var bonusItemsOfType = bonusItems.Where(z => z.SpecialType == bonusType);

                if (bonusItemsOfType.Any())
                {
                    int minTier = bonusItemsOfType.Min(x => x.Tier);
                    List<Item> minTierItems = bonusItemsOfType.Where(i => i.Tier == minTier).ToList();

                    if (minTierItems.Count < 2)
                    {
                        bonusItems.Remove(minTierItems.First());
                    }
                    else
                    {
                        pairsFound.Add(new ItemPair(minTierItems.First(), minTierItems.Last()));
                        bonusItems.Remove(minTierItems.First());
                        bonusItems.Remove(minTierItems.Last());
                    }
                }
            }
        }

        List<Item> items = _tiles.Values.Where(x => x.ItemTenent != null &&
                                               !x.ItemTenent.IsDragging &&
                                               x.ItemTenent.SpecialType == SpecialItemType.None &&
                                               x.ItemTenent.Tier < maxItemTier)
                                        .Select(x => x.ItemTenent).ToList();

        while (items.Count > 1 && pairsFound.Count < pairsToFind)
        {
            int minTier = items.Min(x => x.Tier);
            List<Item> minTierItems = items.Where(i => i.Tier == minTier).ToList();

            if (minTierItems.Count < 2)
            {
                items.Remove(minTierItems.First());
            }
            else
            {
                pairsFound.Add(new ItemPair(minTierItems.First(), minTierItems.Last()));
                items.Remove(minTierItems.First());
                items.Remove(minTierItems.Last());
            }
        }

        return pairsFound;
    }

    public static void DestroyGrid()
    {
        // Delete tiles and items
        foreach (Transform child in s_gridParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        _tiles.Clear();
    }

    public static void EnforceMinItemTier()
    {
        List<Item> items = _tiles.Values.Where(x => x.ItemTenent != null && x.ItemTenent.SpecialType == SpecialItemType.None)
                                        .Select(x => x.ItemTenent).ToList();

        foreach (Item item in items)
        {
            int minTier = Item.GetMinTier();

            if (item.Tier < minTier)
            {
                item.Tier = minTier;
                item.UpdateItem();
            }
        }
    }

    public static bool GridContainsSpecialItems()
    {
        return _tiles.Values.Where(x => x.ItemTenent != null && x.ItemTenent.SpecialType != SpecialItemType.None).Any();
    }
}

[Serializable]
public class SerializableVector2
{
    public float x;
    public float y;

    public SerializableVector2(Vector2 vector)
    {
        x = vector.x;
        y = vector.y;
    }
}