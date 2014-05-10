using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;
using A7TSGenerator.Common;
using A7TSGenerator.Extensions;
using A7TSGenerator.Interfaces;
using A7TSGenerator.Models;

namespace A7TSGenerator.Templates
{
    public partial class TypeScript9ServiceTemplate
    {
        public Service Service { get; set; }

        public string ModelReferences
        {
            get
            {
                var references = new List<string>();
                foreach (var kvp in Service.Models)
                {
                    if(!ReflectionUtility.IsNativeType(kvp.Value))
                        references.Add("/// <reference path=\"../models/" + kvp.Key + ".ts\" />");
                }
                return string.Join(Environment.NewLine, references.ToArray());
            }
        }
    }
}
