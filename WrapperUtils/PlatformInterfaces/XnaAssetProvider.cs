using System;

namespace XnaWrapper.PlatformInterfaces
{
	public abstract class XnaAssetProvider
	{
		public static XnaAssetProvider Instance;

		public abstract bool LoadsFromAssetBundles();

		public virtual UnityEngine.Object LoadAsset(string path) { throw new NotImplementedException("This function should be overridden"); }
		public virtual int MaxAssetsPerUpdate() { throw new NotImplementedException("This function should be overridden"); }
	}
}
