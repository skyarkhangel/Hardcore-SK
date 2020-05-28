using System;

namespace Planets_Code
{
	public class ModMethodData
	{
		public string PackageId { get; }
		public string TypeName { get; }
		public string MethodName { get; }

		public ModMethodData(string packageId, string typeName, string methodName)
		{
			PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
			TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
			MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
		}
	}
}
