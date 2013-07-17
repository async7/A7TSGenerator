using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A7TSGenerator.Models
{
    public class ActionParameter
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsOptional { get; set; }
        public object DefaultValue { get; set; }
    }
}
