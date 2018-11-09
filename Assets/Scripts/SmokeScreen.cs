using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
