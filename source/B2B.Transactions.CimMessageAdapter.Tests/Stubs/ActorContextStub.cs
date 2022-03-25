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
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;

namespace B2B.CimMessageAdapter.Tests.Stubs
{
    public class ActorContextStub : IActorContext
    {
        private readonly Actor _validActor = new Actor(Guid.NewGuid(), "GLN", "5799999933318", string.Empty);

        public ActorContextStub()
        {
            SetValidActor();
        }

        public Actor? CurrentActor { get; set; }

        public Actor DataHub { get; } = new Actor(Guid.Empty, "GLN", "Fake", string.Empty);

        public void SetValidActor()
        {
            CurrentActor = _validActor;
        }

        public void UseInvalidActor()
        {
            CurrentActor = new Actor(Guid.Empty, "GLN", "Fake", string.Empty);
        }
    }
}