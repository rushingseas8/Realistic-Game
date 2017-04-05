using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Generator : MonoBehaviour {
	/**
	 * @param densityValue: Expects an integer greater than or equal to one. Accepts floats below 1, but 
	 * 	you're on your own for those. Notably, 0.5f seems to work well.
	 */
	public static Mesh generateTerrain(int width, int length, float densityValue, float xOffset, float yOffset, float heightScale, float heightOffset, float sizeScale, float[] heightMap) {
		float density = (float)densityValue;

		//Used to save (a lot) of calculations
		int wd = (int)(width * density);
		int wd1 = (int)(width * density) + 1;
		int ld1 = (int)(length * density) + 1;

		//float[] heightMap = createHeightMap (wd1, ld1, xOffset, yOffset);

		int[] iwd1 = new int[wd1];

		//Vertex generation
		Profiler.BeginSample("Vertex generation");
		Vector3[] vertices = new Vector3[wd1 * ld1];
		//Debug.Log ("Vertices size: " + (wd1 * ld1));
		for (int i = 0; i < wd1; i++) {
			iwd1[i] = i * wd1;
			for (int j = 0; j < ld1; j++) {
				//Debug.Log ("i,j:" + i + "," + j + " index used: " + (iwd1 [i] + j));
				vertices [iwd1[i] + j] = new Vector3 (
					j / density, 
					//(heightScale * Mathf.PerlinNoise(
					//	xOffset + (sizeScale * j / density / length), yOffset + (sizeScale * i / density / width))) + heightOffset,
					//(heightScale * landGenFunction(xOffset + (sizeScale * j / density / length), yOffset + (sizeScale * i / density / width))) + heightOffset,
					(heightScale * landGenFunction(j / density / length, i / density / width, xOffset, yOffset, 1 / sizeScale)) + (heightMap[iwd1[i] + j] * heightScale),
					//heightMap[iwd1[i] + j] * heightScale,
					i / density);
			}
		}
		Profiler.EndSample();
			
		//UV mapping generation
		Profiler.BeginSample("UV generation");
		Vector2[] uvs = new Vector2[wd1 * ld1];
		for (int i = 0; i < wd1; i++) {
			for (int j = 0; j < ld1; j++) {
				uvs [iwd1[i] + j] = new Vector2 (j / density, i / density);
			}
		}
		Profiler.EndSample();

		//Triangle generation
		Profiler.BeginSample("Triangle generation");
		int[] triangles = new int[(int)(6 * width * length * density * density)];
		for (int i = 0; i < wd; i++) {
			int i1 = i + 1;
			int i1wd1 = i1 * wd1;

			int iwd = i * wd;

			for (int j = 0; j < length * density; j++) {
				int baseIndex = 6 * (iwd + j);

				int iwd1j = iwd1[i] + j;
				int i1wd1j = i1wd1 + j;

				triangles [baseIndex + 0] = iwd1j;
				triangles [baseIndex + 1] = i1wd1j + 1;
				triangles [baseIndex + 2] = iwd1j + 1;
				triangles [baseIndex + 3] = iwd1j;
				triangles [baseIndex + 4] = i1wd1j;
				triangles [baseIndex + 5] = i1wd1j + 1;
			}
		}
		Profiler.EndSample ();

		/*
		Debug.Log ("Vertices");
		for (int i = 0; i < vertices.Length; i++) {
			Debug.Log (vertices [i]);
		}
			
		Debug.Log ("UVs");
		for (int i = 0; i < uvs.Length; i++) {
			Debug.Log (vertices [i]);
		}

		Debug.Log ("Triangles");
		for (int i = 0; i < triangles.Length; i++) {
			Debug.Log (triangles [i]);
		}
		*/

		return createMesh (vertices, uvs, triangles);
	}

	private static float landGenFunction(float x, float y, float xOffset, float yOffset, float sizeScale) {
		//Debug.Log ("Land gen for " + x + ", " + y + ". Offset = " + xOffset + ", " + yOffset);

		/*
		int levels = 6;
		float freq = sizeScale;
		float amp = 1;
		float value = 0;
		float persistence = 0.5f;
		for (int k = 0; k < levels; k++) {
			value += amp * Mathf.PerlinNoise ((xOffset + x) * freq, (yOffset + y) * freq);
			amp *= persistence;
			freq *= 2;
		}
		return value * (1 - persistence);
		*/
		return perlinOctaves (6, 0.5f, sizeScale, (xOffset + x), (yOffset + y));
	}

	//private static float[] precalculated = new float[] { -0.00816257, -0.01035374, -0.01312532, -0.01662636, -0.02104134, -0.02659699, -0.03356922, -0.04228976, -0.05315113, -0.06660803, -0.08317269, -0.1034004, -0.1278616, -0.1570954, -0.1915453, -0.2314752, -0.2768781, -0.327393, -0.3822521, -0.4402863, -0.4999999, -0.5597136, -0.6177478, -0.6726069, -0.7231218, -0.7685248, -0.8084546, -0.8429044, -0.8721384, -0.8965995, -0.9168273, -0.9333919, -0.9468489, -0.9577102, -0.9664308, -0.973403, -0.9789587, -0.9833736, -0.9868747, -0.9896463, -0.9918374, -0.9935679, -0.9949334, -0.9960101, -0.9968588, -0.9975274, -0.9980539, -0.9984685, -0.9987949, -0.9990518};
	private static float[] precalculated = new float[] { -0.00816257f, -0.01035374f, -0.01312532f, -0.01662636f, -0.02104134f, -0.02659699f, -0.03356922f, -0.04228976f, -0.05315113f, -0.06660803f, -0.08317269f, -0.1034004f, -0.1278616f, -0.1570954f, -0.1915453f, -0.2314752f, -0.2768781f, -0.327393f, -0.3822521f, -0.4402863f, -0.4999999f, -0.5597136f, -0.6177478f, -0.6726069f, -0.7231218f, -0.7685248f, -0.8084546f, -0.8429044f, -0.8721384f, -0.8965995f, -0.9168273f, -0.9333919f, -0.9468489f, -0.9577102f, -0.9664308f, -0.973403f, -0.9789587f, -0.9833736f, -0.9868747f, -0.9896463f, -0.9918374f, -0.9935679f, -0.9949334f, -0.9960101f, -0.9968588f, -0.9975274f, -0.9980539f, -0.9984685f, -0.9987949f, -0.9990518f};

	public static float[] createHeightMap(int width, int length, float xOffset, float yOffset) {
		/**
		 * The scale for the perlin noise used to determine the heightmap. Determines the size of 
		 * the continents formed. Low values = bigger continents; high values = smaller islands.
		 * Default: 0.777. 
		 */
		float scale = 0.777f;

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

		float[] toReturn = new float[width * length];
		for (int i = 0; i < width; i++) {
			float y = (float)i / length;
			for (int j = 0; j < length; j++) {
				float x = (float)j / width;

				float val = perlinOctaves(2, 0.6f, scale, (xOffset + x), (yOffset + y));
				/*
				if (val < 0.3f) {
					toReturn [(i * width) + j] = 0;
				} else if (val > 0.7f) {
					toReturn [(i * width) + j] = -1;
				} else {
					val = val - 0.3f;
					val = Mathf.Pow (val, 0.5f);
					val = val * 1.58f;
					toReturn [(i * width) + j] = -val;
				}*/

				//if (val > 0.4f && val < 0.6f) {
				//	count++;
				//}


				//val = -1.0f / (1 + (Mathf.Exp (landDropoff * (val - targetLandCoverage))));
				if(val < 0) 
					val = 0;
				if (val > 1)
					val = 1;
				

				//toReturn [(i * width) + j] = val;
				toReturn [(i * width) + j] = precalculated[(int)Mathf.Floor(val * 50.0f)];
			}
		}
		return toReturn;
	}

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

	public static GameObject createTerrainObject(Mesh mesh, Vector3 position, Material material, PhysicMaterial pmat) {
		GameObject obj = new GameObject ();

		obj.AddComponent<MeshFilter> ();
		obj.AddComponent<MeshRenderer> ();
		obj.AddComponent<MeshCollider> ();

		obj.GetComponent<MeshFilter> ().mesh = mesh;
		obj.GetComponent<MeshRenderer> ().material = material;
		obj.GetComponent<MeshCollider> ().material = pmat;

		/* TODO: create simplified compound collider with boxcolliders */
		obj.GetComponent<MeshCollider>().sharedMesh = mesh; 

		obj.transform.position = position;
		return obj;
	}

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
