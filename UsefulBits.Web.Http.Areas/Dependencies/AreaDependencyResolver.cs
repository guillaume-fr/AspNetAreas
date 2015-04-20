using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace UsefulBits.Web.Http.Areas.Dependencies
{
  /// <summary>
  /// Represent a dependency resolver that support per area dependencies
  /// </summary>
  public sealed class AreaDependencyResolver : AreaDependencyScope, IDependencyResolver
  {
    private readonly IDependencyResolver _rootDependencyResolver;
    private readonly IReadOnlyDictionary<string, IDependencyResolver> _areaDependencyResolvers;

    /// <summary>
    /// Initializes a new instance of the <see cref="AreaDependencyResolver"/> class.
    /// </summary>
    /// <param name="rootDependencyResolver">The root dependency resolver.</param>
    /// <param name="areaDependencyResolvers">The area dependency resolvers.</param>
    /// <param name="areaNamespaces">The area namespaces.</param>
    public AreaDependencyResolver(IDependencyResolver rootDependencyResolver,
      IReadOnlyDictionary<string, IDependencyResolver> areaDependencyResolvers,
      IReadOnlyDictionary<string, ICollection<string>> areaNamespaces)
      : base(rootDependencyResolver, areaDependencyResolvers.ToDictionary(kv => kv.Key, kv => (IDependencyScope)kv.Value), new AreaDependencyResolverCache(areaNamespaces))
    {
      _rootDependencyResolver = rootDependencyResolver;
      _areaDependencyResolvers = areaDependencyResolvers;
    }

    /// <summary>
    /// Starts a resolution scope.
    /// </summary>
    /// <returns>
    /// The dependency scope.
    /// </returns>
    public IDependencyScope BeginScope()
    {
      return new AreaDependencyScope(_rootDependencyResolver.BeginScope(),
        _areaDependencyResolvers.ToDictionary(kv => kv.Key, kv => kv.Value.BeginScope()),
        NamespacesCache);
    }
  }
}
