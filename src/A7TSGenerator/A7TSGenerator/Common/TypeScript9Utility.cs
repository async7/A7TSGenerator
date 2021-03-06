﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace A7TSGenerator.Common
{
    public static class TypeScript9Utility
    {

        private static string getTsDefaultValue(object defaultValue)
        {
            if (System.DBNull.Value != defaultValue && defaultValue != null)
            {
                if (Equals(typeof(string), defaultValue.GetType()))
                {
                    return "\"" + defaultValue + "\"";
                }
                else
                {
                    var value = defaultValue.ToString();
                    switch(value){
                        case "False": 
                            value = "false";
                            break;
                        case "True":
                            value = "true";
                            break;
                    }
                    return value;
                }
            }
            return "null";
        }

        public static string GetTSType(string typeName)
        {                      
            var tsType = "any";

            switch (ReflectionUtility.GetTypeAlias(typeName).ToLower().Replace("?", ""))
            {
                case "void":
                    tsType = "void";
                    break;
                case "object":
                case "date":
                case "byte":
                case "sbtye":
                case "datetime":
                    tsType = "any";
                    break;
                case "string":
                case "char":
                    tsType = "string";
                    break;
                case "bool":
                    tsType = "boolean";
                    break;
                case "short":
                case "ushort":
                case "int":
                case "uint":
                case "long":
                case "ulong":
                case "float":
                case "double":
                case "decimal":
                    tsType = "number";
                    break;
                case "icollection`1":
                    tsType = "any[]";
                    break;
                default:
                    tsType = "Models." + typeName;
                    break;
            }

            return tsType;
        }

        public static string GetTSType(Type type)
        {
            if (type.IsGenericType && type.GenericTypeArguments.Count() == 1)
            {
                if (type.Name.StartsWith("Nullable"))
                    return GetTSType(type.GenericTypeArguments.First());
                else
                    return "A7.ICollection<" + GetTSType(type.GenericTypeArguments.First()) + ">";
            }
            else if (type.IsGenericType)
            {
                return "any";
            }

            var typeName = ReflectionUtility.GetTypeAsText(type);
            var tsType = "any";

            switch (ReflectionUtility.GetTypeAlias(typeName).ToLower().Replace("?", ""))
            {
                case "void":
                    tsType = "void";
                    break;
                case "object":
                case "date":
                case "byte":
                case "sbtye":
                case "datetime":
                    tsType = "any";
                    break;
                case "string":
                case "char":
                    tsType = "string";
                    break;
                case "bool":
                    tsType = "boolean";
                    break;
                case "short":
                case "ushort":
                case "int":
                case "uint":
                case "long":
                case "ulong":
                case "float":
                case "double":
                case "decimal":
                    tsType = "number";
                    break;
                case "icollection`1":
                    tsType = "any[]";
                    break;
                default:
                    tsType = "Models." + typeName;
                    break;
            }

            return tsType;
        }

        public static string GetMethodParametersAsTypeScriptArgs(ApiDescription apiDescription)
        {
            return GetMethodParametersAsTypeScriptArgs(apiDescription, true);
        }

        public static string GetMethodParametersAsTypeScriptArgs(ApiDescription apiDescription, bool includeDefaultValues)
        {
            var lstParams = apiDescription.ActionDescriptor.GetParameters().ToList()
                                .Select(param =>
                                {

                                    var paramType = GetTSType(param.ParameterType);
                                    var paramName = param.ParameterName + (param.IsOptional && !includeDefaultValues ? "?" : "");

                                    if (param.IsOptional & includeDefaultValues)
                                    {
                                        if (param.ParameterType.IsEnum)
                                        {
                                            paramType += " = Models." + param.ParameterType.Name + '.' + getTsDefaultValue(param.DefaultValue);
                                        }
                                        else
                                        {
                                            paramType += " = " + getTsDefaultValue(param.DefaultValue);
                                        }
                                    }
                                    
                                    return paramName + ": " + paramType;
                                });

            return String.Join(",", lstParams.ToArray());
        }

        public static string GetMethodParametersAsTypescriptObject(ApiDescription apiDescription)
        {
            var parameters = apiDescription.ActionDescriptor.GetParameters();

            if (parameters.Count() == 1 && !ReflectionUtility.IsNativeType(parameters.First().ParameterType))
            {
               return parameters.First().ParameterName;
            }

            var lstParams = parameters.ToList()
                                .Select(param =>
                                {
                                    var paramName = param.ParameterName;
                                    return paramName + ": " + paramName;
                                });

            return "{ " + String.Join(",", lstParams.ToArray()) + " }";
        }

        public static string GetActionMethodName(string actionName, string httpVerb)
        {
            switch (httpVerb)
            {
                case "Get":
                    break;
                case "Post":
                    return actionName.StartsWith("Post") ? "Add" + actionName.Substring(4) : actionName;
                case "Put":
                    //return actionName.StartsWith("Put") ? actionName.Substring(3) : actionName;
                    break;
                case "Delete":
                    //return actionName.StartsWith("Delete") ? actionName.Substring(6) : actionName;
                    break;
            }
            return actionName;
        }

    }
}
