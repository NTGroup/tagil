using System.Web.Http;
using po.fwdr.api.AppConfigurators;

namespace po.fwdr.api
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			GlobalConfiguration.Configure(RoutesConfig.Register);
			GlobalConfiguration.Configure(JsonContentConfig.Register);
		}
	}
}
