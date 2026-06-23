namespace LeaveRequest.Infrastructure.Extensions;

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using LeaveRequest.Application.Interfaces;
using LeaveRequest.Application.Services;
using LeaveRequest.Domain.Interfaces.Repositories;
using LeaveRequest.Infrastructure.Messaging;
using LeaveRequest.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class NotificationServiceCollectionExtensions
{
    /// <summary>
    /// Registers IF-002 notification services.
    /// When UseStub=true (default), wires stubs — no Service Bus or SMTP required.
    /// Production: requires "ServiceBus" and "Smtp" config sections and "Notification:HrEmail".
    /// </summary>
    public static IServiceCollection AddNotificationIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ServiceBusOptions>(configuration.GetSection(ServiceBusOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<NotificationOptions>(configuration.GetSection(NotificationOptions.SectionName));

        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<INotificationService, NotificationService>();

        var sbConfig = configuration
            .GetSection(ServiceBusOptions.SectionName)
            .Get<ServiceBusOptions>() ?? new ServiceBusOptions();

        if (sbConfig.UseStub)
        {
            services.AddSingleton<IMessagePublisher, MessagePublisherStub>();
            services.AddHostedService<EmailConsumerStub>();
            services.AddSingleton<IEmailConsumer>(sp =>
                (IEmailConsumer)sp.GetServices<IHostedService>()
                    .OfType<EmailConsumerStub>()
                    .First());
            return services;
        }

        // Production: register Azure Service Bus client + publisher + consumer
        services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceBusOptions>>().Value;
            return string.IsNullOrWhiteSpace(opts.ConnectionString)
                ? new ServiceBusClient(opts.FullyQualifiedNamespace,
                    new Azure.Identity.DefaultAzureCredential())
                : new ServiceBusClient(opts.ConnectionString);
        });

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceBusOptions>>().Value;
            return client.CreateSender(opts.TopicName);
        });

        services.AddSingleton<IMessagePublisher, ServiceBusPublisher>();

        services.AddHostedService<EmailConsumer>();
        services.AddSingleton<IEmailConsumer>(sp =>
            (IEmailConsumer)sp.GetServices<IHostedService>()
                .OfType<EmailConsumer>()
                .First());

        return services;
    }
}
