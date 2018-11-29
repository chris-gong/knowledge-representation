using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemplateItem: Item
{
    public TemplateItem(string argItemName = "FILL NAME HERE")
        : base(argItemName, 1)
    {
    }

    public override void OnUse()
    {
        base.OnUse();
    }
}

public class Template : MonoBehaviour
{
    void Start()
    {
    }

}
