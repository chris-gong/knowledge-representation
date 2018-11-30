using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMenu : MonoBehaviour
{
    private InventoryController invCtl;
    private GameObject panel;
    private List<InventoryButton> buttons;


    public void OpenMenu(){
        panel.SetActive(true);
    }
    public void CloseMenu(){
        panel.SetActive(false);
    }

    public void UpdateMenu()
    {
        foreach(InventoryButton button in buttons) {
            if (button.index >= invCtl.itemList.Count) {
                button.UpdateItem(null);
            }
            else {
                button.UpdateItem(invCtl.itemList[button.index]);
            }
        }
    }

    public void InitMenu()
    {
        invCtl = GameController.GetInstance().GetInvCtl();
        buttons = new List<InventoryButton>();
        panel = transform.GetChild(0).gameObject;
        for (int i = 0; i < 3; i++) {
            Transform colPanel = transform.GetChild(0).GetChild(i);
            for (int j = 0; j < 6; j++) {
                GameObject buttonObj = colPanel.GetChild(j).gameObject;
                InventoryButton button = buttonObj.GetComponent<InventoryButton>();
                int index = (6 * i) + j;
                buttons.Add(button);
                button.InitButton(index);
            }
        }
        UpdateMenu();
    }
}
