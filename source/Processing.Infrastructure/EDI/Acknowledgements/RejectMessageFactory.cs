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
using NodaTime;
using Processing.Infrastructure.EDI.Common;

namespace Processing.Infrastructure.EDI.Acknowledgements
{
    public static class RejectMessageFactory
    {
        public static RejectMessage ChangeOfSupplier(
            MarketRoleParticipant sender,
            MarketRoleParticipant receiver,
            Instant createdDateTime,
            MarketActivityRecordWithReasons marketActivityRecord)
        {
            return Defaults()
                with
                {
                    DocumentName = "RejectRequestChangeOfSupplier_MarketDocument",
                    Type = "D16",
                    ProcessType = "E03",
                    Sender = sender,
                    Receiver = receiver,
                    CreatedDateTime = createdDateTime,
                    MarketActivityRecord = marketActivityRecord,
                };
        }

        public static RejectMessage MoveIn(
            MarketRoleParticipant sender,
            MarketRoleParticipant receiver,
            Instant createdDateTime,
            MarketActivityRecordWithReasons marketActivityRecord)
        {
            return Defaults()
                with
                {
                    DocumentName = "RejectRequestChangeCustomerCharacteristics_MarketDocument",
                    Type = "D16",
                    ProcessType = "E03",
                    Sender = sender,
                    Receiver = receiver,
                    CreatedDateTime = createdDateTime,
                    MarketActivityRecord = marketActivityRecord,
                };
        }

        private static RejectMessage Defaults()
        {
            return new RejectMessage(
                DocumentName: string.Empty,
                Id: Guid.NewGuid().ToString(),
                Type: "E59", // Changes with the document type. eg E59 for ConfirmRequestChangeAccountingPointCharacteristics_MarketDocument
                ProcessType: string.Empty, // Changes with BRS, eg D15 for connect
                BusinessSectorType: "23", // Electricity
                Sender: new MarketRoleParticipant(
                    Id: string.Empty,
                    CodingScheme: string.Empty,
                    Role: string.Empty),
                Receiver: new MarketRoleParticipant(
                    Id: string.Empty,
                    CodingScheme: string.Empty,
                    Role: string.Empty),
                CreatedDateTime: Instant.MinValue,
                ReasonCode: "A02", // Reject
                MarketActivityRecord: new MarketActivityRecordWithReasons(
                    Id: string.Empty,
                    OriginalTransaction: string.Empty,
                    MarketEvaluationPoint: string.Empty,
                    Array.Empty<Reason>()));
        }
    }
}
