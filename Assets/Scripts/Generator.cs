using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Generator : MonoBehaviour {

	public static Mesh[,] meshGrid;

	public static int meshDimension = 4;

	public static float sizeScale = 128f; //16f
	public static float heightScale = 100f;

	public static int renderDiameter = 41;
	public static int renderRadius = renderDiameter / 2;

	//public static float[] boundaries = new float[]{ 0f, 0.1f, 0.3f, 0.5f };
	//public static float transitionSize = 0.1f;

	public static void generateLand(float xStart, float yStart) {
		//TODO: Have multiple materials/textures and blend between them
		Material material = Resources.Load ("Materials/Grass", typeof(Material)) as Material;

		PhysicMaterial pmat = new PhysicMaterial ();
		pmat.bounciness = 0f;
		pmat.dynamicFriction = 0.4f;
		pmat.staticFriction = 0.7f;

		for (int j = -renderRadius; j <= renderRadius; j++) {
			for (int i = -renderRadius; i <= renderRadius; i++) {
				Mesh mesh = Generator.generateTerrain(xStart + i, yStart + j);
				Vector3 position = new Vector3(i * meshDimension - (meshDimension / 2), 0, j * meshDimension - (meshDimension / 2));
				Generator.createTerrainObject (mesh, position, material, pmat);
			}
		}
	}

	public static Mesh generateTerrain(float worldX, float worldY) {
		//Debug.Log ("Generating terrain for " + worldX + ", " + worldY);

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
		float heightOffset = -0.5f * heightScale;
		float[] perlinMap = new float[w1 * l1];
		for (int i = 0; i < w1; i++) {
			for (int j = 0; j < l1; j++) {
				perlinMap [(i * w1) + j] = heightOffset + getHeightForCoordinate (worldX, worldY, i, j);
				//float perlin = -0.5f + perlinOctaves (6, 0.5f, adjustedScale, worldX + ((float)j / length), worldY + ((float)i / width));
				//float height = heightMap[(i * w1) + j];
				//float val = perlin + height;

				//TODO: Automate the addition of ranges, add a variable for the transitions, and have it 
				//automatically deal with calculating the appropriate interpolation bounds.

				//float transitionSize = 0.05f;

				//perlinMap [(i * w1) + j] = 100 * perlin;

				/*
				if (val < -0.2f) {
					perlinMap [(i * w1) + j] = 150 * (val - (0.8f * perlin));
				} else if (val >= -0.2f && height < -0.1f) {
					//perlinMap [(i * w1) + j] = 70 * val;
				} else if (val > -0.1f && height < 0.1f - transitionSize) {
					perlinMap [(i * w1) + j] = 10 * val;
				} else if (val >= 0.1f - transitionSize && val < 0.1 + transitionSize) {
					//Interpolate
					float[] interp = Helper.interpolate (-0.05f, 0.05f, 0.15f, 10 * -0.05f, 10 * 0.05f, 40 * (0.15f - 0.1f));
					perlinMap [(i * w1) + j] = (interp [2] * val * val) + (interp [1] * val) + interp [0];
				} else if (val >= 0.1f + transitionSize && val < 0.3f - transitionSize) {
					perlinMap [(i * w1) + j] = 40 * (val - 0.1f);
					//perlinMap [(i * w1) + j] = (100 * (height - 0.1f)) + (0 * (perlin - 0.1f));
				} else if (val >= 0.3f - transitionSize && val < 0.3f + transitionSize) {
					//Interpolate. The last two points are the transition range; the first is back a distance equal to the transition distance.
					//Pass in: x1 - (x2 - x1), x1, x2, f1(x1), f1(x2), f2(x3)
					float[] interp = Helper.interpolate (0.15f, 0.25f, 0.35f, 40 * (0.15f - 0.1f), 40 * (0.25f - 0.1f), 300 * (0.35f - 0.3f));
					perlinMap [(i * w1) + j] = (interp [2] * val * val) + (interp [1] * val) + interp [0];
				} else if (val >= 0.3f + transitionSize) {
					perlinMap [(i * w1) + j] = 300 * (val - 0.3f);
				} else {
					Debug.Log ("Found value that didn't meet cases: " + val);
				}
				*/

				//-0.2f-: Abyss
				//-0.2f to -0.1f: Shelf
				//-0.1f to 0.1f: Coast
				//0.1f to 0.3f: Hill
				//0.3f+: Mountain

				/*
				float abyssScale = 150f;
				float shelfStart = -0.2f;
				float shelfScale = 70f;
				float coastStart = -0.1f;
				float coastScale = 10f;
				float hillStart = 0.1f;
				float hillScale = 400f;
				float mountainStart = 0.3f;
				float mountainScale = 3000f;

				if (perlin < shelfStart - transitionSize) { //Abyss
					perlinMap [(i * w1) + j] = abyssScale * (perlin );
				} else if (perlin >= shelfStart - transitionSize && perlin < shelfStart + transitionSize) { //Abyss-shelf transition

					//perlinMap [(i * w1) + j] = abyssScale * (perlin);
					float x1 = shelfStart - transitionSize;
					float x2 = shelfStart + transitionSize;
					float x0 = x1 - (x2 - x1);

					float[] interp = Helper.interpolate (x0, x1, x2,
						shelfScale * (x0 + 0.3f),
						shelfScale * (x1 + 0.3f),
						abyssScale * (x2 - shelfStart));
					
				} else if (perlin >= shelfStart + transitionSize && perlin < coastStart - transitionSize) { //Shelf
					perlinMap [(i * w1) + j] = shelfScale * (perlin );
				} else if (perlin >= coastStart - transitionSize && perlin < coastStart + transitionSize) { //Shelf-coast transition

					perlinMap [(i * w1) + j] = shelfScale * (perlin );

					float x1 = coastStart - transitionSize;
					float x2 = coastStart + transitionSize;
					float x0 = x1 - (x2 - x1);

					float[] interp = Helper.interpolate (x0, x1, x2,
						shelfScale * (x0 - shelfStart),
						shelfScale * (x1 - shelfStart),
						coastScale * (x2 - coastStart));

				} else if (perlin >= coastStart + transitionSize && perlin < hillStart - transitionSize) { //Coast
					perlinMap [(i * w1) + j] = coastScale * (perlin );
				} else if (perlin >= hillStart - transitionSize && perlin < hillStart + transitionSize) { //Coast-hill transition

					float x1 = hillStart - transitionSize;
					float x2 = hillStart + transitionSize;
					float x0 = x1 - (x2 - x1);

					float[] interp = Helper.interpolate (x0, x1, x2,
						coastScale * (x0 - coastStart),
						coastScale * (x1 - coastStart),
						hillScale * (x2 - hillStart));
					//float[] interp = Helper.interpolate (-0.05f, 0.05f, 0.15f, 10 * -0.05f, 10 * 0.05f, 40 * (0.15f - 0.1f));
					perlinMap [(i * w1) + j] = (interp [2] * perlin * perlin) + (interp [1] * perlin) + interp [0];
				} else if (perlin >= hillStart + transitionSize && perlin < mountainStart - transitionSize) { //Hill
					perlinMap [(i * w1) + j] = hillScale * (perlin );
				} else if (perlin >= mountainStart - transitionSize && perlin < mountainStart + transitionSize) { //Hill-mountain transition
					
					float x1 = mountainStart - transitionSize;
					float x2 = mountainStart + transitionSize;
					float x0 = x1 - (x2 - x1);


					float[] interp = Helper.interpolate (x0, x1, x2,
						hillScale * (x0 - hillStart),
						hillScale * (x1 - hillStart),
						mountainScale * (x2 - mountainStart));
					perlinMap [(i * w1) + j] = (interp [2] * perlin * perlin) + (interp [1] * perlin) + interp [0];

					//float[] interp = Helper.interpolate (0.15f, 0.25f, 0.35f, 40 * (0.15f - 0.1f), 40 * (0.25f - 0.1f), 300 * (0.35f - 0.3f));
					//perlinMap [(i * w1) + j] = (interp [2] * perlin * perlin) + (interp [1] * perlin) + interp [0];
				} else if (perlin >= mountainStart + transitionSize) { //Mountain
					perlinMap [(i * w1) + j] = mountainScale * (perlin );
					//perlinMap [(i * w1) + j] = (300 * perlin) + (300 * height) - (0.3f * 300);
					//perlinMap [(i * w1) + j] = (-150 * height) + (10 * perlin);
				}
				*/

				//(0.25, 0.04) -> (0.3231, 0.06923) -> (0.35, 0.15)
			}
		}

		//TODO: apply a smooth/boost function to make the perlin map terrain more diverse.
		//Specifically, 0-0.35 gets flattened, 0.35-0.6 gets a smooth slope, 0.6-0.8 is hilly, and 0.8-1.0 is mountainous.

		int[] iw1 = new int[w1];

		//Vertex generation
		Profiler.BeginSample("Vertex generation");
		Vector3[] vertices = new Vector3[w1 * l1];
		for (int i = 0; i < w1; i++) {
			iw1[i] = i * w1;
			for (int j = 0; j < l1; j++) {
				vertices [iw1[i] + j] = new Vector3 (j, perlinMap[(i * w1) + j], i);
			}
		}
		Profiler.EndSample();

		//UV mapping generation
		Profiler.BeginSample("UV generation");
		Vector2[] uvs = new Vector2[w1 * l1];
		for (int i = 0; i < w1; i++) {
			for (int j = 0; j < l1; j++) {
				uvs [iw1[i] + j] = new Vector2 (j, i);
			}
		}
		Profiler.EndSample();

		//Triangle generation
		Profiler.BeginSample("Triangle generation");
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

		float perlin = perlinOctaves (6, 0.5f, adjustedScale, worldX + ((float)j / meshDimension), worldY + ((float)i / meshDimension));
		//float height = getHeightMapForCoordinate (worldX, worldY, i, j, adjustedScale);

		//float val = perlin + height;

		//float transitionSize = 0.05f;

		return heightScale * perlin;

		/*
		if (val < -0.2f) {
			return 150 * (val - (0.8f * perlin));
		} else if (val >= -0.2f && height < -0.1f) {
			return 70 * val;
		} else if (val > -0.1f && height < 0.1f - transitionSize) {
			return 10 * val;
		} else if (val >= 0.1f - transitionSize && val < 0.1 + transitionSize) {
			//Interpolate
			float[] interp = Helper.interpolate (-0.05f, 0.05f, 0.15f, 10 * -0.05f, 10 * 0.05f, 40 * (0.15f - 0.1f));
			return (interp [2] * val * val) + (interp [1] * val) + interp [0];
		} else if (val >= 0.1f + transitionSize && val < 0.3f - transitionSize) {
			return 40 * (val - 0.1f);
		} else if (val >= 0.3f - transitionSize && val < 0.3f + transitionSize) {
			//Interpolate. The last two points are the transition range; the first is back a distance equal to the transition distance.
			//Pass in: x1 - (x2 - x1), x1, x2, f1(x1), f1(x2), f2(x3)
			float[] interp = Helper.interpolate (0.15f, 0.25f, 0.35f, 40 * (0.15f - 0.1f), 40 * (0.25f - 0.1f), 300 * (0.35f - 0.3f));
			return (interp [2] * val * val) + (interp [1] * val) + interp [0];
		} else if (val >= 0.3f + transitionSize) {
			return 300 * (val - 0.3f);
		} else {
			Debug.Log ("Found value that didn't meet cases: " + val);
			return -1;
		}
		*/
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

	public static GameObject createTerrainObject(Mesh mesh, Vector3 position) {				
		Material material = Resources.Load ("Materials/Grass", typeof(Material)) as Material;

		PhysicMaterial pmat = new PhysicMaterial ();
		pmat.bounciness = 0f;
		pmat.dynamicFriction = 0.4f;
		pmat.staticFriction = 0.7f;

		return createTerrainObject (mesh, position, material, pmat);
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
