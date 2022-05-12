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
using System.Linq;
using System.Threading.Tasks;
using Processing.Application.Common;
using Processing.Application.MoveIn;
using Processing.Domain.BusinessProcesses.MoveIn.Errors;
using Processing.Domain.Consumers;
using Processing.Domain.EnergySuppliers.Errors;
using Processing.Domain.MeteringPoints.Errors;
using Processing.Domain.SeedWork;
using Xunit;
using Xunit.Categories;
using Consumer = Processing.Application.MoveIn.Consumer;

namespace Processing.IntegrationTests.Application.MoveIn
{
    [IntegrationTest]
    public class MoveInTests : TestHost
    {
        public MoveInTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Consumer_identifier_is_required()
        {
            var request = CreateRequest() with
            {
                Consumer = new Consumer("ConsumerName", string.Empty),
            };

            var result = await SendRequestAsync(request).ConfigureAwait(false);

            AssertValidationError<ConsumerIdentifierIsRequired>(result, "ConsumerIdentifierIsRequired");
        }

        [Fact]
        public async Task Consumer_name_is_required()
        {
            var request = CreateRequest() with
            {
                Consumer = new Consumer(),
            };

            var result = await SendRequestAsync(request).ConfigureAwait(false);

            AssertValidationError<ConsumerNameIsRequired>(result, "ConsumerNameIsRequired");
        }

        [Fact]
        public async Task Energy_supplier_must_be_known()
        {
            CreateAccountingPoint();
            SaveChanges();

            var request = CreateRequest();

            var result = await SendRequestAsync(request).ConfigureAwait(false);

            AssertValidationError<UnknownEnergySupplier>(result, "UnknownEnergySupplier");
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Accounting_point_must_exist()
        {
            CreateEnergySupplier(Guid.NewGuid(), SampleData.GlnNumber);
            SaveChanges();

            var request = CreateRequest();

            var result = await SendRequestAsync(request).ConfigureAwait(false);

            Assert.False(result.Success);
            AssertValidationError<UnknownAccountingPoint>(result, "UnknownAccountingPoint");
        }

        [Fact]
        public async Task Accept_WhenConsumerIsRegisteredBySSN_ConsumerIsRegistered()
        {
            CreateEnergySupplier(Guid.NewGuid(), SampleData.GlnNumber);
            CreateAccountingPoint();
            SaveChanges();

            var request = CreateRequest();
            await SendRequestAsync(request).ConfigureAwait(false);

            var consumer = await GetService<IConsumerRepository>().GetBySSNAsync(CprNumber.Create(request.Consumer.Identifier)).ConfigureAwait(false);
            Assert.NotNull(consumer);
        }

        [Fact]
        public async Task Accept_WhenConsumerIsRegisteredByVAT_ConsumerIsRegistered()
        {
            CreateEnergySupplier(Guid.NewGuid(), SampleData.GlnNumber);
            CreateAccountingPoint();
            SaveChanges();

            var request = CreateRequest(false);
            await SendRequestAsync(request).ConfigureAwait(false);

            var consumer = await GetService<IConsumerRepository>().GetByVATNumberAsync(CvrNumber.Create(request.Consumer.Identifier)).ConfigureAwait(false);
            Assert.NotNull(consumer);
        }

        [Fact]
        public async Task Move_in_on_top_of_move_in_should_result_in_reject_message()
        {
            CreateEnergySupplier();
            CreateAccountingPoint();
            SaveChanges();

            var request = CreateRequest(false);
            await SendRequestAsync(request).ConfigureAwait(false);
            await SendRequestAsync(request).ConfigureAwait(false);
        }

        private static void AssertValidationError<TRuleError>(BusinessProcessResult rulesValidationResult, string? expectedErrorCode = null, bool errorExpected = true)
        where TRuleError : ValidationError
        {
            if (rulesValidationResult == null) throw new ArgumentNullException(nameof(rulesValidationResult));
            var error = rulesValidationResult.ValidationErrors.FirstOrDefault(error => error is TRuleError);
            Assert.NotNull(error);
            if (expectedErrorCode is not null)
            {
                Assert.Equal(expectedErrorCode, error?.Code);
            }
        }

        private static MoveInRequest CreateRequest(bool registerConsumerBySSN = true)
        {
            var consumerIdType = registerConsumerBySSN ? ConsumerIdentifierType.CPR : ConsumerIdentifierType.CVR;
            var consumerId = consumerIdType == ConsumerIdentifierType.CPR ? SampleData.ConsumerSSN : SampleData.ConsumerVAT;

            return new MoveInRequest(
                new Consumer(SampleData.ConsumerName, consumerId, consumerIdType),
                SampleData.Transaction,
                SampleData.GlnNumber,
                SampleData.GsrnNumber,
                SampleData.MoveInDate);
        }
    }
}
