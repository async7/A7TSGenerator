using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace A7TSGenerator
{
    public class TsGeneratorOptions        
    {
        public TsGeneratorOptions(HttpConfiguration configuration)
        {
            HttpConfiguration = configuration;
            BaseApiUrl = "/api/";
            NestedModelsDepthLimit = 3;
            ModelsToSkipNestedModels = new string[] { };
            ModelsToSkip = new string[] { };
            TypeScriptVersion = TYPE_SCRIPT_VERSION.v1;
        }

        public string BaseApiUrl { get; set; }
        public HttpConfiguration HttpConfiguration { get; set; }
        public int NestedModelsDepthLimit { get; set; }
        public string[] ModelsToSkipNestedModels { get; set; }
        public string[] ModelsToSkip { get; set; }
        public TYPE_SCRIPT_VERSION TypeScriptVersion { get; set; }
    }

    public enum TYPE_SCRIPT_VERSION
    {
        v1
    }
}
