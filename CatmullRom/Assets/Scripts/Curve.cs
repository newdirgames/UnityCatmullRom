using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/********************************************************************************************
 * This class generates and works with Catmull-Rom Splines (a.k.a Cubic Hermite 
 * Spline). It will be extended to other curves later with some inline class or 
 * something.
 * 
 * Learn about Catmull-Rom splines here: 
 * http://answers.unity3d.com/questions/566683/catmull-rom-curve-interpolation-in-unity.html
 * TODO: Make a static class.
 * Created by alex.samuelsson@gmail.com.
 ********************************************************************************************/
namespace Curve {
	
	public static class Catmull {

		/* curvePointAt
		 * @param time, the time along the path between cp1 and cp2.
		 * NOTE: when calculating the patch between new points you have to switch points, 
		 * the time is always between cp1 and cp2.
		 * @return the curve point as a Vector3.
		 */
		public static Vector3 CurvePointAt(float t, Vector3 cp0, Vector3 cp1, Vector3 cp2, Vector3 cp3) {

			float t2 = t * t;
			float t3 = t2 * t;
			
			Vector3 point = ( (-0.5f * cp0 + 1.5f * cp1 - 1.5f * cp2 + 0.5f * cp3) * t3
			              	+ (1.0f * cp0 - 2.5f * cp1 + 2.0f * cp2 - 0.5f * cp3) * t2
			               	+ (-0.5f * cp0 + 0.5f * cp2) * t
			          		+ cp1 );

			return point;
		}

		/***************************************************************************
		 * NormalizedTangentAt
		 * @return the tangent at the spline at the point between cp1 and cp2 in
		 * @param time time t.
		 * @return the normalized tangent.
		 ***************************************************************************
		 */
		public static Vector3 NormalizedTangentAt(float t, Vector3 cp0, Vector3 cp1, Vector3 cp2, Vector3 cp3) {

			Vector3 pointA = CurvePointAt(t, cp0, cp1, cp2, cp3);
			Vector3 pointB = CurvePointAt(t + 0.05f, cp0, cp1, cp2, cp3);

			return Vector3.Normalize(pointB - pointA);
		}
	
		/***************************************************************************
		 * TangentAt
		 * The tangent at @param t of the spline.
		 *************************************************************************** 
		 */
		public static Vector3 TangentAt(float t, Vector3 cp0, Vector3 cp1, Vector3 cp2, Vector3 cp3) {

			Vector3 pointA = CurvePointAt(t, cp0, cp1, cp2, cp3);
			Vector3 pointB = CurvePointAt(t + 0.05f, cp0, cp1, cp2, cp3);

			return pointB - pointA;
		}

		public static void Test() {

			Vector3 cp0 = new Vector3{x = 0.0f, y = 0.0f, z = 0.0f};
			Vector3 cp1 = new Vector3{x = 0.0f, y = 0.33f, z = 0.33f};
			Vector3 cp2 = new Vector3{x = 0.66f, y = 1.0f, z = 0.66f};
			Vector3 cp3 = new Vector3{x = 1.0f, y = 1.0f, z = 1.0f};

			Vector3 res, tangent;

			for (float t = 0; t <= 1.0f; t += 0.1f) {
				res = Curve.Catmull.CurvePointAt(t, cp0, cp1, cp2, cp3);
				tangent = Curve.Catmull.NormalizedTangentAt(t, cp0, cp1, cp2, cp3);
				Debug.Log("t: " + t + " " + res.x + " " + res.y + " " + res.z);
				Debug.Log("tangent t: " + t + " " + tangent.x + " " + tangent.y + " " + tangent.z);
			}
		}
	}
}
