using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace A7TSGenerator.Common
{
    public static class ReflectionUtility
    {
        //Reflection - Parameters and Result Types
        public static string[] GetNativeTypes()
        {
            return new string[] { "Object", "String", "Boolean", "Byte", "SByte", "Int16", "UInt16", "Int32", "UInt32", "Int64", "UInt64", "Single", "Double", "Decimal", "Char", "DateTime", "Void" };
        }

        public static bool IsNativeType(Type type)
        {
            return GetNativeTypes().Any(x => x == type.Name.Replace("System.", "").Replace("[]",""));
        }

        public static string GetTypeAlias(string type)
        {
            var dict = new Dictionary<string, string>();
            var typeName = type.Replace("System.", "");

            dict.Add("Object", "object");
            dict.Add("String", "string");
            dict.Add("Boolean", "bool");
            dict.Add("Byte", "byte");
            dict.Add("SByte", "sbyte");
            dict.Add("Int16", "short");
            dict.Add("UInt16", "ushort");
            dict.Add("Int32", "int");
            dict.Add("UInt32", "uint");
            dict.Add("Int64", "long");
            dict.Add("UInt64", "ulong");
            dict.Add("Single", "float");
            dict.Add("Double", "double");
            dict.Add("Decimal", "decimal");
            dict.Add("Char", "char");
            dict.Add("Void", "void");

            if (dict.ContainsKey(typeName))
            {
                return dict[type];
            }
            else
            {
                return typeName;
            }

        }

        public static string GetTypeAsText(Type type)
        {
            string typeAsText = GetTypeAlias(type.Name);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                typeAsText = GetTypeAlias(type.GetGenericArguments()[0].Name) + "?";
            }
            else if (type.IsGenericType)
            {
                typeAsText = type.GetGenericTypeDefinition().Name.Substring(0, type.Name.IndexOf('`'));
                var lstGenArgs = type.GetGenericArguments().ToList().Select(arg => GetTypeAsText(arg));
                typeAsText += "<" + String.Join(", ", lstGenArgs.ToArray()) + ">";
            }
            return typeAsText;
        }

        public static string GetParameterDefaultValue(ParameterInfo param)
        {
            var defaultValue = param.DefaultValue;
            if (System.DBNull.Value != defaultValue && defaultValue != null)
            {
                if (Equals(typeof(string), defaultValue.GetType()))
                {
                    return "\"" + defaultValue + "\"";
                }
                else
                {
                    var value = defaultValue.ToString();
                    return value == "False" ? "false" : value;
                }
            }
            return "null";
        }

        public static Type GetGenericType(Type type)
        {
            if (type == null) return typeof(void);

            if (!type.IsGenericType) return type;

            return type.GetGenericArguments().First();
        }

        //Reflection - Methods
        public static string GetMethodParameterSignature(MethodInfo method)
        {
            var lstParams = method.GetParameters().ToList()
                            .Select(param =>
                            {
                                var paramType = GetTypeAsText(param.ParameterType);
                                var paramName = param.Name;

                                if (System.DBNull.Value != param.DefaultValue)
                                {
                                    paramName += " = " + GetParameterDefaultValue(param);
                                }

                                return paramType + " " + paramName;
                            });

            return String.Join(",", lstParams.ToArray());
        }

        public static string GetMethodSignature(MethodInfo method)
        {
            return GetTypeAsText(method.ReturnType) + " " + method.Name + "(" + GetMethodParameterSignature(method) + ")";
        }

        public static string GetMethodParameterSignatureWithoutType(MethodInfo method)
        {
            var lstParams = method.GetParameters().ToList()
                                .Select(param => param.Name);

            return String.Join(",", lstParams.ToArray());
        }
    }
}
