// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/PointCloud"
{
	Properties
	{
		_DepthTex("Depth", 2D) = "white" {}
		_SpriteTex("Color (RGB)", 2D) = "white" {}
		_Size("Size", Range(0, 3)) = 0.5
		_Modifier("Distance Modifier", Range(0, 2)) = 0.1
	}

		SubShader
	{
		Pass
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 300
		Cull Off

		CGPROGRAM
#pragma target 5.0
#pragma vertex VS_Main
#pragma fragment FS_Main
#pragma geometry GS_Main
#include "UnityCG.cginc" 

		// **************************************************************
		// Data structures												*
		// **************************************************************

		struct appdata 
	{
		float2 tex0		: TEXCOORD0;
	};

		struct GS_INPUT
	{
		float4	pos		: POSITION;
		float2  tex0	: TEXCOORD0;
		float4 col		: COLOR;
	};

	struct FS_INPUT
	{
		float4	pos		: POSITION;
		float2  tex0	: TEXCOORD0;
		float4 col		: COLOR;
	};


	// **************************************************************
	// Vars															*
	// **************************************************************

	float _Size;
	float _Modifier;
	float4x4 _vp;
	float4 _adjustment;
	float4x4 _cam2World;
	sampler2D _SpriteTex;
	sampler2D _DepthTex;
	SamplerState sampler_SpriteTex;

	static const float4 translation = float4(1.9985242312092553e-02, -7.4423738761617583e-04, -1.0916736334336222e-02, 0);

	static const float4x4 adjustment = float4x4(
		9.9984628826577793e-01, 1.2635359098409581e-03, 1.7487233004436643e-02, 1.9985242312092553e-02,
		1.4779096108364480e-03, 9.9992385683542895e-01, 1.2251380107679535e-02, 7.4423738761617583e-04,
		1.7470421412464927e-02, 1.2275341476520762e-02, 9.9977202419716948e-01, 1.0916736334336222e-02,
		0.00000000000000000000, 0.00000000000000000000, 0.00000000000000000000, 1.00000000000000000000);

	//adjustment matrix found on http://nicolas.burrus.name/index.php/Research/KinectCalibration

	// **************************************************************
	// Shader Programs												*
	// **************************************************************

	// Vertex Shader ------------------------------------------------
	GS_INPUT VS_Main(appdata v)
	{
		GS_INPUT output = (GS_INPUT)0;

		//output.pos = mul(unity_ObjectToWorld, v.vertex);
		float4 vcolor = tex2Dlod(_DepthTex, float4(v.tex0, 0, 0));

		int rawdepth = (int)(vcolor.r * 255) + (int)(vcolor.g * 255 * 255);
		//float dmap = vcolor.r * 255 + vcolor.g * 255 * 255;
		//dmap = (dmap / 1000);
		//dmap = 0.01601143863 * dmap * dmap - 19.09324643 * dmap + 6218.984291;
		//dmap = dmap / 1000;

		//calibration parameters found on http://nicolas.burrus.name/index.php/Research/KinectCalibration
		//float dmap = 1.0 / ((float)rawdepth * -0.0030711016 + 3.3309495161);
		float dmap = (float)rawdepth / 1000;
		//dmap = dmap / 256;



		float x_d = (v.tex0.x + 0.5) * 320;
		float y_d = (v.tex0.y + 0.5) * 240;

		float fx_d = 594.21434211923247;
		float fy_d = 591.04053696870778;
		float cx_d = 339.30780975300314;
		float cy_d = 242.73913761751615;

		float4 unpv;
		unpv.x = (x_d - cx_d) * rawdepth / (fx_d);
		unpv.y = (y_d - cy_d) * rawdepth / (fy_d);
		unpv.z = rawdepth;
		unpv.w = 1;

		unpv.x = unpv.x / 320;
		unpv.y = unpv.y / 240;
		unpv.z = unpv.z / 1000;


		/*
		float4 screenspace = float4(v.tex0.x * 2 - 1, v.tex0.y * 2 - 1, 0.5f, 1);

		float4 unpv = mul(_vpi, screenspace);
		unpv.z = unpv.z / unpv.w;
		unpv.y = unpv.y / unpv.w;
		unpv.x = unpv.x / unpv.w;
		unpv.w = 1;
		*/

		//unpv = mul(_world2Cam, unpv);
		//unpv = normalize(unpv);
		//unpv.w = 0;
		//unpv = normalize(unpv) * dmap;


		//unpv.x = (v.tex0.x - 0.5) * 4/3;
		//unpv.y = (v.tex0.y - 0.5);
		//unpv.z = dmap;
		//unpv.w = 1;
		unpv = mul(_cam2World, unpv);
		//unpv.w = 1;
		output.pos = unpv;


		float4 ss2 = mul(UNITY_MATRIX_VP, output.pos);
		//ss2 = ComputeScreenPos(ss2);
		ss2.z = ss2.z / ss2.w;
		ss2.y = ss2.y / ss2.w;
		ss2.x = ss2.x / ss2.w;
		ss2.w = 1;


		float aw = 1;
		output.col = ss2 / aw;//float4(ss2.z / aw, ss2.z / aw, ss2.z / aw, 1);
		//output.col.b = dmap;
		if (dmap <= 0.001) {
			output.col.r = -2;
		}

		//output.col.b = distance(output.pos.xyz, _WorldSpaceCameraPos);


		//float2 ccds = v.tex0;
		//ccds = ccds * _adjustment.x;
		//ccds.x -= _adjustment.y;
		//ccds.y += _adjustment.z;




		
		/*
		float4 colorcoords = mul(_vp, output.pos);
		colorcoords.xyz = colorcoords.xyz / colorcoords.w;
		colorcoords.w = 1;
		colorcoords.x = colorcoords.x * 3 / 4;
		colorcoords.xy = colorcoords.xy * (2 / (dmap));
		*/

		/*
		float2 normdcoords = v.tex0 - float2(0.5, 0.5);
		float2 colorcoords;
		float depth = dmap / _Modifier;
		colorcoords = v.tex0 + float2(depth, depth);
		colorcoords = colorcoords + float2(0.5, 0.5);
		*/


		//colorcoords.x = v.tex0.x + dmap / _Modifier;
		//colorcoords.y = v.tex0.y + dmap / _Modifier;

		output.tex0 = v.tex0;
		//output.col = float4(unpv.w * 1, unpv.w * 1, unpv.w * 1, 1);

		return output;
	}

	// Geometry Shader -----------------------------------------------------
	[maxvertexcount(8)]
	void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
	{

		float3 clr = p[0].col;
		if (clr.r >= -1 && clr.r <= 1 && clr.g >= -1 && clr.g <= 1) {

			float3 up = UNITY_MATRIX_IT_MV[1].xyz;
			float3 look = _WorldSpaceCameraPos - p[0].pos;
			up = normalize(up);
			look = normalize(look);
			float3 right = cross(up, look);

			float halfS = 0.5f * _Size;

			float4x4 vp = UNITY_MATRIX_VP;
			FS_INPUT pIn;
			pIn.tex0 = p[0].tex0;
			pIn.col = p[0].col;

			float size = _Size;//* pow(clr.b, 0.25); //+ (_Modifier * distance(_WorldSpaceCameraPos, p[0].pos));


			float4 vtcs[8];

			for (int i = 0; i < 8; ++i) {
				int d = i * 45;
				float r = radians(d);
				vtcs[i] = float4(p[0].pos + cos(r) * size * right + sin(r) * size * up, 1.0f);
			}

			pIn.pos = mul(vp, vtcs[0]);
			triStream.Append(pIn);

			pIn.pos = mul(vp, vtcs[1]);
			triStream.Append(pIn);

			pIn.pos = mul(vp, vtcs[7]);
			triStream.Append(pIn);

			pIn.pos = mul(vp, vtcs[2]);
			triStream.Append(pIn);

			pIn.pos = mul(vp, vtcs[6]);
			triStream.Append(pIn);

			pIn.pos = mul(vp, vtcs[3]);
			triStream.Append(pIn);

			pIn.pos = mul(vp, vtcs[5]);
			triStream.Append(pIn);

			pIn.pos = mul(vp, vtcs[4]);
			triStream.Append(pIn);

		}
	}


	// Fragment Shader -----------------------------------------------
	float4 FS_Main(FS_INPUT input) : COLOR
	{
		//return input.col;
		//return float4(input.tex0/2, 0, 1);
		return tex2Dlod(_SpriteTex, float4(input.tex0, 0, 0));
	}

		ENDCG
	}
	}
}
