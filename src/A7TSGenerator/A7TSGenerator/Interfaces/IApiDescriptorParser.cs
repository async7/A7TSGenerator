using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using A7TSGenerator.Models;

namespace A7TSGenerator.Interfaces
{
    public interface IApiDescriptorParser
    {
        ServiceMethod GetServiceMethod();
    }
}
