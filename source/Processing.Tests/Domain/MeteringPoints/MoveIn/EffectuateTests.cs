// Copyright 2020 Energinet DataHub A/S
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
using NodaTime;
using Processing.Domain.Consumers;
using Processing.Domain.EnergySuppliers;
using Processing.Domain.MeteringPoints;
using Processing.Domain.MeteringPoints.Events;
using Xunit;
using Xunit.Categories;

namespace Processing.Tests.Domain.MeteringPoints.MoveIn
{
    [UnitTest]
    public class EffectuateTests
    {
        private readonly SystemDateTimeProviderStub _systemDateTimeProvider = new SystemDateTimeProviderStub();

        public EffectuateTests()
        {
            _systemDateTimeProvider.SetNow(Instant.FromUtc(2020, 1, 1, 0, 0));
        }

        [Fact]
        public void Effectuate_WhenAheadOfEffectiveDate_IsNotPossible()
        {
            var (accountingPoint, consumerId, energySupplierId, businessProcessId) = SetupScenario();
            var moveInDate = _systemDateTimeProvider.Now().Plus(Duration.FromDays(1));
            accountingPoint.AcceptConsumerMoveIn(consumerId, energySupplierId, moveInDate, businessProcessId);

            Assert.Throws<BusinessProcessException>(() =>
                accountingPoint.EffectuateConsumerMoveIn(businessProcessId, _systemDateTimeProvider.Now()));
        }

        [Fact]
        public void Effectuate_WhenProcessIdDoesNotExists_IsNotPossible()
        {
            var (accountingPoint, _, _, _) = SetupScenario();
            var nonExistingProcessId = BusinessProcessId.New();

            Assert.Throws<BusinessProcessException>(() =>
                accountingPoint.EffectuateConsumerMoveIn(nonExistingProcessId, _systemDateTimeProvider.Now()));
        }

        [Fact]
        public void Effectuate_WhenEffectiveDateIsDue_IsSuccessful()
        {
            var (accountingPoint, consumerId, energySupplierId, businessProcessId) = SetupScenario();
            var moveInDate = _systemDateTimeProvider.Now();
            accountingPoint.AcceptConsumerMoveIn(consumerId, energySupplierId, moveInDate, businessProcessId);

            accountingPoint.EffectuateConsumerMoveIn(businessProcessId, _systemDateTimeProvider.Now());

            Assert.Contains(accountingPoint.DomainEvents, @event => @event is EnergySupplierChanged);
            Assert.Contains(accountingPoint.DomainEvents, @event => @event is ConsumerMovedIn);

            var consumerMovedIn = accountingPoint.DomainEvents.FirstOrDefault(de => de is ConsumerMovedIn) as ConsumerMovedIn;

            if (consumerMovedIn != null) Assert.NotNull(consumerMovedIn.MoveInDate);
        }

        private static (AccountingPoint AccountingPoint, ConsumerId ConsumerId, EnergySupplierId EnergySupplierId, BusinessProcessId ProcessId) SetupScenario()
        {
            return (
                AccountingPoint.CreateConsumption(AccountingPointId.New(), GsrnNumber.Create(SampleData.GsrnNumber)),
                new ConsumerId(Guid.NewGuid()),
                new EnergySupplierId(Guid.NewGuid()),
                BusinessProcessId.New());
        }
    }
}
