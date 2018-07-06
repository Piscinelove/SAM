Shader "SolarSystem/Ring" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_ShadowTex ("Texture", 2D) = "white" {}
		_Light("Light", Vector) = (1.0, 1.0, 1.0, 1.0)
	}

	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Pass {
			Tags{ "LightMode" = "ForwardAdd" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Front
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			uniform sampler2D _MainTex;
			uniform sampler2D _ShadowTex;
			uniform half4 _Light;
			
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

            	o.pos = UnityObjectToClipPos(v.vertex);
				o.tex = v.texcoord;

				half3 normalDir = normalize(mul(unity_ObjectToWorld, half4(v.normal, 0)).xyz);
				half3 lightDir = normalize(mul(unity_ObjectToWorld, v.vertex) - _WorldSpaceLightPos0.xyz);
				o.tex.z = saturate(dot(normalDir, lightDir) * .6 + .4);
				
				return o;
			}
			
			half4 frag(vertexOutput i) : Color {
				half4 c = tex2D(_MainTex, i.tex.xy) * tex2D(_ShadowTex, half2(i.tex.x / _Light.x, i.tex.y));
				half3 rgb = c.rgb * i.tex.z;
				return half4(rgb, c.a);
			}
			
			ENDCG
		}

		Pass{
			Tags{ "LightMode" = "ForwardAdd" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Back

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D _MainTex;
			uniform sampler2D _ShadowTex;
			uniform half4 _Light;

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

				o.pos = UnityObjectToClipPos(v.vertex);
				o.tex = v.texcoord;

				half3 normalDir = normalize(mul(unity_ObjectToWorld, half4(v.normal, 0)).xyz);
				half3 lightDir = normalize(mul(unity_ObjectToWorld, v.vertex) - _WorldSpaceLightPos0.xyz);
				o.tex.z = saturate(dot(normalDir, -lightDir) * .6 + .4);

				return o;
			}

			half4 frag(vertexOutput i) : Color{
				half4 c = tex2D(_MainTex, i.tex.xy) * tex2D(_ShadowTex, half2(i.tex.x / _Light.x, i.tex.y));
				half3 rgb = c.rgb * i.tex.z;
				return half4(rgb, c.a);
			}

				ENDCG
			}
		
	}

	Fallback "Transparent/VertexLit"
}