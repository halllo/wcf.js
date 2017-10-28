using System;
using System.Collections.Generic;
using System.Threading;

namespace WCFX.DesktopClient
{
	public class WeakCache : IDisposable
	{
		public WeakCache() : this(new WeakOnlyStrategy())
		{
		}

		public WeakCache(IStrategy strategie)
		{
			mCache = new EasyDictionary<IComparable, CacheEntry>();
			mStrategie = strategie;
			keepRunning = true;
			cacheCleaner = new Thread(CleanCache);
			cacheCleaner.IsBackground = true;
			cacheCleaner.Name = "WeakCache Thread - " + DateTime.Now.ToShortDateString();
			cacheCleaner.Start();
		}

		/// <summary>
		/// Zeitraum (in ms) nach dem regelmässig der Cache bereinigt wird. (Default ist 10000ms)
		/// </summary>
		public int CleanInterval
		{
			get { return mCleanInterval; }
			set { mCleanInterval = value; }
		}

		/// <summary>
		/// Die Strategie das Caches.
		/// </summary>
		public IStrategy Strategie
		{
			get { return mStrategie; }
			set { mStrategie = value; }
		}

		public int Count
		{
			get { return mCache.Count; }
		}

		public virtual void Add(IComparable key, object value)
		{
			lock (mCache)
			{
				var entry = new CacheEntry(value, mStrategie);
				mCache.Add(key, entry);
			}
		}

		public bool ContainsKey(IComparable key)
		{
			return Get<object>(key) != null;
		}

		public virtual T Get<T>(IComparable key)
		{
			CacheEntry entry = null;
			lock (mCache)
			{
				entry = mCache.ContainsKey(key) ? mCache[key] : null;
			}
			if (entry == null)
			{
				return default(T);
			}

			return entry.InternalGetEntry<T>();
		}

		public virtual void Remove(IComparable key)
		{
			lock (mCache)
			{
				if (mCache.ContainsKey(key))
				{
					var entry = mCache[key];
					mCache.Remove(key);
					if (OnRemove != null)
					{
						OnRemove(entry.InternalGetEntry<object>());
					}
				}
			}
		}

		protected virtual void OnObjectCleaned(object obj)
		{
		}

		protected virtual void OnThreadRun()
		{
		}

		private void CleanCache()
		{
			while (true && keepRunning)
			{
				try
				{
					OnThreadRun();
					var keyList = new List<IComparable>();
					var objList = new List<object>();
					var keys = new IComparable[0];
					lock (mCache)
					{
						ICollection<IComparable> mKeys = mCache.Keys;
						if (mKeys != null)
						{
							keys = new IComparable[mKeys.Count];
							mKeys.CopyTo(keys, 0);
						}
					}
					foreach (var key in keys)
					{
						var entry = mCache[key];
						if (entry != null && entry.HasStrongRef)
						{
							if (mStrategie.CanRemove(entry))
							{
								keyList.Add(key);
							}
							else if (mStrategie.CanMakeWeak(entry))
							{
								entry.MakeWeak();
							}
						}
						else if (entry == null || !entry.IsAlive)
						{
							keyList.Add(key);
							objList.Add(entry.GetEntry<object>());
						}
					}

					//Console.WriteLine("Cleaning cache...(" + list.Count + " elements to remove.)");
					foreach (var key in keyList)
					{
						Remove(key);
					}
					//Console.WriteLine("Cleaning cache...(" + list.Count + " elements to remove.)");
					foreach (var obj in objList)
					{
						OnObjectCleaned(obj);
					}

					Thread.Sleep(mCleanInterval);
				}
				catch (ThreadAbortException)
				{
					keepRunning = false;
					//Bye, and thanks for all the fish...
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					//Das ist nur zur Sicherheit, damit der Thread nicht abbricht.
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			keepRunning = false;
			cacheCleaner.Abort();
			mCache.Clear();
		}

		#endregion

		protected static object semaphore = new object();
		private readonly Thread cacheCleaner = null;
		private readonly EasyDictionary<IComparable, CacheEntry> mCache;
		public Action<object> OnRemove;
		private bool keepRunning = true;

		/// <summary>
		/// Membervariable der Eigenschaft CleanInterval.
		/// </summary>
		private int mCleanInterval = 10000;

		/// <summary>
		/// Membervariable der Eigenschaft Strategie.
		/// </summary>
		private IStrategy mStrategie = null;
	}































	public class EasyDictionary<TKey, TValue> : Dictionary<TKey, TValue>
	{
		public new TValue this[TKey key]
		{
			get
			{
				lock (semaphore)
				{
					var ret = default(TValue);
					base.TryGetValue(key, out ret);
					return ret;
				}
			}
		}

		public new void Add(TKey key, TValue value)
		{
			lock (semaphore)
			{
				if (base.ContainsKey(key))
				{
					base.Remove(key);
				}
				base.Add(key, value);
			}
		}

		public new bool Remove(TKey key)
		{
			lock (semaphore)
			{
				try
				{
					return base.Remove(key);
				}
				catch (KeyNotFoundException)
				{
					return false;
				}
			}
		}

		private readonly object semaphore = new object();

		//public new bool ContainsKey(TKey key)
		//{
		//    lock (semaphore)
		//    {
		//        return base.ContainsKey(key);
		//    }
		//}
	}































	public interface IStrategy
	{
		/// <summary>
		/// Gibt an, ob ein Element, das dem Cache hinzugefügt wird, initial eine starke Referenz erhalten soll.
		/// </summary>
		bool IsWeakOnly { get; }

		/// <summary>
		/// Gibt an, ob für das Cacheelement aus der starken eine schwache Referenz gemacht werden darf.
		/// Die schwache Referenz wird automatisch entfernt, wenn das Objekt vom GC recycled wurde.
		/// </summary>
		/// <param name="entry">Der Cacheeintrag</param>
		/// <returns></returns>
		bool CanMakeWeak(CacheEntry entry);

		/// <summary>
		/// Wird nur für starke Referenzen aufgerufen, um zu ermitteln, ob der Cacheeintrag entfernt werden soll.
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		bool CanRemove(CacheEntry entry);
	}





	public class FixedLifetimeAfterAccessStrategy : IStrategy
	{
		public FixedLifetimeAfterAccessStrategy(int lifetimeSeconds)
		{
			mLifetimeSeconds = lifetimeSeconds;
		}

		public bool CanMakeWeak(CacheEntry entry)
		{
			var duration = DateTime.Now - entry.LastAccess;
			return (duration.TotalSeconds > mLifetimeSeconds);
		}

		public bool CanRemove(CacheEntry entry)
		{
			var duration = DateTime.Now - entry.LastAccess;
			return (duration.TotalSeconds > mLifetimeSeconds);
		}

		bool IStrategy.IsWeakOnly
		{
			get { return false; }
		}

		private int mLifetimeSeconds;
	}



	public class WeakOnlyStrategy : IStrategy
	{
		public bool IsWeakOnly
		{
			get { return false; }
		}

		public bool CanMakeWeak(CacheEntry entry)
		{
			return true;
		}

		public bool CanRemove(CacheEntry entry)
		{
			return false;
		}
	}









































	public class CacheEntry : WeakReference
	{
		/// <summary>
		/// Erzeugt eine CacheEntry zur Verwalung eines Objektes.
		/// </summary>
		/// <param name="entry">das zu verwaltende Objekt</param>
		/// <param name="strategie">Soll das Objekt ausschließlich mit einer schwachen Referenz gehalten werden?</param>
		public CacheEntry(object entry, IStrategy strategie) : base(entry)
		{
			mEntryDate = DateTime.Now;
			mLastAccess = mEntryDate;
			EntryType = entry.GetType();
			if (!strategie.IsWeakOnly && !strategie.CanMakeWeak(this))
			{
				strongRef = entry;
			}
		}

		/// <summary>
		/// Hat das verwaltete Objekt einen starken Verweis?
		/// </summary>
		public bool HasStrongRef
		{
			get { return strongRef != null; }
		}

		/// <summary>
		/// Der Type des Cache Elements
		/// </summary>
		public Type EntryType { get; set; }

		/// <summary>
		/// Der Zeitpunkt zu dem das Objekt in den Cache gelegt wurde.
		/// </summary>
		public DateTime EntryDate
		{
			get { return mEntryDate; }
			set { mEntryDate = value; }
		}

		/// <summary>
		/// Letzter Zugriff
		/// </summary>
		public DateTime LastAccess
		{
			get { return mLastAccess; }
			set { mLastAccess = value; }
		}

		/// <summary>
		/// Anzahl Zugriffe
		/// </summary>
		public int AccessCount
		{
			get { return mAccessCount; }
			set { mAccessCount = value; }
		}

		/// <summary>
		/// Entfernt den starken Verweus auf das verwaltetet Objekt.
		/// </summary>
		public void MakeWeak()
		{
			strongRef = null;
		}

		/// <summary>
		/// Es wird versucht das enthaltene Objekt zurückzugeben. Ist dieses bereits
		/// durch den Garbage Collector recycled worden, wird null zurück gegeben.
		/// </summary>
		public T GetEntry<T>()
		{
			var entry = (IsAlive) ? (T)Target : default(T);
			return entry;
		}

		/// <summary>
		/// Benutzt GetEntry und ändert AccessDate etc. 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		internal T InternalGetEntry<T>()
		{
			var entry = GetEntry<T>();
			if (entry != null)
			{
				mLastAccess = DateTime.Now;
				mAccessCount++;
			}
			return entry;
		}

		/// <summary>
		/// Membervariable der Eigenschaft AccessCount.
		/// </summary>
		private int mAccessCount = 0;

		/// <summary>
		/// Membervariable der Eigenschaft EntryDate.
		/// </summary>
		private DateTime mEntryDate;

		/// <summary>
		/// Membervariable der Eigenschaft LastAccess.
		/// </summary>
		private DateTime mLastAccess;

		/// <summary>
		/// hiermit kann eine starke Referenz auf das verwaltetet Objekt gehalten werden.
		/// </summary>
		private object strongRef = null;
	}







}
