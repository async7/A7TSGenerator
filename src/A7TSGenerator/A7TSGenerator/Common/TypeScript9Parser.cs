using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;
using A7TSGenerator.Interfaces;
using A7TSGenerator.Models;
using A7TSGenerator.Extensions;

namespace A7TSGenerator.Common
{
    public class TypeScript9Parser : IApiDescriptorParser
    { private readonly ApiDescription _apiDescription;
        private IEnumerable<ActionParameter> _parameters;
        private const string MODELS_NAMESPACE = "Models";
        private readonly string _serviceUrl;

        public TypeScript9Parser(ApiDescription apiDescription, string serviceUrl)
        {
            _apiDescription = apiDescription;
            _serviceUrl = serviceUrl;
        }

        public IEnumerable<ActionParameter> Parameters
        {
            get
            {
                if (_parameters == null)
                {
                    _parameters = _apiDescription.Parameters();
                }
                return _parameters;
            }
        }

        public ServiceMethod GetServiceMethod()
        {

            switch (_apiDescription.HttpMethod.ToString().ToLower())
            {
                case "get":
                    return GetServiceMethodForGETAction();
                case "post":
                    return GetServiceMethodForPOSTAction();
                case "put":
                    return GetServiceMethodForPUTAction();
                case "delete":
                    return GetServiceMethodForDELETEAction();
                default:
                    return getGenericServiceMethod();
            }

        }

        public ServiceMethod GetServiceMethodForGETAction()
        {
            ServiceMethod serviceMethod;
            var action = _apiDescription.ActionDescriptor;

            if (action.ActionName.ToLower() == "get" && Parameters.FirstOrDefault(x => x.Name.ToLower() == "id") != null && Parameters.Count() == 1)
            {
                serviceMethod = new ServiceMethod();
                serviceMethod.Name = "GetById";
                serviceMethod.Arguments = "id: number";
                serviceMethod.ArgumentsWithoutDefaultParams = serviceMethod.Arguments;
                serviceMethod.ReturnType = action.ReturnType == null ? "void" : "JQueryPromise<" + TypeScript9Utility.GetTSType(action.ReturnType) + '>';
                serviceMethod.Body = "return super.GetById(id);";
            }
            else
            {
                serviceMethod = getGenericServiceMethod();                
            }
                          
            return serviceMethod;
        }

        public ServiceMethod GetServiceMethodForPOSTAction(){
            ServiceMethod serviceMethod;
            var action = _apiDescription.ActionDescriptor;

            if (action.ActionName.ToLower() == "post" && Parameters.FirstOrDefault(x => !ReflectionUtility.IsNativeType(x.Type)) != null && Parameters.Count() == 1)
            {
                var parameter = Parameters.First();

                serviceMethod = new ServiceMethod();
                serviceMethod.Name = "Insert";
                serviceMethod.Arguments = parameter.Name + ": " + MODELS_NAMESPACE + "." + parameter.Type.Name;
                serviceMethod.ArgumentsWithoutDefaultParams = serviceMethod.Arguments;
                serviceMethod.ReturnType = action.ReturnType == null ? "JQueryPromise" : "JQueryPromise<" + TypeScript9Utility.GetTSType(action.ReturnType) + '>';
                serviceMethod.Body = "return super.Insert(" + parameter.Name + ");";
            }
            else
            {
                serviceMethod = getGenericServiceMethod();
            }

            return serviceMethod;
            
        }

        public ServiceMethod GetServiceMethodForPUTAction()
        {
            ServiceMethod serviceMethod;
            var action = _apiDescription.ActionDescriptor;

            if (action.ActionName.ToLower() == "put" && Parameters.FirstOrDefault(x => !ReflectionUtility.IsNativeType(x.Type)) != null && Parameters.Count() == 1)
            {
                var parameter = Parameters.First();

                serviceMethod = new ServiceMethod();
                serviceMethod.Name = "Update";
                serviceMethod.Arguments = parameter.Name + ": " + MODELS_NAMESPACE + "." + parameter.Type.Name;
                serviceMethod.ArgumentsWithoutDefaultParams = serviceMethod.Arguments;
                serviceMethod.ReturnType = action.ReturnType == null ? "JQueryPromise" : "JQueryPromise<" + TypeScript9Utility.GetTSType(action.ReturnType) + '>';
                serviceMethod.Body = "return super.Update(" + parameter.Name + ");";
            }
            else
            {
                serviceMethod = getGenericServiceMethod();
            }

            return serviceMethod;

        }

        public ServiceMethod GetServiceMethodForDELETEAction()
        {
            ServiceMethod serviceMethod;
            var action = _apiDescription.ActionDescriptor;

            if (action.ActionName.ToLower() == "delete" && Parameters.FirstOrDefault(x => x.Name.ToLower() == "id") != null && Parameters.Count() == 1)
            {
                serviceMethod = new ServiceMethod();
                serviceMethod.Name = "DeleteById";
                serviceMethod.Arguments = "id: number";
                serviceMethod.ArgumentsWithoutDefaultParams = serviceMethod.Arguments;
                serviceMethod.ReturnType = action.ReturnType == null ? "JQueryPromise" : "JQueryPromise<" + TypeScript9Utility.GetTSType(action.ReturnType) + '>';
                serviceMethod.Body = "return super.DeleteById(id);";
            }
            else
            {
                serviceMethod = getGenericServiceMethod();
            }

            return serviceMethod;
        }

        private ServiceMethod getGenericServiceMethod()
        {
            var serviceMethod = new ServiceMethod();
            var body = "var url = ";

            if(_apiDescription.RelativePath.ToLower().Contains(_serviceUrl.ToLower())) {
                body += _apiDescription.RelativePath.ToLower().Replace(_serviceUrl.ToLower(), "this._url + '");
            } else {
                body += "'" + _apiDescription.RelativePath.ToLower();
            }

            body += "';" + Environment.NewLine;

            serviceMethod.Name = _apiDescription.ActionDescriptor.ActionName;
            serviceMethod.Arguments = TypeScript9Utility.GetMethodParametersAsTypeScriptArgs(_apiDescription);
            serviceMethod.ArgumentsWithoutDefaultParams = TypeScript9Utility.GetMethodParametersAsTypeScriptArgs(_apiDescription, false);
            serviceMethod.ReturnType = _apiDescription.ActionDescriptor.ReturnType == null ? "JQueryPromise" : "JQueryPromise<" + TypeScript9Utility.GetTSType(_apiDescription.ActionDescriptor.ReturnType) + '>';

            body += String.Join(Environment.NewLine, _apiDescription.ActionDescriptor.GetParameters()
                .Select(x => _apiDescription.RelativePath.Contains("{" + x.ParameterName + "}") ? x.ParameterName : null)
                .Where(x => x != null)
                .Select(x => "            url = url.replace(/{" + x + "}/gi, " + x + ".toString());")
                .ToArray());

            body += Environment.NewLine + "            return ";

            //if (_apiDescription.HttpMethod.ToString().ToLower() == "get")
            //{
            //    body += "A7.AJAX." + _apiDescription.HttpMethod.ToString().CaptilizeFirstLetter() + "(url, { });";
            //}
            //else
            //{
                body += "A7.AJAX." + _apiDescription.HttpMethod.ToString().CaptilizeFirstLetter() + (serviceMethod.ReturnType.IndexOf("Collection") == -1 ? "" : "Collection") + "(url, " + TypeScript9Utility.GetMethodParametersAsTypescriptObject(_apiDescription) + ");";
            //}

            serviceMethod.Body = body;

            return serviceMethod;
        }

    }
}