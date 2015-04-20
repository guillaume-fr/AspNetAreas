using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;

namespace UsefulBits.Web.Http.Areas.Dependencies
{
  internal static class AreaDependencyResolverConfiguration
  {
    private static readonly ConcurrentDictionary<string, IDependencyResolver> _areaDependencyResolvers = new ConcurrentDictionary<string,IDependencyResolver>();
    private static readonly ConcurrentDictionary<string, ICollection<string>> _areaNamespaces = new ConcurrentDictionary<string, ICollection<string>>();

    public static void Register(string areaName, IDependencyResolver dependencyResolver, ICollection<string> namespaces)
    {
      if (!_areaDependencyResolvers.TryAdd(areaName, dependencyResolver)
        || !_areaNamespaces.TryAdd(areaName, namespaces))
        throw new ArgumentException("A DependencyResolver is already registered for area " + areaName, "area");      
    }
    
    public static AreaDependencyResolver BuildDependencyResolver(IDependencyResolver rootDependencyResolver)
    {
      return new AreaDependencyResolver(rootDependencyResolver,
        _areaDependencyResolvers.ToDictionary(kv => kv.Key, kv => kv.Value),
        _areaNamespaces.ToDictionary(kv => kv.Key, kv => kv.Value));
    }
  }
}
