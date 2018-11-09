using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Item
{
    public string name;
    public int count;

    public Item(string itemName, int itemCount = 1)
    {
        name = itemName;
        count = itemCount;
    }
}

public class MurderWeapon:Item{
    public float detectProb;
    public bool concealable;

    public MurderWeapon(string itemName, bool argConcealable = false, float argDetectProb = 0):base(itemName,1)
    {
        detectProb = argDetectProb;
        concealable = argConcealable;
    }
}
/*
public class ConsumableItem:Item{
    
    public ConsumableItem(string itemName, int itemCount = 1)
    {
        base(itemName, itemCount);
    }
}

*/