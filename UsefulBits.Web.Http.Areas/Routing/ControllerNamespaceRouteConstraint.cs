using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Routing;

namespace UsefulBits.Web.Http.Areas.Routing
{
  /// <summary>
  /// Route Constraint to apply on {controller} argument so it allows only controller in specified namespaces.
  /// </summary>
  public sealed class ControllerNamespaceRouteConstraint : IRouteConstraint
  {
    private readonly object innerConstraint;
    private readonly Lazy<HashSet<string>> controllerNames;
    /// <summary>
    /// Static cache of all availables controllers
    /// </summary>
    private static readonly Lazy<ILookup<string, Type>> controllerTypes = new Lazy<ILookup<string, Type>>(GetControllerTypes);

    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerNamespaceRouteConstraint"/> class.
    /// </summary>
    /// <param name="namespaces">The namespaces in which allowed controllers reside.</param>
    /// <param name="innerConstraint">Another constraint to apply or null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="namespaces"/> is null</exception>
    public ControllerNamespaceRouteConstraint(ICollection<string> namespaces, object innerConstraint = null)
    {
      if (namespaces == null)
        throw new ArgumentNullException("namespaces");
      this.innerConstraint = innerConstraint;
      this.controllerNames = new Lazy<HashSet<string>>(() => new HashSet<string>(GetControllerNames(namespaces), StringComparer.OrdinalIgnoreCase));
    }

    private static ILookup<string, Type> GetControllerTypes()
    {
      IAssembliesResolver assembliesResolver = GlobalConfiguration.Configuration.Services.GetAssembliesResolver();
      IHttpControllerTypeResolver controllersResolver = GlobalConfiguration.Configuration.Services.GetHttpControllerTypeResolver();
      ICollection<Type> controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);
      return controllerTypes.ToLookup(t => t.Namespace ?? String.Empty);
    }

    private static IEnumerable<string> GetControllerNames(ICollection<string> namespaces)
    {
      return controllerTypes.Value.Where(t => namespaces.Any(n => AreaNamespace.IsNamespaceMatch(n, t.Key)))
                                  .SelectMany(g => g)
                                  .Select(t => t.Name.Substring(0, t.Name.Length - DefaultHttpControllerSelector.ControllerSuffix.Length));
    }

    /// <summary>
    /// Determines whether the URL parameter contains a valid value for this constraint.
    /// </summary>
    /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param>
    /// <param name="route">The object that this constraint belongs to.</param>
    /// <param name="parameterName">The name of the parameter that is being checked.</param>
    /// <param name="values">An object that contains the parameters for the URL.</param>
    /// <param name="routeDirection">An object that indicates whether the constraint check is being performed when an incoming request is being handled or when a URL is being generated.</param>
    /// <returns>
    /// true if the URL parameter contains a valid value; otherwise, false.
    /// </returns>
    public bool Match(System.Web.HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
    {
      object parameterValue;
      values.TryGetValue(parameterName, out parameterValue);

      string requestedControllerName = Convert.ToString(parameterValue, CultureInfo.InvariantCulture);
      if (!controllerNames.Value.Contains(requestedControllerName))
      {
        return false;
      }

      if (innerConstraint != null)
      {
        IRouteConstraint customConstraint = innerConstraint as IRouteConstraint;
        if (customConstraint != null)
        {
          return customConstraint.Match(httpContext, route, parameterName, values, routeDirection);
        }

        // If there was no custom constraint, then treat the constraint as a string which represents a Regex.
        string constraintsRule = innerConstraint as string;
        if (constraintsRule == null)
        {
          throw new InvalidOperationException(string.Format("The constraint has an invalid type, {3} or string expected. ParameterName {0}, Route {1}", parameterName, route.Url, typeof(IRouteConstraint).Name));
        }

        string constraintsRegEx = "^(" + constraintsRule + ")$";
        return Regex.IsMatch(requestedControllerName, constraintsRegEx, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
      }

      return true;
    }
  }
}
