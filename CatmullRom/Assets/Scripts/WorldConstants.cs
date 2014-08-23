using UnityEngine;

/*==================================================
 * In this class we define the world constants.
 *================================================== 
 */
public class WorldConstants {

	public static Vector3 worldUp = new Vector3() { x = 0.0f, y = 1.0f, z = 0.0f };

	public static Vector3 GetWorldUp() {
		return worldUp;
	}
}
