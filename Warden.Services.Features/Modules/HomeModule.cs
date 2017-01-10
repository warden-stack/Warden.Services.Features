namespace Warden.Services.Features.Modules
{
    public class HomeModule : ModuleBase
    {
        public HomeModule()
        {
            Get("", args => "Welcome to the Warden.Services.Features API!");
        }
    }
}