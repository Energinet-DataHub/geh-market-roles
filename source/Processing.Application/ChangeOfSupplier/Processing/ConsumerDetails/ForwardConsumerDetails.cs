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
using Processing.Application.Common.Commands;

namespace Processing.Application.ChangeOfSupplier.Processing.ConsumerDetails
{
    public class ForwardConsumerDetails : InternalCommand
    {
        public ForwardConsumerDetails(Guid accountingPointId, Guid businessProcessId, string transaction)
        {
            AccountingPointId = accountingPointId;
            BusinessProcessId = businessProcessId;
            Transaction = transaction;
        }

        public ForwardConsumerDetails(Guid id, Guid accountingPointId, Guid businessProcessId, string transaction)
        {
            AccountingPointId = accountingPointId;
            BusinessProcessId = businessProcessId;
            Transaction = transaction;
            Id = id;
        }

        public Guid AccountingPointId { get; }

        public Guid BusinessProcessId { get; }

        public string Transaction { get; }
    }
}