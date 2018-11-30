using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeScreenItem : Item
{
    public int radius;
    public float duration;
    public SmokeScreenItem(string argItemName = "SmokeScreen", int argRadius = 5, float argDuration = 5f)
        : base(argItemName, 1)
    {
        radius = argRadius;
        duration = argDuration;
    }

    public override void OnUse()
    {
        GameObject blankSS = Resources.Load<GameObject>("SmokeScreen");
        base.OnUse();
        Vector3 pos = GameController.GetInstance().GetPlayer().transform.position;
        GameObject ssobj = GameController.Instantiate(blankSS, pos, Quaternion.identity);
        GameController.GetInstance().GetInvCtl().RemoveItem(this);
        // TODO remove this item from the inventory after use
    }
}

public class SmokeScreen : MonoBehaviour {

    private static float time = 5f;

	void Start () {
        StartCoroutine("Countdown");
	}
	IEnumerator Countdown()
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
        yield break;
    }
	
}
