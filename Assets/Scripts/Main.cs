using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Main : MonoBehaviour {

	public Vector3[] vertices = new Vector3[] {
		new Vector3(0, 0, 0),
		new Vector3(1, 0, 0),
		new Vector3(0, 1, 0),
		new Vector3(1, 1, 0)
	};

	public Vector2[] uvs = new Vector2[] {
		new Vector2(0, 0),
		new Vector2(1, 0),
		new Vector2(0, 1),
		new Vector2(1, 1)
	};

	public int[] triangles = new int[6] {
		0, 3, 1, 0, 2, 3
	};

	public Material material;

	// Use this for initialization
	void Start () {
		transform.GetComponent<MeshFilter> ();

		if (!transform.GetComponent<MeshFilter> () || !transform.GetComponent<MeshRenderer> ()) {
			transform.gameObject.AddComponent<MeshFilter> ();
			transform.gameObject.AddComponent<MeshRenderer> ();
		}

		string output = "float[] precalculated = new float[] {";
		for (int i = 0; i < 50; i++) {
			float val = i * (1.0f / 50.0f);
			output += " " + (-1.0f / (1 + (Mathf.Exp (-12 * (val - 0.4f))))) + "f,";
		}
		Debug.Log (output);
			
		//generateHills ();


		int meshWidth = 8;
		int meshLength = 8;

		float density = 16f;

		//Actual random seed
		int xStart = Random.Range (-10000, 10000);
		int yStart = Random.Range (-10000, 10000);

		//Fixed random seed (for testing)
		//int xStart = 6592;
		//int yStart = -192;

		float heightScale = 1;
		//float heightOffset = -heightScale / 2;
		float sizeScale = 0.125f; //2f

		float amountAboveSeaLevel = 0.5f + ((Random.Range (-10, 10) - Random.Range (-10, 10)) / 100.0f);
		float heightOffset = -heightScale * (1 - amountAboveSeaLevel);

		material = Resources.Load ("Materials/Grass", typeof(Material)) as Material;

		PhysicMaterial pmat = new PhysicMaterial ();
		pmat.bounciness = 0f;
		pmat.dynamicFriction = 0.4f;
		pmat.staticFriction = 0.7f;

		//Debug.Log (material);


		/*
		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < 4; j++) {

				GameObject obj = new GameObject ();

				obj.AddComponent<MeshFilter> ();
				obj.AddComponent<MeshRenderer> ();
				obj.AddComponent<MeshCollider> ();

				//Mesh mesh = Generator.generateTerrain(
				//	meshWidth, meshLength, density, xStart + (sizeScale * i), yStart + (sizeScale * j), heightScale, heightOffset, sizeScale);
				Mesh mesh = Generator.generateTerrain(
					meshWidth, meshLength, density, xStart + (sizeScale * i), yStart + (sizeScale * j), heightScale, heightOffset, sizeScale);
				
				obj.GetComponent<MeshFilter> ().mesh = mesh;
				
				obj.GetComponent<MeshRenderer> ().material = material;

				obj.GetComponent<MeshCollider> ().material = pmat;

				// TODO: create simplified compound collider with boxcolliders
				obj.GetComponent<MeshCollider>().sharedMesh = mesh; 

				obj.transform.position = new Vector3 (-100 + (i * meshWidth), 0, -100 + (j * meshLength));

				//transform.GetComponent<MeshFilter> ().mesh = Generator.generateTerrain(meshWidth, meshLength, density, xStart, yStart);
				//transform.gameObject.GetComponent<Renderer>().material = material;
			}
		}*/

		int xCount = 5;
		int yCount = 5;

		for (int i = 0; i < xCount; i++) {
			for (int j = 0; j < yCount; j++) {

				Profiler.BeginSample("Heightmap generation");
				float[] heightMap = Generator.createHeightMap ((int)(meshWidth * density) + 1, (int)(meshLength * density) + 1, xStart + i, yStart + j);
				Profiler.EndSample();

				Mesh mesh = Generator.generateTerrain(
					meshWidth, meshLength, density, xStart + i, yStart + j, heightScale, heightOffset, sizeScale, heightMap);

				Vector3 position = new Vector3 (-(xCount * meshWidth / 2) + (i * meshWidth), 0, -(yCount * meshLength / 2) + (j * meshLength));

				Generator.createTerrainObject (mesh, position, material, pmat);
			}
		}

	}

	void generateHills() {
		material = Resources.Load ("Materials/Grass", typeof(Material)) as Material;

		int meshWidth = 1000;
		int meshLength = 1000;

		float density = 0.125f;

		int xStart = Random.Range (-10000, 10000);
		int yStart = Random.Range (-10000, 10000);

		float heightScale = 1000;
		float heightOffset = -heightScale / 2;
		float sizeScale = 1;

		PhysicMaterial pmat = new PhysicMaterial ();
		pmat.bounciness = 0f;
		pmat.dynamicFriction = 0.4f;
		pmat.staticFriction = 0.7f;

		int xCount = 5;
		int yCount = 5;

		for (int i = 0; i < xCount; i++) {
			for (int j = 0; j < yCount; j++) {
				float[] heightMap = Generator.createHeightMap (meshWidth, meshLength, xStart + i, yStart + j);

				Mesh mesh = Generator.generateTerrain(
					meshWidth, meshLength, density, xStart + i, yStart + j, heightScale, heightOffset, sizeScale, heightMap);

				Vector3 position = new Vector3 (-(xCount * meshWidth / 2) + (i * meshWidth), 0, -(yCount * meshLength / 2) + (j * meshLength));

				Generator.createTerrainObject (mesh, position, material, pmat);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
