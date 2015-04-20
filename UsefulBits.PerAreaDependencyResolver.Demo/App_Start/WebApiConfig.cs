using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using UsefulBits.Web.Http.Areas;
using UsefulBits.Web.Http.Areas.Routing;

namespace UsefulBits.Web.Demo
{
  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      // Web API configuration and services
      config.Services.Replace(typeof(IHttpControllerSelector), new AreaHttpControllerSelector(config));

      // Web API routes
      config.MapHttpAttributeRoutes();

      config.Routes.MapHttpRoute(
          name: "DefaultApi",
          routeTemplate: "api/{controller}/{id}",
          defaults: new { id = RouteParameter.Optional }
      );
    }
  }
}
