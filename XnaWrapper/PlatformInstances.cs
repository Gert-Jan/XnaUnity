using XnaWrapper.PlatformInterfaces;

namespace XnaWrapper
{
	public static class PlatformInstances
	{
		public static bool IsEditor = true;

		public static IGamePad GamePad;
		public static IAssetLoadingInfo AssetLoadingInfo;
		public static IStorage Storage;
	}

}
