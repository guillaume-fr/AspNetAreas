using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsefulBits
{
  internal static class AreaNamespace
  {
    //Method from ASP.Net MVC
    internal static bool IsNamespaceMatch(string requestedNamespace, string targetNamespace)
    {
      // degenerate cases
      if (requestedNamespace == null)
      {
        return false;
      }
      else if (requestedNamespace.Length == 0)
      {
        return true;
      }

      if (!requestedNamespace.EndsWith(".*", StringComparison.OrdinalIgnoreCase))
      {
        // looking for exact namespace match
        return String.Equals(requestedNamespace, targetNamespace, StringComparison.OrdinalIgnoreCase);
      }
      else
      {
        // looking for exact or sub-namespace match
        requestedNamespace = requestedNamespace.Substring(0, requestedNamespace.Length - ".*".Length);
        if (!targetNamespace.StartsWith(requestedNamespace, StringComparison.OrdinalIgnoreCase))
        {
          return false;
        }

        if (requestedNamespace.Length == targetNamespace.Length)
        {
          // exact match
          return true;
        }
        else if (targetNamespace[requestedNamespace.Length] == '.')
        {
          // good prefix match, e.g. requestedNamespace = "Foo.Bar" and targetNamespace = "Foo.Bar.Baz"
          return true;
        }
        else
        {
          // bad prefix match, e.g. requestedNamespace = "Foo.Bar" and targetNamespace = "Foo.Bar2"
          return false;
        }
      }
    }
  }
}
