using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Generator : MonoBehaviour {

	public static Mesh[,] meshGrid;

	public static int meshDimension = 4;

	public static float sizeScale = 128f;
	public static float heightScale = 100f;

	public static int renderDiameter = 1;
	public static int renderRadius = renderDiameter / 2;

	private static float uvScale = 2f;

	//public static float[] boundaries = new float[]{ 0f, 0.1f, 0.3f, 0.5f };
	//public static float transitionSize = 0.1f;

	public static void generateLand(int xStart, int yStart) {
		//TODO: Have multiple materials/textures and blend between them
		Material material = Resources.Load ("Materials/Combined-Minecraft", typeof(Material)) as Material;
		//material.color = new Color(1.0f, 0.855f, 0.725f);
		//material.color = new Color(0.55f, 0.741f, 0.4f);
		//material.color = new Color(0, 0, 0);

		PhysicMaterial pmat = new PhysicMaterial ();
		pmat.bounciness = 0f;
		pmat.dynamicFriction = 0.4f;
		pmat.staticFriction = 0.7f;

		for (int j = -renderRadius; j <= renderRadius; j++) {
			for (int i = -renderRadius; i <= renderRadius; i++) {
				Mesh mesh = Generator.generateTerrain(xStart + i, yStart + j);
				Vector3 position = new Vector3(i * meshDimension - (meshDimension / 2), 0, j * meshDimension - (meshDimension / 2));
				Generator.createTerrainObject (mesh, xStart + i, yStart + j, position, material, pmat);
			}
		}
	}

	public static Mesh generateTerrain(float worldX, float worldY) {
		//Debug.Log ("Generating terrain for " + worldX + ", " + worldY);

		int width = meshDimension;
		int length = meshDimension;

		//Generating terrain does so on a 1x1 tile. The mesh is 128 meters wide (by default)- this fixes that scale.
		//This also allows meshDimension and sizeScale to be two independent variables.
		//float adjustedScale = sizeScale * (meshDimension / 128.0f) / 128.0f;

		int w1 = width + 1;
		int l1 = length + 1;

		//Generate a heightmap based on 2 passes of Perlin noise. This makes oceans and continents.
		//float[] heightMap = createHeightMap (width, length, worldX, worldY, adjustedScale);

		//Generate a perlin noise map for the actual land itself. 
		//float max = 0;
		float[] perlinMap = new float[w1 * l1];
		for (int i = 0; i < w1; i++) {
			for (int j = 0; j < l1; j++) {
				perlinMap [(i * w1) + j] = getHeightForCoordinate (worldX, worldY, i, j);
				//if (perlinMap [(i * w1) + j] > max) {
				//	max = perlinMap [(i * w1) + j];
				//}
				//Debug.Log(perlinMap [(i * w1) + j]);
			}
		}
		//Debug.Log ("Max for this generation: " + max);

		Block newBlock = null;

		/** 
		 * Based on the generated height of the terrain, add some blocks to the data structure.
		 * 
		 * Specifically, we go through each 2x2 set of generated points (which correspond to the 
		 * four corners of a "cube"), and calculate the volume of said cube with the base snapped 
		 * to the nearest integer height. If this volume is less than a cube, we add one more to 
		 * the base and consider that as the cube. 
		 * 
		 * The height is calculated by avg(points) - (int)(min(points)). Width and length is 1.
		 */
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < length; j++) {
				float p1 = perlinMap [(i * w1) + j];
				float p2 = perlinMap [(i * w1) + j + 1];
				float p3 = perlinMap [((i + 1) * w1) + j];
				float p4 = perlinMap [((i + 1) * w1) + j + 1];

				float avg = (p1 + p2 + p3 + p4) / 4.0f;
				int min = (int)(Mathf.Min (Mathf.Min (p1, p2), Mathf.Min (p3, p4)));
				float volume = avg - min;

				/*
				if (volume < 1.0f) {
					volume += 1.0f;
					min -= 1;
				}*/
				newBlock = Block.add ((int)((worldX * meshDimension) + i), (int)((worldY * meshDimension) + j), min);
				newBlock.volume = volume;
				newBlock.blockID = Random.Range (0, 2) == 0 ? BlockType.GRASS : BlockType.SAND;

				//if (i > width - 10 && j < 10) {
					//Debug.Log (p1 + ", " + p2 + ", " + p3 + ", " + p4);
					//Debug.Log ("Adding block at " + (int)((worldX * meshDimension) + i) + ", " + (int)((worldY * meshDimension) + j) + ", " + min + ". Avg: " + avg);
				//}
			}
		}
		//Debug.Log ("Recording: " + Block.count () + " blocks.");


		int[] iw = new int[width];
		int[] iw1 = new int[w1];

		/* 
		 * TODO: To properly support texturing each tile, we need to have 4 vertices per tile. As of right now,
		 * vertices are shared. Triangle count is unaffected.
		 */

		//Vertex generation
		Profiler.BeginSample("Vertex generation");
		Vector3[] vertices = new Vector3[4 * width * length];
		/*
		for (int i = 0; i < width; i++) {
			iw [i] = i * width;
			iw1[i] = i * w1;
			for (int j = 0; j < length; j++) {
				vertices [(4 * (iw[i] + j)) + 0] = new Vector3 (j, perlinMap[(i * w1) + j], i);
				vertices [(4 * (iw[i] + j)) + 1] = new Vector3 (j + 1, perlinMap[(i * w1) + j + 1], i);
				vertices [(4 * (iw[i] + j)) + 2] = new Vector3 (j, perlinMap[((i + 1) * w1) + j], i + 1);
				vertices [(4 * (iw[i] + j)) + 3] = new Vector3 (j + 1, perlinMap[((i + 1) * w1) + j + 1], i + 1);
			}
		}*/

		Vector3[,,] verticesUnpacked = new Vector3[width, length, 4];
		for (int i = 0; i < width; i++) {
			iw [i] = i * width;
			iw1[i] = i * w1;
			for (int j = 0; j < length; j++) {
				verticesUnpacked[i,j,0] = new Vector3(j, perlinMap[(i * w1) + j], i);
				verticesUnpacked[i,j,1] = new Vector3 (j + 1, perlinMap[(i * w1) + j + 1], i);
				verticesUnpacked[i,j,2] = new Vector3 (j, perlinMap[((i + 1) * w1) + j], i + 1);
				verticesUnpacked[i,j,3] = new Vector3 (j + 1, perlinMap[((i + 1) * w1) + j + 1], i + 1);
			}
		}

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < length; j++) {
				for (int k = 0; k < 4; k++) {
					Debug.Log ("[" + ((4 * ((i * width) + j)) + k) + "]: " + verticesUnpacked [i, j, k]);
					vertices [(4 * ((i * width) + j)) + k] = verticesUnpacked [i, j, k];
				}
			}
		}

		Profiler.EndSample();

		//UV mapping generation
		Profiler.BeginSample("UV generation");
		Vector2[] uvs = new Vector2[4 * width * length];
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < length; j++) {
				//uvs [iw1[i] + j] = new Vector2 ((float)j / uvScale, (float)i / uvScale);

				if (newBlock.blockID == BlockType.GRASS) {
					uvs [(4 * (iw[i] + j)) + 0] = new Vector2 ((float)j / uvScale, (float)i / uvScale);
					uvs [(4 * (iw[i] + j)) + 1] = new Vector2 ((float)(j + 1) / uvScale, (float)i / uvScale);
					uvs [(4 * (iw[i] + j)) + 2] = new Vector2 ((float)j / uvScale, (float)(i + 1) / uvScale);
					uvs [(4 * (iw[i] + j)) + 3] = new Vector2 ((float)(j + 1) / uvScale, (float)(i + 1) / uvScale);
				}
			}
		}
		Profiler.EndSample();

		//Triangle generation
		Profiler.BeginSample("Triangle generation");
		/*
		int[] triangles = new int[(int)(6 * width * length)];
		for (int i = 0; i < width; i++) {
			int i1 = i + 1;
			int i1wd1 = i1 * w1;

			int iwd = i * width;

			for (int j = 0; j < length; j++) {
				int baseIndex = 6 * (iwd + j);

				int iwd1j = iw1[i] + j;
				int i1wd1j = i1wd1 + j;

				triangles [baseIndex + 0] = iwd1j;
				triangles [baseIndex + 1] = i1wd1j + 1;
				triangles [baseIndex + 2] = iwd1j + 1;
				triangles [baseIndex + 3] = iwd1j;
				triangles [baseIndex + 4] = i1wd1j;
				triangles [baseIndex + 5] = i1wd1j + 1;
			}
		}

		for (int i = 0; i < triangles.Length; i++) {
			Debug.Log ("[" + i + "]:" + triangles [i]);
		}
		*/

		int[] triangles = new int[(int)(6 * width * length)];
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < length; j++) {
				int baseIndex = 6 * ((i * width) + j);

				//0, 3, 1
				//0, 2, 3
				triangles[baseIndex + 0] = iw[i] + j + 0;
				triangles[baseIndex + 1] = iw[i] + j + 3;
				triangles[baseIndex + 2] = iw[i] + j + 1;

				triangles[baseIndex + 3] = iw[i] + j + 0;
				triangles[baseIndex + 4] = iw[i] + j + 2;
				triangles[baseIndex + 5] = iw[i] + j + 3;
			}
		}

		Profiler.EndSample ();

		Mesh mesh = new Mesh ();

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;

		mesh.RecalculateNormals ();
		//Manually calculate normals to get rid of seams
		/*
		Vector3[] normals = new Vector3[w1 * l1];
		for (int i = 0; i < w1; i++) {
			for (int j = 0; j < l1; j++) {
				//Get the derivative in the x axis
				float left = -1, right = -1;
				if (j == 0) {
					left = getHeightForCoordinate(worldX, worldY, i, j - 1);
					right = vertices [iw1 [i] + j + 1].y;
				} else if (j == length) {
					left = vertices [iw1 [i] + j - 1].y;
					right = getHeightForCoordinate(worldX, worldY, i, j + 1);
				} else {
					left = vertices [iw1 [i] + j - 1].y;
					right = vertices [iw1 [i] + j + 1].y;
				}

				float xDer = (right - left) * 0.5f;

				//Get the derivative in the z axis
				float back = -1, front = -1;
				if (i == 0) {
					back = getHeightForCoordinate(worldX, worldY, i - 1, j);
					front = vertices [iw1 [i + 1] + j].y;
				} else if (i == width) {
					back = vertices [iw1 [i - 1] + j].y;
					front = getHeightForCoordinate(worldX, worldY, i + 1, j);
				} else {
					back = vertices [iw1 [i - 1] + j].y;
					front = vertices [iw1 [i + 1] + j].y;
				}

				float zDer = (front - back) * 0.5f;

				normals [iw1 [i] + j] = new Vector3(-xDer, 1f, -zDer).normalized;
			}
		}
		mesh.normals = normals;
		*/

		return mesh;
	}

	//Simulates the terrain generation for a specific point. Used in calculating normals in neighbors.
	public static float getHeightForCoordinate(float worldX, float worldY, int i, int j) {
		float adjustedScale = sizeScale * (meshDimension / 128.0f) / 128.0f;

		float perlin = perlinOctaves (8, 0.5f, adjustedScale, worldX + ((float)j / meshDimension), worldY + ((float)i / meshDimension));
		float heightOffset = -0.5f * heightScale;

		return (heightOffset + (heightScale * perlin));
	}

	public static IEnumerator generateTerrainBackground(float worldX, float worldY, GameObject unfinishedObject) {
		int width = meshDimension;
		int length = meshDimension;

		//Generating terrain does so on a 1x1 tile. The mesh is 128 meters wide (by default)- this fixes that scale.
		//This also allows meshDimension and sizeScale to be two independent variables.
		float adjustedScale = sizeScale * (meshDimension / 128.0f) / 128.0f;

		int w1 = width + 1;
		int l1 = length + 1;

		//Generate a heightmap based on 2 passes of Perlin noise. This makes oceans and continents.
		float[] heightMap = createHeightMap (width, length, worldX, worldY, adjustedScale);

		//Generate a perlin noise map for the actual land itself. 
		float[] perlinMap = new float[w1 * l1];
		for (int i = 0; i < w1; i++) {
			for (int j = 0; j < l1; j++) {
				perlinMap [(i * w1) + j] = 
					heightScale * (perlinOctaves (6, 0.5f, adjustedScale, worldX + ((float)j / length), worldY + ((float)i / width)) + heightMap[(i * w1) + j]);
			}
		}
		yield return null;

		//TODO: apply a smooth/boost function to make the perlin map terrain more diverse.
		//Specifically, 0-0.35 gets flattened, 0.35-0.6 gets a smooth slope, 0.6-0.8 is hilly, and 0.8-1.0 is mountainous.

		int[] iw1 = new int[w1];

		//Vertex generation
		Vector3[] vertices = new Vector3[w1 * l1];
		for (int i = 0; i < w1; i++) {
			iw1[i] = i * w1;
			for (int j = 0; j < l1; j++) {
				vertices [iw1[i] + j] = new Vector3 (j, perlinMap[(i * w1) + j], i);
			}
		}
		yield return null;

		//UV mapping generation
		Vector2[] uvs = new Vector2[w1 * l1];
		for (int i = 0; i < w1; i++) {
			for (int j = 0; j < l1; j++) {
				uvs [iw1[i] + j] = new Vector2 (j, i);
			}
		}
		yield return null;

		//Triangle generation
		int[] triangles = new int[(int)(6 * width * length)];
		for (int i = 0; i < width; i++) {
			int i1 = i + 1;
			int i1wd1 = i1 * w1;

			int iwd = i * width;

			for (int j = 0; j < length; j++) {
				int baseIndex = 6 * (iwd + j);

				int iwd1j = iw1[i] + j;
				int i1wd1j = i1wd1 + j;

				triangles [baseIndex + 0] = iwd1j;
				triangles [baseIndex + 1] = i1wd1j + 1;
				triangles [baseIndex + 2] = iwd1j + 1;
				triangles [baseIndex + 3] = iwd1j;
				triangles [baseIndex + 4] = i1wd1j;
				triangles [baseIndex + 5] = i1wd1j + 1;
			}
		}
		yield return null;

		Mesh newMesh = createMesh (vertices, uvs, triangles);
		unfinishedObject.GetComponent<MeshFilter> ().mesh = newMesh;

		yield return null;

		unfinishedObject.GetComponent<MeshCollider>().sharedMesh = newMesh;
	}

	public static float[] createHeightMap(int width, int length, float worldX, float worldY, float scaleMod) {
		int w1 = width + 1;
		int l1 = length + 1;

		/**
		 * The scale for the perlin noise used to determine the heightmap. Determines the size of 
		 * the continents formed. Low values = bigger continents; high values = smaller islands.
		 * Default: 0.777. 
		 */
		float scale = scaleMod * 0.777f;

		/**
		 * The algorithm uses a sigmoid function to separate land and water.
		 * This parameter determines how steep of a dropoff this is.
		 * Default: -12. Higher values = sharp borders, lower values = uniform terrain.
		 */
		float landDropoff = -12f;

		/**
		 * The algorithm uses this as the target land coverage. This is a percentage
		 * from 0-1 representing how much land we should try to generate. Note that at
		 * high values for "landDropoff", and extreme values for this parameter, you might
		 * get slightly inaccurate results. For example, with landDropoff=-20, and coverage=0.1,
		 * you'll really get something like 0.125 as the true land coverage.
		 * Default: 0.4. Higher values = more land.
		 */
		float targetLandCoverage = 0.4f;

		float[] toReturn = new float[w1 * l1];
		for (int i = 0; i < w1; i++) {
			float y = (float)i / length;
			for (int j = 0; j < l1; j++) {
				float x = (float)j / width;

				//2 passes of perlin noise
				float val = perlinOctaves(2, 0.6f, scale, (worldX + x), (worldY + y));

				//Smooth/boost sigmoid function (makes border between ocean + land more clear)
				val = -1.0f / (1 + (Mathf.Exp (landDropoff * (val - targetLandCoverage))));
				//val = (2.0f / (1 + (Mathf.Exp (landDropoff * (val - targetLandCoverage))))) - 1.0f;

				toReturn [(i * w1) + j] = val;
			}
		}
		return toReturn;
	}

	public static float getHeightMapForCoordinate(float worldX, float worldY, int i, int j, float scaleMod) {
		float scale = scaleMod * 0.777f;
		float y = (float)i / meshDimension;
		float x = (float)j / meshDimension;
		float perlin = perlinOctaves(2, 0.6f, scale, (worldX + x), (worldY + y));
		float landDropoff = -12f;
		float targetLandCoverage = 0.4f;

		return -1.0f / (1 + (Mathf.Exp (landDropoff * (perlin - targetLandCoverage))));
	}

	/**
	 * Generates "levels" amount of perlin noise. This is normalized to be between 0-1.
	 * @param levels: How many iterations to run. Higher = more bumpy terrain, lower = smoother.
	 * @param persistence: How much impact each level has. 0.5 is default; higher = more random, lower = more uniform.
	 * @param scale: How "zoomed in" is this noise? Higher = more dense spikes, lower = more spread out hills.
	 */
	private static float perlinOctaves(int levels, float persistence, float scale, float x, float y) {
		float amp = 1;
		float value = 0;
		float sumAmps = 0;
		for (int k = 0; k < levels; k++) {
			sumAmps += amp;
			value += amp * Mathf.PerlinNoise (x * scale, y * scale);
			amp *= persistence;
			scale *= 2;
		}
		return value / sumAmps;
	}

	public static GameObject createTerrainObject(Mesh mesh, int x, int y, Vector3 position, Material material, PhysicMaterial pmat) {
		GameObject obj = new GameObject ();

		obj.AddComponent<MeshFilter> ();
		obj.AddComponent<MeshRenderer> ();
		obj.AddComponent<MeshCollider> ();
		obj.AddComponent<ChunkData> ();

		obj.GetComponent<MeshFilter> ().mesh = mesh;
		obj.GetComponent<MeshRenderer> ().material = material;
		obj.GetComponent<MeshCollider> ().material = pmat;
		obj.GetComponent<ChunkData> ().worldX = x;
		obj.GetComponent<ChunkData> ().worldY = y;

		/* TODO: create simplified compound collider with boxcolliders */
		obj.GetComponent<MeshCollider>().sharedMesh = mesh; 

		obj.transform.position = position;
		return obj;
	}

	/*
	public static GameObject createTerrainObject(Mesh mesh, Vector3 position) {				
		Material material = Resources.Load ("Materials/Grass", typeof(Material)) as Material;

		PhysicMaterial pmat = new PhysicMaterial ();
		pmat.bounciness = 0f;
		pmat.dynamicFriction = 0.4f;
		pmat.staticFriction = 0.7f;

		return createTerrainObject (mesh, position, material, pmat);
	}*/

	public static Mesh createMesh(Vector3[] vertices, Vector2[] uvs, int[] triangles) {
		return createMesh (null, vertices, uvs, triangles);
	}

	public static Mesh createMesh(string name, Vector3[] vertices, Vector2[] uvs, int[] triangles) {
		Mesh mesh = new Mesh ();

		if (name != null) {
			mesh.name = name;
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;

		mesh.RecalculateNormals ();

		return mesh;
	}
}
