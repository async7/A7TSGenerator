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

        private readonly bool  _useDynamicNestedModels;

        public TypeScript9ModelTemplate(Type modelType, bool useDynamicNestedModels = false)
        {
            this.ModelType = modelType;
            _useDynamicNestedModels = useDynamicNestedModels;
        }

        public Type ModelType { get; set; }
        public string Properties { get { return getTypeScriptProperties(); } }
        public string ModelReferences { get { return getModelReferences(); } }


        public ICollection<Type> GetNonNativePropertyTypes(){
            var types = new HashSet<Type>();

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
                            types.Add(propType);
                        }

                    });

            return types;
        }

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

                        if (!ReflectionUtility.IsNativeType(propType) && !_useDynamicNestedModels)
                        {
                            if (propType.Name.ToLower() == "void")
                                Console.WriteLine("shouldn't hit this");
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

                        var typeAsText = TypeScript9Utility.GetTSType(propType);

                        if (typeAsText.StartsWith("A7.ICollection"))
                        {
                            typeAsText = typeAsText.Replace("A7.ICollection<", "").Replace(">", "") + "[]";
                        }

                        if (typeAsText.Contains("Models.") && _useDynamicNestedModels) typeAsText = "any";

                        lstProps.Add(prop.Name + ": " + typeAsText + ";");
                    });

            return String.Join(Environment.NewLine + "    	", lstProps.ToArray());
        }
        
    }
}
