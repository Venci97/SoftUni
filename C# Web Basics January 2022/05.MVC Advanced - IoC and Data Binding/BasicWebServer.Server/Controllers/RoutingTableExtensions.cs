using BasicWebServer.Server.Attributes;
using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Routing;
using System.Linq;
using System.Reflection;

namespace BasicWebServer.Server.Controllers
{
    public static class RoutingTableExtensions
    {
        public static IRoutingTable MapGet<TController>(
            this IRoutingTable routingTable,
            string path,
            Func<TController, Response> controllerFunction)

            where TController : Controller
            => routingTable.MapGet(path, request => controllerFunction(
                CreateController<TController>(request)));

        public static IRoutingTable MapPost<TController>(
            this IRoutingTable routingTable,
            string path,
            Func<TController, Response> controllerFunction)
            where TController : Controller
            => routingTable.MapPost(path, request => controllerFunction(
                CreateController<TController>(request)));

        public static IRoutingTable MapControllers(this IRoutingTable routingTable)
        {
            IEnumerable<MethodInfo> controllerActions = GetControllerAction();

            foreach (var controllerAction in controllerActions)
            {
                string controllerName = controllerAction
                    .DeclaringType
                    .Name
                    .Replace(nameof(Controller), string.Empty);
                string actionName = controllerAction.Name;
                string path = $"/{controllerName}/{actionName}";

                var responseFunction = GetResponseFunction(controllerAction);

                Method httpMethod = Method.Get;
                var actionMethodAttribute = controllerAction
                    .GetCustomAttribute<HttpMethodAttribute>();
                if (actionMethodAttribute != null)
                {
                    httpMethod = actionMethodAttribute.HttpMethod;
                }

                routingTable.Map(httpMethod, path, responseFunction);
            }

            return routingTable;
        }

        private static Func<Request, Response> GetResponseFunction(MethodInfo controllerAction)
        {
            return request =>
            {
                var controllerInstance = CreateController(controllerAction.DeclaringType, request);
                var parameterValues = GetParameterValues(controllerAction, request);

                return (Response)controllerAction.Invoke(controllerInstance, parameterValues);
            };
        }

        private static object[] GetParameterValues(MethodInfo controllerAction, Request request)
        {
            var actionParameters = controllerAction
                .GetParameters()
                .Select(p => new
                {
                    p.Name,
                    p.ParameterType
                })
                .ToArray();

            var parameterValues = new object[actionParameters.Count()];

            for (int i = 0; i < actionParameters.Length; i++)
            {
                var parameter = actionParameters[i];
                if (parameter.ParameterType.IsPrimitive || 
                    parameter.ParameterType == typeof(string))
                {
                    var parameterValue = request.GetValue(parameter.Name);
                    parameterValues[i] = Convert.ChangeType(parameterValue, parameter.ParameterType);
                }
                else
                {
                    var parameterValue = Activator.CreateInstance(parameter.ParameterType);
                    var parameterProporties = parameter.ParameterType.GetProperties();

                    foreach (var property in parameterProporties)
                    {
                        var propertyValue = request.GetValue(parameter.Name);
                        property.SetValue(
                            parameterValue,
                            Convert.ChangeType(propertyValue, property.PropertyType));
                    }

                    parameterValues[i] = parameterValue;
                }
            }

            return parameterValues;
        }

        private static IEnumerable<MethodInfo> GetControllerAction()
            => Assembly
            .GetEntryAssembly()
            .GetExportedTypes()
            .Where(t => t.IsAbstract == false)
            .Where(t => t.IsAssignableTo(typeof(Controller)))
            .Where(t => t.Name.EndsWith(nameof(Controller)))
            .SelectMany(t => t
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.ReturnType.IsAssignableTo(typeof(Response)))
            ).ToList();

        private static TController CreateController<TController>(Request request)
            => (TController)Activator
            .CreateInstance(typeof(TController), new[] { request });

        private static Controller CreateController(Type controllerType, Request request)
        {
            var controller = (Controller)Request.ServiceCollection.CreateInstance(controllerType);

            controllerType
                .GetProperty("Request", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(controller, request);

            return controller;
        }

        private static string GetValue(this Request request, string name)
           => request.Query.GetValueOrDefault(name) ??
           request.Form.GetValueOrDefault(name);
    }
}
