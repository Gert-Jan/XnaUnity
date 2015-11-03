namespace XnaWrapper.PlatformInterfaces
{
	public interface IAssetProvider
	{
		UnityEngine.Object LoadAsset(string path);
		int MaxAssetsPerUpdate();
	}
}
