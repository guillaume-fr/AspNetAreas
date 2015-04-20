using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;
using UsefulBits.Web.Http.Areas.Dependencies;

namespace UsefulBits.Web.Http.Areas
{
  /// <summary>
  /// Extention methods on HttpConfiguration
  /// </summary>
  public static class HttpConfigurationExtensions
  {
    /// <summary>
    /// Enables the per area dependency resovler.
    /// </summary>
    /// <param name="configuration">The http configuration.</param>
    /// <param name="rootDependencyResolver">The root dependency resolver used to resolve types outside areas.</param>
    public static void EnablePerAreaDependencyResovler(this HttpConfiguration configuration, IDependencyResolver rootDependencyResolver = null)
    {
      configuration.DependencyResolver = AreaDependencyResolverConfiguration.BuildDependencyResolver(rootDependencyResolver);
    }
  }
}
