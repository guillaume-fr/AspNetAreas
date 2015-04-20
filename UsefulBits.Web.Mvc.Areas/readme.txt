
=================================================================================================
===========================================   USAGE   ===========================================
=================================================================================================

If you want to register a custom dependency resolver for an area, use SetDependencyResolver

    // ...
    using UsefulBits.Web.Mvc.Areas;
	// ...
	    public override void RegisterArea(AreaRegistrationContext context) 
        {
		    context.SetDependencyResolver(new CustomDependencyResolver());
	//...


=================================================================================================
===========================================   SETUP   ===========================================
=================================================================================================

If you use dependency resolver per area, you have use the AreaDependencyResolver in your config
in Global.asax or UnityMvcActivator.cs :


    UsefulBits.Web.Mvc.Areas.AreaDependencyResolver.SetResolver(new UnityDependencyResolver(container));
