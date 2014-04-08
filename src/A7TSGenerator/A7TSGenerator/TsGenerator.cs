using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Routing;
using A7TSGenerator.Common;
using A7TSGenerator.Interfaces;
using A7TSGenerator.Models;
using A7TSGenerator.Templates;

namespace A7TSGenerator
{
    public class TsGenerator : IHttpHandler
    {

        private IList<string> _lstProcessedModelTypes = new List<string>();
        private IList<string> _models = new List<string>();

        private const string HEADER_DELIMITER = "--++--<br />";
        private const int NESTED_MODEL_DEPTH = 3;        
        

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string baseUrl = "api/";
            if (!String.IsNullOrWhiteSpace(context.Request.QueryString["baseurl"]))
            { 
                baseUrl = context.Request.QueryString["baseurl"];
                if (!baseUrl.EndsWith("/"))
                    baseUrl = baseUrl + "/";
            }

            var explorer = new ApiExplorer(Global.HttpConfiguration);
            var dicServices = new Dictionary<string, Service>();
            var dicModels = new Dictionary<string, Type>();

            explorer.ApiDescriptions.ToList().ForEach(x =>
            {
                var controllerDescriptor = x.ActionDescriptor.ControllerDescriptor;
                IApiDescriptorParser parser;

                Service service = dicServices.ContainsKey(controllerDescriptor.ControllerName) ?
                    dicServices[controllerDescriptor.ControllerName] :
                    new Service() { Name = controllerDescriptor.ControllerName, Url = baseUrl + controllerDescriptor.ControllerName.ToLower() };

                parser = new TypeScript9Parser(x, service.Url);

                //Check for duplicate service methods
                ServiceMethod serviceMethod = parser.GetServiceMethod();
                if(!service.ServiceMethods.Any(s => s.Name == serviceMethod.Name))
                    service.ServiceMethods.Add(serviceMethod);

                var parameters = x.ActionDescriptor.GetParameters()
                    .Where(p => !ReflectionUtility.IsNativeType(p.ParameterType))
                    .Select(p => p.ParameterType).ToList();

                parameters.ForEach(p =>
                {
                    var type = ReflectionUtility.GetGenericType(p);
                    var typeAsText = ReflectionUtility.GetTypeAsText(type);
                    service.Models[typeAsText] = type;
                    dicModels[typeAsText] = type;
                });

                var returnType = ReflectionUtility.GetGenericType(x.ActionDescriptor.ReturnType);
                var returnTypeAsText = ReflectionUtility.GetTypeAsText(returnType);
                if(!ReflectionUtility.IsNativeType(returnType))
                    service.Models[returnTypeAsText] = returnType;
                dicModels[returnTypeAsText] = returnType;

                dicServices[controllerDescriptor.ControllerName] = service;
                               
            });

            if (dicServices.Count() == 0)
            {
                context.Response.Write("{No Services Found}");
                return;
            }

            var fileDelimiter = "<--- FILE DELIMITER ---><br />";
            
            var services = new List<string>();
            
            var modelTypesProcessed = new List<string>();

            foreach (var kvp in dicServices)
            {
                var template = new TypeScript9ServiceTemplate() { Service = kvp.Value };
                services.Add("Service" + HEADER_DELIMITER + kvp.Value.Name + "Service" + HEADER_DELIMITER + template.TransformText());
            }

           foreach (var kvp in dicModels)
            {
                processModel(kvp.Value, processChildModels);
            };

            context.Response.Write(string.Join(fileDelimiter, services.ToArray()));

            if (_models.Count() > 0)
            {
                context.Response.Write(fileDelimiter);
                context.Response.Write(string.Join(fileDelimiter, _models.ToArray()));
            }

        }

        private void processModel(Type modelType, Action<TypeScript9ModelTemplate, int> onProcessedModel, bool useDynamicNestedModels = false)
        {
            var type = ReflectionUtility.GetGenericType(modelType);
            var typeAsText = ReflectionUtility.GetTypeAsText(type);

            //Checks to prevent native javascript .ts files from being created
            if (!ReflectionUtility.IsNativeType(type) && !_lstProcessedModelTypes.Contains(typeAsText))
            {
                var template = new TypeScript9ModelTemplate(type, useDynamicNestedModels);
                _models.Add("Model" + HEADER_DELIMITER + typeAsText + HEADER_DELIMITER + template.TransformText());
                _lstProcessedModelTypes.Add(typeAsText);
                onProcessedModel(template, 2);
            }
        }

        private void processChildModels(TypeScript9ModelTemplate template, int currentDepth)
        {
            if (currentDepth > NESTED_MODEL_DEPTH) return;

            template.GetNonNativePropertyTypes().ToList().ForEach(modelType =>
            {
                processModel(modelType, (tmpl, depth) => processChildModels(template, depth + 1), currentDepth == NESTED_MODEL_DEPTH);
            });
        }
    }
}
