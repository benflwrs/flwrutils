

namespace com.benflwrs.flwrutils.Patterns
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Pool;

	public static class Pooling
	{
		private static Dictionary<string, Pooler> poolMap;

		private static bool IsInitialized = false;

		//[InitializeOnLoadMethod]
		public static void InitializePooling()
		{
			//PoolContainer = new GameObject(nameof(PoolContainer));
			//GameObject.DontDestroyOnLoad(PoolContainer);
			Debug.Log("Initialize Pooling");
			poolMap = new Dictionary<string, Pooler>();
			IsInitialized = true;
		}

		public static Pooler GetPool(string poolID)
		{
			if (!IsInitialized) InitializePooling();
			return poolMap[poolID];
		}

		public static bool HasPool(string poolID)
		{
			if (!IsInitialized) InitializePooling();
			return poolMap.ContainsKey(poolID);
		}

		public static Pooler CreatePool(string poolID)
		{
			if (HasPool(poolID))
			{
				Debug.LogError($"CreatePool: Pool of ID '{poolID}' already exists");
				return null;
			}

			Pooler newPool = new GameObject($"Pool_{poolID}").AddComponent<Pooler>();
			newPool.InitializePool(poolID);
			//AddPool(newPool);
			//poolMap.Add(poolID, newPool);

			return newPool;
		}

		public static void AddPool(Pooler pool)
		{
			if (!IsInitialized) InitializePooling();
			poolMap.Add(pool.ID, pool);
		}

		public static void RemovePool(Pooler pool)
		{
			if (!IsInitialized) InitializePooling();
			poolMap.Remove(pool.ID);
		}

		public static void DestroyPool(string poolID)
		{
			if (!HasPool(poolID))
			{
				Debug.LogError($"Pool of ID '{poolID}' doesn't exists");
				return;
			}

			Pooler poolToDestroy = GetPool(poolID);
			RemovePool(poolToDestroy);

			//Destroy pool
			//...
			GameObject.Destroy(poolToDestroy.gameObject);

		}

		public static bool TryRelease(GameObject gameObject)
		{
			PoolObject poolObject = gameObject.GetComponent<PoolObject>();

			if (poolObject)
			{
				poolObject.Release();
				return true;
			}

			return false;
		}

	}

	public class Pooler : MonoBehaviour
	{
		[Header("Pool Settings")]
		[SerializeField] public string ID;
		[SerializeField] protected PoolObject prefab;
		[SerializeField] protected int defaultCapacity = 5;
		[SerializeField] protected int maxSize = 30;

		[Header("Pool Emergency Tactics")]

		public bool hurryObjects = true;
		public bool releaseFirstActiveWhenEmpty = true;
		public int hurryWhenRemaining = 5;
		public int activeObjectsToHurry = 5;

		protected ObjectPool<PoolObject> objectPool;
		protected List<PoolObject> activePooledObjects;
		protected bool IsInitialized = false;
		public float emptinessRatio => (float)objectPool.CountActive / maxSize;

		public void InitializePool(string poolID)
		{
			ID = poolID;
			Pooling.AddPool(this);
			activePooledObjects = new();
			IsInitialized = true;
		}

		protected void OnValidate()
		{
			gameObject.name = $"Pool_{ID}";
		}

		public void Awake()
		{
			if (!IsInitialized)
				InitializePool(ID);

			objectPool = new ObjectPool<PoolObject>
			(
				CreateObject,
				OnGetObject,
				OnReleaseObject,
				OnDestroyObject,
				true,
				defaultCapacity,
				maxSize
			);

			CreateBaseCapacity(defaultCapacity);
		}

		public void OnDestroy()
		{
			if (Pooling.HasPool(ID))
			{
				Pooler pool = Pooling.GetPool(ID);
				//if true it means we're not being destroy by Pooling.DestroyPool
				if (this == pool)
				{
					objectPool.Clear();
					Pooling.RemovePool(this);
				}
			}
		}

		public PoolObject Get()
		{
			if (hurryObjects)
			{
				if (objectPool.CountInactive <= hurryWhenRemaining)
				{
					HurryObjects(activeObjectsToHurry);
				}
			}

			if (releaseFirstActiveWhenEmpty && objectPool.CountInactive <= 0 && activeObjectsToHurry > 0)
			{
				activePooledObjects[0].Release();
			}

			return objectPool.Get();
		}

		public void Release(PoolObject poolObject)
		{
			poolObject.IsHurried = false;
			objectPool.Release(poolObject);
		}

		public PoolObject CreateObject()
		{
			//Setting original prefab inactive to avoid Awake() call
			prefab.gameObject.SetActive(false);

			PoolObject newPoolObject = Instantiate<PoolObject>(prefab);
			newPoolObject.gameObject.SetActive(false);
			//newPoolObject.pooler = this;
			newPoolObject.SetPooler(this);
			newPoolObject.transform.SetParent(transform);

			//Setting prefab back
			prefab.gameObject.SetActive(true);

			return newPoolObject;
		}

		public void OnGetObject(PoolObject poolObject)
		{
			poolObject.OnGet?.Invoke();
			poolObject.transform.SetParent(null);
			poolObject.gameObject.SetActive(true);

			if (!activePooledObjects.Contains(poolObject))
				activePooledObjects.Add(poolObject);
		}

		public void OnReleaseObject(PoolObject poolObject)
		{
			poolObject.OnRelease?.Invoke();
			poolObject.transform.SetParent(transform);
			poolObject.gameObject.SetActive(false);

			if (activePooledObjects.Contains(poolObject))
				activePooledObjects.Remove(poolObject);
		}

		public void OnDestroyObject(PoolObject poolObject)
		{
			//poolObject.pooler = null;
			if (activePooledObjects.Contains(poolObject))
				activePooledObjects.Remove(poolObject);

			poolObject.SetPooler(null);
			Destroy(poolObject);
		}

		public void CreateBaseCapacity(int quantity)
		{
			PoolObject poolObject;
			for (int i = 0; i < quantity; i++)
			{
				poolObject = objectPool.Get();
			}

			for (int i = activePooledObjects.Count - 1; i >= 0; i--)
			{
				Release(activePooledObjects[i]);
			}
		}

		public void HurryObjects(int quantity)
		{
			int hurriedObjects = 0;
			PoolObject poolObject;
			for (int i = 0; i < activePooledObjects.Count; i++)
			{
				poolObject = activePooledObjects[i];
				if (!poolObject.IsHurried)
				{
					poolObject.HurryUp();
					hurriedObjects++;
				}
				if (hurriedObjects >= quantity)
					return;
			}
		}
	}
}
