using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A small helper class that tags a bit of extra data with each chunk's mesh's gameobject.
 * This allows raycasting from the player's camera to get back information about where
 * in the world they just looked at, by checking mesh -> gameobject -> chunk data -> coords.
 */
public class ChunkData : MonoBehaviour {
	public int worldX, worldY;

	public ChunkData(int x, int y) {
		this.worldX = x;
		this.worldY = y;
	}
}
