using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
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
        private ApiExplorer _explorer;
        private IDictionary<string, Service> _dicServices;
        private IDictionary<string, Type> _dicModels;
        private IApiDescriptorParser _parser;

        private const string HEADER_DELIMITER = "--++--<br />";
        private const string FILE_DELIMITER = "<--- FILE DELIMITER ---><br />";

        public void ProcessRequest(HttpContext context)
        {
            validateOptions(Options);
            
            var modelTypesProcessed = new List<string>();
            var controllerFilter = context.Request["controllerFilter"] ?? "";          

            _explorer = new ApiExplorer(Options.HttpConfiguration);
            _dicServices = new Dictionary<string, Service>();
            _dicModels = new Dictionary<string, Type>();

            _explorer.ApiDescriptions.ToList().ForEach(x =>
            {                
                
                var apiDescriptor = x.ActionDescriptor;
                var controllerName = apiDescriptor.ControllerDescriptor.ControllerName;

                if (controllerFilter == "" || controllerName.ToLower().StartsWith(controllerFilter))
                {
                    _parser = getParser(x, Options.BaseApiUrl + controllerName.ToLower());
                    Service service = getService(apiDescriptor);

                    _dicServices[controllerName] = service;
                }

            });

            generateResponse(context);

        }

        public static TsGeneratorOptions Options { get; set; }

        private void generateModels(HttpContext context)
        {
            foreach (var kvp in _dicModels)
            {
                var skipNestedModels = Options.ModelsToSkipNestedModels.Any(x => x.ToLower() == kvp.Key.ToLower());
                processModel(kvp.Value, processChildModels, skipNestedModels);
            };

            if (_models.Count() > 0)
            {
                context.Response.Write(FILE_DELIMITER);
                context.Response.Write(string.Join(FILE_DELIMITER, _models.ToArray()));
            }
        }

        private void generateResponse(HttpContext context)
        {
            if (_dicServices.Count() == 0)
            {
                context.Response.Write("{No Services Found}");
                return;
            }

            generateServices(context);
            generateModels(context);

        }

        private void generateServices(HttpContext context)
        {
            var services = new List<string>();

            foreach (var kvp in _dicServices)
            {
                var template = new TypeScript9ServiceTemplate() { Service = kvp.Value };
                services.Add("Service" + HEADER_DELIMITER + kvp.Value.Name + "Service" + HEADER_DELIMITER + template.TransformText());
            }

            context.Response.Write(string.Join(FILE_DELIMITER, services.ToArray()));
        }

        private IApiDescriptorParser getParser(ApiDescription apiDescription, string serviceUrl)
        {
            switch(Options.TypeScriptVersion){
                 default: return new TypeScript9Parser(apiDescription, serviceUrl);
            }
        }

        private Service getService(HttpActionDescriptor apiDescriptor)
        {
            var controllerName = apiDescriptor.ControllerDescriptor.ControllerName;
            var service = _dicServices.ContainsKey(controllerName) ?
                    _dicServices[controllerName] :
                    new Service() { Name = controllerName, Url = Options.BaseApiUrl + controllerName.ToLower() };

            initServiceMethods(service);
            initServiceTypes(service, apiDescriptor);

            return service;
        }

        private void initServiceMethods(Service service)
        {
            //Check for duplicate service methods, always replace with the last one to ensure the default route is used
            ServiceMethod newServiceMethod = _parser.GetServiceMethod();
            var oldServiceMethod = service.ServiceMethods.FirstOrDefault(s => s.Name == newServiceMethod.Name);
            if (oldServiceMethod == null)
            {
                service.ServiceMethods.Add(newServiceMethod);
            }
            else
            {
                service.ServiceMethods.Remove(oldServiceMethod);
                service.ServiceMethods.Add(newServiceMethod);
            }
        }

        private void initServiceTypes(Service service, HttpActionDescriptor apiDescriptor)
        {
            var parameters = apiDescriptor.GetParameters()
                    .Where(p => !ReflectionUtility.IsNativeType(p.ParameterType))
                    .Select(p => p.ParameterType).ToList();

            parameters.ForEach(p =>
            {
                var type = ReflectionUtility.GetGenericType(p);
                var typeAsText = ReflectionUtility.GetTypeAsText(type);
                service.Models[typeAsText] = type;
                _dicModels[typeAsText] = type;
            });

            var returnType = ReflectionUtility.GetGenericType(apiDescriptor.ReturnType);
            var returnTypeAsText = ReflectionUtility.GetTypeAsText(returnType);
            if (!ReflectionUtility.IsNativeType(returnType))
                service.Models[returnTypeAsText] = returnType;
            _dicModels[returnTypeAsText] = returnType;
        }

        private void processModel(Type modelType, Action<TypeScript9ModelTemplate, int> onProcessedModelProcessChildren, bool useDynamicNestedModels = false)
        { 
            var type = ReflectionUtility.GetGenericType(modelType);
            var typeAsText = ReflectionUtility.GetTypeAsText(type);

            //Checks to prevent native javascript .ts files from being created
            if (!ReflectionUtility.IsNativeType(type) && !_lstProcessedModelTypes.Contains(typeAsText))
            {
                var template = new TypeScript9ModelTemplate(type, useDynamicNestedModels);
                _models.Add("Model" + HEADER_DELIMITER + typeAsText + HEADER_DELIMITER + template.TransformText());
                _lstProcessedModelTypes.Add(typeAsText);

                if(!useDynamicNestedModels) onProcessedModelProcessChildren(template, 2);
            }
        }

        private void processChildModels(TypeScript9ModelTemplate template, int currentDepth)
        {
            if (currentDepth > Options.NestedModelsDepthLimit) return;

            template.GetNonNativePropertyTypes().ToList().ForEach(modelType =>
            {
                var skipNestedModels = currentDepth == Options.NestedModelsDepthLimit || Options.ModelsToSkipNestedModels.Any(x => x.ToLower() == modelType.Name.ToLower());
                processModel(modelType, (tmpl, depth) => processChildModels(tmpl, depth + 1), skipNestedModels);
            });
        }

        private void validateOptions(TsGeneratorOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("TsGenerator.Options is null and is required.  Set this static property on application start");
            }

            if (options.HttpConfiguration == null)
            {
                throw new ArgumentNullException("The WebApi HttpConfiguration is a required option and is null. Set this on application start via TsGenerator.Options.HttpConfiguration");
            }

            if (!options.BaseApiUrl.EndsWith("/")) options.BaseApiUrl += "/";

            options.ModelsToSkipNestedModels = options.ModelsToSkipNestedModels ?? new string[] { };

        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
