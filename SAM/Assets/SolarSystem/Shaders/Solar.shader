// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SolarSystem/Solar" {

	Properties {
		_MainTex("Texture", 2D) = "black" {}
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_SurfaceShininess("Surface Shininess", Float) = 1.0
		_SurfaceFalloff("Surface Falloff", Float) = 1.0
	}
	
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			uniform sampler2D _MainTex;
			uniform half4 _Color;
        	uniform half _SurfaceFalloff;
        	uniform half _SurfaceShininess;
			
			struct vertexInput {
				half4 vertex : POSITION;
				half3 normal : NORMAL;
				half4 texcoord : TEXCOORD0;
			};
			struct vertexOutput {
				half4 pos : SV_POSITION;
				half4 tex : TEXCOORD0;
			};
			
			vertexOutput vert(vertexInput v) {
				vertexOutput o;
				
				half3 normalDir = normalize (mul (unity_ObjectToWorld, half4 (v.normal,0)).xyz);
				half3 viewDir = normalize (_WorldSpaceCameraPos.xyz - mul (unity_ObjectToWorld, v.vertex));
				
				half atmo;
            	atmo = saturate (pow (1.0 - dot (viewDir, normalDir), _SurfaceFalloff) * _SurfaceShininess);
            	
            	o.pos = UnityObjectToClipPos (v.vertex);
				o.tex = v.texcoord;
				o.tex.w = atmo;
				
				return o;
			}
			
			half4 frag(vertexOutput i) : Color {
				
				half4 tex = tex2D (_MainTex, i.tex.xy);
				
				return lerp (tex, _Color, i.tex.w);
			}
			
			ENDCG
		}
		
	}
	
	Fallback "Diffuse"
}