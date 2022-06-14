﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using MediatR;
using Messaging.Api;
using Messaging.Infrastructure.Configuration;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Messaging.ArchitectureTests
{
    public class CompositionRootTests
    {
        private readonly IHost _host;

        public CompositionRootTests()
        {
            _host = Program.ConfigureHost(Program.DevelopmentTokenValidationParameters(), new TestEnvironment());
        }

        #region Member data providers

        public static IEnumerable<object[]> GetRequestHandlerRequirements()
            => ResolveTypes(
                typeof(IRequestHandler<,>),
                new[] { ApplicationAssemblies.Application, ApplicationAssemblies.Infrastructure });

        public static IEnumerable<object[]> GetNotificationsHandlerRequirements()
            => ResolveTypes(
                typeof(INotificationHandler<>),
                new[] { ApplicationAssemblies.Application, ApplicationAssemblies.Infrastructure });

        public static IEnumerable<object[]> GetFunctionRequirements()
        {
            var allTypes = ReflectionHelper.FindAllTypes();
            var functionTypes = ReflectionHelper.FindAllFunctionTypes();
            var constructorDependencies = ReflectionHelper.FindAllConstructorDependenciesForType();

            return functionTypes(allTypes(typeof(Program)))
                .Select(f => new object[] { new Requirement(f.Name, constructorDependencies(f)) });
        }

        public static IEnumerable<object[]> GetMiddlewareRequirements()
        {
            var allTypes = ReflectionHelper.FindAllTypes();
            var middlewareTypes = ReflectionHelper.FindAllTypesThatImplementType();
            var constructorDependencies = ReflectionHelper.FindAllConstructorDependenciesForType();

            return
                middlewareTypes(typeof(IFunctionsWorkerMiddleware), allTypes(typeof(Program)))
                    .Select(m => new object[] { new Requirement(m.Name, constructorDependencies(m)) });
        }

        #endregion

        [Theory]
        [MemberData(nameof(GetRequestHandlerRequirements))]
        public void All_request_handlers_are_registered(Requirement requirement)
        {
            using var scope = _host.Services.CreateScope();
            Assert.True(scope.ServiceProvider.CanSatisfyRequirement(requirement));
        }

        [Theory]
        [MemberData(nameof(GetNotificationsHandlerRequirements))]
        public void All_notification_handlers_are_registered(Requirement requirement)
        {
            using var scope = _host.Services.CreateScope();
            Assert.True(scope.ServiceProvider.CanSatisfyRequirement(requirement));
        }

        [Theory]
        [MemberData(nameof(GetMiddlewareRequirements))]
        public void All_dependencies_can_be_resolved_for_middleware(Requirement requirement)
        {
            using var scope = _host.Services.CreateScope();
            Assert.True(scope.ServiceProvider.CanSatisfyRequirement(requirement));
        }

        [Theory]
        [MemberData(nameof(GetFunctionRequirements))]
        public void All_dependencies_can_be_resolved_in_functions(Requirement requirement)
        {
            using var scope = _host.Services.CreateScope();
            Assert.True(scope.ServiceProvider.CanSatisfyRequirement(requirement));
        }

        private static IEnumerable<object[]> ResolveTypes(Type targetType, Assembly[] assemblies)
        {
            var allTypes = ReflectionHelper.FindAllTypesInAssemblies();
            var allImplementations = ReflectionHelper.FindAllTypesThatImplementGenericInterface();
            var getGenericTypeDefinition = ReflectionHelper.MapToUnderlyingType();

            return allImplementations(targetType, allTypes(assemblies))
                .Select(type => new object[] { new Requirement(type.Name, getGenericTypeDefinition(type, targetType)) });
        }

        private class TestEnvironment : RuntimeEnvironment
        {
            public override string? MOVE_IN_REQUEST_ENDPOINT => "https://test.dk";

            public override string? INCOMING_MESSAGE_QUEUE_SENDER_CONNECTION_STRING =>
                CreateFakeServiceBusConnectionString();

            public override string? REQUEST_RESPONSE_LOGGING_CONNECTION_STRING =>
                CreateFakeServiceBusConnectionString();

            public override string? INCOMING_MESSAGE_QUEUE_MANAGE_CONNECTION_STRING =>
                CreateFakeServiceBusConnectionString();

            public override string? MESSAGEHUB_QUEUE_CONNECTION_STRING =>
                CreateFakeServiceBusConnectionString();

            public override bool IsRunningLocally()
            {
                return true;
            }

            protected override string? GetEnvironmentVariable(string variable)
            {
                return Guid.NewGuid().ToString();
            }

            private static string CreateFakeServiceBusConnectionString()
            {
                return new StringBuilder()
                    .Append(CultureInfo.InvariantCulture, $"Endpoint=sb://sb-{Guid.NewGuid():N}.servicebus.windows.net/;")
                    .Append("SharedAccessKeyName=send;")
                    .Append(CultureInfo.InvariantCulture, $"SharedAccessKey={Guid.NewGuid():N}")
                    .ToString();
            }
        }
    }
}
