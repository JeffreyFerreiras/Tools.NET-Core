﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Tools.Exceptions;
using Tools.Extensions.Validation;

namespace Tools.Extensions.Conversion
{
    public static class ConversionEx
    {
        public static int ToInt32(this object o) => Convert.ToInt32(o);
        public static long ToInt64(this object o) => Convert.ToInt64(o);
        public static float ToFloat(this object o) => Convert.ToSingle(o);
        public static double ToDouble(this object o) => Convert.ToDouble(o);
        public static decimal ToDecimal(this object o) => Convert.ToDecimal(o);

        public static T ChangeType<T>(this object o)
        {
            return (T)Convert.ChangeType(o, typeof(T));
        }

        /// <summary>
        /// Converts string representation of enum to given type of enum
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string s, bool ignoreCase = true)
        {
            object item = s.ToEnum(typeof(T), ignoreCase); //Boxing because of compiler error with converting dynamic

            return (T)item;
        }

        /// <summary>
        /// Converts string representation of enum to given type of enum
        /// </summary>
        /// <param name="s"></param>
        /// <param name="type"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static dynamic ToEnum(this string s, Type type, bool ignoreCase = true)
        {
            Array modeTypes = type.GetTypeInfo().GetEnumValues();

            foreach(object mode in modeTypes)
            {
                if(ignoreCase && s.Equals(mode.ToString(), StringComparison.OrdinalIgnoreCase)
                    || !ignoreCase && s.Equals(mode.ToString()))
                {
                    return mode;
                }
            }

            throw new InvalidOperationException(
                "conversion failed because the input did" +
                " not match any enum value of the given type");
        }

        /// <summary>
        /// Converts an object to standard Date Time format MM/dd/yyyy h:mm:ss tt
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string ToDateString(this object o)
        {
            string format = "MM/dd/yyyy h:mm:ss tt";
            if(o is DateTime) return ((DateTime)o).ToString(format);
            if(o is string && o.IsValid()) return Convert.ToDateTime(o).ToString(format);

            return string.Empty;
        }

        /// <summary>
        /// Converts an object into a dictioanry of key value pair.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>

        public static IDictionary<string, dynamic> ToDictionary<T>(this T model)
        {
            Guard.AssertArgs(model.IsValid(), $"{typeof(T).Name} not valid");

            IDictionary<string, dynamic> dict = new Dictionary<string, dynamic>();

            foreach(var p in typeof(T).GetProperties())
            {
                if(!p.CanRead) continue;

                dynamic val = p.GetValue(model);

                dict[p.Name] = val;
            }

            return dict;
        }

        /// <summary>
        /// Returns a deep copy of an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T DeepClone<T>(this T source) where T : class
        {
            if(source == null) return null;

            MethodInfo method = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
            T clone = (T)method.Invoke(source, null);

            foreach(FieldInfo field in source.GetType().GetRuntimeFields())
            {
                if(field.IsStatic) continue;
                if(field.FieldType.GetTypeInfo().IsPrimitive) continue;

                object sourceValue = field.GetValue(source);
                field.SetValue(clone, DeepClone(sourceValue));
            }

            return clone;
        }
    }
}