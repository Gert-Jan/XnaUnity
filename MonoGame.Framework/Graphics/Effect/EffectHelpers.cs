//#region File Description
//-----------------------------------------------------------------------------
// EffectHelpers.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
//#endregion

//#region Using Statements
using System;
//#endregion

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Track which effect parameters need to be recomputed during the next OnApply.
    /// </summary>
    [Flags]
    internal enum EffectDirtyFlags
    {
        WorldViewProj   = 1,
        World           = 2,
        EyePosition     = 4,
        MaterialColor   = 8,
        Fog             = 16,
        FogEnable       = 32,
        AlphaTest       = 64,
        ShaderIndex     = 128,
        All             = -1
    }


    /// <summary>
    /// Helper code shared between the various built-in effects.
    /// </summary>
    internal static class EffectHelpers
    {
        /// <summary>
        /// Lazily recomputes the world+view+projection matrix and
        /// fog vector based on the current effect parameter settings.
        /// </summary>
        internal static EffectDirtyFlags SetWorldViewProjAndFog(EffectDirtyFlags dirtyFlags,
                                                                ref Matrix world, ref Matrix view, ref Matrix projection, ref Matrix worldView,
                                                                ref Matrix worldViewProj)
        {
            // Recompute the world+view+projection matrix?
            if ((dirtyFlags & EffectDirtyFlags.WorldViewProj) != 0)
            {
                Matrix.Multiply(ref world, ref view, out worldView);
                Matrix.Multiply(ref worldView, ref projection, out worldViewProj);
                
                //worldViewProjParam.SetValue(worldViewProj);
                
                dirtyFlags &= ~EffectDirtyFlags.WorldViewProj;
            }

            return dirtyFlags;
        }

        /// <summary>
        /// Sets the diffuse/emissive/alpha material color parameters.
        /// </summary>
        internal static void SetMaterialColor(ref Vector3 diffuseColor, 
                                              EffectParameter diffuseColorParam)
        {
            // Desired lighting model:
            //
            //     ((AmbientLightColor + sum(diffuse directional light)) * DiffuseColor) + EmissiveColor
            //
            // When lighting is disabled, ambient and directional lights are ignored, leaving:
            //
            //     DiffuseColor + EmissiveColor
            //
            // For the lighting disabled case, we can save one shader instruction by precomputing
            // diffuse+emissive on the CPU, after which the shader can use DiffuseColor directly,
            // ignoring its emissive parameter.
            //
            // When lighting is enabled, we can merge the ambient and emissive settings. If we
            // set our emissive parameter to emissive+(ambient*diffuse), the shader no longer
            // needs to bother adding the ambient contribution, simplifying its computation to:
            //
            //     (sum(diffuse directional light) * DiffuseColor) + EmissiveColor
            //
            // For futher optimization goodness, we merge material alpha with the diffuse
            // color parameter, and premultiply all color values by this alpha.
            Vector4 diffuse = new Vector4();

            diffuse.X = diffuseColor.X;
            diffuse.Y = diffuseColor.Y;
            diffuse.Z = diffuseColor.Z;
            diffuse.W = 1;

            diffuseColorParam.SetValue(diffuse);
        }
    }
}
