namespace XnaWrapper.PlatformInterfaces
{
	public interface IAssetLoadingInfo
	{
		int MaxAssetsLoadingParallel { get; }
		float MaxSecondsSpentLoadingPerUpdate { get; }
		bool CanLoadFromAssetDatabase { get; }

		UnityEngine.Object LoadFromAssetDatabase(string path);
	}
}
