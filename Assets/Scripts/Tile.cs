using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IDropHandler
{
    [System.NonSerialized] public int GridX;
    [System.NonSerialized] public int GridY;

    public Item ItemTenent { get; set; }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        ItemTenent = eventData.pointerDrag.GetComponent<Item>();
        if (ItemTenent == null)
        {
            throw new System.Exception("OnDrop tile event triggered with cursor data but with a null module");
        }
    }

    public void LoadFromSerializedTile(SerializableTile sTile)
    {
        GridX = sTile.GridX;
        GridY = sTile.GridY;
        //ItemTenent = sTile.ItemTenent;
    }
}

[Serializable]
public class SerializableTile
{
    public int GridX { get; set; }
    public int GridY { get; set; }
    public SerializableItem ItemTenent { get; set; }

    public SerializableTile(Tile tile)
    {
        GridX = tile.GridX;
        GridY = tile.GridY;
        ItemTenent = new SerializableItem(tile.ItemTenent);
    }
}
