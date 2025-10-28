

namespace com.benflwrs.flwrutils.Patterns
{
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;

	[DefaultExecutionOrder(-5)]
	public class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		private static T _Instance;

		public static T Instance
		{
			get
			{
				return _Instance;
			}
			set
			{
				_Instance = value;
			}
		}

		public static bool InstanceExists => _Instance != null;

		public virtual void Awake()
		{
			if (!InstanceExists)
			{
				Instance = this as T;
			}
			else
			{
				Debug.LogWarning(name + "Instance has been destroyed");
				Destroy(this);
			}
		}

		public virtual void OnDestroy()
		{
			if (this == Instance)
			{
				Instance = null;
			}
		}
	}
}
