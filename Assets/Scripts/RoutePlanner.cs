using UnityEngine;
using System.Collections;
using lpsolve55;

public class RoutePlanner : MonoBehaviour {

	private int numVars = 0;
	private int[,] adj_matrix; 
	private int rows = 0;
	private int cols = 0;

	public Material auxMaterial;

	private int[] BuildIntArray(ArrayList l) {
		int[] x = new int[l.Count];
		int i = 0;
		foreach (var item in l) {
			x[i++] = (int)item;
		}
		return x;
	}

	private double[] BuildDoubleArray(ArrayList l) {
		double[] x = new double[l.Count];
		int i = 0;
		foreach (var item in l) {
			x[i++] = (double) item;
		}
		return x;
	}

	private ArrayList FindAdjacencies(int i, int j, int rows, int cols) {
		ArrayList adjacencies = new ArrayList ();
		// TODO: (optional) Could be reduced
		if (i-1 >= 0) adjacencies.Add(new int[2] {i-1, j}); 	// Left
		if (i+1 < rows) adjacencies.Add(new int[2] {i+1, j}); 	// Right
		if (j-1 >= 0) adjacencies.Add(new int[2] {i, j-1});		// Bottom
		if (j+1 < cols) adjacencies.Add(new int[2] {i, j+1}); 	// Top
		return adjacencies;
	}

	private int CoordinateToFlat(int i, int j, int n) {
		return i * n + j;
	}

	private int[] FlatToCoordinate(int f, int n) {
		return new int[2]{ (int)f/n, f%n };
	}

//	private void OnGUI() {
//		GUIStyle boxStyle = GUI.skin.GetStyle ("Box");
//		boxStyle.fontSize = 12;
//		boxStyle.alignment = TextAnchor.UpperLeft;
//		if (this.adj_matrix != null) {
//			int n = this.numVars;
//
//			string adjs = "";
//			adjs += "\t\t\t";
//			for (int i = 0; i < n; i++) {
//				adjs += i + "\t";
//			}
//			adjs += "\n";
//			for (int i = 0; i < n; i++) {
//				adjs += i + "\t|\t";
//				for (int j = 0; j < n; j++) {
//					adjs += "\t" + this.adj_matrix[i, j];
//				}
//				adjs += "\n";
//			}
//			GUI.Box(new Rect(0,0, 250, 200), adjs);
//		}
//	}

	public void InitMatrix(GameObject[,] tiles) {
		this.rows = tiles.GetLength (0);
		this.cols = tiles.GetLength (1);
		// 1. Init the adj Table with cols and rows
		this.numVars = this.rows * this.cols;
		adj_matrix = new int[this.numVars, this.numVars];

		// 2. Add adjacencies
		for (int i = 0; i < this.rows; i++) {
			for (int j = 0; j < this.cols; j++) {
				ArrayList adj = FindAdjacencies(i, j, this.rows, this.cols);

				int flatpos = CoordinateToFlat(i, j, this.cols);
				foreach (int[] item in adj) {
					string tile_type = tiles[item[0], item[1]].gameObject.GetComponent<TileData>().type.ToString();
					if (tile_type != "Boundary") 
						this.adj_matrix[flatpos, CoordinateToFlat(item[0], item[1], this.cols)] = 1;
				}
				// TODO: Discover how to dinamically change to block paths with Packager when not holding a rack 
				// and Rack when holding rack
			}
		}
//		GenLinear (3, 2, 0,0);
	}

	public ArrayList GenLinear(int start_x, int start_y, int end_x, int end_y) {
		if (this.adj_matrix == null) return null;
		int lp, totalVars, startf, endf;
		double[] obj;
		ArrayList zero_vals, zeros, outs, out_vals, ins, in_vals;
		double[] var_results;
		ArrayList path = new ArrayList();

		// 1. Set LP Solve matrix with 0 rows and ajd_matrix N cols
		// TODO: (optional) Optimize and set only variables used (not full table)
		totalVars = (int)Mathf.Pow (this.numVars, 2);
		var_results = new double[totalVars];
		lp = lpsolve.make_lp(0, totalVars);
		if (lp == 0) return null;

		// 2. Set Name for variables for debugging
		for (int i = 0; i < (int)Mathf.Pow(this.numVars, 2); i++) {
			int x = i / this.numVars;
			int y = i % this.numVars;
//			string var_name = x + ", " + y;
			string var_name = "x" + x + "y" + y;
			lpsolve.set_col_name(lp, i+1, var_name);
		}

		// 3. Set Objective Function (Minimize Sum of all vars)
		obj = new double[totalVars+1];
		for (int i = 0; i < totalVars+1; i++) {
			obj [i] = 1;
		}
		lpsolve.set_obj_fn(lp, obj);

		// 4. Set Constraints:
		lpsolve.set_add_rowmode(lp, true);
		// 4.1. Starting point to X = 1 and End Point to X = 1
		startf = CoordinateToFlat (start_x, start_y, this.cols);
		endf = CoordinateToFlat (end_x, end_y, this.cols);

		// 4.2. ins - outs = 0
		zero_vals = new ArrayList();
		zeros = new ArrayList();

		for (int i = 0; i < this.numVars; i++) {
			outs = new ArrayList();
			out_vals = new ArrayList();
			ins = new ArrayList();
			in_vals = new ArrayList();

			// TODO: Check where is Start and End
			for (int j = 0; j < this.numVars; j++) {
//				Debug.Log("(" + i + ", " + j + ")" + this.adj_matrix[i, j] + "|" + this.adj_matrix[j, i]);
				if (this.adj_matrix[i, j] == 1) {
					outs.Add(CoordinateToFlat(i, j, this.numVars) + 1);
					out_vals.Add(1.0d);

					if (this.adj_matrix[j, i] == 1) {
						ins.Add(CoordinateToFlat(j, i, this.numVars) + 1);
						in_vals.Add(-1.0d);
					}

				} else {
					zeros.Add (CoordinateToFlat(i, j, this.numVars) + 1);
					zero_vals.Add(1.0d);
				}
			}

			if (i == startf) {
				lpsolve.add_constraintex (lp, 
				                          outs.Count, 
				                          BuildDoubleArray(out_vals), 
				                          BuildIntArray(outs), 
				                          lpsolve.lpsolve_constr_types.EQ, 
				                          1);
				lpsolve.add_constraintex (lp, 
				                          outs.Count, 
				                          BuildDoubleArray(in_vals), 
				                          BuildIntArray(ins), 
				                          lpsolve.lpsolve_constr_types.EQ, 
				                          0);
			} else if (i == endf) {
				lpsolve.add_constraintex (lp, 
				                          outs.Count, 
				                          BuildDoubleArray(out_vals), 
				                          BuildIntArray(outs), 
				                          lpsolve.lpsolve_constr_types.EQ, 
				                          0);
				lpsolve.add_constraintex (lp, 
				                          outs.Count, 
				                          BuildDoubleArray(in_vals), 
				                          BuildIntArray(ins), 
				                          lpsolve.lpsolve_constr_types.EQ, 
				                          -1);
			} else {
				out_vals.AddRange(in_vals);
				outs.AddRange(ins);
				lpsolve.add_constraintex (lp, 
				                          outs.Count, 
				                          BuildDoubleArray(out_vals), 
				                          BuildIntArray(outs), 
				                          lpsolve.lpsolve_constr_types.EQ, 
				                          0);
			}
		}

		// 4.3. Block all imposible ins/outs (based on table)
		lpsolve.add_constraintex (lp, 
		                          zeros.Count, 
		                          BuildDoubleArray(zero_vals), 
		                          BuildIntArray(zeros), 
		                          lpsolve.lpsolve_constr_types.EQ, 
		                          0);

		// 4.4. Block ins to Packager if returning a rack (optional)
		// 4.5. Block ins to Racks if holding a Rack
		// 4.6. Block ins to a Kiva position (if path blocked)

		lpsolve.set_add_rowmode(lp, false);

		// 5. Solve.
		lpsolve.lpsolve_return result;
		lpsolve.set_minim (lp);
		lpsolve.write_lp(lp, "model.lp");

		result = lpsolve.solve(lp);
		if (result == lpsolve.lpsolve_return.OPTIMAL) {
//			Debug.Log ("Objective = " + lpsolve.get_objective (lp));
			lpsolve.get_variables(lp, var_results);
			for (int i = 0; i < totalVars; i++) {
				if (var_results[i] > 0) {
//					Debug.Log(lpsolve.get_col_name(lp, i + 1) + ": " + var_results[i]);	
					int[] aux_coord = FlatToCoordinate(i, this.numVars);
//					Debug.Log (aux_coord[0] + ", " + aux_coord[1]);
					aux_coord = FlatToCoordinate(aux_coord[1], this.cols);
//					Debug.Log (aux_coord[0] + ", " + aux_coord[1]);
//					Camera.main.gameObject.GetComponent<TileCreator>().tiles[aux_coord[0], 
//					                                                         aux_coord[1]].transform.FindChild("Plane").renderer.material = auxMaterial;
					path.Add(new int[2] {aux_coord[0], aux_coord[1]});
				}
			}
		} else {
			Debug.Log(result);
			return null;
		}

		lpsolve.delete_lp(lp);

		// 6. Return Path to Kiva
		return path;
	}
}
