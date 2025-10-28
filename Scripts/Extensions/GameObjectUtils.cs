using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static partial class GameObjectExtension
{
	public static void SetLayerRecursively(this GameObject gameObject, int layer)
	{
		gameObject.layer = layer;

		if (gameObject.transform.childCount > 0)
		{
			for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
			{
				gameObject.transform.GetChild(i).gameObject.SetLayerRecursively(layer);
			}
		}
	}


}

public static partial class TransformUtils
{
	public static Quaternion LocalDisplacementToRotation(Vector3 localPoint, Vector3 localDisplacement)
	{
		Vector3 localPointNormalized = localPoint.normalized;
		Vector3 localDisplacementNormalized = localDisplacement.normalized;
		Vector3 vectorToDisplacedLocation = localPoint + localDisplacement;
		Vector3 vectorToDisplacedLocationNormalized = vectorToDisplacedLocation.normalized;
		float dot = Mathf.Clamp(Vector3.Dot(localPointNormalized, vectorToDisplacedLocationNormalized), -1f, 1f);

		float angleRotation = Mathf.Acos(dot) * Mathf.Rad2Deg;
		Vector3 axisRotation = Vector3.Cross(localDisplacementNormalized, -localPointNormalized);

		return Quaternion.Euler(axisRotation * angleRotation);
	}

	public static void AddDisplacementAtPoint(Vector3 localPoint, Vector3 localDisplacement, out Quaternion rotation, out Vector3 resiudalDisplacement)
	{
		Vector3 localPointNormalized = localPoint.normalized;
		Vector3 localDisplacementNormalized = localDisplacement.normalized;
		Vector3 vectorToDisplacedLocation = localPoint + localDisplacement;
		Vector3 vectorToDisplacedLocationNormalized = vectorToDisplacedLocation.normalized;
		float dot = Mathf.Clamp(Vector3.Dot(localPointNormalized, vectorToDisplacedLocationNormalized), -1f, 1f);

		float angleRotation = Mathf.Acos(dot) * Mathf.Rad2Deg;
		Vector3 axisRotation = Vector3.Cross(localDisplacementNormalized, -localPointNormalized);

		rotation = Quaternion.Euler(axisRotation * angleRotation);

		resiudalDisplacement = vectorToDisplacedLocation - (rotation * localPoint);
	}
}
