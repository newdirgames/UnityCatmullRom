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
public class CurveObject : MonoBehaviour {

	public float mStripWidth = 4;
	public Vector2[] sQuadUVs;

	private MeshFilter mMeshFilter;
	private MeshRenderer mMeshRenderer;
	private Mesh mMesh;
	private List<Vector3> mControlPoints;


	public List<Vector3> GetControlPoints() {
		return mControlPoints;
	}

	/***************************************************************************
	 * CreateCatmullStrip
	 * Creates a Catmull strip (one quad in width) mesh. 
	 * @param controlPoints, the control points of the Catmull-Rom Spline.
	 * @param density, the mesh density measured in in game distance.
	 ***************************************************************************/
	public void CreateCatmullCurve(List<Vector3> controlPoints, float density) {

		mControlPoints = controlPoints;

		if (!InitMeshFilterAndRenderer()) {
			return;
		}

		int numPoints = controlPoints.Count;

		if (numPoints < 4) {
			// A Catmull-Rom Spline needs at least 4 control points.
			Debug.LogError("Too few control points!");
			return;
		}

		List<Vector3> verts = new List<Vector3>();
		Vector3 cp0, cp1, cp2, cp3;

		// The width of the curve in number of verts.
		float curveWidthOffsetLen = 0.5f;
		int curveVertsWidth = (int) (mStripWidth / curveWidthOffsetLen);

		// Go through all the control points and build the curve along them.
		for (int cpIdx = 1; cpIdx < numPoints - 2; ++cpIdx) {
			// Go through the time between each control point.
			for (float time = 0.0f; time <= 1.0f; time += 0.05f) {
				// Build the strip by plotting the curve and a curve with a bi-normal offset to it.
				cp0 = controlPoints[(cpIdx + (numPoints - 1)) % numPoints];
				cp1 = controlPoints[(cpIdx) % numPoints];
				cp2 = controlPoints[(cpIdx + 1) % numPoints];
				cp3 = controlPoints[(cpIdx + 2) % numPoints];

				Vector3 tangent = Curve.Catmull.NormalizedTangentAt(time, cp0, cp1, cp2, cp3);
				Vector3 normal = WorldConstants.GetWorldUp();
				Vector3 biNormal = Vector3.Cross(normal, tangent);

				for (float widthOffset = -mStripWidth / 2.0f; widthOffset < (mStripWidth / 2.0f); widthOffset += curveWidthOffsetLen) { 
					// Offset curve along biNormal.
					Vector3 curveOffset = biNormal * widthOffset;
					verts.Add( Curve.Catmull.CurvePointAt(time, cp0 + curveOffset, cp1 + curveOffset, 
					                                           cp2 + curveOffset, cp3 + curveOffset) );
				}

			}
		}

		CopyDataToMesh(verts, curveVertsWidth);
		AssignMaterial();
	}

	/***************************************************************************
	 * CopyDataToMesh
	 * @param verts the vertices to copy to the mesh object.
	 ***************************************************************************
	 */
	void CopyDataToMesh(List<Vector3> verts, int curveVertsWidth) {

		mMesh = mMeshFilter.sharedMesh;
		// Clear the mesh before adding anything to make sure the triangles are in bounds. This is the recommended way.
		mMesh.Clear();
		// Building the mesh.
		mMesh.vertices = verts.ToArray();

		List<int> triangleIndices = new List<int>();
		List<Vector2> texCoords = new List<Vector2>();
		// The num time offsets is the vertex count divided by two.
		int curveVertsLength = verts.Count / curveVertsWidth; 

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

		for (int lenIdx = 0; lenIdx < curveVertsLength; ++lenIdx) {
			
			for (int wthIdx = 0; wthIdx < curveVertsWidth; ++wthIdx) {
				
				texCoords.Add( new Vector2( (float) (wthIdx % 2), (float) (lenIdx % 2) ) );
			}
		}


		// Add triangles to mesh object.
		mMesh.triangles = triangleIndices.ToArray();
		mMesh.uv = texCoords.ToArray();

		//mesh.RecalculateBounds(); //NOTE: If we want CD.
		mMesh.RecalculateNormals();
		mMesh.Optimize();
	}

	/***************************************************************************
	 * AssignMaterial
	 * This function assigns the default Diffuse material to the mesh. 
	 ***************************************************************************
	 */
	void AssignMaterial() {
		mMeshRenderer.material = new Material(Shader.Find("Diffuse"));
	}

	/***************************************************************************
	 * AssignCustomMaterial
	 * @param material the material to add to this mesh.
	 * This function assigns a custom material to the mesh. 
	 ***************************************************************************
	 */
	void AssignCustomMaterial(string material) {
		mMeshRenderer.material = new Material(Shader.Find(material));
	}

	/***************************************************************************
	 * InitMeshFilterAndRenderer
	 ***************************************************************************
	 */
	private bool InitMeshFilterAndRenderer() {
		mMeshFilter = GetComponent<MeshFilter>();
		mMeshRenderer = GetComponent<MeshRenderer>();

		bool success = true;

		if (mMeshFilter == null) {
			Debug.LogError("MeshFilter not found!");
			success = false;
		}

		if (mMeshRenderer == null) {
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
