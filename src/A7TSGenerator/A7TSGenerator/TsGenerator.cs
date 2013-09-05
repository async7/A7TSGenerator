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
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var explorer = new ApiExplorer(Global.HttpConfiguration);
            var dicServices = new Dictionary<string, Service>();
            var dicModels = new Dictionary<string, Type>();

            explorer.ApiDescriptions.ToList().ForEach(x =>
            {
                var controllerDescriptor = x.ActionDescriptor.ControllerDescriptor;
                IApiDescriptorParser parser;

                Service service = dicServices.ContainsKey(controllerDescriptor.ControllerName) ?
                    dicServices[controllerDescriptor.ControllerName] :
                    new Service() { Name = controllerDescriptor.ControllerName, Url = "api/" + controllerDescriptor.ControllerName.ToLower() };

                parser = new TypeScript9Parser(x, service.Url);
                service.ServiceMethods.Add(parser.GetServiceMethod());

                var parameters = x.ActionDescriptor.GetParameters()
                    .Where(p => !ReflectionUtility.IsNativeType(p.ParameterType))
                    .Select(p => p.ParameterType).ToList();

                parameters.ForEach(p =>
                {
                    var typeAsText = ReflectionUtility.GetTypeAsText(p);
                    service.Models[typeAsText] = p;
                    dicModels[typeAsText] = p;
                });

                var returnType = x.ActionDescriptor.ReturnType;
                dicModels[ReflectionUtility.GetTypeAsText(returnType)] = returnType;

                dicServices[controllerDescriptor.ControllerName] = service;
                               
            });

            if (dicServices.Count() == 0)
            {
                context.Response.Write("{No Services Found}");
                return;
            }

            var fileDelimiter = "<--- FILE DELIMITER ---><br />";
            var headerDelimiter = "--++--<br />";
            var services = new List<string>();
            var models = new List<string>();
            var modelTypesProcessed = new List<string>();

            foreach (var kvp in dicServices)
            {
                var template = new TypeScript9ServiceTemplate() { Service = kvp.Value };
                services.Add("Service" + headerDelimiter + kvp.Value.Name + "Service" + headerDelimiter + template.TransformText());
            }

           foreach (var kvp in dicModels)
            {
                var type = ReflectionUtility.GetGenericType(kvp.Value);
                var typeAsText = ReflectionUtility.GetTypeAsText(type);

                if (!ReflectionUtility.IsNativeType(type) && !modelTypesProcessed.Contains(typeAsText))
                {
                    var template = new TypeScript9ModelTemplate(type);
                    models.Add("Model" + headerDelimiter + typeAsText + headerDelimiter + template.TransformText());
                    modelTypesProcessed.Add(typeAsText);
                }
            };

            context.Response.Write(string.Join(fileDelimiter, services.ToArray()));

            if (models.Count() > 0)
            {
                context.Response.Write(fileDelimiter);
                context.Response.Write(string.Join(fileDelimiter, models.ToArray()));
            }

        }
    }
}
