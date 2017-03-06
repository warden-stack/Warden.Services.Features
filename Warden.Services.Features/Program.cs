using Warden.Common.Host;
using Warden.Services.Features.Framework;
using Warden.Messages.Commands.Organizations;
using Warden.Messages.Events.Organizations;
using Warden.Messages.Commands.Users;
using Warden.Messages.Events.Users;
using Warden.Messages.Commands.WardenChecks;
using Warden.Messages.Events.WardenChecks;

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
                .SubscribeToEvent<SignedUp>()
                .SubscribeToEvent<WardenCreated>()
                .SubscribeToEvent<OrganizationCreated>()
                .Build()
                .Run();
        }
    }
}
