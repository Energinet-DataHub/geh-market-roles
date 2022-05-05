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
using System.Text;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Processing.Api;
using Processing.Api.Configuration;
using Xunit;

namespace Processing.ArchitectureTests
{
    public class CompositionRootTests
    {
        [Fact]
        public void All_dependencies_can_be_resolved_in_functions()
        {
            var host = Program.ConfigureHost(new TestEnvironment());

            var allTypes = FunctionsReflectionHelper.FindAllTypes();
            var functionTypes = FunctionsReflectionHelper.FindAllFunctionTypes();
            var constructorDependencies = FunctionsReflectionHelper.FindAllConstructorDependencies();

            var dependencies = constructorDependencies(functionTypes(allTypes(typeof(Program))));

            using var scope = host.Services.CreateScope();

            foreach (var dependency in dependencies)
            {
                var resolvedInstance = scope.ServiceProvider.GetService(dependency);
                Assert.True(resolvedInstance != null, $"Unable to resolve {dependency.Name}");
            }
        }

        [Fact]
        public void All_dependencies_can_be_resolved_for_middleware()
        {
            var host = Program.ConfigureHost(new TestEnvironment());

            var allTypes = FunctionsReflectionHelper.FindAllTypes();
            var middlewareTypes = FunctionsReflectionHelper.FindAllTypesThatImplementType();
            var constructorDependencies = FunctionsReflectionHelper.FindAllConstructorDependencies();

            var dependencies = constructorDependencies(middlewareTypes(typeof(IFunctionsWorkerMiddleware), allTypes(typeof(Program))));
            using var scope = host.Services.CreateScope();

            foreach (var dependency in dependencies)
            {
                var resolvedInstance = scope.ServiceProvider.GetService(dependency);
                Assert.True(resolvedInstance != null, $"Unable to resolve {dependency.Name}");
            }
        }

        private class TestEnvironment : RuntimeEnvironment
        {
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
#pragma warning disable
                var sb = new StringBuilder();
                sb.Append($"Endpoint=sb://sb-{Guid.NewGuid():N}.servicebus.windows.net/;");
                sb.Append("SharedAccessKeyName=send;");
                sb.Append($"SharedAccessKey={Guid.NewGuid():N}");
                return sb.ToString();
#pragma warning restore
            }
        }
    }
}