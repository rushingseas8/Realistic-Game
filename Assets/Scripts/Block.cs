using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType {
	GRASS = 1,
	SAND = 2
}

public class Block {

	public int x, y, z;
	public float volume;
	public BlockType blockID;

	private static Hashtable blocks = new Hashtable();

	public Block() {}

	public Block(int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public override string ToString() {
		return this.blockID + " block @ (" + this.x + ", " + this.y + ", " + this.z + ")";
	}

	// End instance-specific methods

	public static Block add(Block data) {
		string key = hashcode (data.x, data.y, data.z);
		blocks.Add (key, data);

		return data;
	}

	public static Block add(float x, float y, float z) {
		string key = hashcode ((int)x, (int)y, (int)z);
		Block block = new Block ((int)x, (int)y, (int)z);
		blocks.Add (key, block);

		return block;
	}

	public static List<Block> get(float x, float y) {
		int _x = (int)x;
		int _y = (int)y;
		List<Block> toReturn = new List<Block> ();
		foreach (DictionaryEntry pair in blocks) {
			Block b = (Block)(pair.Value);
			if (b.x == _x && b.y == _y) {
				toReturn.Add (b);
			}
		}
		return toReturn;
	}

	public static Block get(float x, float y, float z) {
		string key = hashcode ((int)x, (int)y, (int)z);
		if (blocks.Contains (key)) {
			return (Block)blocks [key];
		} else {
			return null;
		}
	}

	public static int count() {
		return blocks.Count;
	}

	private static string hashcode(int x, int y, int z) {
		return "x" + x + "y" + y + "z" + z;
	}
}

