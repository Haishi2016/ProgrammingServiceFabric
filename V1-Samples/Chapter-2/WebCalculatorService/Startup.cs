using Owin;
using System.Web.Http;
using WebCalculatorService.App_Start;
using WebCalculatorService.Interfaces;

namespace WebCalculatorService
{
    public class Startup : IOwinAppBuilder
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();
            FormatterConfig.ConfigureFormatters(config.Formatters);
            RouteConfig.RegisterRoutes(config.Routes);
            appBuilder.UseWebApi(config);
        }
    }
}
