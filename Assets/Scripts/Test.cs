using UnityEngine;
using System.Collections;
using lpsolve55;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		lpsolve.Init(".");
		int[] obj = {300, 160, 360, 220, 130, 280, 100, 80, 140};
		int[,] restr = {{1, 0, 0, 1, 0, 0, 1, 0, 0},
						{0, 1, 0, 0, 1, 0, 0, 1, 0},
						{0, 0, 1, 0, 0, 1, 0, 0, 1},
						{1, 0, 0, 0, 0, 0, 0, 0, 0},
						{0, 1, 0, 0, 0, 0, 0, 0, 0},
						{0, 0, 1, 0, 0, 0, 0, 0, 0},
						{0, 0, 0, 1, 0, 0, 0, 0, 0},
						{0, 0, 0, 0, 1, 0, 0, 0, 0},
						{0, 0, 0, 0, 0, 1, 0, 0, 0},
						{0, 0, 0, 0, 0, 0, 1, 0, 0},
						{0, 0, 0, 0, 0, 0, 0, 1, 0},
						{0, 0, 0, 0, 0, 0, 0, 0, 1}};
		int[] res = {30, 30, 30, 4, 8, 3, 8, 13, 10, 22, 20, 18};

		int lp = lpsolve.make_lp (12, 9);
		Debug.Log (lp);

	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
