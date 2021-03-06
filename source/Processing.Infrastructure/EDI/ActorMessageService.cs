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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using NodaTime;
using Processing.Application.EDI;
using Processing.Domain.Actors;
using Processing.Domain.SeedWork;
using Processing.Infrastructure.EDI.Acknowledgements;
using Processing.Infrastructure.EDI.Common;
using Processing.Infrastructure.EDI.Extensions;
using Processing.Infrastructure.EDI.GenericNotification;
using Actor = Energinet.DataHub.Core.App.Common.Abstractions.Actor.Actor;
using IdentificationType = Energinet.DataHub.Core.App.Common.Abstractions.Actor.IdentificationType;

namespace Processing.Infrastructure.EDI
{
    public class ActorMessageService : IActorMessageService
    {
        private readonly ISystemDateTimeProvider _dateTimeProvider;
        private readonly IMessageHubDispatcher _messageHubDispatcher;
        private readonly IActorContext _actorContext;

        public ActorMessageService(
            ISystemDateTimeProvider dateTimeProvider,
            IMessageHubDispatcher messageHubDispatcher,
            IActorContext actorContext)
        {
            _dateTimeProvider = dateTimeProvider;
            _messageHubDispatcher = messageHubDispatcher;
            _actorContext = actorContext;
        }

        public async Task SendGenericNotificationMessageAsync(string transactionId, string gsrn, Instant startDateAndOrTime, string receiverGln)
        {
            var message = GenericNotificationMessageFactory.GenericNotification(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator),
                receiver: new MarketRoleParticipant(receiverGln, "A10", "DDQ"), // TODO: Re-visit when actor context has been implemented properly
                createdDateTime: _dateTimeProvider.Now(),
                gsrn,
                startDateAndOrTime,
                transactionId);

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.GenericNotification, receiverGln, gsrn).ConfigureAwait(false);
        }

        public async Task SendChangeOfSupplierConfirmAsync(string transactionId, string gsrn)
        {
            if (_actorContext.CurrentActor is null) throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = ConfirmMessageFactory.ChangeOfSupplier(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator), // TODO: verify roles
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new Acknowledgements.MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.ConfirmChangeOfSupplier, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendChangeOfSupplierRejectAsync(string transactionId, string gsrn, IEnumerable<ErrorMessage> errors)
        {
            if (_actorContext.CurrentActor is null) throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = RejectMessageFactory.ChangeOfSupplier(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator), // TODO: verify roles
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectChangeOfSupplier, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendMoveInConfirmAsync(string transactionId, string gsrn)
        {
            if (_actorContext.CurrentActor is null) throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = ConfirmMessageFactory.MoveIn(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator), // TODO: verify roles
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new Acknowledgements.MarketActivityRecord(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.ConfirmMoveIn, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        public async Task SendMoveInRejectAsync(string transactionId, string gsrn, IEnumerable<ErrorMessage> errors)
        {
            if (_actorContext.CurrentActor is null) throw new InvalidOperationException("Can't create message when current actor is not set (null)");

            var message = RejectMessageFactory.MoveIn(
                sender: Map(_actorContext.DataHub, Role.MeteringPointAdministrator), // TODO: verify roles
                receiver: Map(_actorContext.CurrentActor, Role.GridAccessProvider),
                createdDateTime: _dateTimeProvider.Now(),
                marketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: Guid.NewGuid().ToString(),
                    MarketEvaluationPoint: gsrn,
                    OriginalTransaction: transactionId,
                    Reasons: errors.Select(error => new Reason(error.Code, error.Description)).ToList()));

            await _messageHubDispatcher.DispatchAsync(message, DocumentType.RejectMoveIn, _actorContext.CurrentActor.Identifier, gsrn).ConfigureAwait(false);
        }

        private static MarketRoleParticipant Map(Actor actor, Role documentRole)
        {
            var codingScheme = actor.IdentificationType.ToUpperInvariant() switch
            {
                nameof(IdentificationType.GLN) => "A10",
                nameof(IdentificationType.EIC) => "A01",
                _ => throw new InvalidOperationException($"Unknown party identifier type: {actor.IdentificationType}"),
            };

            var currentRole = actor.GetRole(documentRole);
            var role = currentRole.Name switch
            {
                nameof(Role.MeteringPointAdministrator) => "DDZ",
                nameof(Role.GridAccessProvider) => "DDM",
                nameof(Role.BalancePowerSupplier) => "DDQ",
                nameof(Role.SystemOperator) => "EZ",
                nameof(Role.MeteredDataResponsible) => "MDR",
                _ => throw new InvalidOperationException($"Unknown party role: {currentRole.Name}"),
            };

            return new MarketRoleParticipant(actor.Identifier, codingScheme, role);
        }
    }
}
