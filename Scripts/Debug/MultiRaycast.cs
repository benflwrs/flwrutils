using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
public class MultiRaycastEditor : Editor
{
	MultiRaycast targetScript;

	void OnEnable()
    {
        targetScript = (MultiRaycast)target;
    }

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
	}
}
#endif

public class MultiRaycast : MonoBehaviour
{
	[SerializeField] protected List<Vector3> rayOrigins;
	[SerializeField] protected Vector3 direction;
	[SerializeField] protected float distance = 0.5f;
	[SerializeField] protected float scale = 1f;
	[SerializeField] protected LayerMask layerMask;
	public RaycastHit hitInfo { get; protected set; }
	public int rayHitIndex { get; protected set; }
	public bool hasHit { get; protected set; } = false;
	// Start is called once before the first execution of Update after the MonoBehaviour is created

	public void OnDrawGizmos()
	{
		if (!enabled) return;

		Vector3 currentOrigin;
		Vector3 localDirection = transform.TransformVector(direction);
		for (int i = rayOrigins.Count - 1; i >= 0; i--)
		{
			currentOrigin = transform.TransformPoint(rayOrigins[i] * scale);
			Gizmos.color = i == rayHitIndex ? Color.green : Color.red;
			Gizmos.DrawSphere(currentOrigin, 0.05f);
			Gizmos.DrawRay(currentOrigin, localDirection * distance);
		}

	}

	protected bool Raycast(out RaycastHit finalHit, out int index)
	{
		Vector3 currentOrigin;
		Vector3 localDirection = transform.TransformVector(direction);
		RaycastHit hitInfo;
		finalHit = default;
		index = -1;

		for (int i = 0; i < rayOrigins.Count - 1; i++)
		{
			currentOrigin = transform.TransformPoint(rayOrigins[i] * scale);
			if (Physics.Raycast(currentOrigin, localDirection, out hitInfo, distance, layerMask))
			{
				finalHit = hitInfo;
				index = i;
				return true;
			}
		}
		return false;
	}

	public bool Raycast()
	{
		RaycastHit finalHit;
		int index;

		hasHit = Raycast(out finalHit, out index);
		rayHitIndex = index;
		hitInfo = finalHit;

		return hasHit;
	}

}
