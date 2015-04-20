using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace UsefulBits.Web.Http.Areas.Routing
{
  /// <summary>
  /// <see cref="IHttpControllerSelector"/> instance for choosing a <see cref="HttpControllerDescriptor"/> given a <see cref="HttpRequestMessage"/>.
  /// This implementation handles controllers in different areas filtering controller namespace depending on incoming message's route.
  /// This implementation can be registered via the <see cref="HttpConfiguration.Services"/>.
  /// </summary>
  public class AreaHttpControllerSelector : DefaultHttpControllerSelector, IHttpControllerSelector
  {
    private struct AreaController
    {
      private string controller;
      private string area;

      public AreaController(string area, string controller)
      {
        this.area = area;
        this.controller = controller;
      }
      string Area { get { return area; } }
      string Controller { get { return controller; } }

      public override bool Equals(object obj)
      {
        if (!(obj is AreaController))
          return false;
        var other = (AreaController)obj;
        return StringComparer.OrdinalIgnoreCase.Equals(other.Area, this.Area)
          && StringComparer.OrdinalIgnoreCase.Equals(other.Controller, this.Controller);
      }

      public override int GetHashCode()
      {
        unchecked
        {
          int hash = 17;
          hash = hash * 23 + (Area == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(this.Area));
          hash = hash * 23 + (Controller == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(this.Controller));
          return hash;
        }
      }
    }
    internal const string AreaRouteVariableName = "area";
    internal const string NamespacesRouteVariableName = "namespaces";

    private readonly HttpConfiguration _configuration;
    private readonly HttpControllerTypeCache _controllerTypeCache;
    private readonly Lazy<ConcurrentDictionary<AreaController, HttpControllerDescriptor>> _controllerInfoCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultHttpControllerSelector"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public AreaHttpControllerSelector(HttpConfiguration configuration)
      : base(configuration)
    {
      _controllerInfoCache = new Lazy<ConcurrentDictionary<AreaController, HttpControllerDescriptor>>(InitializeControllerInfoCache);
      _configuration = configuration;
      _controllerTypeCache = new HttpControllerTypeCache(_configuration);
    }


    private static string GetAreaName(IHttpRoute route)
    {
      if (route.DataTokens == null)
        return null;
      object areaName;
      if (route.DataTokens.TryGetValue(AreaRouteVariableName, out areaName))
        return areaName as string;
      else
        return null;
    }

    private static ICollection<string> GetNamespaces(IHttpRoute route)
    {
      if (route.DataTokens == null)
        return new string[0];
      object areaNamespaces;
      if (route.DataTokens.TryGetValue(NamespacesRouteVariableName, out areaNamespaces))
        return areaNamespaces as ICollection<string>;
      else
        return new string[0];
    }

    private static string GetAreaName(HttpRequestMessage request)
    {
      var data = request.GetRouteData();
      return GetAreaName(data.Route);
    }

    /// <summary>
    /// Selects a <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor" /> for the given <see cref="T:System.Net.Http.HttpRequestMessage" />.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <returns>
    /// The <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor" /> instance for the given <see cref="T:System.Net.Http.HttpRequestMessage" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">request</exception>
    /// <exception cref="HttpResponseException">
    /// </exception>
    /// <exception cref="HttpError">
    /// </exception>
    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
    public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
    {
      if (request == null)
      {
        throw new ArgumentNullException("request");
      }

      string controllerName = GetControllerName(request);
      if (String.IsNullOrEmpty(controllerName))
      {
        throw new HttpResponseException(request.CreateErrorResponse(
            HttpStatusCode.NotFound,
            new HttpError(string.Format("No HTTP resource was found that matches the request URI '{0}'", request.RequestUri))
            { MessageDetail = string.Format("No route providing a controller name was found to match request URI '{0}'", request.RequestUri) }));
      }

      HttpControllerDescriptor controllerDescriptor;
      if (_controllerInfoCache.Value.TryGetValue(new AreaController(GetAreaName(request), controllerName), out controllerDescriptor))
      {
        return controllerDescriptor;
      }

      ICollection<Type> matchingTypes = _controllerTypeCache.GetControllerTypes(controllerName);

      // ControllerInfoCache is already initialized.
      Contract.Assert(matchingTypes.Count != 1);

      if (matchingTypes.Count == 0)
      {
        // no matching types
        throw new HttpResponseException(request.CreateErrorResponse(
            HttpStatusCode.NotFound,
            new HttpError(string.Format("No HTTP resource was found that matches the request URI '{0}'", request.RequestUri))
            { MessageDetail = string.Format("No type was found that matches the controller named '{0}'.", controllerName) }));
      }
      else
      {
        // multiple matching types
        throw CreateAmbiguousControllerException(request.GetRouteData().Route, controllerName, matchingTypes);
      }
    }

    private static Exception CreateAmbiguousControllerException(IHttpRoute route, string controllerName, ICollection<Type> matchingTypes)
    {
      Contract.Assert(route != null);
      Contract.Assert(controllerName != null);
      Contract.Assert(matchingTypes != null);

      // Generate an exception containing all the controller types
      StringBuilder typeList = new StringBuilder();
      foreach (Type matchedType in matchingTypes)
      {
        typeList.AppendLine();
        typeList.Append(matchedType.FullName);
      }

      string errorMessage = string.Format("Multiple types were found that match the controller named '{0}'. This can happen if the route that services this request ('{1}') found multiple controllers defined with the same name but differing namespaces, which is not supported.{3}{3}The request for '{0}' has found the following matching controllers:{2}", controllerName, route.RouteTemplate, typeList, Environment.NewLine);
      return new InvalidOperationException(errorMessage);
    }

    private ConcurrentDictionary<AreaController, HttpControllerDescriptor> InitializeControllerInfoCache()
    {
      var result = new ConcurrentDictionary<AreaController, HttpControllerDescriptor>();
      var duplicateControllers = new HashSet<AreaController>();
      Dictionary<string, ILookup<string, Type>> controllerTypeGroups = _controllerTypeCache.Cache;

      var routeNamespaces = _configuration.Routes.SelectMany(r => GetNamespaces(r).Select(ns => new { Route = r, Namespace = ns }));

      foreach (KeyValuePair<string, ILookup<string, Type>> controllerTypeGroup in controllerTypeGroups)
      {
        string controllerName = controllerTypeGroup.Key;

        foreach (IGrouping<string, Type> controllerTypesGroupedByNs in controllerTypeGroup.Value)
        {
          foreach (Type controllerType in controllerTypesGroupedByNs)
          {
            var areaNames = routeNamespaces.Where(ns => AreaNamespace.IsNamespaceMatch(ns.Namespace, controllerType.Namespace)).Select(ns => GetAreaName(ns.Route)).ToList();
            //If no area matches, register it in root
            if (!areaNames.Any())
              areaNames.Add(null);
            foreach (var areaName in areaNames)
            {
              var key = new AreaController(areaName, controllerName);
              if (result.Keys.Contains(key))
              {
                duplicateControllers.Add(key);
                break;
              }
              else
              {
                result.TryAdd(key, new HttpControllerDescriptor(_configuration, controllerName, controllerType));
              }
            }
          }
        }
      }

      foreach (var duplicateController in duplicateControllers)
      {
        HttpControllerDescriptor descriptor;
        result.TryRemove(duplicateController, out descriptor);
      }

      return result;
    }
  }
}
