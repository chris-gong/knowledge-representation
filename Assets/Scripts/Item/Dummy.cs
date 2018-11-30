using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyItem: Item
{
    public DummyItem(string argItemName = "FILL NAME HERE")
        : base(argItemName, 1)
    {
    }

    public override void OnUse()
    {
        base.OnUse();
    }
}

public class Dummy : MonoBehaviour
{
    void Start()
    {
    }

}
