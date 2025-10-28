
namespace com.benflwrs.flwrutils.Patterns
{
	using System.Collections.Generic;
	using UnityEngine.Events;
	using UnityEngine.Pool;
	using UnityEngine;

	public class PoolObject : MonoBehaviour
	{
		public UnityEvent OnGet;
		public UnityEvent OnRelease;
		public UnityEvent Hurry;
		public Pooler pooler { get; protected set; }
		public string poolerID = default;
		//public bool createPoolerIfNull = true;
		public bool fetchPoolerIfNull = true;
		public bool IsHurried = false;

		public void Awake()
		{
			if (!pooler && fetchPoolerIfNull && poolerID != default)
			{
				if (Pooling.HasPool(poolerID))
				{
					pooler = Pooling.GetPool(poolerID);
				}
				//else if (createPoolerIfNull)
				//{
				//	pooler = Pooling.CreatePool(poolerID);
				//}
			}
		}

		public void SetPooler(Pooler pooler)
		{
			if (!pooler)
			{
				pooler = null;
				poolerID = "";
				return;
			}

			this.pooler = pooler;
			poolerID = pooler.ID;
		}

		public void Release()
		{
			if (pooler)
				pooler.Release(this);
			else
			{
				Destroy(gameObject);
			}
		}

		public void HurryUp()
		{
			IsHurried = true;
			Hurry?.Invoke();
		}
	}
}
