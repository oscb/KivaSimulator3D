using UnityEngine;
using System.Collections;

public class Kiva : MonoBehaviour {

	private ArrayList path;
	private Vector3 start_point;
	private Vector3 next_point;
	private Vector3 final_point;
	private string rackTag = "TileRack";
	private string packagerTag = "TilePackager";
	private Vector3 return_position;

	public enum Status {Ready, Moving, Waiting};
	public enum Objectives {Rack, Packager, Return};
	public float speed = 1.0F;
	public Status cur_status;
	public Objectives cur_objective = Objectives.Rack;
	public GameObject rack;

	private ArrayList CalculateRoute(Vector3 where) {
		// TODO: If the kiva is not exactly on place it may not return a good result, 
		// better to set start position checking wich tiles is directly below it (raycast) and getting its position instead
		int[] start = new int[2] {(int) this.transform.position.x, (int) this.transform.position.z};
		int[] end = new int[2]{(int) where.x, (int) where.z};

		return Camera.main.GetComponent<RoutePlanner> ().GenLinear (start[0], start[1], end[0], end[1]);
	}

	private Vector3 ConvertPointToVector3(int[] point) {
		return new Vector3 (point [0], this.transform.position.y, point [1]);
	}

	private Vector3 EqualizePointsHeight(Vector3 destination) {
		return ConvertPointToVector3 (new int[2] {(int) destination.x, (int) destination.z});
	}

	private Vector3 FindObjective() {
		// If no holding rack, select a random rack as objective
		// If holding rack go to random packager
		// If holding rack and went already to packager return rack
		Vector3 objective = Vector3.zero;
		
		switch (this.cur_objective) {
			case Objectives.Rack:
				Debug.Log("Rack");
				this.rack = FindNextRack();
				if (this.rack) {
					this.return_position = this.rack.transform.position;
					objective = EqualizePointsHeight(this.return_position);
				} else {
					Debug.Log("ERROR: No se encuentra ningún rack disponible");
				}
				break;
			case Objectives.Packager:
				Debug.Log("Packager");
				GameObject packager = FindPackager();
				if (packager != null) {
					objective = EqualizePointsHeight(packager.transform.position);
				}
				break;
			case Objectives.Return:
				Debug.Log ("Return");
				if (this.return_position != null) {
					objective = EqualizePointsHeight(this.return_position);
				}
				break;
		}
		
		return objective;
	}

	private GameObject FindNextRack() {
		GameObject target = null;
		GameObject[] tiles = GameObject.FindGameObjectsWithTag (rackTag);

		while (target == null && tiles.Length > 0) {
			int i = (int)Random.Range (0, tiles.Length);
			RackData rd = tiles [i].GetComponent<RackData> ();
			if (rd.selected == false) {
				rd.selected = true;
				target = tiles[i];
			}
		}
		return target;
	}

	private GameObject FindPackager() {
		GameObject target;
		GameObject[] tiles = GameObject.FindGameObjectsWithTag (packagerTag);

		if (tiles.Length > 0) {
			int i = (int)Random.Range (0, tiles.Length);
			return tiles[i];
		}
		return null;
	}


	// Use this for initialization
	private void Start () {
		this.cur_status = Status.Ready;
		this.cur_objective = Objectives.Rack;
		this.start_point = this.transform.position;
	}
	
	// Update is called once per frame
	private void Update () {

		if (SimulatorControls.GetState() == 0) {

			if (this.path == null) {
				// Set the finalpoint and set the path
				this.final_point = FindObjective();
				this.path = this.CalculateRoute(this.final_point);
			}

			switch (cur_status) {
				case Status.Ready:
					
					if (this.path != null) {
						int min_index = -1;
						float distance = 99999999;
						for (int i = 0; i < this.path.Count; i++) {
							float d = Vector3.Distance (this.transform.position, new Vector3 ((this.path[i] as int[])[0], 
						                                                                  this.transform.position.y, 
						                                                                  (this.path[i] as int[])[1]));
							if (d < distance) {
								distance = d;
								min_index = i;
							}
						}
						if (min_index != -1) {
							next_point = new Vector3 ((this.path[min_index] as int[])[0], this.transform.position.y, (this.path[min_index] as int[])[1]);
							this.path.RemoveAt(min_index);
							cur_status = Status.Moving;
						} else {
							Debug.Break();
						}
					} else {
						if (transform.position == this.final_point) {
							Debug.Log(this.cur_objective);
							Debug.Break();
						} else {
							Debug.Log("ERROR: Imposible llegar al destino");
						}
					}
					break;

				case Status.Moving:
					Debug.DrawLine(transform.position, next_point, Color.green);
				          
					float step = speed * Time.deltaTime;
					transform.position = Vector3.MoveTowards(transform.position, next_point, step);
					if (next_point != transform.position) {
						Quaternion rotation = Quaternion.LookRotation(next_point - transform.position);
			          transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step * 500);
					}
				                                           
					if ( transform.position == next_point) {
						this.cur_status = Status.Ready;
						this.start_point = this.transform.position;
					}
					// TODO: Detect raycast if collission, stop and wait
					break;

				case Status.Waiting:
					break;

				default:
					break;
			}
		}
	}


}
