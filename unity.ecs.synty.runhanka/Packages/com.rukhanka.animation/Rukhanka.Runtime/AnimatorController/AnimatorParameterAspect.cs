using Unity.Entities;
using FixedStringName = Unity.Collections.FixedString512Bytes;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
public readonly partial struct AnimatorParametersAspect: IAspect
{
	[Optional]
	readonly RefRO<AnimatorControllerParameterIndexTableComponent> indexTable;
	readonly DynamicBuffer<AnimatorControllerParameterComponent> parametersArr;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public float GetFloatParameter(FastAnimatorParameter fp) => GetParameterValue(fp).floatValue;
	public int GetIntParameter(FastAnimatorParameter fp) => GetParameterValue(fp).intValue;
	public bool GetBoolParameter(FastAnimatorParameter fp) => GetParameterValue(fp).boolValue;
	public float GetFloatParameter(uint h) => GetParameterValue(h).floatValue;
	public int GetIntParameter(uint h) => GetParameterValue(h).intValue;
	public bool GetBoolParameter(uint h) => GetParameterValue(h).boolValue;
	public float GetFloatParameter(FixedStringName n) => GetParameterValue(n).floatValue;
	public int GetIntParameter(FixedStringName n) => GetParameterValue(n).intValue;
	public bool GetBoolParameter(FixedStringName n) => GetParameterValue(n).boolValue;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public ParameterValue GetParameterValue(FastAnimatorParameter fp)
	{
		ParameterValue rv;
		if (indexTable.IsValid)
			fp.GetRuntimeParameterData(indexTable.ValueRO.value, parametersArr, out rv);
		else
			fp.GetRuntimeParameterData(parametersArr, out rv);
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public ParameterValue GetParameterValue(uint parameterHash)
	{
		var fp = new FastAnimatorParameter()
		{
			hash = parameterHash,
		};
		return GetParameterValue(fp);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public ParameterValue GetParameterValue(FixedStringName parameterName)
	{
		var fp = new FastAnimatorParameter(parameterName);
		return GetParameterValue(fp);
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public ParameterValue GetParameterValueByIndex(int paramIndex)
	{
		return parametersArr.ElementAt(paramIndex).value;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetParameterValue(FastAnimatorParameter fp, ParameterValue value)
	{
		if (indexTable.IsValid)
			fp.SetRuntimeParameterData(indexTable.ValueRO.value, parametersArr, value);
		else
			fp.SetRuntimeParameterData(parametersArr, value);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetTrigger(FastAnimatorParameter fp)
	{
		SetParameterValue(fp, new ParameterValue() { boolValue = true });
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void ResetTrigger(FastAnimatorParameter fp)
	{
		SetParameterValue(fp, new ParameterValue() { boolValue = false });
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetIntParameter(FastAnimatorParameter fp, int v)
	{
		SetParameterValue(fp, new ParameterValue() { intValue = v });
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetFloatParameter(FastAnimatorParameter fp, float v)
	{
		SetParameterValue(fp, new ParameterValue() { floatValue = v });
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetBoolParameter(FastAnimatorParameter fp, bool v)
	{
		SetParameterValue(fp, new ParameterValue() { boolValue = v });
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetParameterValueByIndex(int paramIndex, ParameterValue value)
	{
		parametersArr.ElementAt(paramIndex).value = value;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetParameterValue(uint parameterHash, ParameterValue value)
	{
		var fp = new FastAnimatorParameter()
		{
			hash = parameterHash,
		};
		SetParameterValue(fp, value);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetParameterValue(FixedStringName parameterName, ParameterValue value)
	{
		var fp = new FastAnimatorParameter(parameterName);
		SetParameterValue(fp, value);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public int GetParameterIndex(FastAnimatorParameter fp)
	{
		var index = indexTable.IsValid ?
			fp.GetRuntimeParameterIndex(indexTable.ValueRO.value, parametersArr) :
			fp.GetRuntimeParameterIndex(parametersArr);

		return index;
	}
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public bool HasParameter(FastAnimatorParameter fp)
	{
		return GetParameterIndex(fp) != -1;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public bool HasParameter(uint parameterHash)
	{
		var fp = new FastAnimatorParameter()
		{
			hash = parameterHash,
		};
		return HasParameter(fp);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public bool HasParameter(FixedStringName parameterName)
	{
		var fp = new FastAnimatorParameter(parameterName);
		return HasParameter(fp);
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public int ParametersCount() => parametersArr.Length;
}
}
