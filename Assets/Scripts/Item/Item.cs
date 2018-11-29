using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Item
{
    public string name;
    public int count;

    public Item(string argItemName, int itemCount = 1)
    {
        name = argItemName;
        count = itemCount;
    }

    public virtual void OnUse()
    {
        Debug.Log(string.Format("Item used: {0}", name));
        return;
    }
}

public class MurderWeaponItem:Item{
    public float detectProb;
    public bool concealable;
    public string name;

    public MurderWeaponItem(string itemName, bool argConcealable = false, float argDetectProb = 0):base(itemName,1)
    {
        name = itemName;
        detectProb = argDetectProb;
        concealable = argConcealable;
    }

    public override void OnUse()
    {
        GameObject player = GameController.GetInstance().GetPlayer();
        Debug.Log(string.Format("Item used: {0}", name));

        if(name.ToLower() == "knife")
        {
            player.GetComponent<KillAgent>().equipPlayer(this);
            GameController.GetInstanceInventoryController().getMenu().CloseMenu();
            GameController.GetInstanceLevelController().SetEventText("Knife Equipped (press space to use)", 0);
        }
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