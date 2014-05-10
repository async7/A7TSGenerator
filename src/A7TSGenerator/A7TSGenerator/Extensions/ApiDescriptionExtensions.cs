using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;
using A7TSGenerator.Models;

namespace A7TSGenerator.Extensions
{
    public static class ApiDescriptionExtensions
    {
        public static IEnumerable<ActionParameter> Parameters(this ApiDescription description)
        {
            return description.ParameterDescriptions.Where(x => x.ParameterDescriptor != null).Select(x => new ActionParameter()
            {
                Name = x.ParameterDescriptor.ParameterName,
                Type = x.ParameterDescriptor.ParameterType,
                IsOptional = x.ParameterDescriptor.IsOptional,
                DefaultValue = x.ParameterDescriptor.DefaultValue
            });
        }
    }
}
