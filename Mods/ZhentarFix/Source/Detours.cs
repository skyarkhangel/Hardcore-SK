using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Verse;


namespace ZhentarFix
{
	//Detour Implementation derived from CCL
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public class DetourMember : Attribute
	{
		public const Type DefaultTargetClass = null;
		public const string DefaultTargetMemberName = "";

		public Type targetClass;
		public string targetMember;

		public DetourMember(Type targetClass, string targetMember)
		{
			this.targetClass = targetClass;
			this.targetMember = targetMember;
		}

		public DetourMember(Type targetClass) : this(targetClass, DefaultTargetMemberName)
		{
		}

		public DetourMember(string targetMember) : this(DefaultTargetClass, targetMember)
		{
		}

		public DetourMember() : this(DefaultTargetClass, DefaultTargetMemberName)
		{
		}

	}

	public class DetourPair
	{
		public Type sourceMethodTarget;
		public MethodInfo sourceMethod;
		public Type destinationMethodTarget;
		public MethodInfo destinationMethod;

		public DetourPair(Type sourceMethodTarget, MethodInfo sourceMethod, Type destinationMethodTarget, MethodInfo destinationMethod)
		{
			this.sourceMethodTarget = sourceMethodTarget;
			this.sourceMethod = sourceMethod;
			this.destinationMethodTarget = destinationMethodTarget;
			this.destinationMethod = destinationMethod;
		}

	}

	[StaticConstructorOnStartup]
	internal static class Detours
	{
		public const BindingFlags UniversalBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;



		static Detours()
		{
			LongEventHandler.QueueLongEvent(Inject, "Initializing", true, null);
		}

		private static void Inject()
		{
			var detourPairs = GetDetours(typeof(Detours).Assembly);

			foreach (var detourPair in detourPairs)
			{
				if (!TryDetourFromTo(detourPair.sourceMethod, detourPair.destinationMethod))
				{
					Log.Error("Detouring Failed!");
				}
			}

		}


		/// <summary>
		/// Gets a list detours in an assembly with matching sequence and timing parameters.
		/// </summary>
		/// <returns>The list of detours with matching sequence and timing</returns>
		/// <param name="assembly">Assembly to get detours from</param>
		/// <param name="sequence">Injection sequence</param>
		/// <param name="timing">Injection timing</param>
		public static List<DetourPair> GetDetours(Assembly assembly)
		{
			// Get only types which have methods and/or properties marked with either of the detour attributes
			List<Type> toTypes = assembly
				.GetTypes()
				.Where(toType => (
					(toType.GetMethods(UniversalBindingFlags).Any(toMethod => toMethod.HasAttribute<DetourMember>())) ||
					(toType.GetProperties(UniversalBindingFlags).Any(toProperty => toProperty.HasAttribute<DetourMember>()))
				))
				.ToList();

			// No types are detouring, return null
			if (toTypes.NullOrEmpty())
			{
				return null;
			}

			// Create return list for the detours
			var detours = new List<DetourPair>();

			// Process the types and fetch their detours
			foreach (var toType in toTypes)
			{
				// Get the raw methods
				GetDetouredMethods(ref detours, toType);
				// Get the cloaked methods (properties)
				GetDetouredProperties(ref detours, toType);
			}

			// Return the list for this sequence and timing
			return detours;
		}

		/// <summary>
		/// Returns a list of detour methods with matching sequence and timing from a class.
		/// </summary>
		/// <param name="detours">List to add the detours from this class to</param>
		/// <param name="destinationType">The class to check for detour methods</param>
		/// <param name="sequence">Injection sequence</param>
		/// <param name="timing">Injection timing</param>
		private static void GetDetouredMethods(ref List<DetourPair> detours, Type destinationType)
		{
			var destinationMethods = destinationType
				.GetMethods(UniversalBindingFlags)
				.Where(destinationMethod => destinationMethod.HasAttribute<DetourMember>())
				.ToList();
			if (destinationMethods.NullOrEmpty())
			{   // No methods to detour
				return;
			}
			foreach (var destinationMethod in destinationMethods)
			{
				DetourMember attribute = null;
				if (destinationMethod.TryGetAttribute(out attribute))
				{
					var memberClass = GetDetourTargetClass(destinationMethod, attribute);
					if (memberClass == null)
					{   // Report and ignore any missing classes
						Log.Error(string.Format("MemberClass '{2}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationMethod.Name, FullNameOfType(attribute.targetClass)));
						continue;
					}
					var sourceMethod = GetDetouredMethodInt(memberClass, attribute.targetMember, destinationMethod);
					if (sourceMethod == null)
					{   // Report and ignore any missing methods
						Log.Error(string.Format("TargetMember '{2}.{3}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationMethod.Name, memberClass.FullName, attribute.targetMember));
						continue;
					}
					// Add detour for method
					detours.Add(new DetourPair(GetMethodTargetClass(sourceMethod, null), sourceMethod, GetMethodTargetClass(destinationMethod, attribute), destinationMethod));
				}
			}
		}

		/// <summary>
		/// Returns a list of detour property methods (get/set) with matching sequence and timing from a class.
		/// </summary>
		/// <param name="detours">List to add the detours from this class to</param>
		/// <param name="destinationType">The class to check for detour properties</param>
		/// <param name="sequence">Injection sequence</param>
		/// <param name="timing">Injection timing</param>
		private static void GetDetouredProperties(ref List<DetourPair> detours, Type destinationType)
		{
			var destinationProperties = destinationType
				.GetProperties(UniversalBindingFlags)
				.Where(destinationProperty => destinationProperty.HasAttribute<DetourMember>())
				.ToList();
			if (destinationProperties.NullOrEmpty())
			{   // No properties to detour
				return;
			}
			foreach (var destinationProperty in destinationProperties)
			{
				DetourMember attribute = null;
				if (destinationProperty.TryGetAttribute(out attribute))
				{
					var memberClass = GetDetourTargetClass(destinationProperty, attribute);
					if (memberClass == null)
					{   // Report and ignore any missing classes
						Log.Error(string.Format("MemberClass '{2}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, FullNameOfType(attribute.targetClass)));
						continue;
					}
					var sourceProperty = GetDetouredPropertyInt(memberClass, attribute.targetMember, destinationProperty);
					if (sourceProperty == null)
					{   // Report and ignore any missing properties
						Log.Error(string.Format("TargetMember '{2}.{3}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, memberClass.FullName, attribute.targetMember));
						continue;
					}
					var destinationMethod = destinationProperty.GetGetMethod(true);
					if (destinationMethod != null)
					{   // Check for get method detour
						var sourceMethod = sourceProperty.GetGetMethod(true);
						if (sourceMethod == null)
						{   // Report and ignore missing get method
							Log.Error(string.Format("TargetMember '{2}.{3}' has no get method for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, memberClass.FullName, attribute.targetMember));
						}
						else
						{   // Add detour for get method
							detours.Add(new DetourPair(GetMethodTargetClass(sourceMethod, null), sourceMethod, GetMethodTargetClass(destinationMethod, attribute), destinationMethod));
						}
					}
					destinationMethod = destinationProperty.GetSetMethod(true);
					if (destinationMethod != null)
					{   // Check for set method detour
						var sourceMethod = sourceProperty.GetSetMethod(true);
						if (sourceMethod == null)
						{   // Report and ignore missing set method
							Log.Error(string.Format("TargetMember '{2}.{3}' has no set method for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, memberClass.FullName, attribute.targetMember));
						}
						else
						{   // Add detour for set method
							detours.Add(new DetourPair(GetMethodTargetClass(sourceMethod, null), sourceMethod, GetMethodTargetClass(destinationMethod, attribute), destinationMethod));
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the specific method that is detoured.
		/// </summary>
		/// <returns>The MethodInfo of the method to be detoured or null on failure</returns>
		/// <param name="sourceClass">Class that contains the expected method to be detoured</param>
		/// <param name="sourceMember">Name of the expected method to be detoured</param>
		/// <param name="destinationMethod">MethodInfo of the detour</param>
		private static MethodInfo GetDetouredMethodInt(Type sourceClass, string sourceMember, MethodInfo destinationMethod)
		{
			if (sourceMember == DetourMember.DefaultTargetMemberName)
			{
				sourceMember = destinationMethod.Name;
			}
			MethodInfo sourceMethod = null;
			try
			{   // Try to get method direct
				sourceMethod = sourceClass.GetMethod(sourceMember, UniversalBindingFlags);
			}
			catch
			{   // May be ambiguous, try from parameter types (thanks Zhentar for the change from count to types)
				sourceMethod = sourceClass.GetMethods(UniversalBindingFlags)
										  .FirstOrDefault(checkMethod => (
											 (checkMethod.Name == sourceMember) &&
											 (checkMethod.ReturnType == destinationMethod.ReturnType) &&
											 (checkMethod.GetParameters().Select(checkParameter => checkParameter.ParameterType)
																	 	 .SequenceEqual(destinationMethod.GetParameters()
																										 .Skip(GetMethodType(destinationMethod) == MethodType.Extension ? 1 : 0)
																										 .Select(destinationParameter => destinationParameter.ParameterType)))
											));
			}
			var fixedName = GetFixedMemberName(sourceMember);
			if (
				(sourceMethod == null) &&
				(sourceMember != fixedName)
			)
			{
				return GetDetouredMethodInt(sourceClass, fixedName, destinationMethod);
			}
			return sourceMethod;
		}

		/// <summary>
		/// Gets the specific property that is detoured.
		/// </summary>
		/// <returns>The PropertyInfo of the method to be detoured or null on failure</returns>
		/// <param name="sourceClass">Class that contains the expected property to be detoured</param>
		/// <param name="sourceMember">Name of the expected property to be detoured</param>
		/// <param name="destinationProperty">PropertyInfo of the detour</param>
		private static PropertyInfo GetDetouredPropertyInt(Type sourceClass, string sourceMember, PropertyInfo destinationProperty)
		{
			if (sourceMember == DetourMember.DefaultTargetMemberName)
			{
				sourceMember = destinationProperty.Name;
			}
			var sourceProperty = sourceClass.GetProperty(sourceMember, UniversalBindingFlags);
			var fixedName = GetFixedMemberName(sourceMember);
			if (
				(sourceProperty == null) &&
				(sourceMember != fixedName)
			)
			{
				return GetDetouredPropertyInt(sourceClass, fixedName, destinationProperty);
			}
			return sourceProperty;
		}

		public static bool TryDetourFromTo(MethodInfo sourceMethod, MethodInfo destinationMethod)
		{
			// Error out on null arguments
			if (sourceMethod == null)
			{
				Log.Error("Source MethodInfo is null: Detours");
				return false;
			}

			if (destinationMethod == null)
			{
				Log.Error("Destination MethodInfo is null: Detours");
				return false;
			}

			// Used for deeper method checks to return failure string
			var reason = string.Empty;

			// Make sure the class containing the detour doesn't contain instance fields
			if (!DetourContainerClassIsFieldSafe(destinationMethod.DeclaringType))
			{
				Log.Error(string.Format("'{0}' contains fields which are not static!  Detours can not be defined in classes which have instance fields!", FullNameOfType(destinationMethod.DeclaringType)));
				return false;
			}

			// Make sure the two methods are call compatible
			if (!MethodsAreCallCompatible(GetMethodTargetClass(sourceMethod), sourceMethod, GetMethodTargetClass(destinationMethod), destinationMethod, out reason))
			{
				Log.Error(string.Format("Methods are not call compatible when trying to detour '{0}' to '{1}' :: {2}", FullMethodName(sourceMethod), FullMethodName(destinationMethod), reason));
				return false;
			}

			// Method is now detoured, we are doneski!
			return TryDetourFromToInt(sourceMethod, destinationMethod);
		}

		/**
			This is a basic first implementation of the IL method 'hooks' (detours) made possible by RawCode's work;
			https://ludeon.com/forums/index.php?topic=17143.0

			Performs detours, spits out basic logs and warns if a method is detoured multiple times.
		**/
		public static unsafe bool TryDetourFromToInt(MethodBase source, MethodBase destination)
		{

			// keep track of detours and spit out some messaging
			string sourceString = source.DeclaringType.FullName + "." + source.Name + " @ 0x" + source.MethodHandle.GetFunctionPointer().ToString("X" + (IntPtr.Size * 2).ToString());
			string destinationString = destination.DeclaringType.FullName + "." + destination.Name + " @ 0x" + destination.MethodHandle.GetFunctionPointer().ToString("X" + (IntPtr.Size * 2).ToString());

			if (IntPtr.Size == sizeof(Int64))
			{
				// 64-bit systems use 64-bit absolute address and jumps
				// 12 byte destructive

				// Get function pointers
				long Source_Base = source.MethodHandle.GetFunctionPointer().ToInt64();
				long Destination_Base = destination.MethodHandle.GetFunctionPointer().ToInt64();

				// Native source address
				byte* Pointer_Raw_Source = (byte*)Source_Base;

				// Pointer to insert jump address into native code
				long* Pointer_Raw_Address = (long*)(Pointer_Raw_Source + 0x02);

				// Insert 64-bit absolute jump into native code (address in rax)
				// mov rax, immediate64
				// jmp [rax]
				*(Pointer_Raw_Source + 0x00) = 0x48;
				*(Pointer_Raw_Source + 0x01) = 0xB8;
				*Pointer_Raw_Address = Destination_Base; // ( Pointer_Raw_Source + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
				*(Pointer_Raw_Source + 0x0A) = 0xFF;
				*(Pointer_Raw_Source + 0x0B) = 0xE0;

			}
			else
			{
				// 32-bit systems use 32-bit relative offset and jump
				// 5 byte destructive

				// Get function pointers
				int Source_Base = source.MethodHandle.GetFunctionPointer().ToInt32();
				int Destination_Base = destination.MethodHandle.GetFunctionPointer().ToInt32();

				// Native source address
				byte* Pointer_Raw_Source = (byte*)Source_Base;

				// Pointer to insert jump address into native code
				int* Pointer_Raw_Address = (int*)(Pointer_Raw_Source + 1);

				// Jump offset (less instruction size)
				int offset = (Destination_Base - Source_Base) - 5;

				// Insert 32-bit relative jump into native code
				*Pointer_Raw_Source = 0xE9;
				*Pointer_Raw_Address = offset;
			}

			// done!
			return true;
		}

		/// <summary>
		/// This classification is used to validate detours and to get the appropriate
		/// operating context of methods.
		/// A method should never be classed as "invalid", this classification only happens
		/// when a method has the "ExtensionAttribute" but no parameters and therefore no
		/// "this" parameter.
		/// </summary>
		private enum MethodType
		{
			Invalid,
			Instance,
			Extension,
			Static
		}

		/// <summary>
		/// Return the full class and method name with optional method address
		/// </summary>
		/// <returns>Full class and name</returns>
		/// <param name="methodInfo">MethodInfo of method</param>
		/// <param name="withAddress">Optional bool flag to add the address</param>
		private static string FullMethodName(MethodInfo methodInfo, bool withAddress = false)
		{
			var rVal = methodInfo.DeclaringType.FullName + "." + methodInfo.Name;
			if (withAddress)
			{
				rVal += " @ 0x" + methodInfo.MethodHandle.GetFunctionPointer().ToString("X" + (IntPtr.Size * 2).ToString());
			}
			return rVal;
		}

		/// <summary>
		/// null safe method to get the full name of a type for debugging
		/// </summary>
		/// <returns>The name of type or "null"</returns>
		/// <param name="type">Type to get the full name of</param>
		private static string FullNameOfType(Type type)
		{
			return type == null ? "null" : type.FullName;
		}

		/// <summary>
		/// Trims underscores one at a time from the begining of a member name
		/// </summary>
		/// <returns>The fixed member name</returns>
		/// <param name="memberName">Member name</param>
		private static string GetFixedMemberName(string memberName)
		{
			if (memberName[0] == "_"[0])
			{
				return memberName.Substring(1, memberName.Length - 1);
			}
			return memberName;
		}

		/// <summary>
		/// Return the type of method from the MethodInfo
		/// </summary>
		/// <returns>MethodType of method</returns>
		/// <param name="methodInfo">MethodInfo of method</param>
		private static MethodType GetMethodType(MethodInfo methodInfo)
		{
			if (!methodInfo.IsStatic)
			{
				return MethodType.Instance;
			}
			if (methodInfo.IsDefined(typeof(ExtensionAttribute), false))
			{
				return (!methodInfo.GetParameters().NullOrEmpty())
					? MethodType.Extension
					: MethodType.Invalid;
			}
			return MethodType.Static;
		}

		/// <summary>
		/// Gets the class that the method will operate in the context of.
		/// This is NOT necessarily the class that the method exists in.
		/// For extension methods the "this" parameter (first) will be returned
		/// regardless of whether it is a detour or not, pure static methods
		/// will return null (again, regardless of being a detour) as they
		/// don't operate in the context of class.  Instance methods will
		/// return the defining class for non-detours and the class being
		/// injected into for detours.
		/// </summary>
		/// <returns>The method target class</returns>
		/// <param name="info">MethodInfo of the method to check</param>
		/// <param name="attribute">DetourMember attribute</param>
		private static Type GetMethodTargetClass(MethodInfo info, DetourMember attribute = null)
		{
			var methodType = GetMethodType(info);

			if (methodType == MethodType.Static)
			{   // Pure static methods don't have a target class
				return null;
			}
			if (methodType == MethodType.Extension)
			{   // Regardless of whether this is the detour method or the method to be detoured, for extension methods we take the target class from the first parameter
				return info.GetParameters()[0].ParameterType;
			}

			if (attribute == null)
			{
				info.TryGetAttribute(out attribute);
			}
			if (attribute != null)
			{
				if (attribute.targetClass != DetourMember.DefaultTargetClass)
				{
					return attribute.targetClass;
				}
				return info.DeclaringType.BaseType;
			}

			return info.DeclaringType;
		}

		/// <summary>
		/// Gets the class that a detour will be injected into.
		/// </summary>
		/// <returns>The detour target class</returns>
		/// <param name="info">MemberInfo of the member to check</param>
		/// <param name="attribute">DetourMember attribute</param>
		private static Type GetDetourTargetClass(MemberInfo info, DetourMember attribute)
		{
			if (attribute != null)
			{
				if (attribute.targetClass != DetourMember.DefaultTargetClass)
				{
					return attribute.targetClass;
				}
				var methodInfo = info as MethodInfo;
				if (
					(info.DeclaringType.BaseType == typeof(System.Object)) &&
					(methodInfo != null) &&
					(GetMethodType(methodInfo) == MethodType.Extension)
				)
				{
					return methodInfo.GetParameters()[0].ParameterType;
				}
				return info.DeclaringType.BaseType;
			}

			return info.DeclaringType;
		}

		/// <summary>
		/// Checks that the class containing a detour does not contain instance fields.
		/// </summary>
		/// <returns>True if there are no instance fields; False if any instance field is contained in the class</returns>
		/// <param name="detourContainerClass">Detour container class</param>
		private static bool DetourContainerClassIsFieldSafe(Type detourContainerClass)
		{
			var fields = detourContainerClass.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (fields.NullOrEmpty())
			{   // No fields, no worries
				return true;
			}

			var baseFields = (detourContainerClass.BaseType?.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(f => !f.IsPrivate).Select(finfo => finfo.Name) ?? Enumerable.Empty<string>()).ToArray();
			//Make sure all instance fields were inherited from the parent class
			return fields.All(f => baseFields.Contains(f.Name));
		}

		/// <summary>
		/// Checks that B is a valid detour for A based on method types and class context (targets)
		/// </summary>
		/// <returns>True if B is a valid detour for A; False and reason string set otherwise</returns>
		/// <param name="targetA">Target class of A</param>
		/// <param name="typeA">MethodType of A</param>
		/// <param name="nameA">Method name of A</param>
		/// <param name="targetB">Target class of B</param>
		/// <param name="typeB">MethodType of B</param>
		/// <param name="nameB">Method name of B</param>
		/// <param name="reason">Return string with reason for failure</param>
		private static bool DetourTargetsAreValid(Type targetA, MethodType typeA, string nameA, Type targetB, MethodType typeB, string nameB, out string reason)
		{
			if (
				(typeA == MethodType.Instance) ||
				(typeA == MethodType.Extension)
			)
			{
				if (typeB == MethodType.Static)
				{
					reason = string.Format(
						"'{0}' is static but not an extension method",
						nameB
					);
					return false;
				}
				else if (targetA != targetB)
				{
					reason = string.Format(
						"Target classes do not match :: '{0}' target is '{1}'; '{2}' target is '{3}'",
						nameA,
						FullNameOfType(targetA),
						nameB,
						FullNameOfType(targetB)
					);
					return false;
				}
			}
			reason = string.Empty;
			return true;
		}


		/// <summary>
		/// Validates that two methods are call compatible
		/// </summary>
		/// <returns>True if the methods are call compatible; False and reason string set otherwise</returns>
		/// <param name="sourceTargetClass">Source method target class</param>
		/// <param name="sourceMethod">Source method</param>
		/// <param name="destinationTargetClass">Destination method target class</param>
		/// <param name="destinationMethod">Destination method</param>
		/// <param name="reason">Return string with reason for failure</param>
		private static bool MethodsAreCallCompatible(Type sourceTargetClass, MethodInfo sourceMethod, Type destinationTargetClass, MethodInfo destinationMethod, out string reason)
		{
			reason = string.Empty;
			if (sourceMethod.ReturnType != destinationMethod.ReturnType)
			{   // Return types don't match
				reason = string.Format(
					"Return type mismatch :: Source={1}, Destination={0}",
					sourceMethod.ReturnType.Name,
					destinationMethod.ReturnType.Name
				);
				return false;
			}

			// Get the method types
			var sourceMethodType = GetMethodType(sourceMethod);
			var destinationMethodType = GetMethodType(destinationMethod);

			// Make sure neither method is invalid
			if (sourceMethodType == MethodType.Invalid)
			{
				reason = "Source method is not an instance, valid extension, or static method";
				return false;
			}
			if (destinationMethodType == MethodType.Invalid)
			{
				reason = "Destination method is not an instance, valid extension, or static method";
				return false;
			}

			// Check validity of target classes
			if (!DetourTargetsAreValid(
				sourceTargetClass, sourceMethodType, FullMethodName(sourceMethod),
				destinationTargetClass, destinationMethodType, FullMethodName(destinationMethod),
				out reason))
			{
				return false;
			}

			// Method types and targets are all valid, now check the parameter lists
			var sourceParamsBase = sourceMethod.GetParameters();
			var destinationParamsBase = destinationMethod.GetParameters();

			// Get the first parameter index that isn't "this"
			var sourceParamBaseIndex = sourceMethodType == MethodType.Extension ? 1 : 0;
			var destinationParamBaseIndex = destinationMethodType == MethodType.Extension ? 1 : 0;

			// Parameter counts less "this"
			var sourceParamCount = sourceParamsBase.Length - sourceParamBaseIndex;
			var destinationParamCount = destinationParamsBase.Length - destinationParamBaseIndex;

			// Easy check that they have the same number of parameters
			if (sourceParamCount != destinationParamCount)
			{
				reason = "Parameter count mismatch";
				return false;
			}

			// Pick smaller parameter count (to skip "this")
			var paramCount = sourceParamCount > destinationParamCount ? destinationParamCount : sourceParamCount;

			// Now examine parameter-for-parameter
			if (paramCount > 0)
			{
				for (var offset = 0; offset < paramCount; offset++)
				{
					// Get parameter
					var sourceParam = sourceParamsBase[sourceParamBaseIndex + offset];
					var destinationParam = destinationParamsBase[destinationParamBaseIndex + offset];

					// Parameter types and attributes are all we care about
					if (
						(sourceParam.ParameterType != destinationParam.ParameterType) ||
						(sourceParam.Attributes != destinationParam.Attributes)
					)
					{   // Parameter type mismatch
						reason = string.Format(
							"Parameter type mismatch at index {6} :: Source='{0}', type='{1}', attributes='{2}'; Destination='{3}', type='{4}', attributes='{5}'",
							sourceParam.Name, sourceParam.ParameterType.FullName, sourceParam.Attributes,
							destinationParam.Name, destinationParam.ParameterType.FullName, destinationParam.Attributes,
							offset
						);
						return false;
					}
				}
			}

			// Methods are call compatible!
			return true;
		}
	}
}
