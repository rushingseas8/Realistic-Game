using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Main : MonoBehaviour {
	public static int xStart;
	public static int yStart;

	// Use this for initialization
	void Start () {
		//Actual random seed
		//int xStart = Random.Range (-10000, 10000);
		//int yStart = Random.Range (-10000, 10000);

		//Fixed random seed (for testing)
		xStart = 6592;
		yStart = -192;

		Generator.generateLand (xStart, yStart);


		/*
		for (int i = 0; i < 10; i++) {
			GameObject newTree = Instantiate(Resources.Load ("Tree 1")) as GameObject;
			newTree.transform.position = new Vector3 (Random.Range (-10, 10), 0, Random.Range (-10, 10));
		}*/
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
