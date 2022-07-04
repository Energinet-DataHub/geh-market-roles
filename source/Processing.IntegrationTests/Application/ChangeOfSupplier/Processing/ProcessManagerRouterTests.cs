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
using System.Threading;
using System.Threading.Tasks;
using Processing.Application.ChangeOfSupplier.Processing;
using Processing.Application.ChangeOfSupplier.Processing.ConsumerDetails;
using Processing.Application.ChangeOfSupplier.Processing.EndOfSupplyNotification;
using Processing.Application.ChangeOfSupplier.Processing.MeteringPointDetails;
using Processing.Domain.Consumers;
using Processing.Domain.EnergySuppliers;
using Processing.Domain.MeteringPoints;
using Processing.Domain.MeteringPoints.Events;
using Xunit;
using Xunit.Categories;

namespace Processing.IntegrationTests.Application.ChangeOfSupplier.Processing
{
    [IntegrationTest]
    public class ProcessManagerRouterTests : TestHost
    {
        private readonly ProcessManagerRouter _router;
        private readonly BusinessProcessId _businessProcessId;
        private readonly AccountingPoint _accountingPoint;
        private readonly Domain.EnergySuppliers.EnergySupplier _energySupplier;

        public ProcessManagerRouterTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            var consumer = CreateConsumer();
            _energySupplier = CreateEnergySupplier(Guid.NewGuid(), SampleData.GlnNumber);
            _accountingPoint = CreateAccountingPoint();
            _businessProcessId = BusinessProcessId.New();
            var newEnergySupplier = CreateEnergySupplier(Guid.NewGuid(), "7495563456235");
            SetConsumerMovedIn(_accountingPoint, consumer.ConsumerId, _energySupplier.EnergySupplierId);
            RegisterChangeOfSupplier(_accountingPoint, newEnergySupplier.EnergySupplierId, _businessProcessId);
            MarketRolesContext.SaveChanges();

            _router = new ProcessManagerRouter(ProcessManagerRepository, CommandScheduler);
        }

        [Fact]
        public async Task EnergySupplierChangeIsRegistered_WhenStateIsNotStarted_ForwardMasterDataDetailsCommandIsEnqueued()
        {
            await _router.Handle(CreateSupplierChangeRegisteredEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            var command = await GetEnqueuedCommandAsync<ForwardMeteringPointDetails>(_businessProcessId).ConfigureAwait(false);

            Assert.NotNull(command);
            Assert.Equal(_businessProcessId.Value, command?.BusinessProcessId);
        }

        [Fact]
        public async Task MeteringPointDetailsAreDispatched_WhenStateIsAwaitingMeteringPointDetailsDispatch_ForwardConsumerDetailsCommandIsEnqueued()
        {
            await _router.Handle(CreateSupplierChangeRegisteredEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new MeteringPointDetailsDispatched(_accountingPoint.Id, _businessProcessId), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            var command = await GetEnqueuedCommandAsync<ForwardConsumerDetails>(_businessProcessId).ConfigureAwait(false);
            Assert.NotNull(command);
            Assert.Equal(_businessProcessId.Value, command?.BusinessProcessId);
        }

        [Fact]
        public async Task ConsumerDetailsAreDispatched_WhenStateIsAwaitingConsumerDetailsDispatch_NotifyCurrentSupplierCommandIsEnqueued()
        {
            await _router.Handle(CreateSupplierChangeRegisteredEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new MeteringPointDetailsDispatched(_accountingPoint.Id, _businessProcessId), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new ConsumerDetailsDispatched(_accountingPoint.Id, _businessProcessId), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            var command = await GetEnqueuedCommandAsync<NotifyCurrentSupplier>(_businessProcessId).ConfigureAwait(false);
            Assert.NotNull(command);
            Assert.Equal(_businessProcessId.Value, command?.BusinessProcessId);
        }

        [Fact]
        public async Task CurrentSupplierIsNotified_WhenStateIsAwaitingCurrentSupplierNotification_ChangeSupplierCommandIsScheduled()
        {
            await _router.Handle(CreateSupplierChangeRegisteredEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new MeteringPointDetailsDispatched(_accountingPoint.Id, _businessProcessId), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new ConsumerDetailsDispatched(_accountingPoint.Id, _businessProcessId), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new CurrentSupplierNotified(_accountingPoint.Id, _businessProcessId), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            var command = await GetEnqueuedCommandAsync<ChangeSupplier>(_businessProcessId).ConfigureAwait(false);
            Assert.NotNull(command);
            Assert.Equal(_accountingPoint.Id.Value, command?.AccountingPointId);
        }

        [Fact]
        public async Task SupplierIsChanged_WhenStateIsAwaitingSupplierChange_ProcessIsCompleted()
        {
            await _router.Handle(CreateSupplierChangeRegisteredEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new MeteringPointDetailsDispatched(_accountingPoint.Id, _businessProcessId), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new ConsumerDetailsDispatched(_accountingPoint.Id, _businessProcessId), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new CurrentSupplierNotified(_accountingPoint.Id, _businessProcessId), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(CreateEnergySupplierChangedEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            var processManager = await ProcessManagerRepository.GetAsync<ChangeOfSupplierProcessManager>(_businessProcessId).ConfigureAwait(false);
            Assert.True(processManager?.IsCompleted());
        }

        private EnergySupplierChangeRegistered CreateSupplierChangeRegisteredEvent()
        {
            return new EnergySupplierChangeRegistered(
                _accountingPoint.Id,
                _accountingPoint.GsrnNumber,
                _businessProcessId,
                EffectiveDate,
                _energySupplier.EnergySupplierId);
        }

        private EnergySupplierChanged CreateEnergySupplierChangedEvent()
        {
            return new EnergySupplierChanged(
                _accountingPoint.Id.Value,
                _accountingPoint.GsrnNumber.Value,
                _businessProcessId.Value,
                _energySupplier.EnergySupplierId.Value,
                EffectiveDate);
        }
    }
}