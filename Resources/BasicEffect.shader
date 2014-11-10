
Shader "Custom/SpriteShader"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			uniform float4x4 _WorldViewProj;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
			/*
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
				*/
				appdata_t input = IN;
				v2f output;

				output.vertex = mul(_WorldViewProj, IN.vertex);
				//output.vertex = float4(0.5, 1.0, 0.5, 1.0) * input.vertex;
				//output.vertex = mul(_WorldViewProj, float4(0.5, 1.0, 0.5, 1.0) * input.vertex);
				output.texcoord = input.texcoord;
				output.color = input.color;

				return output;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : COLOR
			{
				//return IN.color * float4(1, 0, 0, 1);
				//return float4(1, 0, 0, 0);
				return tex2D(_MainTex, IN.texcoord) * IN.color;
			}
		ENDCG
		}
	}
}


/*
Shader "Custom/SpriteShader"
{
	Properties
	{
		_MainTex ("Some texture", 2D) = "white" {}
		_Diffuse ("Diffuse color", Color) = (1.0, 1.0, 1.0, 0.0)
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			//uniform sampler2D _MainTex;
			//uniform float4 _Diffuse;
			//uniform float4x4 _WorldViewProj;

			struct vertInput
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 texcoords : TEXCOORD0;
			};

			struct vertOutput
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float4 texcoords : TEXCOORD0;
			};

			vertOutput vert(vertInput input)
			{
				vertOutput output;

				output.vertex = input.vertex;
				//output.vertex = float4(0.5, 1.0, 0.5, 1.0) * input.vertex;
				//output.vertex = mul(_WorldViewProj, float4(0.5, 1.0, 0.5, 1.0) * input.vertex);
				//output.texcoords = input.texcoords;
				//output.color = input.color * _Diffuse;

				return output;
			}

			float4 frag(vertOutput input) : COLOR
			{
				return float4(1.0, 0, 0, 1.0);
				//return tex2D(_MainTex, input.texcoords.xy) * input.color;
			}

			ENDCG
		}
	}
}
*/
