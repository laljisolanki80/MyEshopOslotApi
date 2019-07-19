﻿using Autofac;
using Autofac.Core;
using FluentValidation;
using MediatR;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;
using Ordering.API.Application.DomainEventHandlers.OrderStartedEvent;
using Ordering.API.Application.Validations;
using Ordering.API.Infrastructure.Behaviors;
using Ordering.API.Application.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.AutofacModules
{
    public class MediatorModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly)
                .AsImplementedInterfaces();
            
            // Register all the Command classes (they implement IAsyncRequestHandler) in assembly holding the Commands
            builder.RegisterAssemblyTypes(typeof(CancelOrderCommand).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IAsyncRequestHandler<,>));

            // Register all the event classes (they implement IAsyncNotificationHandler) in assembly holding the Commands
            builder.RegisterAssemblyTypes(typeof(ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IAsyncNotificationHandler<>));
            
            builder.Register<SingleInstanceFactory>(context =>
            {
                var componentContext = context.Resolve<IComponentContext>();
                return t => { object o; return componentContext.TryResolve(t, out o) ? o : null; };
            });

            builder.Register<MultiInstanceFactory>(context =>
            {
                var componentContext = context.Resolve<IComponentContext>();

                return t =>
                {
                    var resolved = (IEnumerable<object>)componentContext.Resolve(typeof(IEnumerable<>).MakeGenericType(t));
                    return resolved;
                };
            });

            builder.RegisterGeneric(typeof(LoggingBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(ValidatorBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        }
    }
}
