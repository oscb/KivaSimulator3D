using UnityEngine;
using System;
using System.Collections;

public class TileCreator : MonoBehaviour {
	
	private int rows;
	private int cols;
	private SimulatorControls settings;

	public GameObject floor;
	public GameObject[,] tiles;
	public GameObject[] tileTypes;
	public GameObject emptyTile;
	public Vector3 camInitialPos;

	public void ChangeTile(int x, int y, int type) {
		Vector3 pos = this.tiles [x, y].transform.position;
		Destroy (this.tiles [x, y]);
		this.tiles [x, y] = (GameObject)GameObject.Instantiate (this.tileTypes [type]);
		this.tiles [x, y].transform.position = pos;
		this.tiles [x, y].transform.parent = floor.transform;
		// TODO: Check that there's only one packager and max 3 kivas
	}

	public GameObject[,] GetTiles() {
		return this.tiles;
	}

	private void GenerateTiles() {
		if (tiles != null) {
			foreach (var item in tiles) {
				Destroy (item);
			}
		}
		this.tiles = new GameObject[this.rows, this.cols];
		for (int i = 0; i < this.rows; i++) {
			for (int j = 0; j < this.cols; j++) {
				this.tiles[i, j] = (GameObject) GameObject.Instantiate(emptyTile);
				this.tiles[i, j].transform.position = new Vector3((float) i, 0.0f, (float) j);
				this.tiles[i, j].transform.parent = floor.transform;
			}
		}
		PlaceCam ();
	}

	private void PlaceCam() {
//		this.gameObject.transform.position = Vector3.Lerp(camInitialPos, 
//		                                                  new Vector3(this.rows-1, camInitialPos.y, this.cols-1), 
//		                                                  0.5f);
//		this.gameObject.camera.orthographicSize = Mathf.Max (this.rows, this.cols) / 2.5f;
	}

	private void Start () {
		settings = this.gameObject.GetComponent <SimulatorControls>();
		this.rows = (int) settings.nRows;
		this.cols = (int) settings.nCols;
		GenerateTiles ();
	}

	private void Update () {
		if ((int)settings.nCols != this.cols || (int)settings.nRows != this.rows) {
			this.rows = (int) settings.nRows;
			this.cols = (int) settings.nCols;
			GenerateTiles ();
		}
	}

}
