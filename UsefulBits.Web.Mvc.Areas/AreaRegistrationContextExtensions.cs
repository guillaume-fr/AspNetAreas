using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace UsefulBits.Web.Mvc.Areas
{
  public static class AreaRegistrationContextExtensions
  {
    /// <summary>
    /// Provides a registration point for dependency resolvers, using the specified dependency resolver interface as resolver for serives inside this area.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="dependencyResolver">The dependency resolver.</param>
    /// <exception cref="System.ArgumentNullException">context is null</exception>
    public static void SetDependencyResolver(this AreaRegistrationContext context, IDependencyResolver dependencyResolver)
    {
      if (context == null)
        throw new ArgumentNullException("context");
      foreach (var areaNamespace in context.Namespaces)
      {
        AreaDependencyResolver.Instance.Register(areaNamespace, dependencyResolver);
      }
    }
  }
}
