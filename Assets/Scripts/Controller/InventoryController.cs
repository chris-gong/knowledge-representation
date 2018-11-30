using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour {

    private static int menuCdMax = 10;
    private static int MenuSize = 12;

    public List<Item> itemList;

    private GameObject player;
    private GameController gameCtl;
    private bool isMenuOpen;
    private int menuCd;
    private InventoryMenu invMenu;


    public void InitInvCtl()
    {
        itemList = new List<Item>();
        gameCtl = GameController.GetInstance();
        player = gameCtl.GetPlayer();
        isMenuOpen = false;
        menuCd = 0;

        GameObject menu = GameObject.Find("InventoryMenu");
        menu.transform.GetChild(0).gameObject.SetActive(true);

        invMenu = menu.GetComponent<InventoryMenu>();
        invMenu.InitMenu();
        invMenu.CloseMenu();

#if UNITY_EDITOR
        //Default inventory
        AddSmokeScreen();
        AddBlockade();
#endif

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

    public InventoryMenu getMenu()
    {
        return invMenu;
    } 

    [ContextMenu("Add SmokeScreen Item")]
    void AddSmokeScreen()
    {
        this.AddItem(new SmokeScreenItem());
    }

    [ContextMenu("Add Blockade Item")]
    void AddBlockade()
    {
        this.AddItem(new BlockadeItem(argWidth: 4, argLength: 2));
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
