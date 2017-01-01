using Autofac;
using Microsoft.Extensions.Configuration;
using Nancy.Bootstrapper;
using NLog;
using RawRabbit;
using RawRabbit.vNext;
using RawRabbit.Configuration;
using Warden.Common.Caching;
using Warden.Common.Commands;
using Warden.Common.Events;
using Warden.Common.Extensions;
using Warden.Common.Mongo;
using Warden.Common.Nancy;
using Warden.Common.Nancy.Serialization;
using Warden.Services.Features.Handlers;
using Warden.Services.Features.Repositories;
using Warden.Services.Features.Services;
using Warden.Services.Features.Settings;
using Warden.Services.Organizations.Shared.Commands;
using Warden.Services.Organizations.Shared.Events;
using Warden.Services.Users.Shared.Commands;
using Warden.Services.Users.Shared.Events;
using Warden.Services.WardenChecks.Shared.Commands;
using Warden.Services.WardenChecks.Shared.Events;
using Newtonsoft.Json;
using System.Reflection;

namespace Warden.Services.Features.Framework
{
    public class Bootstrapper : AutofacNancyBootstrapper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration _configuration;
        public static ILifetimeScope LifetimeScope { get; private set; }

        public Bootstrapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void ConfigureApplicationContainer(ILifetimeScope container)
        {
            base.ConfigureApplicationContainer(container);
            container.Update(builder =>
            {
                builder.RegisterInstance(_configuration.GetSettings<MongoDbSettings>());
                builder.RegisterInstance(_configuration.GetSettings<FeatureSettings>());
                builder.RegisterInstance(_configuration.GetSettings<PaymentPlanSettings>());
                builder.RegisterType<CustomJsonSerializer>().As<JsonSerializer>().SingleInstance();
                builder.RegisterModule<MongoDbModule>();
                builder.RegisterModule<InMemoryCacheModule>();
                builder.RegisterType<MongoDbInitializer>().As<IDatabaseInitializer>();
                builder.RegisterType<DatabaseSeeder>().As<IDatabaseSeeder>();
                var rawRabbitConfiguration = _configuration.GetSettings<RawRabbitConfiguration>();
                builder.RegisterInstance(rawRabbitConfiguration).SingleInstance();
                builder.RegisterInstance(BusClientFactory.CreateDefault(rawRabbitConfiguration))
                    .As<IBusClient>();
                builder.RegisterType<UserRepository>().As<IUserRepository>();
                builder.RegisterType<PaymentPlanRepository>().As<IPaymentPlanRepository>();
                builder.RegisterType<UserPaymentPlanRepository>().As<IUserPaymentPlanRepository>();
                builder.RegisterType<WardenChecksCounter>().As<IWardenChecksCounter>();
                builder.RegisterType<UserFeaturesManager>().As<IUserFeaturesManager>();
                builder.RegisterType<UserPaymentPlanService>().As<IUserPaymentPlanService>();

                var assembly = typeof(Startup).GetTypeInfo().Assembly;
                builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(IEventHandler<>));
                builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(ICommandHandler<>));
            });
            LifetimeScope = container;
        }

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            var databaseSettings = container.Resolve<MongoDbSettings>();
            var databaseInitializer = container.Resolve<IDatabaseInitializer>();
            databaseInitializer.InitializeAsync();
            if (databaseSettings.Seed)
            {
                var seeder = container.Resolve<IDatabaseSeeder>();
                seeder.SeedAsync();
            }
            pipelines.AfterRequest += (ctx) =>
            {
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization, Origin, X-Requested-With, Content-Type, Accept");
            };
            Logger.Info("Warden.Services.Features API Started");
        }
    }
}