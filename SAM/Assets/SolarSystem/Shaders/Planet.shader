Shader "SolarSystem/Planet" {

	Properties {
		_MainTex("Texture", 2D) = "black" {}
		_NightTex("Night Texture", 2D) = "black" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_SpecularMap("Specular Map", 2D) = "black" {}
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Glossiness("Glossiness", Float) = 1.0
		_SurfaceShininess("Surface Shininess", Float) = 1.0
		_SurfaceFalloff("Surface Falloff", Float) = 1.0
		_AtmoColor("Atmosphere Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_AtmoShininess("Atmosphere Shininess", Float) = 1.0
		_AtmoFalloff("Atmosphere Falloff", Float) = 1.0
		_AtmoSize("Atmosphere Size", Float) = 1.0
	}
	
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		/*
		Pass{
			Tags{ "LightMode" = "ForwardBase" }
			Name "BASE"
			// Point light is not supported in ForwardBase Pass
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			struct vertexInput {
				half4 vertex : POSITION;
			};
			struct vertexOutput {
				half4 pos : SV_POSITION;
			};
			vertexOutput vert(vertexInput v) {
				vertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			half4 frag(vertexOutput i) : Color{
				return half4(0, 0, 0, 1);
			}
			ENDCG
		}*/

		Pass {
			Tags { "LightMode" = "ForwardAdd" }
			Blend One Zero
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			uniform sampler2D _MainTex;
			uniform sampler2D _NightTex;
			uniform sampler2D _BumpMap;
			uniform sampler2D _SpecularMap;
			uniform half4 _Color;
			uniform half4 _AtmoColor;
			uniform half _Glossiness;
        	uniform half _SurfaceFalloff;
        	uniform half _SurfaceShininess;
			
			struct vertexInput {
				half4 vertex : POSITION;
				half3 normal : NORMAL;
				half4 tangent : TANGENT;
				half4 texcoord : TEXCOORD0;
			};
			struct vertexOutput {
				half4 pos : SV_POSITION;
				half4 tex : TEXCOORD0;
				half3 normal : TEXCOORD1;
				half3 tangent : TEXCOORD2;
				half3 binormal : TEXCOORD3;
				half3 view : TEXCOORD4;
				half3 light : TEXCOORD5;
			};
			
			vertexOutput vert(vertexInput v) {
				vertexOutput o;
				
				half3 normalDir = normalize (mul (unity_ObjectToWorld, half4 (v.normal,0)).xyz);
				half3 viewDir = normalize (_WorldSpaceCameraPos.xyz - mul (unity_ObjectToWorld, v.vertex));
				half3 lightDir = normalize (mul (unity_ObjectToWorld, v.vertex) - _WorldSpaceLightPos0.xyz);
				
				half atmo;
				half light = saturate (dot (normalDir, -lightDir) + .1);
            	atmo = saturate (pow (1.0 - dot (viewDir, normalDir), _SurfaceFalloff) * _SurfaceShininess);
            	
            	o.pos = UnityObjectToClipPos (v.vertex);
				o.tex = v.texcoord;
				o.tex.zw = half2 (light, atmo);
				
				o.normal = normalDir;
				o.tangent = normalize (mul (float4 (v.tangent.xyz, 0), unity_WorldToObject).xyz);
				o.binormal = normalize (cross (o.normal, o.tangent) * v.tangent.w);
				o.view = viewDir;
				o.light = lightDir;
				
				return o;
			}
			
			half4 frag(vertexOutput i) : Color {
				half4 normalTex = tex2D (_BumpMap, i.tex.xy);
				half3 localCoords = half3 (2.0 * normalTex.ag - half2 (1.0, 1.0), 1.0);
				half3x3 local2WorldTranspose = half3x3(i.tangent, i.binormal, i.normal);
				half3 normalDir = normalize (mul (localCoords, local2WorldTranspose));
				half normalLight = saturate (dot (-i.light, normalDir) + .1);
				
				half4 tex = saturate(tex2D (_MainTex, i.tex.xy) + pow (dot (reflect (i.light, normalDir), i.view) * tex2D (_SpecularMap, i.tex.xy) * _Glossiness, 2));
				
				half4 nightTex = tex2D (_NightTex, i.tex.xy);
				half light = saturate (i.tex.z * 3);
				
				return lerp (nightTex, lerp (tex * normalLight, _Color, i.tex.w) * i.tex.z, light);
			}
			
			ENDCG
		}
		
		Pass {
			Tags { "LightMode" = "ForwardAdd" }
			Name "ATMOSPHERE"
			Cull Front
            Blend SrcAlpha One
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			uniform sampler2D _MainTex;
			uniform half4 _Color;
			uniform half4 _AtmoColor;
			uniform half4 _Light;
        	uniform half _AtmoFalloff;
        	uniform half _AtmoShininess;
        	uniform half _AtmoSize;
			
			struct vertexInput {
				half4 vertex : POSITION;
				half3 normal : NORMAL;
			};
			struct vertexOutput {
				half4 pos : SV_POSITION;
				half4 col : COLOR;
				half3 vertex : TEXCOORD0;
			};
			
			vertexOutput vert(vertexInput v) {
				vertexOutput o;
				
				v.vertex.xyz *= _AtmoSize;
            	o.pos = UnityObjectToClipPos (v.vertex);
				o.vertex = mul (unity_ObjectToWorld, v.vertex);
				
				half3 normalDir = normalize (mul (unity_ObjectToWorld, half4 (v.normal,0)).xyz);
				half3 viewDir = -normalize (_WorldSpaceCameraPos.xyz - mul (unity_ObjectToWorld, v.vertex));
				half3 lightDir = normalize (mul (unity_ObjectToWorld, v.vertex) - _WorldSpaceLightPos0.xyz);
				half atmo;
				half light = saturate (dot (normalDir, -lightDir) + .5);
            	atmo = saturate (pow (dot (viewDir, normalDir), _AtmoFalloff) * _AtmoShininess);
				o.col = _AtmoColor * light * atmo;
				
				return o;
			}
			
			half4 frag(vertexOutput i) : Color {
				return i.col;
			}
			
			ENDCG
		}
	}
	
	Fallback "Diffuse"
}