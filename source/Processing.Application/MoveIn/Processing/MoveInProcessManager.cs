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

using NodaTime;
using Processing.Application.Common.InternalCommands;
using Processing.Application.Common.Processing;
using Processing.Domain.AccountingPoints;
using Processing.Domain.AccountingPoints.Events;
using Processing.Domain.SeedWork;

namespace Processing.Application.MoveIn.Processing
{
    public class MoveInProcessManager : ProcessManager
    {
        private State _state;

        public MoveInProcessManager()
            : base()
        {
            SetInternalState(State.NotStarted);
        }

        public enum State
        {
            NotStarted,
            AwaitingEffectuation,
            Completed,
        }

        public void When(ConsumerMoveInAccepted @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            switch (_state)
            {
                case State.NotStarted:
                    EffectiveDate = @event.MoveInDate;
                    BusinessProcessId = BusinessProcessId.Create(@event.BusinessProcessId);
                    SetInternalState(State.AwaitingEffectuation);
                    ScheduleEffectuation(@event.AccountingPointId, @event.Transaction);
                    break;
                default:
                    ThrowIfStateDoesNotMatch(@event);
                    break;
            }
        }

        public void When(ConsumerMovedIn @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            switch (_state)
            {
                case State.AwaitingEffectuation:
                    SetInternalState(State.Completed);
                    break;
                default:
                    ThrowIfStateDoesNotMatch(@event);
                    break;
            }
        }

        public override bool IsCompleted()
        {
            return _state == State.Completed;
        }

        private void ScheduleEffectuation(Guid accountingPointId, string transaction)
        {
            SendCommand(new EffectuateConsumerMoveIn(accountingPointId, transaction), EffectiveDate);
        }

        private void ThrowIfStateDoesNotMatch(DomainEvent @event)
        {
            throw new InvalidProcessManagerStateException($"The event of {@event.GetType().Name} is not applicable when state is {_state.ToString()}.");
        }

        private void SetInternalState(State state)
        {
            _state = state;
        }

        private void SendCommand(InternalCommand internalCommand, Instant? executionDate = null)
        {
            CommandsToSend.Add(new EnqueuedCommand(internalCommand, BusinessProcessId, executionDate));
        }
    }
}
