using Owin;

namespace WebCalculatorService.Interfaces
{
    public interface IOwinAppBuilder
    {
        void Configuration(IAppBuilder appBuilder);
    }
}
