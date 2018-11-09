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
            GameController.GetInstanceLevelController().setEventText("Knife Equipped (press space to use)", 4);
        }
    }
}

public class SmokeScreenItem : Item
{
    public int radius;
    public float duration;
    public SmokeScreenItem(string argItemName = "SmokeScreen", int argRadius = 5, float argDuration = 5f)
        :base(argItemName,1)
    {
        radius = argRadius;
        duration = argDuration;
    }

    public override void OnUse()
    {
        GameObject blankSS = Resources.Load<GameObject>("SmokeScreen");
        base.OnUse();
        Vector3 pos = GameController.GetInstance().GetPlayer().transform.position;
        GameObject ssobj = GameController.Instantiate(blankSS,pos, Quaternion.identity);
        GameController.GetInstance().GetInvCtl().RemoveItem(this);
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