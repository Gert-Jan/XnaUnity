namespace XnaWrapper.PlatformInterfaces
{
	public abstract class XnaAssetProvider
	{
		public abstract UnityEngine.Object LoadAsset(string path);
		public abstract int MaxAssetsPerUpdate();
	}
}
