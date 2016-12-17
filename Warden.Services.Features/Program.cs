using Warden.Common.Host;
using Warden.Services.Features.Framework;
using Warden.Services.Organizations.Shared.Commands;
using Warden.Services.Organizations.Shared.Events;
using Warden.Services.Users.Shared.Commands;
using Warden.Services.Users.Shared.Events;
using Warden.Services.WardenChecks.Shared.Commands;
using Warden.Services.WardenChecks.Shared.Events;

namespace Warden.Services.Features
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebServiceHost
                .Create<Startup>(port: 5057)
                .UseAutofac(Bootstrapper.LifetimeScope)
                .UseRabbitMq(queueName: typeof(Program).Namespace)
                .SubscribeToCommand<RequestNewApiKey>()
                .SubscribeToCommand<RequestNewWarden>()
                .SubscribeToCommand<RequestProcessWardenCheckResult>()
                .SubscribeToCommand<RequestNewOrganization>()
                .SubscribeToEvent<ApiKeyCreated>()
                .SubscribeToEvent<WardenCheckResultProcessed>()
                .SubscribeToEvent<SignedIn>()
                .SubscribeToEvent<SignedUp>()
                .SubscribeToEvent<WardenCreated>()
                .SubscribeToEvent<OrganizationCreated>()
                .Build()
                .Run();
        }
    }
}
