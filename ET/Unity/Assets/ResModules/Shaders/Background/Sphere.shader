Shader "Unlit/Sphere"

{

Properties

{

_MainTex ("Main Tex", 2D) = "black" {}

_H ("H", Range(0,1)) = 0.5

_S ("S", Range(0,2)) = 1.0

_V ("V", Range(0,2)) = 1.0



_RotatingSpeed ("Rotating Speed", Vector) = (0,0,0,0)



_Star0Fac ("Star 0 Fac", Range(0,1)) = 1

_Star1Fac ("Star 1 Fac", Range(0,1)) = 1

_Star2Fac ("Star 2 Fac", Range(0,1)) = 1



_Star0RotatingSpeed ("Star 0 Rotating Speed", Float) = 0.1

_Star1RotatingSpeed ("Star 1 Rotating Speed", Float) = 0.1

_Star2RotatingSpeed ("Star 2 Rotating Speed", Float) = 0.1

_StarColorRotatingSpeed ("Star Color Rotating Speed", Float) = -4



_StarBrightness ("Star Brightness", Float) = 7.0

_StarDarkness ("Star Darkness", Float) = 0.1

_StarSaturation ("Star Saturation", Float) = 0.9



_Nebular0Fac ("Nebular 0 Fac", Range(0,1)) = 1

_Nebular1Fac ("Nebular 1 Fac", Range(0,1)) = 1



_Nebula0RotatingSpeed ("Nebula 0 Rotating Speed", Float) = -0.15

_Nebula1RotatingSpeed ("Nebula 1 Rotating Speed", Float) = -1.4



_Nebula0Color ("Nebula 0 Color", Color) = (0.0,0.0,0.0,1)

_Nebula1Color ("Nebula 1 Color", Color) = (1.0,1.0,1.0,1)

}



SubShader

{

LOD 100



Pass

{

Name "DrawBackground"

Tags {

"RenderPipeline"="UniversalPipeline"

"RenderType"="Opaque"

"LightMode" = "UniversalForward"

}

Cull Front



HLSLPROGRAM



           #pragma vertex vert

           #pragma fragment frag

// make fog work

           #pragma multi_compile_fog



           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"



struct appdata

{

float4 vertex : POSITION;

float2 uv : TEXCOORD0;

};



struct v2f

{

float3 positionVS : TEXCOORD2;

float4 positionCS : SV_POSITION;



float4 vertex : TEXCOORD8;

};



CBUFFER_START(UnityPerMaterial)

sampler2D _MainTex;

float4 _MainTex_ST;

float4 _MainTex_TexelSize;



float _H;

float _S;

float _V;



float3 _RotatingSpeed;



float _Star0Fac;

float _Star1Fac;

float _Star2Fac;



float _Star0RotatingSpeed;

float _Star1RotatingSpeed;

float _Star2RotatingSpeed;

float _StarColorRotatingSpeed;



float _StarBrightness;

float _StarDarkness;

float _StarSaturation;



float _Nebular0Fac;

float _Nebular1Fac;



float _Nebula0RotatingSpeed;

float _Nebula1RotatingSpeed;



float4 _Nebula0Color;

float4 _Nebula1Color;

CBUFFER_END



struct Gradient

{

int type;

int colorsLength;

int alphasLength;

float4 colors[8];

float2 alphas[8];

};



Gradient ConstructGradient(int colorsLength, int alphasLength)

{

Gradient g;

g.colorsLength = colorsLength;

g.alphasLength = alphasLength;

for (int i = 0; i < 8; i++)

{

g.colors[i] = float4(0, 0, 0, 0);

g.alphas[i] = float2(0, 0);

}

return g;

}



Gradient _BaseRampColor_Definition()

{

Gradient g = ConstructGradient(6, 2);

g.colors[0] = float4(0.890, 0.580, 0.580, 0.468);

g.colors[1] = float4(0.729, 0.451, 0.451, 0.502);

g.colors[2] = float4(0.529, 0.322, 0.322, 0.548);

g.colors[3] = float4(0.439, 0.247, 0.247, 0.596);

g.colors[4] = float4(0.431, 0.231, 0.231, 0.676);

g.colors[5] = float4(0.431, 0.231, 0.231, 0.842);

g.alphas[0] = float2(1, 0);

g.alphas[1] = float2(1, 1);

return g;

}

           #define _BaseRampColor _BaseRampColor_Definition()



float4 SampleGradient(Gradient gradient, float Time)

{

float3 color = gradient.colors[0].rgb;

for (int c = 1; c < 8; c++)

{

float colorPos = saturate((Time - gradient.colors[c-1].w) / (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, gradient.colorsLength-1);

color = lerp(color, gradient.colors[c].rgb, colorPos);

}

float alpha = gradient.alphas[0].x;

for (int a = 1; a < 8; a++)

{

float alphaPos = saturate((Time - gradient.alphas[a-1].y) / (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, gradient.alphasLength-1);

alpha = lerp(alpha, gradient.alphas[a].x, alphaPos);

}

return float4(color, alpha);

}



float Compare(float a, float b, float c)

{

return (abs(a - b) <= max(c, 1e-5)) ? 1.0 : 0.0;

}



float Wrap(float a, float b, float c)

{

float range = b - c;

return a - (range * floor((a - c) / range));



}



float mapTo01(float fromMin, float fromMax, float value)

{

return (lerp(fromMin, fromMax, value) - fromMin)/(fromMax - fromMin);

}



float3x3 eulerToMatrix(float3 euler)

{

float cx = cos(euler.x);

float cy = cos(euler.y);

float cz = cos(euler.z);

float sx = sin(euler.x);

float sy = sin(euler.y);

float sz = sin(euler.z);



float3x3 mat;

mat[0][0] = cy * cz;

mat[0][1] = cy * sz;

mat[0][2] = -sy;



mat[1][0] = sy * sx * cz - cx * sz;

mat[1][1] = sy * sx * sz + cx * cz;

mat[1][2] = cy * sx;



mat[2][0] = sy * cx * cz + sx * sz;

mat[2][1] = sy * cx * sz - sx * cz;

mat[2][2] = cy * cx;

return mat;

}



float3 vectorRotate(float3 vec, float3 axis, float angle)

{

axis = normalize(axis);



float cosValue = cos(angle);

float sinValue = sin(angle);



float x = ((cosValue + (1.0 - cosValue) * axis.x * axis.x) * vec.x) + (((1.0 - cosValue) * axis.x * axis.y - axis.z * sinValue) * vec.y) + (((1.0 - cosValue) * axis.x * axis.z + axis.y * sinValue) * vec.z);

float y = (((1.0 - cosValue) * axis.x * axis.y + axis.z * sinValue) * vec.x) + ((cosValue + (1.0 - cosValue) * axis.y * axis.y) * vec.y) + (((1.0 - cosValue) * axis.y * axis.z - axis.x * sinValue) * vec.z);

float z = (((1.0 - cosValue) * axis.x * axis.z - axis.y * sinValue) * vec.x) + (((1.0 - cosValue) * axis.y * axis.z + axis.x * sinValue) * vec.y) + ((cosValue + (1.0 - cosValue) * axis.z * axis.z) * vec.z);



return float3(x,y,z);

}



           #define rot(x, k) (((x) << (k)) | ((x) >> (32 - (k))))

           #define final(a, b, c) {  c ^= b; c -= rot(b, 14); a ^= c; a -= rot(c, 11); b ^= a; b -= rot(a, 25); c ^= b; c -= rot(b, 16); a ^= c; a -= rot(c, 4); b ^= a; b -= rot(a, 14); c ^= b; c -= rot(b, 24); }

           #define mix(a, b, c)  { a -= c; a ^= rot(c, 4); c += b; b -= a; b ^= rot(a, 6); a += c; c -= b; c ^= rot(b, 8); b += a; a -= c; a ^= rot(c, 16); c += b; b -= a; b ^= rot(a, 19); a += c; c -= b; c ^= rot(b, 4); b += a; }

uint hash1(uint kx)

{

uint a, b, c;

a = b = c = 0xdeadbeefu + (1u << 2u) + 13u;



a += kx;

final(a, b, c);



return c;

}

uint hash2(uint kx, uint ky)

{

uint a, b, c;

a = b = c = 0xdeadbeefu + (2u << 2u) + 13u;



b += ky;

a += kx;



final(a, b, c);



return c;

}



uint hash3(uint kx, uint ky, uint kz)

{

uint a, b, c;

a = b = c = 0xdeadbeefu + (3u << 2u) + 13u;



c += kz;

b += ky;

a += kx;

final(a, b, c);



return c;

}



uint hash4(uint kx, uint ky, uint kz, uint kw)

{

uint a, b, c;

a = b = c = 0xdeadbeefu + (4u << 2u) + 13u;



a += kx;

b += ky;

c += kz;

mix(a, b, c);



a += kw;

final(a, b, c);



return c;

}

           #undef rot

           #undef final

           #undef mix



float hashFloat2Tofloat(float2 k)

{

return float(hash2(asuint(k.x), asuint(k.y)))/float(0xFFFFFFFFu);

}



float2 hashFloat2ToFloat2(float2 k)

{

return float2(float(hash2(asuint(k.x), asuint(k.y))), float(hash3(asuint(k.x), asuint(k.y), asuint(1.0))))/float(0xFFFFFFFFu);

}



float3 hashFloat2ToFloat3(float2 k)

{

return float3(float(hash2(asuint(k.x), asuint(k.y))), float(hash3(asuint(k.x), asuint(k.y), asuint(1.0))), float(hash3(asuint(k.x), asuint(k.y), asuint(2.0))))/float(0xFFFFFFFFu);

}



float3 hashFloat3ToFloat3(float3 k)

{

return float3(float(hash3(asuint(k.x), asuint(k.y), asuint(k.z))), float(hash4(asuint(k.x), asuint(k.y), asuint(k.z), asuint(1.0))), float(hash4(asuint(k.x), asuint(k.y), asuint(k.z), asuint(2.0))))/float(0xFFFFFFFFu);

}



float4 voronoiTex2dSmoothF1Minkowski(float3 coord, float scale, float smoothness, float exponent, float randomness, out float outDistance)

{

randomness = clamp(randomness, 0.0, 1.0);

smoothness = clamp(smoothness / 2.0, 0, 0.5);



float2 scaledCoord = coord.xy * scale;

float2 cellPosition = floor(scaledCoord);

float2 localPosition = scaledCoord - cellPosition;



float smoothDistance = 8.0;

float3 smoothColor = 0.0;

float2 smoothPosition = 0.0;

for (int j = -2; j <= 2; j++) {

for (int i = -2; i <= 2; i++) {

float2 cellOffset = float2(i, j);

float2 pointPosition = cellOffset + hashFloat2ToFloat2(cellPosition + cellOffset) * randomness;

float distanceToPoint = pow(pow(abs(pointPosition.x - localPosition.x), exponent) + pow(abs(pointPosition.y - localPosition.y), exponent), 1.0 / exponent);

float h = smoothstep(0.0, 1.0, 0.5 + 0.5 * (smoothDistance - distanceToPoint) / smoothness);

float correctionFactor = smoothness * h * (1.0 - h);

smoothDistance = lerp(smoothDistance, distanceToPoint, h) - correctionFactor;

correctionFactor /= 1.0 + 3.0 * smoothness;

float3 cellColor = hashFloat2ToFloat3(cellPosition + cellOffset);

smoothColor = lerp(smoothColor, cellColor, h) - correctionFactor;

smoothPosition = lerp(smoothPosition, pointPosition, h) - correctionFactor;

}

}

outDistance = smoothDistance;

return float4(smoothColor, 0);

}



float4 voronoi3dF1Euclidean(float3 coord, float scale, float randomness, out float outDistance)

{

randomness = clamp(randomness, 0.0, 1.0);



float3 scaledCoord = coord * scale;

float3 cellPosition = floor(scaledCoord);

float3 localPosition = scaledCoord - cellPosition;



float minDistance = 8.0;

float3 targetOffset, targetPosition;

for (int k = -1; k <= 1; k++) {

for (int j = -1; j <= 1; j++) {

for (int i = -1; i <= 1; i++) {

float3 cellOffset = float3(i, j, k);

float3 pointPosition = cellOffset + hashFloat3ToFloat3(cellPosition + cellOffset) * randomness;

float distanceToPoint = distance(pointPosition, localPosition);

if (distanceToPoint < minDistance) {

targetOffset = cellOffset;

minDistance = distanceToPoint;

targetPosition = pointPosition;

}

}

}

}

outDistance = minDistance;

return float4(hashFloat3ToFloat3(cellPosition + targetOffset), 0);

}



float fade(float t)

{

return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);

}



float tri_mix(float v0, float v1, float v2, float v3, float v4, float v5, float v6, float v7, float x, float y, float z)

{

float x1 = 1.0 - x;

float y1 = 1.0 - y;

float z1 = 1.0 - z;

return z1 * (y1 * (v0 * x1 + v1 * x) + y * (v2 * x1 + v3 * x)) + z * (y1 * (v4 * x1 + v5 * x) + y * (v6 * x1 + v7 * x));

}



float quad_mix(float v0, float v1, float v2, float v3, float v4, float v5, float v6, float v7, float v8, float v9, float v10, float v11, float v12, float v13, float v14, float v15, float x, float y, float z, float w)

{

return lerp(tri_mix(v0, v1, v2, v3, v4, v5, v6, v7, x, y, z), tri_mix(v8, v9, v10, v11, v12, v13, v14, v15, x, y, z), w);

}



float negate_if(float value, uint condition)

{

return (condition != 0u) ? -value : value;

}



float noise_grad(uint hash, float x, float y, float z)

{

uint h = hash & 15u;

float u = h < 8u ? x : y;

float vt = ((h == 12u) || (h == 14u)) ? x : z;

float v = h < 4u ? y : vt;

return negate_if(u, h & 1u) + negate_if(v, h & 2u);

}



float noise_grad(uint hash, float x, float y, float z, float w)

{

uint h = hash & 31u;

float u = h < 24u ? x : y;

float v = h < 16u ? y : z;

float s = h < 8u ? z : w;

return negate_if(u, h & 1u) + negate_if(v, h & 2u) + negate_if(s, h & 4u);

}



           #define FLOORFRAC(x, x_int, x_fract) { float x_floor = floor(x); x_int = int(x_floor); x_fract = x - x_floor; }

float noise_perlin(float3 vec)

{

int X, Y, Z;

float fx, fy, fz;



FLOORFRAC(vec.x, X, fx);

FLOORFRAC(vec.y, Y, fy);

FLOORFRAC(vec.z, Z, fz);



float u = fade(fx);

float v = fade(fy);

float w = fade(fz);



float r = tri_mix(noise_grad(hash3(X, Y, Z), fx, fy, fz),

noise_grad(hash3(X + 1, Y, Z), fx - 1, fy, fz),

noise_grad(hash3(X, Y + 1, Z), fx, fy - 1, fz),

noise_grad(hash3(X + 1, Y + 1, Z), fx - 1, fy - 1, fz),

noise_grad(hash3(X, Y, Z + 1), fx, fy, fz - 1),

noise_grad(hash3(X + 1, Y, Z + 1), fx - 1, fy, fz - 1),

noise_grad(hash3(X, Y + 1, Z + 1), fx, fy - 1, fz - 1),

noise_grad(hash3(X + 1, Y + 1, Z + 1), fx - 1, fy - 1, fz - 1),

u,

v,

w);



return r;

}



float noise_perlin(float4 vec)

{

int X, Y, Z, W;

float fx, fy, fz, fw;



FLOORFRAC(vec.x, X, fx);

FLOORFRAC(vec.y, Y, fy);

FLOORFRAC(vec.z, Z, fz);

FLOORFRAC(vec.w, W, fw);



float u = fade(fx);

float v = fade(fy);

float t = fade(fz);

float s = fade(fw);



float r = quad_mix(

noise_grad(hash4(X, Y, Z, W), fx, fy, fz, fw),

noise_grad(hash4(X + 1, Y, Z, W), fx - 1.0, fy, fz, fw),

noise_grad(hash4(X, Y + 1, Z, W), fx, fy - 1.0, fz, fw),

noise_grad(hash4(X + 1, Y + 1, Z, W), fx - 1.0, fy - 1.0, fz, fw),

noise_grad(hash4(X, Y, Z + 1, W), fx, fy, fz - 1.0, fw),

noise_grad(hash4(X + 1, Y, Z + 1, W), fx - 1.0, fy, fz - 1.0, fw),

noise_grad(hash4(X, Y + 1, Z + 1, W), fx, fy - 1.0, fz - 1.0, fw),

noise_grad(hash4(X + 1, Y + 1, Z + 1, W), fx - 1.0, fy - 1.0, fz - 1.0, fw),

noise_grad(hash4(X, Y, Z, W + 1), fx, fy, fz, fw - 1.0),

noise_grad(hash4(X + 1, Y, Z, W + 1), fx - 1.0, fy, fz, fw - 1.0),

noise_grad(hash4(X, Y + 1, Z, W + 1), fx, fy - 1.0, fz, fw - 1.0),

noise_grad(hash4(X + 1, Y + 1, Z, W + 1), fx - 1.0, fy - 1.0, fz, fw - 1.0),

noise_grad(hash4(X, Y, Z + 1, W + 1), fx, fy, fz - 1.0, fw - 1.0),

noise_grad(hash4(X + 1, Y, Z + 1, W + 1), fx - 1.0, fy, fz - 1.0, fw - 1.0),

noise_grad(hash4(X, Y + 1, Z + 1, W + 1), fx, fy - 1.0, fz - 1.0, fw - 1.0),

noise_grad(hash4(X + 1, Y + 1, Z + 1, W + 1), fx - 1.0, fy - 1.0, fz - 1.0, fw - 1.0),

u,

v,

t,

s);



return r;

}

           #undef FLOORFRAC



float snoise(float3 p)

{

float r = noise_perlin(p);

return (isinf(r)) ? 0.0 : (0.9820 * r);

}



float noise(float3 p)

{

return 0.5 * snoise(p) + 0.5;

}



float snoise(float4 p)

{

float r = noise_perlin(p);

return (isinf(r)) ? 0.0 : (0.8344 * r);

}



float noise(float4 p)

{

return 0.5 * snoise(p) + 0.5;

}



float fractalNoise(float3 p, float octaves, float roughness)

{

float fscale = 1.0;

float amp = 1.0;

float maxamp = 0.0;

float sum = 0.0;

octaves = clamp(octaves, 0.0, 16.0);

int n = int(octaves);

for (int i = 0; i <= n; i++) {

float t = noise(fscale * p);

sum += t * amp;

maxamp += amp;

amp *= clamp(roughness, 0.0, 1.0);

fscale *= 2.0;

}

float rmd = octaves - floor(octaves);

if (rmd != 0.0) {

float t = noise(fscale * p);

float sum2 = sum + t * amp;

sum /= maxamp;

sum2 /= maxamp + amp;

return (1.0 - rmd) * sum + rmd * sum2;

}

else {

return sum / maxamp;

}

}



float fractalNoise(float4 p, float octaves, float roughness)

{

float fscale = 1.0;

float amp = 1.0;

float maxamp = 0.0;

float sum = 0.0;

octaves = clamp(octaves, 0.0, 16.0);

int n = int(octaves);

for (int i = 0; i <= n; i++) {

float t = noise(fscale * p);

sum += t * amp;

maxamp += amp;

amp *= clamp(roughness, 0.0, 1.0);

fscale *= 2.0;

}

float rmd = octaves - floor(octaves);

if (rmd != 0.0) {

float t = noise(fscale * p);

float sum2 = sum + t * amp;

sum /= maxamp;

sum2 /= maxamp + amp;

return (1.0 - rmd) * sum + rmd * sum2;

}

else {

return sum / maxamp;

}

}



float3 randomFloat3Offset(float seed)

{

return float3(100.0 + hashFloat2Tofloat(float2(seed, 0.0)) * 100.0, 100.0 + hashFloat2Tofloat(float2(seed, 1.0)) * 100.0, 100.0 + hashFloat2Tofloat(float2(seed, 2.0)) * 100.0);

}



float4 randomFloat4Offset(float seed)

{

return float4(100.0 + hashFloat2Tofloat(float2(seed, 0.0)) * 100.0, 100.0 + hashFloat2Tofloat(float2(seed, 1.0)) * 100.0, 100.0 + hashFloat2Tofloat(float2(seed, 2.0)) * 100.0, 100.0 + hashFloat2Tofloat(float2(seed, 3.0)) * 100.0);

}



float4 noise3d(float3 co, float scale, float detail, float roughness, float distortion)

{

float3 p = co * scale;

if (distortion != 0.0) {

p += float3(snoise(p + randomFloat3Offset(0.0)) * distortion,

snoise(p + randomFloat3Offset(1.0)) * distortion,

snoise(p + randomFloat3Offset(2.0)) * distortion);

}



float value = fractalNoise(p, detail, roughness);

return float4(value, fractalNoise(p + randomFloat3Offset(3.0), detail, roughness), fractalNoise(p + randomFloat3Offset(4.0), detail, roughness), 1.0);

}



float4 noise4d(float3 co, float w, float scale, float detail, float roughness, float distortion)

{

float4 p = float4(co, w) * scale;

if (distortion != 0.0) {

p += float4(snoise(p + randomFloat4Offset(0.0)) * distortion,

snoise(p + randomFloat4Offset(1.0)) * distortion,

snoise(p + randomFloat4Offset(2.0)) * distortion,

snoise(p + randomFloat4Offset(3.0)) * distortion);

}



float value = fractalNoise(p, detail, roughness);

return float4(value, fractalNoise(p + randomFloat4Offset(4.0), detail, roughness), fractalNoise(p + randomFloat4Offset(5.0), detail, roughness), 1.0);

}



float4 radialGradient(float3 co)

{

float f = saturate(atan2(co.y, co.x) / (3.141592654 * 2) + 0.5);

return float4(f, f, f, 1.0);

}



float4 RGB2HSV(float4 rgb)

{

float h, s, v;



float cmax = max(rgb[0], max(rgb[1], rgb[2]));

float cmin = min(rgb[0], min(rgb[1], rgb[2]));

float cdelta = cmax - cmin;



v = cmax;

if (cmax != 0.0)

{

s = cdelta / cmax;

}

else

{

s = 0.0;

h = 0.0;

}



if (s == 0.0)

{

h = 0.0;

}

else

{

float3 c = (cmax - rgb.xyz) / cdelta;



if (rgb.x == cmax)

{

h = c[2] - c[1];

}

else if (rgb.y == cmax)

{

h = 2.0 + c[0] - c[2];

}

else

{

h = 4.0 + c[1] - c[0];

}



h /= 6.0;



if (h < 0.0)

{

h += 1.0;

}

}



return float4(h, s, v, rgb.a);

}



float4 HSV2RGB(float4 hsv)

{

float i, f, p, q, t, h, s, v;

float3 rgb;



h = hsv[0];

s = hsv[1];

v = hsv[2];



if (s == 0.0)

{

rgb = float3(v, v, v);

}

else

{

if (h == 1.0)

{

h = 0.0;

}



h *= 6.0;

i = floor(h);

f = h - i;

rgb = float3(f, f, f);

p = v * (1.0 - s);

q = v * (1.0 - (s * f));

t = v * (1.0 - (s * (1.0 - f)));



if (i == 0.0)

{

rgb = float3(v, t, p);

}

else if (i == 1.0)

{

rgb = float3(q, v, p);

}

else if (i == 2.0)

{

rgb = float3(p, v, t);

}

else if (i == 3.0)

{

rgb = float3(p, q, v);

}

else if (i == 4.0)

{

rgb = float3(t, p, v);

}

else

{

rgb = float3(v, p, q);

}

}



return float4(rgb, hsv.w);

}



float4 HSV(float hue, float sat, float value, float fac, float4 col)

{

float4 hsv = RGB2HSV(col);



hsv[0] = frac(hsv[0] + hue + 0.5);

hsv[1] = clamp(hsv[1] * sat, 0.0, 1.0);

hsv[2] = hsv[2] * value;



float4 outcol = HSV2RGB(hsv);



return lerp(col, outcol, fac);

}



v2f vert (appdata v)

{

v2f o;

o.vertex = v.vertex;



VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);

o.positionVS = vertexInput.positionVS;

o.positionCS = vertexInput.positionCS;



return o;

}



float4 frag (v2f i, bool IsFacing : SV_IsFrontFace) : SV_Target

{

float pi = 3.141592654;



float3 V = normalize(mul((float3x3)UNITY_MATRIX_I_V, i.positionVS * (-1)));

float3 coord = i.vertex.xzy + 0.5;



float baseColorSampling = mapTo01(-1 - V.x * V.x * 0.56, 1, coord.z + 0.035);

float4 baseColor = SampleGradient(_BaseRampColor, baseColorSampling);

baseColor = HSV(_H, _S, _V, 1, baseColor);



float3 rotatingSpeed = _RotatingSpeed;



float star0 = 0;

{

float3 starRotate = float3(0, 0, _Star0RotatingSpeed) * rotatingSpeed;

float3 mappingCoord = mul(coord * float3(1, 1, 1.2), eulerToMatrix(starRotate));



float3 r0 = vectorRotate(mappingCoord, float3(0,0,1), pi*0.25);

float3 r1 = vectorRotate(vectorRotate(mappingCoord, float3(1,0,0), pi/2), float3(0,0,1), pi*0.25722);

float3 r2 = vectorRotate(vectorRotate(mappingCoord, float3(0,1,0), (-1) * pi/2), float3(0,0,1), pi*0.21556);

float3 m = lerp(lerp(r0, r1, step(0.577, abs(r1.z))), r2, step(0.577, abs(r2.z)));



float distance = 0;

float4 color = voronoiTex2dSmoothF1Minkowski(m, 50, 0, 0.3, 0, distance);



Gradient g = ConstructGradient(3, 2);

g.colors[0] = float4(1.0, 1.0, 1.0, 0.3);

g.colors[1] = float4(0.269, 0.269, 0.269, 0.498);

g.colors[2] = float4(0.0, 0.0, 0.0, 0.877);

g.alphas[0] = float2(1, 0);

g.alphas[1] = float2(1, 1);



star0 = SampleGradient(g, distance).r;

star0 *= Compare(RGB2HSV(color).x, 0.5, 0.007);

star0 *= 1.140;

}



float star1 = 0;

{

float3 starRotate = float3(0, 0, _Star1RotatingSpeed) * rotatingSpeed;

float3 mappingCoord = mul(coord, eulerToMatrix(starRotate));



float distance = 0;

float4 color = voronoi3dF1Euclidean(mappingCoord, 150, 1, distance);



Gradient g = ConstructGradient(4, 2);

g.colors[0] = float4(1.0, 1.0, 1.0, 0.073);

g.colors[1] = float4(0.522, 0.522, 0.522, 0.120);

g.colors[2] = float4(0.082, 0.082, 0.082, 0.203);

g.colors[3] = float4(0.0, 0.0, 0.0, 0.250);

g.alphas[0] = float2(1, 0);

g.alphas[1] = float2(1, 1);



star1 = SampleGradient(g, distance).r;

star1 *= Compare(RGB2HSV(color).x, 0.5, 0.1);

star1 *= 0.5;

}



float star2 = 0;

{

float3 starRotate = float3(0, 0, _Star2RotatingSpeed) * rotatingSpeed;

float3 mappingCoord = mul(coord, eulerToMatrix(starRotate));



float4 noiseColor = noise3d(mappingCoord, 100, 1, 0.5, 0);



mappingCoord += (noiseColor.rgb - 0.5) * 0.005;



float distance = 0;

float4 color = voronoi3dF1Euclidean(mappingCoord, 80, 1, distance);



Gradient g = ConstructGradient(4, 2);

g.colors[0] = float4(1.0, 1.0, 1.0, 0.073);

g.colors[1] = float4(0.522, 0.522, 0.522, 0.120);

g.colors[2] = float4(0.082, 0.082, 0.082, 0.203);

g.colors[3] = float4(0.0, 0.0, 0.0, 0.250);

g.alphas[0] = float2(1, 0);

g.alphas[1] = float2(1, 1);



star2 = SampleGradient(g, distance).r;

star2 *= Compare(RGB2HSV(color).x, 0.5, 0.17);

}



float starBrightness = 0;

{

float3 colorRotate = float3(0, 0, _StarColorRotatingSpeed * 0.2) * rotatingSpeed;

float3 mappingCoord = mul(coord, eulerToMatrix(colorRotate));



float4 noiseColor = noise4d(mappingCoord, 0, 5, 1, 0, 0);



Gradient g = ConstructGradient(3, 2);

g.colors[0] = float4(0, 0, 0, 0.387);

g.colors[1] = float4(0.5, 0.5, 0.5, 0.647);

g.colors[2] = float4(1, 1, 1, 0.821);

g.alphas[0] = float2(1, 0);

g.alphas[1] = float2(1, 1);



starBrightness = SampleGradient(g, noiseColor.r).r + _StarDarkness;

}



float starFade = 0;

{

float fadeValue = coord.z * 0.5 + 0.5;



Gradient g = ConstructGradient(2, 2);

g.colors[0] = float4(0, 0, 0, 0.422);

g.colors[1] = float4(1, 1, 1, 0.552);

g.alphas[0] = float2(1, 0);

g.alphas[1] = float2(1, 1);



starFade = SampleGradient(g, fadeValue).r;

}



float3 starColor = 0;

{

float3 colorRotate = float3(0, 0, _StarColorRotatingSpeed) * rotatingSpeed;

float3 mappingCoord = mul(coord, eulerToMatrix(colorRotate));

float gradient = radialGradient(mappingCoord).r;



float4 color = 0;

float3 co = float3(Wrap(gradient * 5, 0, 1), 0, 0);

float2 texSize = 1/_MainTex_TexelSize.xy;



co.xy *= texSize;



float2 tc = floor(co.xy - 0.5) + 0.5;



float2 f = co.xy - tc;

float2 f2 = f * f;

float2 f3 = f2 * f;



float2 w3 = f3 / 6.0;

float2 w0 = -w3 + f2 * 0.5 - f * 0.5 + 1.0 / 6.0;

float2 w1 = f3 * 0.5 - f2 * 1.0 + 2.0 / 3.0;

float2 w2 = 1.0 - w0 - w1 - w3;



float2 s0 = w0 + w1;

float2 s1 = w2 + w3;



float2 f0 = w1 / (w0 + w1);

float2 f1 = w3 / (w2 + w3);



float4 finalCo;

finalCo.xy = tc - 1.0 + f0;

finalCo.zw = tc + 1.0 + f1;



finalCo /= texSize.xyxy;



color = (tex2D(_MainTex, finalCo.xy)) * s0.x * s0.y;

color += (tex2D(_MainTex, finalCo.zy)) * s1.x * s0.y;

color += (tex2D(_MainTex, finalCo.xw)) * s0.x * s1.y;

color += (tex2D(_MainTex, finalCo.zw)) * s1.x * s1.y;



color = HSV(0.5, _StarSaturation, _StarBrightness, 1, color);



starColor = color;

}



float starValue = starFade * starBrightness * (lerp(0, star0, _Star0Fac) + lerp(0, star1, _Star1Fac) + lerp(0, star2, _Star2Fac));



float3 star = lerp(baseColor.rgb, starColor.rgb, starValue);



float3 nebula0 = 0;

{

float3 nebulaRotate = float3(_Nebula0RotatingSpeed * 0.7, 0, _Nebula0RotatingSpeed) * rotatingSpeed;



float3 mappingCoord = mul(coord, eulerToMatrix(nebulaRotate));



float noise0 = noise3d(mappingCoord, 8, 8, 0.450, 0).r;



Gradient g0 = ConstructGradient(3, 2);

g0.colors[0] = float4(0, 0, 0, 0.141);

g0.colors[1] = float4(0.569, 0.569, 0.569, 0.552);

g0.colors[2] = float4(1, 1, 1, 1);

g0.alphas[0] = float2(1, 0);

g0.alphas[1] = float2(1, 1);



float color0 = SampleGradient(g0, noise0).r;



float noise1 = noise3d(mappingCoord, 5, 6, 0.5, -0.5);



Gradient g1 = ConstructGradient(3, 2);

g1.colors[0] = float4(0, 0, 0, 0);

g1.colors[1] = float4(0.733, 0.733, 0.733, 0.454);

g1.colors[2] = float4(1, 1, 1, 1);

g1.alphas[0] = float2(1, 0);

g1.alphas[1] = float2(1, 1);



float color1 = SampleGradient(g1, noise1).r;



float colorFac = color0 * color1;



nebula0 = lerp(0, _Nebula0Color, colorFac);

}



float3 nebula1 = 0;

{

float3 nebulaRotate = float3(0, 0, _Nebula1RotatingSpeed) * rotatingSpeed;



float3 mappingCoord = mul(coord * 2, eulerToMatrix(nebulaRotate)) + float3(0, 0, -0.33);



float noise0 = noise3d(mappingCoord, 4, 1.7, 0.65, 0.1).r;



Gradient g0 = ConstructGradient(3, 2);

g0.colors[0] = float4(0, 0, 0, 0.205);

g0.colors[1] = float4(0.486, 0.486, 0.486, 0.532);

g0.colors[2] = float4(1, 1, 1, 0.845);

g0.alphas[0] = float2(1, 0);

g0.alphas[1] = float2(1, 1);



float color0 = SampleGradient(g0, noise0).r;



float noise1 = noise3d(mappingCoord, 2.5, 0, 0, 0.5).r;



Gradient g1 = ConstructGradient(3, 2);

g1.colors[0] = float4(0, 0, 0, 0.245);

g1.colors[1] = float4(0.451, 0.451, 0.451, 0.581);

g1.colors[2] = float4(1, 1, 1, 0.818);

g1.alphas[0] = float2(1, 0);

g1.alphas[1] = float2(1, 1);



float color1 = SampleGradient(g1, noise1).r;



float colorFac = color0 * color1;



nebula1 = lerp(0, _Nebula1Color, colorFac);

}

float nebulaFade = 0;

{

float fadeValue = coord.z * 0.5 + 0.5;



Gradient g = ConstructGradient(3, 2);

g.colors[0] = float4(0, 0, 0, 0.448);

g.colors[1] = float4(1, 1, 1, 0.555);

g.alphas[0] = float2(1, 0);

g.alphas[1] = float2(1, 1);



nebulaFade = SampleGradient(g, fadeValue);

}



float3 nebula = (lerp(0, nebula0, _Nebular0Fac) + lerp(0, nebula1, _Nebular1Fac)) * nebulaFade;



float4 col = float4(0,0,0,1);

col.rgb = nebula + star;

// col.rgb = 0;

// col.rgb = nebula1;



return col;

}

ENDHLSL

}

}

}



