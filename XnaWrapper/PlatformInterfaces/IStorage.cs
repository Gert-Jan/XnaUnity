using Microsoft.Xna.Framework;

namespace XnaWrapper.PlatformInterfaces
{
	public interface IStorage
	{
		string GetLocalStorage(PlayerIndex index);
		string GetLocalStorage();
	}
}
