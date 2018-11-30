using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockadeItem : Item
{
    public float duration;
    public float length;
    public float width;
    public BlockadeItem(string argItemName = "Blockade", float argWidth = 1f,
                       float argLength = 1f)
                        : base(argItemName, 1){
        length = argLength;
        width = argWidth;
    }
    public override void OnUse()
    {
        base.OnUse();
        GameObject obj= Resources.Load<GameObject>("Blockade");
        Vector3 pos = GameController.GetInstance().GetPlayer().transform.position;
        GameObject blockObj = GameController.Instantiate(obj, pos, Quaternion.identity);
        GameController.GetInstance().GetInvCtl().RemoveItem(this);
        blockObj.transform.localScale = new Vector3(width, 1f, length);

    }
}

public class Blockade : MonoBehaviour
{
    private static float time = 10f;

    void Start()
    {
        StartCoroutine("Countdown");
    }
    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
        yield break;
    }

}
