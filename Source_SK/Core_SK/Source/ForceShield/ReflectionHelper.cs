using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;

namespace Enhanced_Development.ShieldUtils
{
    public class ReflectionHelper
    {
        /// <summary>
        /// Uses reflection to get the field value from an object.
        /// </summary>
        ///
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        ///
        /// <returns>The field value from the object.</returns>
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        /*
        internal static object GetInstanceMethod(Type type, object instance, string methodName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty
                | BindingFlags.Static;

            MethodInfo method = type.GetMethod(methodName, bindFlags);

            //FieldInfo field = type.GetField(fieldName, bindFlags);
            return method;
        }

        public static object GetPropertyValue(object scr, string propertyName)
        {
            Log.Message("Getting Type");
            Type currentType = scr.GetType();
            Log.Message("Getting currentPropertyInfo");
            PropertyInfo currentPropertyInfo = currentType.GetProperty(propertyName);
            if (currentPropertyInfo == null)
            {
                Log.Error("currentPropertyInfo null");
            }

            Log.Message("Getting currentValue");
            object currentValue = currentPropertyInfo.GetValue(scr, null);
            Log.Message("Return");

            return currentValue;
        }*/

    }
}
