using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour {

    public Item item;
    public int index = 0;
    private Image image;
    private Text text;
    private Button button;
    public void InitButton(int indexArg)
    {
        index = indexArg;
        image = gameObject.GetComponent<Image>();
        text = transform.GetChild(0).gameObject.GetComponent<Text>();
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(this.UseItem);
    }

    public void UseItem()
    {
        Debug.Log("UseItem() invoked!");
        item.OnUse();
        if(item.consumable){
            GameController.GetInstanceInventoryController().RemoveItem(item);
        }
    }

    public void UpdateItem(Item newItem)
    {
        if (newItem == null) {
            EnableButton(false);
            return;
        }
        else {
            EnableButton(true);
        }
        item = newItem;
        text.text = item.name;
    }

    void EnableButton(bool val)
    {
        button.enabled = val;
        text.enabled = val;
        image.enabled = val;
    }

    

}
