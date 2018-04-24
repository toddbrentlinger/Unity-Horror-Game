using UnityEngine;
using System.Collections;

// Scare Base Class

public class Scare : MonoBehaviour {

    private static Camera mainCam;
    private static Transform player;

	// Use this for initialization
	void Start () {
        mainCam = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
