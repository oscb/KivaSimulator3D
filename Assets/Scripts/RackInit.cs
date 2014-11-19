using UnityEngine;
using System.Collections;

public class RackInit : MonoBehaviour {

	public GameObject[] rackMeshes;
	
	void Start () {
		GameObject rack = rackMeshes [Random.Range (0, rackMeshes.Length)];
		GameObject goRack = (GameObject) GameObject.Instantiate (rack);
		goRack.transform.parent = this.transform;
		goRack.transform.localPosition = new Vector3(0.0f, 0.7f, 0.0f);
	}
}
