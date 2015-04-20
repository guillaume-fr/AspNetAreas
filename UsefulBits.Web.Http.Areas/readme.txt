
=================================================================================================
===========================================   USAGE   ===========================================
=================================================================================================

Register your routes in existing AreaRegistration classes with MapHttpRoute extension method
If you want to register a custom dependency resolver for an area, use SetHttpDependencyResolver

    // ...
    using UsefulBits.Web.Http.Areas;
	// ...
	    public override void RegisterArea(AreaRegistrationContext context) 
        {
          context.MapHttpRoute(
                "MyArea_DefaultApi",
                "MyArea/api/{controller}/{id}",
                defaults: new { id = System.Web.Http.RouteParameter.Optional });

		  context.SetHttpDependencyResolver(new CustomDependencyResolver());
	//...


=================================================================================================
===========================================   SETUP   ===========================================
=================================================================================================

This is NOT mandatory but if you want to allow controllers with same name in different areas,
you have to setup a custom controller selector in WebApiConfig.cs by replacing IHttpControllerSelector.

If you use dependency resolver per area, you have to use EnablePerAreaDependencyResovler method

    public static void Register(HttpConfiguration config)
    {
      // Allows API controllers with duplicate names
      config.Services.Replace(typeof(IHttpControllerSelector), new AreaHttpControllerSelector(config));
	  
	  // Enable custom dependency resolver per Area (mandatory when using SetHttpDependencyResolver)
	  config.EnablePerAreaDependencyResovler(/*rootDependencyResolver: new MyDependencyResolver()*/);