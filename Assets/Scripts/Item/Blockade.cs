using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockadeItem : Item
{
    public float duration;
    public BlockadeItem(string argItemName = "Blockade", float argDuration = 5f)
                        : base(argItemName, 1){
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
