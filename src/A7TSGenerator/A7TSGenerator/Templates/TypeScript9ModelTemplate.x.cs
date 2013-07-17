using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using A7TSGenerator.Common;

namespace A7TSGenerator.Templates
{
    public partial class TypeScript9ModelTemplate
    {
        public TypeScript9ModelTemplate(Type modelType)
        {
            this.ModelType = modelType;
        }

        public Type ModelType { get; set; }
        public string Properties { get { return getTypeScriptProperties(); } }
        public string ModelReferences { get { return getModelReferences(); } }


        private string getModelReferences()
        {
            var lstReferences = new List<string>();

            ModelType
                    .GetProperties(BindingFlags.DeclaredOnly |
                                   BindingFlags.Public |
                                   BindingFlags.Instance)
                    .ToList()
                    .ForEach(prop =>
                    {
                        var propType = prop.PropertyType;

                        if (propType.IsGenericType)
                        {
                            propType = propType.GetGenericArguments()[0];
                        }

                        if (!ReflectionUtility.IsNativeType(propType))
                        {
                            lstReferences.Add("/// <reference path=\"" + propType.Name + ".ts\" />");
                        }

                    });

            return String.Join(Environment.NewLine, lstReferences.ToArray());
        }

        private string getTypeScriptProperties()
        {
            var lstProps = new List<string>();

            ModelType
                    .GetProperties(BindingFlags.DeclaredOnly |
                                   BindingFlags.Public |
                                   BindingFlags.Instance)
                    .ToList()
                    .ForEach(prop =>
                    {
                        var propType = prop.PropertyType;

                        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            propType = propType.GetGenericArguments()[0];
                        }

                        lstProps.Add(prop.Name + ": " + TypeScript9Utility.GetTSType(propType) + ";");
                    });

            return String.Join(Environment.NewLine + "    	", lstProps.ToArray());
        }
    }
}
