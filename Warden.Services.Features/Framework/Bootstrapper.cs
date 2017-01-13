using Autofac;
using Microsoft.Extensions.Configuration;
using Nancy.Bootstrapper;
using NLog;
using RawRabbit.Configuration;
using Warden.Common.Caching;
using Warden.Common.Commands;
using Warden.Common.Extensions;
using Warden.Common.Exceptionless;
using Warden.Common.Events;
using Warden.Common.Handlers;
using Warden.Common.Mongo;
using Warden.Common.Nancy;
using Warden.Common.Nancy.Serialization;
using Warden.Common.Security;
using Warden.Common.RabbitMq;
using Warden.Services.Features.Repositories;
using Warden.Services.Features.Services;
using Warden.Services.Features.Settings;
using Newtonsoft.Json;
using Nancy;
using System.Reflection;

namespace Warden.Services.Features.Framework
{
    public class Bootstrapper : AutofacNancyBootstrapper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IExceptionHandler _exceptionHandler;
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
                builder.RegisterType<UserRepository>().As<IUserRepository>();
                builder.RegisterType<PaymentPlanRepository>().As<IPaymentPlanRepository>();
                builder.RegisterType<UserPaymentPlanRepository>().As<IUserPaymentPlanRepository>();
                builder.RegisterType<WardenChecksService>().As<IWardenChecksService>();
                builder.RegisterType<UserFeaturesManager>().As<IUserFeaturesManager>();
                builder.RegisterType<UserPaymentPlanService>().As<IUserPaymentPlanService>();
                builder.RegisterType<Handler>().As<IHandler>();
                builder.RegisterInstance(_configuration.GetSettings<ExceptionlessSettings>()).SingleInstance();
                builder.RegisterType<ExceptionlessExceptionHandler>().As<IExceptionHandler>().SingleInstance();

                var assembly = typeof(Startup).GetTypeInfo().Assembly;
                builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(IEventHandler<>));
                builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(ICommandHandler<>));

                SecurityContainer.Register(builder, _configuration);
                RabbitMqContainer.Register(builder, _configuration.GetSettings<RawRabbitConfiguration>());
            });
            LifetimeScope = container;
        }

        protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context)
        {
            pipelines.OnError.AddItemToEndOfPipeline((ctx, ex) =>
            {
                _exceptionHandler.Handle(ex, ctx.ToExceptionData(),
                    "Request details", "Warden", "Service", "Features");

                return ctx.Response;
            });
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
            _exceptionHandler = container.Resolve<IExceptionHandler>();
            pipelines.SetupTokenAuthentication(container);
            Logger.Info("Warden.Services.Features API has started.");
        }
    }
}