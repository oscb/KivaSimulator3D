using UnityEngine;
using System.Collections;

public class TileData : MonoBehaviour {
	
	public enum TileType { Empty, Rack, Boundary, Kiva, Packager };
	public TileType type = TileType.Empty;
}
