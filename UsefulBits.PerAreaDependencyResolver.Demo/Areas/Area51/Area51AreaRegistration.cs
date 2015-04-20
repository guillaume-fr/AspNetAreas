using System.Web.Mvc;
using Microsoft.Practices.Unity.Mvc;
using UsefulBits.Web.Mvc.Areas;
using UsefulBits.Web.Http.Areas;

namespace UsefulBits.Web.Demo.Areas.Area51
{
    public class Area51AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Area51";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
          context.MapHttpRoute(
                "Area51_DefaultApi",
                "Area51/api/{controller}/{id}",
            defaults: new { id = System.Web.Http.RouteParameter.Optional });

            context.MapRoute(
                "Area51_default",
                "Area51/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );

            context.SetDependencyResolver(new UnityDependencyResolver(UnityConfig.GetConfiguredContainer()));
        }
    }
}