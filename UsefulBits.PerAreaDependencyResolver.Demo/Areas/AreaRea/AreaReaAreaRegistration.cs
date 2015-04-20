using System.Web.Mvc;
using UsefulBits.Web.Http.Areas;

namespace UsefulBits.Web.Demo.Areas.AreaRea
{
    public class AreaReaAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "AreaRea";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
          context.MapHttpRoute(
                "AreaRea_DefaultApi",
                "AreaRea/api/{controller}/{id}",
            defaults: new { id = System.Web.Http.RouteParameter.Optional });

            context.MapRoute(
                "AreaRea_default",
                "AreaRea/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}