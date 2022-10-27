using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class Item : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private SpriteAtlas _atlas;
    [SerializeField] private GameObject _mergeEffectPrefab;

    private Image _image;
    private CanvasGroup _canvasGroup;

    private Transform _gridParentTransform;
    private Canvas _draggingCanvas;

    private Vector3 _lastPosition;

    public static int BestItemTierThisRun { get; set; }

    public bool IsDragging { get; private set; }
    public int Tier { get; set; }
    public Tile TileLandlord { get; set; }

    public SpecialItemType SpecialType { get; set; }

    public const int MaxTier = 80;
    public const int SpecialItemMaxTier = 5;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _image = GetComponent<Image>();

        Tier = 1;
    }

    public static int GetMinTier()
    {
        return UpgradeManager.GeneratorMinTier.GetMinTierBonus() + PrestiegeSkillTreeManager.Instance.MinimumItemTier.GetBonusFlat();
    }

    // Since prefrabs cannot hold serialized fields from non-prefabs, we must assign them at runtime.
    public void SetCanvasRefs(Canvas dragging)
    {
        _gridParentTransform = transform.parent;
        _draggingCanvas = dragging;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsDragging)
        {
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // prevent starting another drag if the item is already moving
        if (IsDragging)
        {
            // In case we end up missing the pointer up, and an item is stuck on the cursor, another down click will drop it.
            OnPointerUp(eventData);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _lastPosition = transform.position;
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100));

            // Switch to the canvas that will be on top.
            transform.SetParent(_draggingCanvas.transform);
            

            _canvasGroup.alpha = 0.5f;
            _canvasGroup.blocksRaycasts = false;

            IsDragging = true;
        }
        else if (eventData.button == PointerEventData.InputButton.Right && this.SpecialType != SpecialItemType.None)
        {
            switch (this.SpecialType)
            {
                case SpecialItemType.GeneratorSpeedup:
                    Generator.Instance.StartSpecialItemBonus(this.Tier);
                    break;
                case SpecialItemType.AutoMergeSpeedup:
                    AutoMerger.Instance.StartSpecialItemBonus(this.Tier);
                    break;
                case SpecialItemType.CurrencyBonus:
                    CurrencyManager.Instance.StartSpecialItemBonus(this.Tier);
                    break;
            }

            TileLandlord.ItemTenent = null;
            Destroy(this.gameObject);
        }
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        bool resetPosition = true;

        foreach (GameObject obj in eventData.hovered)
        {
            Tile tile = obj.GetComponent<Tile>();
            Item existingItem = obj.GetComponent<Item>();

            if (tile != null && tile.ItemTenent == null)
            {
                resetPosition = false;

                // move out of the old tile
                TileLandlord.ItemTenent = null;

                // move into the new tile
                tile.ItemTenent = this;

                // sign the lease
                TileLandlord = tile;

                // change your mailing address
                name = $"Item.tier-{Tier}.{tile.name}";

                // park your car in the driveway
                transform.position = tile.transform.position;

                break;
            }
            else if (existingItem != null && existingItem.Tier == this.Tier && existingItem.SpecialType == this.SpecialType &&
                     (this.SpecialType == SpecialItemType.None && this.Tier < MaxTier || this.SpecialType != SpecialItemType.None && this.Tier < SpecialItemMaxTier))
            {
                existingItem.MergeUpgrade();
                existingItem.UpdateItem();
                existingItem.PlayUpgradeAnimation();
                Destroy(gameObject);
            }
        }

        // only allow dropping item prefabs onto tiles, otherwise snap back to the old spot
        if (resetPosition)
        {
            transform.position = _lastPosition;
        }

        transform.SetParent(_gridParentTransform);
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;

        IsDragging = false;
    }

    private void SpawnMergeEffect()
    {
        GameObject mergeEffectObj = Instantiate(_mergeEffectPrefab, _gridParentTransform);
        mergeEffectObj.transform.position = this.transform.position;
        Destroy(mergeEffectObj, 2f);
    }

    public void MergeUpgrade()
    {
        SpawnMergeEffect();

        if (SpecialType == SpecialItemType.None)
        {
            if (Tier < BestItemTierThisRun - 3 &&
            UnityEngine.Random.Range(0f, 1f) < PrestiegeSkillTreeManager.Instance.AddChanceForMergePlusThree.GetBonusPercent())
            {
                Tier += 4;
                Debug.Log($"Merging two items of tier {Tier - 4} got a bonus tier, producing an item of tier {Tier}");
            }
            else if (Tier < BestItemTierThisRun - 2 &&
            UnityEngine.Random.Range(0f, 1f) < PrestiegeSkillTreeManager.Instance.AddChanceForMergePlusTwo.GetBonusPercent())
            {
                Tier += 3;
                Debug.Log($"Merging two items of tier {Tier - 3} got a bonus tier, producing an item of tier {Tier}");
            }
            else if (Tier < BestItemTierThisRun - 1 &&
            UnityEngine.Random.Range(1f, 2f) < UpgradeManager.MergeUpOne.GetBonus())
            {
                Tier += 2;
                Debug.Log($"Merging two items of tier {Tier - 2} got a bonus tier, producing an item of tier {Tier}");
            }
            else
            {
                Tier++;
                Debug.Log($"Merging two items of tier {Tier - 1}, no bonus triggered");
            }

            CheckAgainstBestItemTier(Tier);
            AchievementManager.UpdateItemTrackers(Tier);
        }
        else
        {
            if (UnityEngine.Random.Range(0f, 1f) < PrestiegeSkillTreeManager.Instance.BonusItemMergeUpgradeChance.GetBonusPercent())
            {
                Tier += 2;
                Debug.Log($"Merging two BONUS items of tier {Tier - 2} got a bonus tier, producing an item of tier {Tier}");
            }
            else
            {
                Tier++;
                Debug.Log($"Merging two BONUS items of tier {Tier - 1}, no bonus triggered");
            }

            AchievementManager.UpdateSpecialItemTrackers();
        }
    }

    public void UpdateItem()
    {
        switch (this.SpecialType)
        {
            case SpecialItemType.None:
                name = $"Item.tier-{Tier}.{TileLandlord.name}";
                _image.sprite = _atlas.GetSprite($"num_{Tier}");
                _image.color = ColorController.GetColorForTier(Tier);
                break;
            case SpecialItemType.GeneratorSpeedup:
                name = $"Item.special_G-{Tier}.{TileLandlord.name}";
                _image.sprite = _atlas.GetSprite($"512_G_special_t{Tier}");
                break;
            case SpecialItemType.AutoMergeSpeedup:
                name = $"Item.special_M-{Tier}.{TileLandlord.name}";
                _image.sprite = _atlas.GetSprite($"512_M_special_t{Tier}");
                break;
            case SpecialItemType.CurrencyBonus:
                name = $"Item.special_C-{Tier}.{TileLandlord.name}";
                _image.sprite = _atlas.GetSprite($"512_C_special_t{Tier}");
                break;
        }
        
    }

    public void PlaySpawnAnimation()
    {
        transform.LeanScale(new Vector3(1, 1, 1), 0.2f).setEaseOutQuart();
    }

    public void PlayMergeAnimation(Vector3 positionToMoveTo, float animLength)
    {
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0.5f;

        transform.LeanScale(new Vector3(0.6f, 0.6f, 1), 0.2f).setEaseOutQuart();
        transform.LeanMoveLocal(positionToMoveTo, animLength);
        StartCoroutine(DestroySelf(animLength));
    }

    public void PlayUpgradeAnimation()
    {
        transform.LeanScale(new Vector3(1.04f, 1.04f, 1), 0.1f).setEaseOutQuart();
        transform.LeanScale(new Vector3(1, 1, 1), 0.1f).setEaseOutQuart().setDelay(0.1f);
    }

    IEnumerator DestroySelf(float timeToLive)
    {
        yield return new WaitForSeconds(timeToLive);
        Destroy(gameObject);
    }

    public static void CheckAgainstBestItemTier(int itemTier)
    {
        // Used to enforce automatic merge cap, and upgrade item caps.
        if (itemTier > BestItemTierThisRun)
        {
            BestItemTierThisRun = itemTier;
        }
    }

    public void Deserialize(SerializableItem sItem, Tile landlord)
    {
        Tier = sItem.Tier;
        SpecialType = sItem.SpecialType;
        TileLandlord = landlord;
    }
}

[Serializable]
public class SerializableItem
{
    public int Tier { get; set; }
    public SpecialItemType SpecialType { get; set; }

    public SerializableItem(Item item)
    {
        if (item != null)
        {
            Tier = item.Tier;
            SpecialType = item.SpecialType;
        }
    }
}

public class ItemPair
{
    public Item ItemOne { get; set; }
    public Item ItemTwo { get; set; }

    public ItemPair(Item itemOne, Item itemTwo)
    {
        ItemOne = itemOne;
        ItemTwo = itemTwo;
    }
}

public enum SpecialItemType
{
    None = 0,
    GeneratorSpeedup = 1,
    AutoMergeSpeedup = 2,
    CurrencyBonus = 3,
}