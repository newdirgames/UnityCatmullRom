/********************************************************************************************
 * @class CurveMesh
 * This class creates a curve mesh with a MeshFilter and a MeshRenderer
 ********************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using Curve;

//[RequireComponent (typeof (MeshCollider))] // We might need this later if we let Unity do the CD.
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class CurveMesh : MonoBehaviour {

	public float mStripWidth = 4;

	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private Mesh mesh;

	public CurveMesh() {

	}



	/***************************************************************************
	 * CreateCatmullStrip
	 * Creates a Catmull strip (one quad in width) mesh. 
	 * @param controlPoints, the control points of the Catmull-Rom Spline.
	 * @param density, the mesh density measured in in game distance.
	 ***************************************************************************/
	public void CreateCatmullStrip(List<Vector3> controlPoints, float density) {

		if (!InitMeshFilter()) {
			return;
		}

		int numPoints = controlPoints.Count;

		if (numPoints < 4) {
			// A Catmull-Rom Spline needs at least 4 control points.
			Debug.LogError("Too few control points!");
			return;
		}

		List<Vector3> stripVerts = new List<Vector3>();
		Vector3 cp0, cp1, cp2, cp3;

		// The width of the curve in number of verts.
		float curveWidthOffsetLen = 0.5f;
		int curveVertsWidth = (int) (mStripWidth / curveWidthOffsetLen);

		// Go through all the control points and build the curve along them.
		for (int cpIdx = 1; cpIdx < numPoints - 1; ++cpIdx) {
			// Go through the time between each control point.
			for (float time = 0.0f; time <= 0.95f; time += 0.05f) {
				// Build the strip by plotting the curve and a curve with a bi-normal offset to it.
				cp0 = controlPoints[(cpIdx + 3) % numPoints];
				cp1 = controlPoints[(cpIdx) % numPoints];
				cp2 = controlPoints[(cpIdx + 1) % numPoints];
				cp3 = controlPoints[(cpIdx + 2) % numPoints];

				Vector3 tangent = Curve.Catmull.NormalizedTangentAt(time, cp0, cp1, cp2, cp3);
				Vector3 normal = WorldConstants.GetWorldUp();
				Vector3 biNormal = Vector3.Cross(normal, tangent);

				for (float widthOffset = -mStripWidth / 2.0f; widthOffset < (mStripWidth / 2.0f); widthOffset += curveWidthOffsetLen) { 
					// Offset curve along biNormal.
					Vector3 curveOffset = biNormal * widthOffset;
					stripVerts.Add( Curve.Catmull.CurvePointAt(time, cp0 + curveOffset, cp1 + curveOffset, 
					                                           cp2 + curveOffset, cp3 + curveOffset) );
				}

			}
		}

		CopyDataToMesh(stripVerts, curveVertsWidth);
	}

	/***************************************************************************
	 * CopyDataToMesh
	 * @param verts the vertices to copy to the mesh object.
	 ***************************************************************************
	 */
	void CopyDataToMesh(List<Vector3> verts, int curveVertsWidth) {

		mesh = meshFilter.sharedMesh;
		// Clear the mesh before adding anything to make sure the triangles are in bounds. This is the recommended way.
		mesh.Clear();
		// Building the mesh.
		mesh.vertices = verts.ToArray();

		List<int> triangleIndices = new List<int>();
		// The num time offsets is the vertex count divided by two.
		int curveVertsLength = verts.Count / curveVertsWidth; 

		// Build the triangle indices.
//		for (int i = 0; i < numTimeOffsets; i += 2) {
//
//			//TODO: Treat the condition when we go from the edge of the curve row 
//			// to the new row.
//			/*// First triangle.
//			triangleIndices.Add(i);
//			triangleIndices.Add(i + curveWidth);
//			triangleIndices.Add(i + 1);
//			// Second triangle.
//			triangleIndices.Add(i + curveWidth);
//			triangleIndices.Add(i + curveWidth + 1);
//			triangleIndices.Add(i + 1);
//			*/
//			
//			// Build a quad.
//			// First triangle.
//			triangleIndices.Add(i);
//			triangleIndices.Add(i + 2);
//			triangleIndices.Add(i + 1);
//			// Second triangle.
//			triangleIndices.Add(i + 2);
//			triangleIndices.Add(i + 3);
//			triangleIndices.Add(i + 1);
//
//
//		}

		for (int lenIdx = 0; lenIdx < curveVertsLength - 1; ++lenIdx) {

			for (int wthIdx = 0; wthIdx < curveVertsWidth - 1; ++wthIdx) {

				int thisRow = lenIdx * curveVertsWidth;
				int nextRow = ( (lenIdx + 1) * curveVertsWidth );

				// Build a quad.
				// First triangle.
				triangleIndices.Add(thisRow + wthIdx);
				triangleIndices.Add(nextRow + wthIdx);
				triangleIndices.Add(thisRow + wthIdx + 1);
				// Second triangle.
				triangleIndices.Add(nextRow + wthIdx);
				triangleIndices.Add(nextRow + wthIdx + 1);
				triangleIndices.Add(thisRow + wthIdx + 1);
			}
		}

		// Add triangles to mesh object.
		mesh.triangles = triangleIndices.ToArray();

		//mesh.RecalculateBounds(); //NOTE: If we want CD.
		mesh.RecalculateNormals();
		mesh.Optimize();
	}

	/***************************************************************************
	 * CreateQuadMesh
	 * Creates a Quad mesh. NOTE: Not a curve. This was only used for testing
	 * and as a reference.
	 ***************************************************************************
	 */
	public void CreateQuadMesh() {

		if (!InitMeshFilter()) {
			return;
		}
		/* In the case of MeshFilter (and SkinnedMeshRenderer), calling mesh will cause an instance to be created 
		 * if it hasn't already done so, allowing you to modify the mesh without manipulating all the other instances. 
		 * SharedMesh (which isn't deprecated for these classes) doesn't creae a new instance, which is generally what 
		 * you want to use when reading from the mesh and not modifying. */
		mesh = meshFilter.sharedMesh;
		// Clear the mesh before adding anything to make sure the triangles are in bounds. This is the recommended way.
		mesh.Clear();

		mesh.vertices = new Vector3[] {
			// OpenGL format quad.
			new Vector3 { x = 0.0f, y = 0.0f, z = 0.0f },
			new Vector3 { x = 0.0f, y = 1.0f, z = 0.0f },
			new Vector3 { x = 1.0f, y = 1.0f, z = 0.0f },
			new Vector3 { x = 1.0f, y = 0.0f, z = 0.0f }
		};

		mesh.triangles = new int[] {
			// Triangles is really vertex indices.
			// Unity uses clockwise indexing for backface culling, just as OpenGL.
			0, 1, 2,
			2, 3, 0
		};

		mesh.RecalculateNormals();
		// Recalculate the bounding volume.
		//mesh.RecalculateBounds();

		/* This operation might take a while but will make the geometry displayed be faster. 
		 * You should use it if you generate a mesh from scratch procedurally and you want to trade better runtime performance
		 * against higher load time. Internally it optimizes the triangles for vertex cache locality. 
		 * For imported models you should never call this as the import pipeline already does it for you. */
		mesh.Optimize();

	}

	private bool InitMeshFilter() {
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();

		bool success = true;

		if (meshFilter == null) {
			Debug.LogError("MeshFilter not found!");
			success = false;
		}

		if (meshRenderer == null) {
			Debug.LogError("MeshRenderer not found!");
			success = false;
		}
		return success;
	}

	void OnDrawGizmosSelected() {

	}

	// TODO: This might need to be implemented to avoid leaks later.
	/*void OnDestroy() {
		mesh.Clear();
	}*/


}
