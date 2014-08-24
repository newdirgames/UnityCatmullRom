using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
// NOTE: We don't need to declare using on the files in our scripts folder.

[CustomEditor (typeof (CurveObject))] 
public class CurveMeshEditor : Editor {
		
	// Add a menu item to the Unity Editor.
	[MenuItem ("GameObject/Create Other/Curve")]
	static void Create() {

		GameObject gameObject = new GameObject("CurveMesh");
		CurveObject cm = gameObject.AddComponent<CurveObject>();
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		meshFilter.mesh = new Mesh();

		List<Vector3> controlPoints = new List<Vector3>();
		controlPoints.Add(new Vector3() { x = 0.0f, y = 0.0f, z = 0.0f });
		controlPoints.Add(new Vector3() { x = 40.0f, y = 1.0f, z = 0.0f });
		controlPoints.Add(new Vector3() { x = 50.0f, y = 4.0f, z = 60.0f });
		controlPoints.Add(new Vector3() { x = 100.0f, y = 0.0f, z = 80.0f });
		controlPoints.Add(new Vector3() { x = 150.0f, y = 0.0f, z = 80.0f });
		controlPoints.Add(new Vector3() { x = 200.0f, y = 0.0f, z = 140.0f });

//		cm.CreateQuadMesh();
		cm.CreateCatmullCurve(controlPoints, 0.0f);
	}
}
