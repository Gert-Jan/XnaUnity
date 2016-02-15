namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectPass
	{
		private Effect ownerEffect;

		internal EffectPass(Effect effect)
		{
			ownerEffect = effect;
			Name = string.Empty;
        }

		// Clone
		internal EffectPass(Effect effect, EffectPass other)
		{
			ownerEffect = effect;
			Name = other.Name;
		}

		public string Name { get; private set; }

		public void Apply()
		{
			ownerEffect.device.activeEffect = ownerEffect;
		}
	}
}
