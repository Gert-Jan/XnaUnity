namespace Microsoft.Xna.Framework.Graphics
{
	public class Effect
	{
		public EffectTechnique CurrentTechnique { get; set; }

		public virtual UnityEngine.Material Material { get; protected set; }

		internal virtual bool OnApply()
		{
			return false;
		}
	}
}
