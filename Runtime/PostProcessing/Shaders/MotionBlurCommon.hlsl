#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Builtin/BuiltinData.hlsl"

#define TILE_SIZE                   32u
#define WAVE_SIZE                   64u

#ifdef VELOCITY_PREPPING 
RW_TEXTURE2D(float3, _VelocityAndDepth);
#else
TEXTURE2D(_VelocityAndDepth);
#endif

#ifdef GEN_PASS
RW_TEXTURE2D(float3, _TileMinMaxVel);
#else
TEXTURE2D(_TileMinMaxVel);
#endif

#if NEIGHBOURHOOD_PASS
RW_TEXTURE2D(float3, _TileMaxNeighbourhood);
#else
TEXTURE2D(_TileMaxNeighbourhood);
#endif


CBUFFER_START(MotionBlurUniformBuffer)
float4x4 _PrevVPMatrixNoTranslation;
float4 _TileTargetSize;     // .xy size, .zw 1/size
float4 _MotionBlurParams0;  // Unpacked below.
float4 _MotionBlurParams1;  // Upacked below.
int    _SampleCount;
CBUFFER_END

#define _ScreenMagnitude            _MotionBlurParams0.x
#define _ScreenMagnitudeSq          _MotionBlurParams0.y
#define _MinVelThreshold            _MotionBlurParams0.z
#define _MinVelThresholdSq          _MotionBlurParams0.w
#define _MotionBlurIntensity        _MotionBlurParams1.x
#define _MotionBlurMaxVelocity      _MotionBlurParams1.y
#define _MinMaxVelRatioForSlowPath  _MotionBlurParams1.z
#define _CameraRotationClampNDC     _MotionBlurParams1.w


// --------------------------------------
// Functions that work on encoded representation
// --------------------------------------

float VelocityLengthInPixelsFromEncoded(float2 velocity)
{
    return  velocity.x * _ScreenMagnitude;
}

float2 DecodeVelocityFromPacked(float2 velocity)
{
    float theta = velocity.y * (2.0 * PI) - PI;
    return  (float2(sin(theta), cos(theta)) * velocity.x).yx;
}

float VelocityLengthFromEncoded(float2 velocity)
{
    return  velocity.x;
}

float2 MaxVel(float2 v, float2 w)
{
    return (VelocityLengthFromEncoded(v) < VelocityLengthFromEncoded(w)) ? w : v;
}
