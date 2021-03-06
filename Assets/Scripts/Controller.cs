﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

	public KeyCode[] forward { get; set; } 
	public KeyCode[] backward { get; set; }
	public KeyCode[] left { get; set; }
	public KeyCode[] right { get; set; }
	public KeyCode[] up { get; set; }
	public KeyCode[] down { get; set; }

	public Camera mainCamera;

	public static float thirdPersonDistance = 0;

	public static bool flyingMode = true;

	private static float movementScale = 0.3f;
	private static float rotationScale = 5f;

	private int xChunk = 0;
	private int zChunk = 0;

	// Use this for initialization
	void Start () {
		forward	= new KeyCode[]{ KeyCode.W, KeyCode.UpArrow };
		backward = new KeyCode[]{ KeyCode.S, KeyCode.DownArrow };
		left = new KeyCode[]{ KeyCode.A, KeyCode.LeftArrow };
		right = new KeyCode[]{ KeyCode.D, KeyCode.RightArrow };
		up = new KeyCode[]{ KeyCode.LeftShift, KeyCode.Space };
		down = new KeyCode[]{ KeyCode.LeftControl, KeyCode.LeftAlt };

		mainCamera = Camera.main;

		//Cursor.lockState = CursorLockMode.Locked;

		/*
		if (flyingMode)
			movementScale = 2.5f;
		else
			movementScale = 0.2f;*/
	}

	bool keycodePressed(KeyCode[] arr) {
		for(int i = 0; i < arr.Length; i++) {
			if(Input.GetKey(arr[i])) {
				return true;
			}
		}
		return false;
	}

	bool keycodeDown(KeyCode[] arr) {
		for(int i = 0; i < arr.Length; i++) {
			if(Input.GetKeyDown(arr[i])) {
				return true;
			}
		}
		return false;
	}

	// Update is called once per frame
	void Update () {
		Vector3 oldPosition = this.gameObject.transform.position;
		Quaternion oldRotation = mainCamera.transform.rotation;

		Vector3 newPosition = oldPosition;
		Quaternion newRotation = oldRotation;

		if (Input.GetKeyDown (KeyCode.Escape)) {
			Cursor.lockState = CursorLockMode.None;
		}

		float planeRotation = mainCamera.transform.rotation.eulerAngles.y;
		Quaternion quat = Quaternion.Euler (new Vector3 (0, planeRotation, 0));

		Quaternion ang = Quaternion.identity;

		bool onGround = false;

		/*
		 * Raycast down and if we hit terrain, ensure movement is constrained to be normal to
		 * the terrain. This ensures we only move on the ground, instead of clipping in/floating.
		 */
		RaycastHit hit;
		if (Physics.Raycast (this.gameObject.transform.position, Vector3.down, out hit)) {
			if (hit.distance <= 1.5f) {
				onGround = true;

				Vector3 fbn = new Vector3 (0, hit.normal.y, hit.normal.z);
				float xAngle = 90 - Vector3.Angle (fbn, Vector3.forward);

				Vector3 lrn = new Vector3 (hit.normal.x, hit.normal.y, 0);
				float zAngle = 90 - Vector3.Angle (lrn, Vector3.left);

				ang = Quaternion.Euler (xAngle, 0, zAngle);
			}
		}

		if (keycodePressed (forward)) {
			newPosition += (ang * quat * Vector3.forward * movementScale);
		}
		if (keycodePressed (backward)) {
			newPosition += (ang * quat * Vector3.back * movementScale);
		}
		if (keycodePressed (left)) {
			newPosition += (ang * quat * Vector3.left * movementScale);
		}
		if (keycodePressed (right)) {
			newPosition += (ang * quat * Vector3.right * movementScale);
		}

		if (flyingMode) {
			if (keycodePressed (up)) {
				newPosition += (Vector3.up * movementScale);
			}
		} else {
			//Only jump if we're on the ground (or very close)
			if (keycodeDown (up)) {	
				if (onGround) {
					this.gameObject.GetComponent<Rigidbody> ().velocity = new Vector3 (0, 5, 0);
				}
			}
		}
			
		if (flyingMode) {
			if (keycodePressed (down)) {
				newPosition += (Vector3.down * movementScale);
			}
		}

		float mouseX = Input.GetAxis ("Mouse X");
		float mouseY = -Input.GetAxis ("Mouse Y");
		if (mouseX != 0 || mouseY != 0) {
			Vector3 rot = oldRotation.eulerAngles;

			float xRot = rot.x;

			float tent = rot.x + (rotationScale * mouseY);

			if (xRot > 270 && tent < 270) {
				xRot = 270;
			} else if (xRot < 90 && tent > 90) {
				xRot = 90;
			} else {
				xRot += rotationScale * mouseY;
			}

			newRotation = Quaternion.Euler (
				new Vector3 (
					xRot,
					rot.y + (rotationScale * mouseX),
					0));
		}

		thirdPersonDistance -= Input.GetAxis ("Mouse ScrollWheel");
		if (thirdPersonDistance < 0)
			thirdPersonDistance = 0;

		this.gameObject.transform.position = newPosition;
		this.gameObject.transform.rotation = ang;

		mainCamera.transform.position = newPosition + (newRotation * new Vector3 (0, 0, -thirdPersonDistance));
		mainCamera.transform.rotation = newRotation;

		/*
		if ((int)newPosition.x / Generator.meshDimension > xChunk) {
			xChunk = (int)(newPosition.x / Generator.meshDimension);
			for (int i = 0; i < Generator.renderDiameter; i++) {
				Vector3 position = new Vector3 (((Generator.renderRadius + xChunk - 0.5f) * Generator.meshDimension), 0, ((-Generator.renderRadius + zChunk + i - 0.5f) * Generator.meshDimension));
				GameObject newObj = Generator.createTerrainObject (new Mesh(), position);
				StartCoroutine(Generator.generateTerrainBackground (Main.xStart + Generator.renderRadius + xChunk, Main.yStart + zChunk - Generator.renderRadius + i, newObj));
			}
		}
		if ((int)newPosition.x / Generator.meshDimension < xChunk) {			
			xChunk = (int)(newPosition.x / Generator.meshDimension);
			for (int i = 0; i < Generator.renderDiameter; i++) {	
				Vector3 position = new Vector3 (((-Generator.renderRadius + xChunk - 0.5f) * Generator.meshDimension), 0, ((-Generator.renderRadius + zChunk + i - 0.5f) * Generator.meshDimension));
				GameObject newObj = Generator.createTerrainObject (new Mesh(), position);
				StartCoroutine(Generator.generateTerrainBackground (Main.xStart - Generator.renderRadius + xChunk, Main.yStart + zChunk - Generator.renderRadius + i, newObj));
			}
		}
		if ((int)newPosition.z / Generator.meshDimension > zChunk) {
			zChunk = (int)(newPosition.z / Generator.meshDimension);
			for (int i = 0; i < Generator.renderDiameter; i++) {	
				Vector3 position = new Vector3 (((-Generator.renderRadius + xChunk + i - 0.5f) * Generator.meshDimension), 0, ((Generator.renderRadius + zChunk - 0.5f) * Generator.meshDimension));
				GameObject newObj = Generator.createTerrainObject (new Mesh(), position);
				StartCoroutine(Generator.generateTerrainBackground (Main.xStart + xChunk - Generator.renderRadius + i, Main.yStart + zChunk + Generator.renderRadius, newObj));
			}
		}
		if ((int)newPosition.z / Generator.meshDimension < zChunk) {
			zChunk = (int)(newPosition.z / Generator.meshDimension);
			for (int i = 0; i < Generator.renderDiameter; i++) {		
				Vector3 position = new Vector3 (((-Generator.renderRadius + xChunk + i - 0.5f) * Generator.meshDimension), 0, ((-Generator.renderRadius + zChunk - 0.5f) * Generator.meshDimension));
				GameObject newObj = Generator.createTerrainObject (new Mesh(), position);
				StartCoroutine(Generator.generateTerrainBackground (Main.xStart + xChunk - Generator.renderRadius + i, Main.yStart + zChunk - Generator.renderRadius, newObj));
			}
		}*/

		if (Input.GetMouseButtonDown (0)) {
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
			//RaycastHit destroyHit = new RaycastHit();

			if (Physics.Raycast (ray.origin, ray.direction, out hit, Mathf.Infinity)) {

				//Pull the data from the mesh we hit. If it doesn't exist, we didn't hit a terrain chunk.
				ChunkData chunkData = hit.collider.gameObject.GetComponent<ChunkData> ();

				if (chunkData != null) {
					int baseX = chunkData.worldX * Generator.meshDimension;
					int baseY = chunkData.worldY * Generator.meshDimension;

					//Debug.Log ("Hit at world point: " + hit.point);

					int index = hit.triangleIndex;
					if (index != -1) {
						MeshCollider mc = hit.collider as MeshCollider;
						Mesh mesh = mc.sharedMesh;

						Vector3[] vertices = mesh.vertices;
						int[] triangles = mesh.triangles;

						Transform hitTransform = hit.collider.transform;

						//This extracts the triangle we hit.
						Vector3[] points = new Vector3[3];
						//int offset = index % 2 == 0 ? 0 : -3;
						for (int i = 0; i < 3; i++) {
							points [i] = vertices [triangles [(index * 3) + i]];
							//points [i] = hitTransform.TransformPoint (points [i]);
							//points [i] = new Vector3 (points [i].x, points [i].y - 1.0f, points [i].z);
							//vertices [triangles [index * 3 + i + 0]] = points [i];
						}
						//mesh.vertices = vertices;
						//mc.sharedMesh = mesh;

						//Grab the bottom-left point. We do this so we consistently get the same point on the square.
						//Technically, we can use either the bottom-left or top-right points.
						Vector3 minPt = points[0];
						for (int i = 0; i < 3; i++) {
							if (points [i].x < minPt.x)
								minPt.x = points [i].x;
							if (points [i].z < minPt.z)
								minPt.z = points [i].z;
						}

						//List<Block> blocks = Block.get (baseX + minPt.z, baseY + minPt.x);
						//foreach (Block b in blocks) {
						//	Debug.Log ("Found a block at the x,y position with z=" + b.z);
						//}
						Block data = Block.get (baseX + minPt.z, baseY + minPt.x, hit.point.y);
						//Debug.Log ("Checking if block exists at x=" + (baseX + minPt.z) + " y=" + (baseY + minPt.x) + " z=" + hit.point.y + ". " + (data != null ? data.ToString() : "Null."));
						if (data == null) {
							data = Block.get (baseX + minPt.z, baseY + minPt.x, hit.point.y - 1);
							//Debug.Log ("Checking if block exists at x=" + (baseX + minPt.z) + " y=" + (baseY + minPt.x) + " z=" + (hit.point.y - 1) + ". " + (data != null ? data.ToString() : "Null."));
						}

						if (data == null) {
							Debug.LogError ("Expected to find block, but failed. Coords: (x=" + (baseX + minPt.z) + " y=" + (baseY + minPt.x) + " z=" + (hit.point.y) + ").");
						}
						//Debug.Log ("Triangle point: " + minPt.z + ", " + minPt.x + ", " + minPt.y);
						//Debug.Log ("Checking if block exists at x=" + (baseX + minPt.z) + " y=" + (baseY + minPt.x) + " z=" + hit.point.y + ". " + (data != null ? data.ToString() : "Null."));

					}
				}


			}
		}
	}

	public static void DrawMeshOutline() {
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
		RaycastHit hit = new RaycastHit();

		if (Physics.Raycast (ray.origin, ray.direction, out hit, Mathf.Infinity)) {
			int index = hit.triangleIndex;
			if (index == -1) {
				//
			} else {
				MeshCollider mc = hit.collider as MeshCollider;
				Mesh mesh = mc.sharedMesh;

				Vector3[] vertices = mesh.vertices;
				int[] triangles = mesh.triangles;


				Transform hitTransform = hit.collider.transform;

				Vector3[] points = new Vector3[6];
				int offset = index % 2 == 0 ? 0 : -3;
				for (int i = 0; i < 6; i++) {
					points [i] = vertices [triangles [index * 3 + i + offset]];
					points [i] = hitTransform.TransformPoint (points [i]);
				}

				GL.Begin(GL.LINES);
				GL.Color(new Color(1f, 1f, 1f, 1f));
				GL.Vertex (points [1]);
				GL.Vertex (points [2]);
				GL.Vertex (points [2]);
				GL.Vertex (points [3]);
				GL.Vertex (points [3]);
				GL.Vertex (points [4]);
				GL.Vertex (points [4]);
				GL.Vertex (points [5]);
				GL.End();
			}
		}
	}
}
