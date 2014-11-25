using UnityEngine;
using System;
using System.Collections;

public class Kiva : MonoBehaviour {

	private ArrayList path;
	private Vector3 start_point;
	private Vector3 next_point;
	private Vector3 final_point;
	private string rackTag = "TileRack";
	private string packagerTag = "TilePackager";
	private Vector3 return_position;
	private float start_time;

	public enum Status {Ready, Moving, Waiting};
	public enum Objectives {Rack, Packager, Return};
	public float speed = 1.0F;
	public float sleep_time = 3.0F;
	public Status cur_status;
	public Objectives cur_objective = Objectives.Rack;
	public GameObject rack;

	private ArrayList CalculateRoute(Vector3 where, Vector3[] blockPositions = null ) {
		Transform t = GetCurrentTile();
		int[] start = new int[2] {(int)t.position.x, (int)t.position.z};
		int[] end = new int[2]{(int) where.x, (int) where.z};
		ArrayList blocked = null;
		
		if (blockPositions != null && blockPositions.Length > 0) {
			blocked = new ArrayList();
			foreach (var item in blockPositions) {
				blocked.Add(new int[2] {(int)item.x, (int)item.z});
			}
		}
		
		return Camera.main.GetComponent<RoutePlanner> ().GenLinear (start[0], start[1], end[0], end[1], 
			this.cur_objective == Objectives.Packager || this.cur_objective == Objectives.Return, 
            this.cur_objective == Objectives.Return, 
            blocked);
	}

	private Vector3 ConvertPointToVector3(int[] point) {
		return new Vector3 (point [0], this.transform.position.y, point [1]);
	}

	private Vector3 EqualizePointsHeight(Vector3 destination) {
		return ConvertPointToVector3 (new int[2] {(int) destination.x, (int) destination.z});
	}

	private bool FindObjective() {
		// If no holding rack, select a random rack as objective
		// If holding rack go to random packager
		// If holding rack and went already to packager return rack
		
		switch (this.cur_objective) {
			case Objectives.Rack:
				this.rack = FindNextRack();
				if (this.rack) {
					this.return_position = this.rack.transform.position;
					this.final_point = EqualizePointsHeight(this.return_position);
					return true;
				} else {
					Debug.Log("ERROR: No se encuentra ningún rack disponible");
				}
				break;
			case Objectives.Packager:
				GameObject packager = FindPackager();
				if (packager != null) {
					this.final_point = EqualizePointsHeight(packager.transform.position);
					return true;
				}
				break;
			case Objectives.Return:
				if (this.return_position != null) {
					this.final_point = EqualizePointsHeight(this.return_position);
					return true;
				}
				break;
		}
		
		return false;
	}

	private GameObject FindNextRack() {
		GameObject target = null;
		ArrayList tiles = new ArrayList();
		tiles.AddRange(GameObject.FindGameObjectsWithTag (rackTag));
		

		while (target == null && tiles.Count > 0) {
			int i = (int) UnityEngine.Random.Range (0, tiles.Count);
			RackData rd = ((GameObject) tiles[i]).GetComponent<RackData> ();
			if (rd.selected == false) {
				rd.selected = true;
				target = (GameObject) tiles[i];
			}
			tiles.RemoveAt(i);
		}
		return target;
	}

	private GameObject FindPackager() {
		GameObject[] tiles = GameObject.FindGameObjectsWithTag (packagerTag);

		if (tiles.Length > 0) {
			int i = (int) UnityEngine.Random.Range (0, tiles.Length);
			return tiles[i];
		}
		return null;
	}
	
	private Transform GetChildWithTag(Transform p, string tag) {
		foreach(Transform child in p){
			if(child.CompareTag(tag))
				return child;
		}
		return null;
	}
	
	private void Start () {
		this.cur_status = Status.Ready;
		this.cur_objective = Objectives.Rack;
		this.start_point = this.transform.position;
		this.transform.GetChild(0).animation.Stop();
	}
	
	private void Update () {
		Transform r;

		if (SimulatorControls.GetState() == 0) {
			this.transform.GetChild(0).animation.Play();
		
			float step = speed * Time.deltaTime;
			if (next_point != transform.position) {
				Quaternion rotation = Quaternion.LookRotation(next_point - transform.position);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, step * 750);
			}
			
			
			if (this.path == null) {
				// Set the finalpoint and set the path
				if (FindObjective()) {
					this.path = this.CalculateRoute(this.final_point);
				} else {
					this.cur_status = Status.Waiting;
					this.start_time = Time.time;
				}
				
			}

			switch (cur_status) {
				case Status.Ready:
					
					if (this.path != null && this.path.Count > 0) {
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
						}
					} else {
						if (transform.position == this.final_point) {
//							Debug.Log(this.cur_objective);
							
							switch (this.cur_objective) {
								case Objectives.Rack:
									r = GetChildWithTag(this.rack.transform, "Racks");
									if (r) {
										r.parent = this.transform;
									}
									this.cur_objective = Objectives.Packager;
									break;
								case Objectives.Packager:
									this.cur_objective = Objectives.Return;
									break;
								case Objectives.Return:
									r = GetChildWithTag(this.transform, "Racks");		
									if (r) {
										r.parent = this.rack.transform;
									}
									this.rack.GetComponent<RackData> ().selected = false;
									this.rack = null;
									this.cur_objective = Objectives.Rack;
									break;
								default:
									break;
							}
							this.path = null;
						} else {
							Debug.Log("ERROR: Imposible llegar al destino");
						}
					}
					break;

				case Status.Moving:

					RaycastHit hit;
					int m = 1 << 11;
					Vector3 aux = this.transform.position;
					aux.y += 0.25f;
					
//					Debug.DrawRay(aux, this.transform.forward, Color.blue);
//					Debug.DrawRay(aux, this.transform.forward + this.transform.right, Color.green);
				
					if (Physics.Raycast(aux, this.transform.forward, out hit, 0.5f, m)) {
						if (hit.collider.transform.forward == -this.transform.forward) {
							// Head to Head Collission, Recalculate Route
							// TODO: Set order with Kiva Packager > Return > Rack
							Objectives other_obj = hit.collider.gameObject.GetComponent<Kiva>().cur_objective;
							if (!(other_obj == Objectives.Packager && this.cur_objective == Objectives.Packager)) {
								if ((this.cur_objective == Objectives.Return && other_obj != Objectives.Rack) || 
									this.cur_objective == Objectives.Rack) {
								
									Transform other = hit.collider.gameObject.GetComponent<Kiva>().GetCurrentTile();
									this.path = this.CalculateRoute(this.final_point, new Vector3[1] {other.position});
									this.cur_status = Status.Ready;
									return;
									
								} else {
									this.cur_status = Status.Waiting;
									this.start_time = Time.time;
									return;
									
								}
							}
						} else if (hit.collider.transform.forward != this.transform.forward) {
							// Head to side, wait
							this.cur_status = Status.Waiting;
							this.start_time = Time.time;	
							return;
						}
					}
					
					if (Physics.Raycast(aux, this.transform.forward + this.transform.right, out hit, 0.5f, m) ) {
						if (this.transform.right == hit.collider.transform.forward || -this.transform.right == hit.collider.transform.forward) {
							this.cur_status = Status.Waiting;
							this.start_time = Time.time;
							return;
						}	 
					}
					
					transform.position = Vector3.MoveTowards(transform.position, next_point, step);
				                                           
					if ( transform.position == next_point) {
						this.cur_status = Status.Ready;
						this.start_point = this.transform.position;
					}
					break;

				case Status.Waiting:
					this.transform.GetChild(0).animation.Stop();
					if (Time.time - this.start_time >= this.sleep_time) {
						this.cur_status = Status.Moving;
					}
					break;

				default:
					break;
			}
		} else {
			this.transform.GetChild(0).animation.Stop();
		}
	}

	public Transform GetCurrentTile() {
		Vector3 aux = this.transform.position;
		aux.y += 0.25f;
		
//		Debug.DrawRay(aux, -this.transform.up, Color.yellow);
		RaycastHit rh = new RaycastHit();
		if (Physics.Raycast(aux, -this.transform.up, out rh)) {
			return rh.collider.transform;
		}
		return null;
	}

}
