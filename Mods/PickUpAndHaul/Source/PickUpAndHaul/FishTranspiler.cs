using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace PickUpAndHaul;
public static class FishTranspiler
{
	public struct Container
	{
		public Container(OpCode opcode, object operand = null)
		{
			OpCode = opcode;
			Operand = operand;
		}
		public Container(CodeInstruction instruction)
		{
			OpCode = instruction.opcode;
			Operand = instruction.operand;
		}
		public override string ToString() => $"{OpCode} {Operand}";
		public CodeInstruction ToInstruction() => new(OpCode, Operand);
		public CodeInstruction ToLoadLocal() => new(OpCode.ToLoadLocal(), Operand);
		public CodeInstruction ToStoreLocal() => new(OpCode.ToStoreLocal(), Operand);
		public CodeInstruction ToLoadField() => new(OpCode.ToLoadField(), Operand);
		public CodeInstruction ToStoreField() => new(OpCode.ToStoreField(), Operand);
		public CodeInstruction ToAddress() => new(OpCode.ToAddress(), GetIndex());
		public CodeInstruction WithLabels(params Label[] labels) => ToInstruction().WithLabels(labels);
		public CodeInstruction WithLabels(IEnumerable<Label> labels) => ToInstruction().WithLabels(labels);
		public int GetIndex() => Operand is LocalBuilder builder ? builder.LocalIndex
			: Operand is ushort ushortVar ? ushortVar
			: Operand is byte byteVar ? byteVar
			: OpCode.TryGetIndex() is { } index ? index
			: Operand as int? ?? throw new ArgumentException($"{OpCode} has operand {Operand}. This is not a supported index.");
		public override bool Equals(object obj) => obj is CodeInstruction code ? this == code : obj is Container helper && this == helper;
		public override int GetHashCode() => unchecked((((1009 * 9176) + OpCode.GetHashCode()) * 9176) + Operand.GetHashCode());
		public static bool operator ==(Container lhs, Container rhs) => lhs.OpCode == rhs.OpCode && CompareOperands(lhs.Operand, rhs.Operand);
		public static bool operator !=(Container lhs, Container rhs) => !(lhs == rhs);
		public static bool operator ==(Container helper, CodeInstruction code) => helper.OpCode == code.opcode && CompareOperands(helper.Operand, code.operand);
		public static bool operator !=(Container helper, CodeInstruction code) => !(helper == code);
		public static bool operator ==(CodeInstruction code, Container helper) => helper == code;
		public static bool operator !=(CodeInstruction code, Container helper) => !(helper == code);
		public static implicit operator CodeInstruction(Container helper) => helper.ToInstruction();
		public OpCode OpCode { get; set; }
		public object Operand { get; set; }
	}

	public static Container Copy(CodeInstruction instruction) => new(instruction);

	public static Container FindArgument(MethodBase method, Type argumentType) => FindArgument(method, p => p.ParameterType == argumentType);
	public static Container FindArgument(MethodBase method, Func<ParameterInfo, bool> predicate) => Argument(FirstArgumentIndex(method, predicate));
	public static Container Argument(MethodBase method, string name) => Argument(FirstArgumentIndex(method, p => p.Name == name));
	public static Container Argument(int index) => new() { OpCode = GetLoadArgumentOpCode(index), Operand = GetOperandFromIndex(index) };

	public static Container FindLoadLocal(MethodBase method, Type localType) => FindLoadLocal(method, l => l.LocalType == localType);
	public static Container FindLoadLocal(MethodBase method, Predicate<LocalVariableInfo> predicate) => FindLoadLocals(method, predicate).First();
	public static IEnumerable<Container> FindLoadLocals(MethodBase method, Predicate<LocalVariableInfo> predicate)
	{
		foreach (var index in GetLocalIndices(method, predicate))
        {
            yield return new() { OpCode = GetLoadLocalOpCode(index), Operand = GetOperandFromIndex(index) };
        }
    }
	public static Container FindLoadLocal(IEnumerable<CodeInstruction> codes, Type localType) => LoadLocal(GetLocalOperandsOrIndices(codes, c => c.Returns(localType)).First());
	public static Container LoadLocal(object operand) => operand is LocalBuilder builder ? LoadLocal(builder) : LoadLocal((int)operand);
	public static Container LoadLocal(LocalBuilder builder) => new() { OpCode = GetLoadLocalOpCode(builder.LocalIndex), Operand = GetOperandFromBuilder(builder) };
	public static Container LoadLocal(int index) => new() { OpCode = GetLoadLocalOpCode(index), Operand = GetOperandFromIndex(index) };

	public static Container FindStoreLocal(MethodBase method, Type localType) => FindStoreLocal(method, l => l.LocalType == localType);
	public static Container FindStoreLocal(MethodBase method, Predicate<LocalVariableInfo> predicate) => FindStoreLocals(method, predicate).First();
	public static IEnumerable<Container> FindStoreLocals(MethodBase method, Predicate<LocalVariableInfo> predicate)
	{
		foreach (var index in GetLocalIndices(method, predicate))
        {
            yield return new() { OpCode = GetStoreLocalOpCode(index), Operand = GetOperandFromIndex(index) };
        }
    }
	public static Container FindStoreLocal(IEnumerable<CodeInstruction> codes, Type localType) => StoreLocal(GetLocalOperandsOrIndices(codes, c => c.Returns(localType)).First());
	public static Container StoreLocal(object operand) => operand is LocalBuilder builder ? StoreLocal(builder) : StoreLocal((int)operand);
	public static Container StoreLocal(LocalBuilder builder) => new() { OpCode = GetStoreLocalOpCode(builder.LocalIndex), Operand = GetOperandFromBuilder(builder) };
	public static Container StoreLocal(int index) => new() { OpCode = GetStoreLocalOpCode(index), Operand = GetOperandFromIndex(index) };

	public static Container LoadConstant(int integer) => new() { OpCode = GetLoadConstantOpCode(integer), Operand = GetOperandOfConstant(integer) };

	public static Container Call<T>(T method, bool forceNoCallvirt = false) where T : Delegate => Call(method.Method, forceNoCallvirt);
	public static Container Call(Expression<Action> expression, bool forceNoCallvirt = false) => Call(SymbolExtensions.GetMethodInfo(expression), forceNoCallvirt);
	public static Container Call(string assembly, string type, string name, Type[] parameters = null, Type[] generics = null, bool forceNoCallvirt = false) => Call(Type.GetType($"{type}, {assembly}"), name, parameters, generics, forceNoCallvirt);
	public static Container Call(Type type, string name, Type[] parameters = null, Type[] generics = null, bool forceNoCallvirt = false) => Call(AccessTools.Method(type, name, parameters, generics), forceNoCallvirt);
	public static Container Call(MethodBase method, bool forceNoCallvirt = false)
		=> method != null ? new() { OpCode = method.IsStatic || forceNoCallvirt ? OpCodes.Call : OpCodes.Callvirt, Operand = method }
		: throw new ArgumentNullException($"method");

	public static Container LoadField(Type type, string name)
	{
		var field = AccessTools.Field(type, name);
		return field != null ? new() { OpCode = field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, Operand = field }
			: throw new ArgumentException($"FishTranspiler.LoadField failed to find a field at {type.FullDescription()}:{name}");
	}
	public static Container LoadFieldAddress(Type type, string name)
	{
		var field = AccessTools.Field(type, name);
		return field != null ? new() { OpCode = field.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda, Operand = field }
			: throw new ArgumentException($"FishTranspiler.LoadFieldAddress failed to find a field at {type.FullDescription()}:{name}");
	}
	public static Container StoreField(Type type, string name)
	{
		var field = AccessTools.Field(type, name);
		return field != null ? new() { OpCode = field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, Operand = field }
			: throw new ArgumentException($"FishTranspiler.StoreField failed to find a field at {type.FullDescription()}:{name}");
	}

	public static Container CallPropertyGetter(Type type, string name, bool forceNoCallvirt = false)
	{
		var method = AccessTools.PropertyGetter(type, name);
		return method != null ? Call(method, forceNoCallvirt) : throw new ArgumentException($"FishTranspiler.CallPropertyGetter failed to find a property at {type.FullDescription()}:{name}");
	}
	public static Container CallPropertySetter(Type type, string name, bool forceNoCallvirt = false)
	{
		var method = AccessTools.PropertySetter(type, name);
		return method != null ? Call(method, forceNoCallvirt) : throw new ArgumentException($"FishTranspiler.CallPropertySetter failed to find a property at {type.FullDescription()}:{name}");
	}

	public static Container LoadString(string text) => new() { OpCode = OpCodes.Ldstr, Operand = text };

	public static Container IfFalse_Short(Label label) => new() { OpCode = OpCodes.Brfalse_S, Operand = label };
	public static Container IfFalse(Label label) => new() { OpCode = OpCodes.Brfalse, Operand = label };
	public static Container IfTrue_Short(Label label) => new() { OpCode = OpCodes.Brtrue_S, Operand = label };
	public static Container IfTrue(Label label) => new() { OpCode = OpCodes.Brtrue, Operand = label };
	public static Container IfNull_Short(Label label) => new() { OpCode = OpCodes.Brfalse_S, Operand = label };
	public static Container IfNull(Label label) => new() { OpCode = OpCodes.Brfalse, Operand = label };
	public static Container IfNotNull_Short(Label label) => new() { OpCode = OpCodes.Brtrue_S, Operand = label };
	public static Container IfNotNull(Label label) => new() { OpCode = OpCodes.Brtrue, Operand = label };
	public static Container IfEqual_Short(Label label) => new() { OpCode = OpCodes.Beq_S, Operand = label };
	public static Container IfEqual(Label label) => new() { OpCode = OpCodes.Beq, Operand = label };
	public static Container IfGreaterThan_Short(Label label) => new() { OpCode = OpCodes.Bgt_S, Operand = label };
	public static Container IfGreaterThan(Label label) => new() { OpCode = OpCodes.Bgt, Operand = label };
	public static Container IfGreaterThanOrEqual_Short(Label label) => new() { OpCode = OpCodes.Bge_S, Operand = label };
	public static Container IfGreaterThanOrEqual(Label label) => new() { OpCode = OpCodes.Bge, Operand = label };
	public static Container IfLessThan_Short(Label label) => new() { OpCode = OpCodes.Blt_S, Operand = label };
	public static Container IfLessThan(Label label) => new() { OpCode = OpCodes.Blt, Operand = label };
	public static Container IfLessThanOrEqual_Short(Label label) => new() { OpCode = OpCodes.Ble_S, Operand = label };
	public static Container IfLessThanOrEqual(Label label) => new() { OpCode = OpCodes.Ble, Operand = label };

	public static Container This => new() { OpCode = OpCodes.Ldarg_0 };
	public static Container Return => new() { OpCode = OpCodes.Ret };

	public static bool CallReturns(this CodeInstruction instruction, Type type) => (instruction.opcode == OpCodes.Callvirt || instruction.opcode == OpCodes.Call) && ((MethodInfo)instruction.operand).ReturnType == type;
	public static bool FieldReturns(this CodeInstruction instruction, Type type) => (instruction.opcode == OpCodes.Ldfld || instruction.opcode == OpCodes.Ldsfld) && ((FieldInfo)instruction.operand).FieldType == type;
	public static bool Returns(this CodeInstruction instruction, Type type) => instruction.CallReturns(type) || instruction.FieldReturns(type);

	public static int FirstArgumentIndex(MethodBase method, Func<ParameterInfo, bool> predicate) => method.GetParameters().First(predicate).Position + (method.IsStatic ? 0 : 1);
	public static int FirstArgumentIndex(MethodBase method, Type argumentType) => FirstArgumentIndex(method, p => p.ParameterType == argumentType);

	public static IEnumerable<object> GetLocalOperandsOrIndices(IEnumerable<CodeInstruction> codes, Predicate<CodeInstruction> predicate)
	{
		CodeInstruction previousCode = null;
		foreach (var code in codes)
		{
			if (previousCode is not null && predicate(previousCode) && code.IsStloc() && code.opcode is var opcode)
			{
				yield return (object)opcode.TryGetIndex()
					?? (code.operand is LocalBuilder builder ? builder
					: code.operand is byte index ? index
					: code.operand is ushort unsigned ? unsigned
					: throw new NotSupportedException($"{code.opcode} returned {code.operand}. This is not supported."));
			}
			previousCode = code;
		}
	}
	public static IEnumerable<int> GetLocalIndices(IEnumerable<CodeInstruction> codes, Predicate<CodeInstruction> predicate)
	{
		foreach (var operand in GetLocalOperandsOrIndices(codes, predicate))
        {
            yield return operand is LocalBuilder builder ? builder.LocalIndex : (int)operand;
        }
    }
	public static IEnumerable<int> GetLocalIndices(MethodBase method, Predicate<LocalVariableInfo> predicate)
	{
		var variables = method.GetMethodBody().LocalVariables;
		for (var i = 0; i < variables.Count; i++)
		{
			if (predicate(variables[i]))
            {
                yield return variables[i].LocalIndex;
            }
        }
	}

	public static IEnumerable<CodeInstruction> MethodReplacer<T, V>(this IEnumerable<CodeInstruction> instructions, T from, V to) where T : Delegate where V : Delegate
		=> instructions.MethodReplacer(from.Method, to.Method);

	public static bool LoadsLocal(this OpCode opcode) => opcode == OpCodes.Ldloc_S || opcode == OpCodes.Ldloc_0 || opcode == OpCodes.Ldloc_1 || opcode == OpCodes.Ldloc_2 || opcode == OpCodes.Ldloc_3 || opcode == OpCodes.Ldloc;
	public static OpCode ToLoadLocal(this OpCode opcode)
		=> opcode.LoadsLocal() ? opcode
		: opcode.TryGetIndex() is { } index ? GetLoadLocalOpCode(index)
		: opcode == OpCodes.Stloc_S ? OpCodes.Ldloc_S
		: opcode == OpCodes.Stloc ? OpCodes.Ldloc
		: throw new InvalidCastException($"{opcode} cannot be cast to Ldloc.");
	public static bool StoresLocal(this OpCode opcode) => opcode == OpCodes.Stloc_S || opcode == OpCodes.Stloc_0 || opcode == OpCodes.Stloc_1 || opcode == OpCodes.Stloc_2 || opcode == OpCodes.Stloc_3 || opcode == OpCodes.Stloc;
	public static OpCode ToStoreLocal(this OpCode opcode)
		=> opcode.StoresLocal() ? opcode
		: opcode.TryGetIndex() is { } index ? GetStoreLocalOpCode(index)
		: opcode == OpCodes.Ldloc_S ? OpCodes.Stloc_S
		: opcode == OpCodes.Ldloc ? OpCodes.Stloc
		: throw new InvalidCastException($"{opcode} cannot be cast to Stloc");
	public static bool LoadsField(this OpCode opcode) => opcode == OpCodes.Ldfld || opcode == OpCodes.Ldsfld || opcode == OpCodes.Ldflda || opcode == OpCodes.Ldsflda;
	public static OpCode ToLoadField(this OpCode opcode)
		=> opcode.LoadsField() ? opcode
		: opcode == OpCodes.Stfld ? OpCodes.Ldfld
		: opcode == OpCodes.Stsfld ? OpCodes.Ldsfld
		: throw new InvalidCastException($"{opcode} cannot be cast to Ldfld");
	public static bool StoresField(this OpCode opcode) => opcode == OpCodes.Stfld || opcode == OpCodes.Stsfld;
	public static OpCode ToStoreField(this OpCode opcode)
		=> opcode.StoresField() ? opcode
		: opcode == OpCodes.Ldfld ? OpCodes.Stfld
		: opcode == OpCodes.Ldsfld ? OpCodes.Stsfld
		: throw new InvalidCastException($"{opcode} cannot be cast to Stfld");
	public static OpCode GetLoadArgumentOpCode(int index) => index switch
	{
		0 => OpCodes.Ldarg_0,
		1 => OpCodes.Ldarg_1,
		2 => OpCodes.Ldarg_2,
		3 => OpCodes.Ldarg_3,
		< 256 => OpCodes.Ldarg_S,
		_ => OpCodes.Ldarg
	};
	public static OpCode GetStoreLocalOpCode(int index) => index switch
	{
		0 => OpCodes.Stloc_0,
		1 => OpCodes.Stloc_1,
		2 => OpCodes.Stloc_2,
		3 => OpCodes.Stloc_3,
		< 256 => OpCodes.Stloc_S,
		_ => OpCodes.Stloc
	};
	public static OpCode GetLoadLocalOpCode(int index) => index switch
	{
		0 => OpCodes.Ldloc_0,
		1 => OpCodes.Ldloc_1,
		2 => OpCodes.Ldloc_2,
		3 => OpCodes.Ldloc_3,
		< 256 => OpCodes.Ldloc_S,
		_ => OpCodes.Ldloc
	};
	public static OpCode GetLoadConstantOpCode(int index) => index switch
	{
		0 => OpCodes.Ldc_I4_0,
		1 => OpCodes.Ldc_I4_1,
		2 => OpCodes.Ldc_I4_2,
		3 => OpCodes.Ldc_I4_3,
		4 => OpCodes.Ldc_I4_4,
		5 => OpCodes.Ldc_I4_5,
		6 => OpCodes.Ldc_I4_6,
		7 => OpCodes.Ldc_I4_7,
		8 => OpCodes.Ldc_I4_8,
		-1 => OpCodes.Ldc_I4_M1,
		< 128 and > -129 => OpCodes.Ldc_I4_S,
		_ => OpCodes.Ldc_I4
	};
	public static OpCode ToAddress(this OpCode opcode)
		=> opcode == OpCodes.Ldarg_0 || opcode == OpCodes.Ldarg_1 || opcode == OpCodes.Ldarg_2 || opcode == OpCodes.Ldarg_3 || opcode == OpCodes.Ldarg_S ? OpCodes.Ldarga_S
		: opcode == OpCodes.Ldarg ? OpCodes.Ldarga
		: opcode == OpCodes.Ldloc_0 || opcode == OpCodes.Ldloc_1 || opcode == OpCodes.Ldloc_2 || opcode == OpCodes.Ldloc_3 || opcode == OpCodes.Ldloc_S ? OpCodes.Ldloca_S
		: opcode == OpCodes.Ldloc ? OpCodes.Ldloca
		: opcode == OpCodes.Ldfld ? OpCodes.Ldflda
		: opcode == OpCodes.Ldsfld ? OpCodes.Ldsflda
		: throw new NotSupportedException($"Cannot cast {opcode} to address opcode");
	public static int? TryGetIndex(this OpCode opcode)
		=> opcode == OpCodes.Ldarg_0 ? 0
		: opcode == OpCodes.Ldarg_1 ? 1
		: opcode == OpCodes.Ldarg_2 ? 2
		: opcode == OpCodes.Ldarg_3 ? 3
		: opcode == OpCodes.Ldloc_0 ? 0
		: opcode == OpCodes.Ldloc_1 ? 1
		: opcode == OpCodes.Ldloc_2 ? 2
		: opcode == OpCodes.Ldloc_3 ? 3
		: opcode == OpCodes.Stloc_0 ? 0
		: opcode == OpCodes.Stloc_1 ? 1
		: opcode == OpCodes.Stloc_2 ? 2
		: opcode == OpCodes.Stloc_3 ? 3
		: opcode == OpCodes.Ldc_I4_0 ? 0
		: opcode == OpCodes.Ldc_I4_1 ? 1
		: opcode == OpCodes.Ldc_I4_2 ? 2
		: opcode == OpCodes.Ldc_I4_3 ? 3
		: opcode == OpCodes.Ldc_I4_4 ? 4
		: opcode == OpCodes.Ldc_I4_5 ? 5
		: opcode == OpCodes.Ldc_I4_6 ? 6
		: opcode == OpCodes.Ldc_I4_7 ? 7
		: opcode == OpCodes.Ldc_I4_8 ? 8
		: opcode == OpCodes.Ldc_I4_M1 ? -1
		: null;
	public static object GetOperandFromIndex(int index) => index > 3 ? index : null;
	public static object GetOperandOfConstant(int integer) => integer is < 9 and > -2 ? null : integer;
	public static object GetOperandFromBuilder(LocalBuilder builder) => builder.LocalIndex > 3 ? builder : null;
	public static bool CompareOperands(object lhs, object rhs) => (lhs is LocalBuilder lhBuilder ? lhBuilder.LocalIndex : lhs) == (rhs is LocalBuilder rhBuilder ? rhBuilder.LocalIndex : rhs);
}