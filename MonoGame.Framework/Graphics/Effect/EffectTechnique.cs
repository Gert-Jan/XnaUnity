namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectTechnique
	{
		public EffectPassCollection Passes { get; private set; }

		internal EffectTechnique(EffectPass onlyPass)
		{
			Passes = new EffectPassCollection(new EffectPass[] { onlyPass });
		}
	}
}
