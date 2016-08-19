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

        private List<PropertyInfo> listProperties(Type t)
        {
            return t
                .GetProperties(
                               // 11/10/2015 remove in order to handle & flatten class hierarchies (in future can mirror hierarchy in TS)
                               //BindingFlags.DeclaredOnly |
                               BindingFlags.Public |
                               BindingFlags.Instance)
                .ToList();
        }

        public ICollection<Type> GetNonNativePropertyTypes(){
            var types = new HashSet<Type>();

            listProperties(ModelType)
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
            var lstReferences = new SortedSet<string>();

            listProperties(ModelType)
                    .ForEach(prop =>
                    {
                        var propType = prop.PropertyType;

                        if (propType.IsGenericType)
                        {
                            propType = propType.GetGenericArguments()[0];
                        }

                        var isSelfReference = ModelType.Name == propType.Name;

                        if (!ReflectionUtility.IsNativeType(propType) && !_useDynamicNestedModels && !isSelfReference)
                        {
                            lstReferences.Add("/// <reference path=\"" + propType.Name + ".ts\" />");
                        }

                    });

            return String.Join(Environment.NewLine, lstReferences.ToArray());
        }

        private string getTypeScriptProperties()
        {
            var lstProps = new List<string>();

            listProperties(ModelType)
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

                        lstProps.Add(prop.Name + ": " + typeAsText + " = null;");
                    });

            return String.Join(Environment.NewLine + "    	", lstProps.ToArray());
        }
        
    }
}
