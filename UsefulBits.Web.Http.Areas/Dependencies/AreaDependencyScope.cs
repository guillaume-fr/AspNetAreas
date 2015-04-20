using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;

namespace UsefulBits.Web.Http.Areas.Dependencies
{
  /// <summary>
  /// Represent a dependency scope that support per area dependencies
  /// </summary>
  public class AreaDependencyScope : IDependencyScope
  {
    IDependencyScope _rootDependencyScope;
    IReadOnlyDictionary<string, IDependencyScope> _areaDependencyScopes;
    internal AreaDependencyResolverCache NamespacesCache { get; private set; }

    internal AreaDependencyScope(IDependencyScope rootDependencyScope,
      IReadOnlyDictionary<string, IDependencyScope> areaDependencyScope,
      AreaDependencyResolverCache namespacesCache)
    {
      _rootDependencyScope = rootDependencyScope;
      _areaDependencyScopes = areaDependencyScope;
      NamespacesCache = namespacesCache;
    }

    private string GetArea(Type serviceType)
    {
      return NamespacesCache.GetArea(serviceType.Namespace);
    }

    private IDependencyScope GetInnerDependencyScope(Type serviceType)
    {
      string area = GetArea(serviceType);
      if (area == null)
      {
        return _rootDependencyScope;
      }
      else
      {
        return _areaDependencyScopes[area];
      }
    }

    /// <summary>
    /// Retrieves a service from the scope.
    /// </summary>
    /// <param name="serviceType">The service to be retrieved.</param>
    /// <returns>
    /// The retrieved service.
    /// </returns>
    public object GetService(Type serviceType)
    {
      var dependencyScope = GetInnerDependencyScope(serviceType);
      if (dependencyScope != null)
      {
        return dependencyScope.GetService(serviceType);
      }
      return null;
    }

    /// <summary>
    /// Retrieves a collection of services from the scope.
    /// </summary>
    /// <param name="serviceType">The collection of services to be retrieved.</param>
    /// <returns>
    /// The retrieved collection of services.
    /// </returns>
    public IEnumerable<object> GetServices(Type serviceType)
    {
      var dependencyScope = GetInnerDependencyScope(serviceType);
      if (dependencyScope != null)
      {
        return dependencyScope.GetServices(serviceType);
      }
      return null;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      _rootDependencyScope.Dispose();
      foreach (var item in _areaDependencyScopes)
      {
        item.Value.Dispose();
      }
    }
  }
}
