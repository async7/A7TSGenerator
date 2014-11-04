using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace A7TSGenerator.Templates
{
    public partial class TypeScript9EnumModelTemplate
    {

        public TypeScript9EnumModelTemplate(Type modelType)
        {
            this.ModelType = modelType;
        }

        public Type ModelType { get; set; }
        public string Properties { get { return getTypeScriptProperties(); } }

        private string getTypeScriptProperties()
        {
            var lstFields = new List<string>();

            ModelType
                    .GetFields(BindingFlags.Public |
                                   BindingFlags.Static)
                    .ToList()
                    .ForEach(fld =>
                    {
                        lstFields.Add(fld.Name + " = " + ((int)fld.GetValue(this)).ToString());
                    });

            return String.Join("," + Environment.NewLine + "    	", lstFields.ToArray());
        }
    }
}
