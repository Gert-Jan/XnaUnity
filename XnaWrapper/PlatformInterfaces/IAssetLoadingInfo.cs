namespace XnaWrapper.PlatformInterfaces
{
	public interface IAssetLoadingInfo
	{
		int MaxAssetsLoadingParallel();
		bool LoadFromAssetDatabase();

		UnityEngine.Object LoadFromAssetDatabase(string path);
	}
}
