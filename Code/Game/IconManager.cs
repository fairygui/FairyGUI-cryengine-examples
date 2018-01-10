using CryEngine;
using CryEngine.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using FairyGUI;
using System.IO;

public delegate void LoadCompleteCallback(NTexture texture);
public delegate void LoadErrorCallback(string error);

/// <summary>
/// Use to load icons from asset bundle, and pool them
/// </summary>
public class IconManager : IGameUpdateReceiver
{
	static IconManager _instance;
	public static IconManager inst
	{
		get
		{
			if (_instance == null)
				_instance = new IconManager();
			return _instance;
		}
	}

	public const int POOL_CHECK_TIME = 30;
	public const int MAX_POOL_SIZE = 10;
	public const int WORKLOAD_PER_FRFAME = 2;

	class LoadItem
	{
		public string url;
		public LoadCompleteCallback onSuccess;
		public LoadErrorCallback onFail;
	}
	List<LoadItem> _items;
	Hashtable _pool;
	string _basePath;
	float _lastCheckPool;

	public IconManager()
	{
		_items = new List<LoadItem>();
		_pool = new Hashtable();
		_basePath = Engine.DataDirectory + "/Icons/";
		_lastCheckPool = Engine.Timer.GetCurrTime();

		GameFramework.RegisterForUpdate(this);
	}

	public void LoadIcon(string url,
					LoadCompleteCallback onSuccess,
					LoadErrorCallback onFail)
	{
		LoadItem item = new LoadItem();
		item.url = url;
		item.onSuccess = onSuccess;
		item.onFail = onFail;
		_items.Add(item);
	}

	public void OnUpdate()
	{
		int handled = 0;
		while (_items.Count > 0)
		{
			LoadItem item = _items[0];
			_items.RemoveAt(0);

			NTexture ntex;
			if (_pool.ContainsKey(item.url))
			{
				Log.Info("hit " + item.url);

				ntex = (NTexture)_pool[item.url];
				ntex.refCount++;

				if (item.onSuccess != null)
					item.onSuccess(ntex);
			}
			else
			{
				try
				{
					Bitmap bm = new Bitmap(_basePath + item.url);
					Texture tex = new Texture(bm.Width, bm.Height, bm.GetPixels());
					bm.Dispose();
					ntex = new NTexture(tex);
					ntex.refCount++;

					if (item.onSuccess != null)
						item.onSuccess(ntex);
				}
				catch (Exception err)
				{
					//Log.Warning("load texture '" + item.url + "' failed.");
					ntex = NTexture.Empty;

					if (item.onFail != null)
						item.onFail(err.Message);
				}

				_pool[item.url] = ntex;
			}

			handled++;
			if (handled == WORKLOAD_PER_FRFAME)
				break;
		}

		float now = Engine.Timer.GetCurrTime();
		if (now - _lastCheckPool > POOL_CHECK_TIME)
		{
			_lastCheckPool = now;
			int cnt = _pool.Count;
			if (cnt > MAX_POOL_SIZE)
			{
				ArrayList toRemove = null;
				foreach (DictionaryEntry de in _pool)
				{
					string key = (string)de.Key;
					NTexture texture = (NTexture)de.Value;
					if (texture.refCount == 0)
					{
						if (toRemove == null)
							toRemove = new ArrayList();
						toRemove.Add(key);
						texture.Dispose();

						//Log.Info("free icon " + de.Key);

						cnt--;
						if (cnt <= 8)
							break;
					}
				}
				if (toRemove != null)
				{
					foreach (string key in toRemove)
						_pool.Remove(key);
				}
			}
		}
	}
}