using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Project
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            Exception unhandleException = Server.GetLastError();
            HttpException httpException = unhandleException as HttpException;

            if(httpException == null)
            {
                Exception innerException = unhandleException.InnerException;
                httpException = innerException as HttpException;
            }

            if(httpException != null)
            {
                int httpCode = httpException.GetHttpCode();
                switch (httpCode)
                {
                    case (int) HttpStatusCode.Unauthorized:
                        Response.Redirect("/Http/Error401");
                        break;

                }
            }
        }
    }
}
