using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ZhentarFix
{
	class Utils
	{
		public static Func<TValue> GetStaticFieldAccessor<TObject, TValue>(string fieldName)
		{
			return GetStaticFieldAccessor<TValue>(fieldName, typeof(TObject));
		}

		public static Func<TValue> GetStaticFieldAccessor<TValue>(string fieldName, Type classType)
		{
			var fieldInfo = classType.GetField(fieldName, Detours.UniversalBindingFlags);
			var member = Expression.Field(null, fieldInfo);
			var lambda = Expression.Lambda(typeof(Func<TValue>), member);
			var compiled = (Func<TValue>)lambda.Compile();
			return compiled;
		}

		public static Func<TObject, TValue> GetFieldAccessor<TObject, TValue>(string fieldName)
		{
			var param = Expression.Parameter(typeof(TObject), "arg");
			var member = Expression.Field(param, fieldName);
			var lambda = Expression.Lambda(typeof(Func<TObject, TValue>), member, param);
			var compiled = (Func<TObject, TValue>)lambda.Compile();
			return compiled;
		}

		public static Action<TObject, TArgs1, TArgs2> GetMethodInvoker<TObject, TArgs1, TArgs2>(string methodName)
		{
			var methodInfo = typeof(TObject).GetMethod(methodName, Detours.UniversalBindingFlags, null, new[] { typeof(TArgs1), typeof(TArgs2) }, null);

			var param = Expression.Parameter(typeof(TObject), "thisArg");

			var argParams = new[]
				{ Expression.Parameter(typeof(TArgs1), "arg1"), Expression.Parameter(typeof(TArgs2), "arg2")};

			var call = Expression.Call(param, methodInfo, argParams);

			var lambda = Expression.Lambda(typeof(Action<TObject, TArgs1, TArgs2>), call, param, argParams[0], argParams[1]);
			var compiled = (Action<TObject, TArgs1, TArgs2>)lambda.Compile();
			return compiled;

		}

		public static Action<TObject, TArgs1> GetMethodInvoker<TObject, TArgs1>(string methodName)
		{
			var methodInfo = typeof(TObject).GetMethod(methodName, Detours.UniversalBindingFlags, null, new[] { typeof(TArgs1)}, null);

			var param = Expression.Parameter(typeof(TObject), "thisArg");

			var argParams = new[] { Expression.Parameter(typeof(TArgs1), "arg1")};

			var call = Expression.Call(param, methodInfo, argParams);

			var lambda = Expression.Lambda(typeof(Action<TObject, TArgs1>), call, param, argParams[0]);
			var compiled = (Action<TObject, TArgs1>)lambda.Compile();
			return compiled;

		}
	}
}
