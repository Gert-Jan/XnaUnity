namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectPass
	{
		public EffectPass(Effect effect, EffectPass pass)
		{
		}

		public string Name { get; private set; }

		public void Apply()
		{
		}
	}
}
