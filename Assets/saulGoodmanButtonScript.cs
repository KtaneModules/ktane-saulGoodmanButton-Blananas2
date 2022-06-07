using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class saulGoodmanButtonScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable Saul;
    public GameObject Goodman;
    public GameObject[] Dice;

    int currentTime = -1;
    int buttonTime = -1;
    int[] values = { -1, -1, -1, -1, -1, -1 };

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    bool held = false;

    void Awake () {
        moduleId = moduleIdCounter++;

        Saul.OnInteract += delegate () { held = true; buttonTime = (int)Bomb.GetTime(); Audio.PlaySoundAtTransform("SAUL", transform); return false; };
        Saul.OnInteractEnded += delegate () { held = false; Audio.PlaySoundAtTransform("GOODMAN", transform); };
        
    }
	
	// Update is called once per frame
	void Update () {
        Goodman.transform.localPosition = new Vector3(0f, (held) ? 0f : 0.015f, 0.02f);
        if (moduleSolved)
            return;
        currentTime = (int)Bomb.GetTime();
        if (held && buttonTime > currentTime) {
            buttonTime = (int)Bomb.GetTime();
            SetDice();
        }
	}

    void SetDice () {
        float rx = 0; float ry = 0; float rz = 0; float rng = 0;
        for (int d = 0; d < 6; d++) {
            values[d] = UnityEngine.Random.Range(1,7);
            rng = UnityEngine.Random.Range(0,4) * 90f;
            switch (values[d]) {
                case 1: rx = 0f; ry = rng; rz = 0f; break;
                case 2: rx = 90f; ry = 0f; rz = rng; break;
                case 3: rx = 0f; ry = rng; rz = 90f; break;
                case 4: rx = 0f; ry = rng; rz = -90f; break;
                case 5: rx = -90f; ry = 0f; rz = rng; break;
                case 6: rx = -180f; ry = rng; rz = 0f; break;
            }
            Dice[d].transform.localRotation = Quaternion.Euler(rx, ry, rz);
        }
        if (Sorted()) {
            StartCoroutine(Confirm());
        }
    }

    bool Sorted () {
        for (int r = 0; r < 5; r++) {
            if (!(values[r] <= values[r+1])) {
                return false;
            }
        }
        return true;
    }

    IEnumerator Confirm () {
        yield return new WaitForSeconds(1.5f);
        if (!held && Sorted()) {
            moduleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[Saul Goodman Button #{0}] You have done well, lad.", moduleId);
        } else {
            Debug.LogFormat("[Saul Goodman Button #{0}] You have done poorly, lad.", moduleId);
        }
    }
}
