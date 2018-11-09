using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour {

    private static int menuCdMax = 10;

    public List<Item> itemList;

    private GameObject player;
    private GameController gameCtl;
    private bool isMenuOpen;
    private int menuCd;
    private InventoryMenu invMenu;


    public void InitInvCtl() {
        itemList = new List<Item>();
        gameCtl = GameController.GetInstance();
        player = gameCtl.GetPlayer();
        isMenuOpen = false;
        menuCd = 0;
        invMenu = GameObject.Find("InventoryMenu").GetComponent<InventoryMenu>();
        invMenu.InitMenu();
    }

    public void AddItem(Item item)
    {
        int i = itemList.Count;
        itemList.Add(item);
        invMenu.UpdateMenu();
    }

    public void RemoveItem(Item item)
    {
        itemList.Remove(item);
        invMenu.UpdateMenu();
    }

    [ContextMenu("Add SmokeScreen Item")]
    void AddSmokeScreen()
    {
        this.AddItem(new SmokeScreenItem());
    }
    void Update(){
        if(menuCd <= 0 && Input.GetKeyDown("i"))
        {
            Debug.Log("Toggling Inventory Menu");
            menuCd = menuCdMax;
            if(isMenuOpen)
            {
                isMenuOpen = false;
                invMenu.CloseMenu();
            }
            else
            {
                isMenuOpen = true;
                invMenu.OpenMenu();
            }
        }
        else
        {
            menuCd--;
        }

    }
	
}
