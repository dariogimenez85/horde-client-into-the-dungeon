Shader "Mobile/CurtainRevealTransition"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0, 0, 0, 1)
        _Progress ("Progress", Range(0,1)) = 0
        _Direction ("Direction", Vector) = (1, 0, 0, 0)
        _Softness ("Softness", Range(0, 0.5)) = 0.1
        _Feather ("Feather", Range(0, 1)) = 0.1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }
        
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            fixed4 _Color;
            float _Progress;
            float4 _Direction;
            float _Softness;
            float _Feather;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Normalizar la dirección
                float2 dir = normalize(_Direction.xy);
                
                // Calcular la posición en la dirección de transición
                float pos = dot(i.uv, dir);
                
                // Crear el gradiente de transición
                float transition = (_Progress * (1.0 + _Softness * 2.0)) - _Softness;
                
                // Calcular la máscara con suavizado
                float mask = smoothstep(transition - _Feather, transition + _Feather, pos);
                
                // El alpha va de 1 (opaco) a 0 (transparente)
                float alpha = 1.0 - mask;
                
                // Devolver el color con alpha modulado
                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
    
   // Fallback "Mobile/Transparent"
}