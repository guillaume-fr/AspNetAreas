using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsefulBits.Web.Http.Areas.Dependencies
{
  internal class AreaDependencyResolverCache
  {
    private readonly IReadOnlyDictionary<string, ICollection<string>> _areaNamespaces;
    private readonly ConcurrentDictionary<string, string> _namespaceAreaCache = new ConcurrentDictionary<string, string>();

    public AreaDependencyResolverCache(IReadOnlyDictionary<string, ICollection<string>> areaNamespaces)
    {
      if (areaNamespaces == null)
        throw new ArgumentNullException("areaNamespaces");
      _areaNamespaces = areaNamespaces;
    }

    public string GetArea(string targetNamespace)
    {
      return _namespaceAreaCache.GetOrAdd(targetNamespace, FindArea);
    }

    private string FindArea(string targetNamespace)
    {
      var query = _areaNamespaces.SelectMany(kv => kv.Value.Select(v => new { area = kv.Key, areaNamespace = v }));
      return query.Where(ns => AreaNamespace.IsNamespaceMatch(ns.areaNamespace, targetNamespace))
                  .Select(ns => ns.area)
                  .FirstOrDefault();
    }
  }
}
