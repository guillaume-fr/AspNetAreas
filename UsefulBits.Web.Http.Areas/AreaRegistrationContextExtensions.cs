using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Http;
using System.Web.Routing;
using System.Text.RegularExpressions;
using System.Web.Http.Dispatcher;
using UsefulBits.Web.Http.Areas.Routing;
using UsefulBits.Web.Http.Areas.Dependencies;

namespace UsefulBits.Web.Http.Areas
{
  /// <summary>
  /// Extensions methods allowing mapping of http route in areas
  /// </summary>
  public static class AreaRegistrationContextExtensions
  {
    #region Routing
    /// <summary>
    /// Maps an HTTP route resolving controllers matching AreaRegistrationContext Namespaces.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="name">The unique route name.</param>
    /// <param name="routeTemplate">The route template.</param>
    /// <param name="defaults">The defaults.</param>
    /// <param name="constraints">The constraints.</param>
    public static Route MapHttpRoute(this AreaRegistrationContext context, string name, string routeTemplate, object defaults = null, object constraints = null)
    {
      if (context == null)
      {
        throw new ArgumentNullException("context");
      }

      var route = context.Routes.MapHttpRoute(name, routeTemplate, defaults, constraints);
      if (route.DataTokens == null)
      {
        route.DataTokens = new RouteValueDictionary();
      }
      route.DataTokens.Add(AreaHttpControllerSelector.AreaRouteVariableName, context.AreaName);
      route.DataTokens.Add(AreaHttpControllerSelector.NamespacesRouteVariableName, context.Namespaces);
      
      object existingControllerContraint;
      route.Constraints.TryGetValue("controller", out existingControllerContraint);
      route.Constraints["controller"] = new ControllerNamespaceRouteConstraint(context.Namespaces, existingControllerContraint);
      return route;
    }
    #endregion

    #region Dependency
    /// <summary>
    /// Provides a registration point for dependency resolvers, using the specified dependency resolver interface as resolver for serives inside this area.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="dependencyResolver">The dependency resolver.</param>
    /// <exception cref="System.ArgumentNullException">context is null</exception>
    public static void SetHttpDependencyResolver(this AreaRegistrationContext context, System.Web.Http.Dependencies.IDependencyResolver dependencyResolver)
    {
      AreaDependencyResolverConfiguration.Register(context.AreaName, dependencyResolver, context.Namespaces);
    }
    #endregion
  }
}
