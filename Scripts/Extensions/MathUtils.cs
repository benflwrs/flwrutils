using Unity.VisualScripting;
using UnityEngine;

public static partial class MathUtils
{
	public const float ROOT_2 = 1.41421356237f;
}

public static partial class FloatExtension
{
	public static bool IsEqual(this float a, float b, float epsilon = 0.0001f)
	{
		return Mathf.Abs(a - b) < epsilon;
	}
}

public static partial class Vector3Extension
{
	public static bool IsEqual(this Vector3 a, Vector3 b, float epsilon = 0.0001f)
	{
		return a.x.IsEqual(b.x) && a.y.IsEqual(b.y) && a.z.IsEqual(b.z);
	}

	public static void Set(ref this Vector3 a, Vector3 b)
	{
		a.x = b.x;
		a.y = b.y;
		a.z = b.z;
	}

	public static Vector3 WithX(this Vector3 a, float x) { a.x = x; return a; }
	public static Vector3 WithY(this Vector3 a, float y) { a.y = y; return a; }
	public static Vector3 WithZ(this Vector3 a, float z) { a.z = z; return a; }

	public static Vector3 FindBarycenter(Vector3[] points)
	{
		Vector3 barycenter = default;
		int listLength = points.Length;

		for (int i = 0; i < listLength; i++)
		{
			barycenter += points[i];
		}

		return barycenter;
	}
}
