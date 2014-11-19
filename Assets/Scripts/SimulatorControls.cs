using UnityEngine;
using System.Collections;

public class SimulatorControls : MonoBehaviour {
	
	private int columnSize = 70;
	private int marginSize = 10;
	private int toolbarHeight = 100;
	private string[] states = new string[] {"►", "II"};
	private string[] tileTypes = new string[] {"Empty", "Block", "Packager", "Rack", "Kiva"};
	private int state = 1; // Pause initially
	private int selectedType = 0;

	public float maxRows = 20.0f;
	public float maxCols = 20.0f;
//	public float nKivas = 1.0f;
	public float nRows = 4.0f;
	public float nCols = 4.0f;
	public int currentState = 1;


	private GameObject FindParentWithTag(GameObject go, string tag) {
		if (go.tag == tag) {
			return go;
		}
		if (go.transform.parent != null) {
			return FindParentWithTag (go.transform.parent.gameObject, tag);
		}
		return null;
	}

	private void Play () {
		// Play pressed
		Debug.Log ("Play");
	}

	private void Stop () {
		Debug.Log ("Stop");
	}

	// GUI Controls
	void OnGUI () {
		GUIStyle labelStyle = GUI.skin.GetStyle ("Label");
		GUIStyle boxStyle = GUI.skin.GetStyle ("Box");
		GUIStyle buttonStyle = GUI.skin.GetStyle ("Button");
		
		labelStyle.alignment = boxStyle.alignment = buttonStyle.alignment = TextAnchor.MiddleCenter;
		boxStyle.fontSize = buttonStyle.fontSize = 32;
		buttonStyle.fontStyle = FontStyle.Bold;
		
		// Controls Left Bottom
		GUILayout.BeginArea (new Rect (marginSize, Screen.height - toolbarHeight - marginSize, 
		                               Screen.width/2 - marginSize, toolbarHeight));
		
		GUILayout.BeginHorizontal ();
		
		GUILayout.BeginVertical (GUILayout.Height (50), GUILayout.Width (columnSize));
		
		if (state != 0) {
			GUILayout.Label ("Rows", labelStyle);
			GUILayout.Box (nRows.ToString ("N0"), boxStyle);
			nRows = GUILayout.HorizontalSlider (nRows, 1.0f, maxRows);
			GUILayout.EndVertical ();
			
			GUILayout.BeginVertical (GUILayout.Height (50), GUILayout.Width (columnSize));
			GUILayout.Label ("Cols", labelStyle);
			GUILayout.Box (nCols.ToString ("N0"), boxStyle);
			nCols = GUILayout.HorizontalSlider (nCols, 1.0f, maxCols);
			GUILayout.EndVertical ();
			
//			GUILayout.BeginVertical (GUILayout.Height (50), GUILayout.Width (columnSize));
//			GUILayout.Label ("Kivas", labelStyle);
//			GUILayout.Box (nKivas.ToString ("N0"), boxStyle);
//			nKivas = GUILayout.HorizontalSlider (nKivas, 1.0f, 10.0f);
//			GUILayout.EndVertical ();
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea ();
		
		// Controls Right Top
		GUILayout.BeginArea (new Rect(Screen.width/2 + marginSize, marginSize,
		                              Screen.width/2 - marginSize * 2, toolbarHeight));
		
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		state = GUILayout.Toolbar(state, states);
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea ();
		
		// Controls Right Bottom
		GUILayout.BeginArea(new Rect(Screen.width/2 + marginSize, Screen.height - toolbarHeight/2, 
		                             Screen.width/2 - marginSize * 2, toolbarHeight/2));
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (state != 0) {
			buttonStyle.fontSize = 16;
			selectedType = GUILayout.SelectionGrid (selectedType, tileTypes, 3);
		}
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea ();	
		
		// Layout Changes 
		if (state != currentState) {
			currentState = state;
			
			// TODO: Use a function array pointer
			if (state == 0) {
				Play ();
			} else if (state == 1) {
				Stop ();
			}
			
		}
	}
	
	void Start () {
		currentState = state;
	}
	
	void Update () {
		
		
		// Check if a tile is selected
		if (Input.GetMouseButtonDown (0)) {
			RaycastHit hit = new RaycastHit();
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				GameObject selectedTile = FindParentWithTag(hit.collider.gameObject, "Tiles");
				if (selectedTile != null) {
					// Change Tile Type
					Vector3 coordinates = selectedTile.transform.position;
					// TODO: Not a good idea to use x, z as the coordinates for the array if 3D sizes change
					this.gameObject.GetComponent<TileCreator>().changeTile((int)coordinates.x, 
					                                                       (int)coordinates.z, 
					                                                       selectedType);
				}

			}
		}
		
	}
}
