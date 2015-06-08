using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public class BasicEffectFlip : BasicEffect
    {
        private static readonly UnityEngine.Shader shader = UnityEngine.Shader.Find("Custom/SpriteShaderFlip");

        public BasicEffectFlip(GraphicsDevice device) :
            base(device)
        {
            base.Material = new UnityEngine.Material(shader);
        }
    }
}
