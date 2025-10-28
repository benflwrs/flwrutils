using bonnie.utils;
using UnityEngine;
using System;
using System.Collections.Generic;

public class GizmoUtils : Singleton<GizmoUtils>
{
	public const int MAX_TEMP_GIZMOS = 100;

	[System.Serializable]
	public struct GizmoData
	{
		public float timeStarted;
		public float duration;
		public Action action;
		public Color color;

		public GizmoData(Action pAction, float pDuration, float pTimeStarted, Color pColor)
		{
			duration = pDuration;
			action = pAction;
			timeStarted = pTimeStarted;
			color = pColor;
		}
	}

	public List<GizmoData> gizmoList;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	public override void Awake()
	{
		base.Awake();
		gizmoList = new List<GizmoData>();
	}

	protected static void TemporaryGizmo(Action action, float duration)
	{
#if (UNITY_EDITOR)
		if (Instance != null)
		{
			if (Instance.gizmoList.Count >= MAX_TEMP_GIZMOS)
				Instance.gizmoList.RemoveAt(0);
			Instance.gizmoList.Add(new GizmoData(action, duration, Time.time, Gizmos.color));
		}
#endif
	}

	public void OnDrawGizmos()
	{
		GizmoData currentData;

		for (int i = gizmoList.Count - 1; i >= 0; i--)
		{
			currentData = gizmoList[i];
			if (Time.time >= currentData.timeStarted + currentData.duration)
				gizmoList.RemoveAt(i);
			else
			{
				Gizmos.color = currentData.color;
				currentData.action();
			}
		}
	}

#region Gizmos Override
	public static void DrawLine(Vector3 from, Vector3 to, float duration = 0.02f)
	{
		TemporaryGizmo(() => Gizmos.DrawLine(from, to), duration);
	}

	public static void DrawSphere(Vector3 center, float radius, float duration = 0.02f)
	{
		TemporaryGizmo(() => Gizmos.DrawSphere(center, radius), duration);
	}

	public static void DrawWireSphere(Vector3 center, float radius, float duration = 0.02f)
	{
		TemporaryGizmo(() => Gizmos.DrawSphere(center, radius), duration);
	}

	public static void DrawCube(Vector3 center, Vector3 size, float duration = 0.02f)
	{
		TemporaryGizmo(() => Gizmos.DrawCube(center, size), duration);
	}

	public static void DrawWireCube(Vector3 center, Vector3 size, float duration = 0.02f)
	{
		TemporaryGizmo(() => Gizmos.DrawWireCube(center, size), duration);
	}


	#endregion
	//#if UNITY_EDITOR
	[RuntimeInitializeOnLoadMethod]
	public static void Initialize()
	{
#if (UNITY_EDITOR)
		GizmoUtils Instance = new GameObject("DebugGizmos").AddComponent<GizmoUtils>();
#endif
	}
//#endif
}
