﻿Shader "Unlit/Decoration"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile _ ON_RENDER_SCENE_VIEW
            #pragma target 5.0

            struct data
            {
                 float3 center;
                 float3 size;
                 uint buildType;
                 float height;
            };
            struct times
            {
                float time;
            };

            struct v2g
            {
                uint2 id : ANY;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            #include "./Geometries/Sphere.cginc"
            #include "./Geometries/TriangularPyramid.cginc"

            uniform StructuredBuffer<data> _Data;
            uniform StructuredBuffer<times> _Times;
            uniform float _Radius, _DofPower;
            uniform float4 _NoonColor, _NightColor;
            uniform int _IsNight;

            v2g vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
            {
                v2g o;
                o.id = uint2(id, inst);

                return o;
            }

            [maxvertexcount(128)]
            void geom(point v2g input[1], inout TriangleStream<g2f> outStream)
            {
                v2g v = input[0];
                uint id = v.id.x;
                uint inst = v.id.y;

                data d = _Data[inst];
                
                if (d.buildType == 0)
                {
                    int2 dir = _TriangularPyramidEveryDirection[id] * (d.size.xz * 0.5 - float2(_Radius, _Radius));
                    float3 p = d.center + float3(dir.x, d.size.y + d.height + _Radius * 0.5, dir.y);

                    AppendSphere(p, _Radius, -1 * ((int)inst + 1), outStream);
                }
                else if (d.buildType == 1)
                {
                    float3 p = d.center + float3(0.0, d.size.y + d.height + _Radius * 0.5, 0.0);

                    if (id == 0) {
                        AppendSphere(p, _Radius, -1 * ((int)inst + 1), outStream);
                    }
                    else if (id == 1)
                    {
                        p.y -= (d.height + _Radius * 0.5);
                        AppendTriangularPyramid(p, float3(_Radius, d.height, _Radius), outStream);
                    }
                }
            }

            float4 frag(g2f i) : COLOR
            {
                if (_IsNight == 0)
                {
                    return _NoonColor;
                }

                int index = round(i.uv.z);
                float4 c = lerp(_NightColor, float4(1.0, 0.0, 0.0, 1.0),
                    index >= 0 ? 0.0 : _Times[abs(index) - 1].time);
                
                return lerp(c, _NightColor, saturate(-1.0 * i.uv.w / _DofPower));
            }

            ENDCG
        }
    }
}
