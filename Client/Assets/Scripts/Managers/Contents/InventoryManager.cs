using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
	const int POTION_TEMPLATE_ID = 1000;

	public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();

	public void Add(Item item)
	{
		Items.Add(item.ItemDbId, item);
	}

    public void Remove(Item item)
    {
        Items.Remove(item.ItemDbId);
    }

    public Item Get(int itemDbId)
	{
		Item item = null;
		Items.TryGetValue(itemDbId, out item);
		return item;
	}

	public Item Find(Func<Item, bool> condition)
	{
		foreach (Item item in Items.Values)
		{
			if (condition.Invoke(item))
				return item;
		}

		return null;
	}

    public List<Item> FindAll(Func<Item, bool> condition)
    {
		List<Item> ret = new List<Item>();

        foreach (Item item in Items.Values)
        {
			if (condition.Invoke(item))
				ret.Add(item);
        }

        return ret;
    }

    public Item FindPotion()
    {
        return Find( i => i.TemplateId == POTION_TEMPLATE_ID);
    }

    public int GetPotionCount()
    {
		List<Item> potionList = FindAll(i =>
		{
			return i.TemplateId == POTION_TEMPLATE_ID;
		});

		return potionList.Count;
    }

	public void UsePotion()
    {
		int potionCount = GetPotionCount();

		if(potionCount == 0)
        {
			Debug.Log("사용 가능한 포션이 없습니다.");
			return;
        }
    }

	public void Clear()
	{
		Items.Clear();
	}


}