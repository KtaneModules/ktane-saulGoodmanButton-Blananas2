using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class saulGoodmanButtonScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMRuleSeedable RuleSeedable;
    private MonoRandom RS;

    public KMSelectable Saul;
    public GameObject Goodman;
    public GameObject[] Dice;
    public GameObject AllDice;
    public Material Fring;

    bool Gustavo = false;
    int currentTime = -1;
    int buttonTime = -1;
    int[] values = { 6, 5, 4, 3, 2, 1 };
    int chosenSlot = -1;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    bool held = false;

    void Awake () {
        moduleId = moduleIdCounter++;
        Saul.OnInteract += delegate () { held = true; buttonTime = (int)Bomb.GetTime(); Audio.PlaySoundAtTransform(Gustavo ? "GUS" : "SAUL", transform); return false; };
        Saul.OnInteractEnded += delegate () { if (held) { held = false; Audio.PlaySoundAtTransform(Gustavo ? "TAVO" : "GOODMAN", transform); } };
        RS = RuleSeedable.GetRNG();
        Gustavo = (RS.Next(0, 2) == 1);
        if (Gustavo) {
            AllDice.transform.localPosition = new Vector3(0.005f, 0f, -0.12f);
            AllDice.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Goodman.GetComponent<MeshRenderer>().material = Fring;
        }
    }
	
	// Update is called once per frame
	void Update () {
        Goodman.transform.localPosition = new Vector3(0f, (held) ? 0f : 0.015f, 0.02f);
        if (moduleSolved)
            return;
        currentTime = (int)Bomb.GetTime();
        if (currentTime % 30 == 0) {
            chosenSlot = Rnd.Range(0,30);
        }
        if (held && buttonTime != currentTime) {
            buttonTime = (int)Bomb.GetTime();
            SetDice();
        }
	}

    void SetDice () {
        values = values.Shuffle();
        float rx = 0; float ry = 0; float rz = 0; float rng = 0;
        for (int d = 0; d < 6; d++) {
            if (currentTime % 30 == chosenSlot) {
                values[d] = d + 1;
            }
            rng = Rnd.Range(0,4) * 90f;
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

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} hold [Holds the button] | !{0} release [Releases the button]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (command.EqualsIgnoreCase("hold"))
        {
            yield return null;
            Saul.OnInteract();
        }
        else if (command.EqualsIgnoreCase("release"))
        {
            yield return null;
            Saul.OnInteractEnded();
            if (Sorted()) yield return "solve";
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (held || (!Sorted() && !held))
        {
            if (!held)
                Saul.OnInteract();
            while (!Sorted()) yield return true;
            Saul.OnInteractEnded();
        }
        while (!moduleSolved) yield return true;
    }
}
