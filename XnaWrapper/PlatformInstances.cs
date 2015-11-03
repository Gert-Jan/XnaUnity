using XnaWrapper.PlatformInterfaces;

namespace XnaWrapper
{
	public static class PlatformInstances
	{
		public static bool IsEditor;

		public static IGamePad GamePad;
		public static IAssetProvider AssetProvider;
		public static IStorage Storage;
	}

}
