using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMenu : MonoBehaviour
{
    private InventoryController invCtl;
    private GameObject panel;


    public void openMenu(){
        panel.SetActive(true);
    }
    public void closeMenu(){
        panel.SetActive(false);
    }

    public void Start()
    {
        invCtl = GetComponent<InventoryController>();
        panel = transform.GetChild(0).gameObject;
    }
}
