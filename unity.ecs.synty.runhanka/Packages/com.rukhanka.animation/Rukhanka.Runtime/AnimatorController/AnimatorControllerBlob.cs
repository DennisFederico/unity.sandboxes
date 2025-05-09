using Unity.Entities;
using Unity.Mathematics;
using System.Runtime.InteropServices;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
public struct ControllerBlob: GenericAssetBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
	public string Name() => name.ToString();
	public float bakingTime;
	public float BakingTime() => bakingTime;
#endif
	public Hash128 hash;
	public Hash128 Hash() => hash;
	
	public BlobArray<LayerBlob> layers;
	public BlobArray<ParameterBlob> parameters;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public enum ControllerParameterType
{
	Int,
	Float,
	Bool,
	Trigger
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

[StructLayout(LayoutKind.Explicit)]
public struct ParameterValue
{
	[FieldOffset(0)]
	public float floatValue;
	[FieldOffset(0)]
	public int intValue;
	[FieldOffset(0)]
	public bool boolValue;

	public static implicit operator ParameterValue(float f) => new ParameterValue() { floatValue = f };
	public static implicit operator ParameterValue(int i) => new ParameterValue() { intValue = i };
	public static implicit operator ParameterValue(bool b) => new ParameterValue() { boolValue = b };
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct ParameterBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
#endif
	public uint hash;
	public ParameterValue defaultValue;
	public ControllerParameterType type;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public enum AnimationBlendingMode
{
	Override = 0,
	Additive = 1
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public enum AnimatorConditionMode
{
	If = 1,
	IfNot = 2,
	Greater = 3,
	Less = 4,
	Equals = 6,
	NotEqual = 7
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct LayerBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
#endif
	public int defaultStateIndex;
	public float initialWeight;
	public AnimationBlendingMode blendingMode;
	public BlobArray<StateBlob> states;
	public Hash128 avatarMaskBlobHash;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct TransitionBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
#endif
	public uint hash;
	public BlobArray<ConditionBlob> conditions;
	public int targetStateId;
	public float duration;
	public float exitTime;
	public float offset;
	public bool hasExitTime;
	public bool hasFixedDuration;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct ConditionBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
#endif
	public int paramIdx;
	public ParameterValue threshold;
	public AnimatorConditionMode conditionMode;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct MotionBlob
{
	public enum Type
	{
		None,
		AnimationClip,
		BlendTree1D,
		BlendTree2DSimpleDirectional,
		BlendTree2DFreeformDirectional,
		BlendTree2DFreeformCartesian,
		BlendTreeDirect
	}
	
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
#endif
	public uint hash;
	public Type type;
	public int animationIndex;
	public BlendTreeBlob blendTree;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct ChildMotionBlob
{
	public MotionBlob motion;
	public float threshold;
	public float timeScale;
	public float2 position2D;
	public int directBlendParameterIndex;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct BlendTreeBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
#endif
	public int blendParameterIndex;
	public int blendParameterYIndex;
	public bool normalizeBlendValues;
	public BlobArray<ChildMotionBlob> motions;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct StateBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
	public BlobString tag;
#endif
	public uint hash;
	public uint tagHash;
	public float speed;
	public int speedMultiplierParameterIndex;
	public int timeParameterIndex;
	public float cycleOffset;
	public int cycleOffsetParameterIndex;
	public BlobArray<TransitionBlob> transitions;
	public MotionBlob motion;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct ControllerAnimationsBlob
{
	public BlobArray<Hash128> animations;
}

}
