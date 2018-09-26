// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/FloorShader"
{
    Properties
    {
		_Color ("Base Color", Color) = (1,1,1,1)
		_UnderWaterColor ("Under Water Color", Color) = (1,1,1,1)
		_BeachColor ("Beach Color", Color) = (1,1,1,1)
		_SlopeColor ("Slope Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {
            Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
 
            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

			fixed4 _Color;
			fixed4 _UnderWaterColor;
			fixed4 _BeachColor;
			fixed4 _SlopeColor;

			struct vertexdata_base {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 neighbours : TEXCOORD3; 
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            struct v2f
            {
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
                nointerpolation fixed3 diff : COLOR0;
                nointerpolation fixed3 ambient : COLOR1;
				nointerpolation fixed4 heightColor : COLOR2;
				nointerpolation fixed3 normal : NORMAL;
                float4 pos : SV_POSITION;
            };

			float rand(float3 co) {
				return frac( sin( dot(co.xyz ,float3(12.9898,78.233,45.5432) )) * 43758.5453);
			}

            v2f vert (vertexdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));
				o.normal = v.normal;
				float neighbour1 = v.neighbours.x;
				float neighbour2 = v.neighbours.y;
				float y = v.vertex.y;
				if ( y != neighbour1 || y != neighbour2) {
					o.heightColor = _SlopeColor;
					if (y <= -11.25 && neighbour1 <= -11.25 && neighbour2 <= -11.25) {
						o.heightColor = _BeachColor;
					}
				} else {
					o.heightColor = y < -13 ? _UnderWaterColor : _Color;
				}

                // compute shadows data
                TRANSFER_SHADOW(o)
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.heightColor;
                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff * shadow + i.ambient;
                col.rgb *= lighting;
				col.rgb *= (0.5 + (rand(i.normal) / 2));
                return col;
            }
            ENDCG
        }
 
        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
