using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace UsefulBits.Web.Mvc.Areas
{
  /// <summary>
  /// Allow service location and dependency resolution per ASP MVC Area.
  /// </summary>
  public sealed class AreaDependencyResolver : IDependencyResolver
  {
    private ConcurrentDictionary<string, IDependencyResolver> _areaDependencyResolvers = new ConcurrentDictionary<string, IDependencyResolver>();
    private ConcurrentDictionary<string, IDependencyResolver> _namespaceCache = new ConcurrentDictionary<string, IDependencyResolver>();
    private IDependencyResolver _baseDependencyResolver;

    private AreaDependencyResolver()
    { }

    private static AreaDependencyResolver _instance = new AreaDependencyResolver();

    internal static AreaDependencyResolver Instance { get { return _instance; } }

    /// <summary>
    /// Provides a registration point for dependency resolvers, using the specified dependency resolver interface as resolver for objects outside a defined area.
    /// </summary>
    /// <param name="rootDependencyResolver">The root dependency resolver.</param>
    public static void SetResolver(IDependencyResolver rootDependencyResolver = null)
    {
      Instance._baseDependencyResolver = rootDependencyResolver;
      DependencyResolver.SetResolver(Instance);
    }

    internal void Register(string areaNamespace, IDependencyResolver dependencyResolver)
    {
      if (!_areaDependencyResolvers.TryAdd(areaNamespace, dependencyResolver))
        throw new ArgumentException("A DependencyResolver is already registered for namespace " + areaNamespace, "areaNamespace");
    }

    /// <summary>
    /// Resolves singly registered services that support arbitrary object creation.
    /// </summary>
    /// <param name="serviceType">The type of the requested service or object.</param>
    /// <returns>
    /// The requested service or object.
    /// </returns>
    public object GetService(Type serviceType)
    {
      var resolver = GetInnerDependencyResolver(serviceType);
      if (resolver != null)
        return resolver.GetService(serviceType);
      else
        return null;
    }

    /// <summary>
    /// Resolves multiply registered services.
    /// </summary>
    /// <param name="serviceType">The type of the requested services.</param>
    /// <returns>
    /// The requested services.
    /// </returns>
    public IEnumerable<object> GetServices(Type serviceType)
    {
      var resolver = GetInnerDependencyResolver(serviceType);
      if (resolver != null)
        return resolver.GetServices(serviceType);
      else
        return Enumerable.Empty<object>();
    }

    private IDependencyResolver GetInnerDependencyResolver(Type serviceType)
    {
      return _namespaceCache.GetOrAdd(serviceType.Namespace,
        serviceNameSpace =>
          _areaDependencyResolvers.FirstOrDefault(kv => AreaNamespace.IsNamespaceMatch(kv.Key, serviceType.Namespace)).Value ?? _baseDependencyResolver
        );
    }
  }
}
