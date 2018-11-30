using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour {

    private Color originalColor;
    public Color dangerColor;
    public int flickerDuration;
    private bool flickered = false;
    private Light lights;
	// Use this for initialization
	void Start () {
        lights = GetComponent<Light>();
        originalColor = lights.color;
	}
	
	// Update is called once per frame
	void Update () {
        //lights only flicker once per round when the murder occurs
		if(!flickered && GameController.GetInstanceTimeController().GetMurderTime() > -1)
        {
            flickered = true;
            StartCoroutine(FlickerLights());
        }
	}

    IEnumerator FlickerLights()
    {
        lights.color = dangerColor;
        lights.intensity = 0;
        for (int i = 0; i < flickerDuration; i++)
        {
            yield return new WaitForSeconds(1);
            lights.intensity = 1;
            yield return new WaitForSeconds(1);
            lights.intensity = 0;
        }
        lights.color = originalColor;
        lights.intensity = 1;
    }
}
