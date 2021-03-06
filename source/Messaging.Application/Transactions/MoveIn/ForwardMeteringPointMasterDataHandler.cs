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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Messaging.Application.Common;
using Messaging.Application.Configuration;
using Messaging.Application.MasterData;
using Messaging.Application.OutgoingMessages;
using Messaging.Application.OutgoingMessages.AccountingPointCharacteristics;
using Messaging.Domain.MasterData.Dictionaries;
using Messaging.Domain.OutgoingMessages;
using NodaTime.Extensions;
using Address = Messaging.Application.OutgoingMessages.AccountingPointCharacteristics.Address;
using Series = Messaging.Application.OutgoingMessages.AccountingPointCharacteristics.Series;

namespace Messaging.Application.Transactions.MoveIn;

public class ForwardMeteringPointMasterDataHandler : IRequestHandler<ForwardMeteringPointMasterData, Unit>
{
    private readonly IMoveInTransactionRepository _transactionRepository;
    private readonly IMarketActivityRecordParser _marketActivityRecordParser;
    private readonly IOutgoingMessageStore _outgoingMessageStore;

    public ForwardMeteringPointMasterDataHandler(
        IMoveInTransactionRepository transactionRepository,
        IMarketActivityRecordParser marketActivityRecordParser,
        IOutgoingMessageStore outgoingMessageStore)
    {
        _transactionRepository = transactionRepository;
        _marketActivityRecordParser = marketActivityRecordParser;
        _outgoingMessageStore = outgoingMessageStore;
    }

    public async Task<Unit> Handle(ForwardMeteringPointMasterData request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        var transaction = _transactionRepository.GetById(request.TransactionId);
        if (transaction is null)
        {
            throw new MoveInException($"Could not find move in transaction '{request.TransactionId}'");
        }

        _outgoingMessageStore.Add(AccountingPointCharacteristicsMessageFrom(request.MasterDataContent, transaction));

        transaction.HasForwardedMeteringPointMasterData();
        return await Task.FromResult(Unit.Value).ConfigureAwait(false);
    }

    private static MarketEvaluationPoint CreateMarketEvaluationPointFrom(
        MasterDataContent masterData,
        MoveInTransaction transaction)
    {
        var address = CreateAddress(masterData);

        return new MarketEvaluationPoint(
            new Mrid(transaction.MarketEvaluationPointId, "A10"),
            null,
            MasterDataTranslation.Translations[masterData.Type],
            MasterDataTranslation.Translations[masterData.SettlementMethod],
            MasterDataTranslation.Translations[masterData.MeteringMethod],
            MasterDataTranslation.Translations[masterData.ConnectionState],
            MasterDataTranslation.Translations[masterData.ReadingPeriodicity],
            MasterDataTranslation.Translations[masterData.NetSettlementGroup],
            MasterDataTranslation.TranslateToNextReadingDate(masterData.ScheduledMeterReadingDate),
            new Mrid(masterData.GridAreaDetails.Code, "NDK"),
            null,
            null,
            new Mrid(masterData.PowerPlantGsrnNumber, "A10"),
            new UnitValue(masterData.Capacity.ToString(CultureInfo.InvariantCulture), "KWT"),
            MasterDataTranslation.Translations[masterData.ConnectionType],
            MasterDataTranslation.Translations[masterData.DisconnectionType],
            MasterDataTranslation.Translations[masterData.AssetType],
            masterData.ProductionObligation.ToString(),
            new UnitValue(masterData.MaximumPower.ToString(CultureInfo.InvariantCulture), "KWT"),
            new UnitValue(masterData.MaximumCurrent.ToString(CultureInfo.InvariantCulture), "AMP"),
            masterData.MeterNumber,
            new Series(
                MasterDataTranslation.Translations[masterData.Series.Product],
                MasterDataTranslation.Translations[masterData.Series.UnitType]),
            new Mrid(transaction.CurrentEnergySupplierId!, "A10"),
            masterData.EffectiveDate.ToUniversalTime().ToInstant(),
            masterData.Address.LocationDescription,
            masterData.Address.GeoInfoReference.ToString(),
            address,
            masterData.Address.IsActualAddress.ToString(),
            string.IsNullOrEmpty(masterData.ParentMarketEvaluationPointId) ? null : new RelatedMarketEvaluationPoint(new Mrid(masterData.ParentMarketEvaluationPointId, "A10"), "description"),
            null);
    }

    private static Address CreateAddress(MasterDataContent masterData)
    {
        return new Address(
            new StreetDetail(
                masterData.Address!.StreetCode,
                masterData.Address.StreetName,
                masterData.Address.BuildingNumber,
                masterData.Address.Floor,
                masterData.Address.Room),
            new TownDetail(
                masterData.Address.MunicipalityCode.ToString(CultureInfo.InvariantCulture),
                masterData.Address.City,
                masterData.Address.CitySubDivision,
                masterData.Address.CountryCode),
            masterData.Address.PostCode);
    }

    private static OutgoingMessage CreateOutgoingMessage(string id, string documentType, string processType, string receiverId, string marketActivityRecordPayload)
    {
        return new OutgoingMessage(
            documentType,
            receiverId,
            Guid.NewGuid().ToString(),
            id,
            processType,
            MarketRoles.EnergySupplier,
            DataHubDetails.IdentificationNumber,
            MarketRoles.MeteringPointAdministrator,
            marketActivityRecordPayload,
            null);
    }

    private OutgoingMessage AccountingPointCharacteristicsMessageFrom(MasterDataContent masterData, MoveInTransaction transaction)
    {
        var marketEvaluationPoint = CreateMarketEvaluationPointFrom(masterData, transaction);
        var marketActivityRecord = new OutgoingMessages.AccountingPointCharacteristics.MarketActivityRecord(
            Guid.NewGuid().ToString(),
            null,
            transaction.EffectiveDate,
            marketEvaluationPoint);

        return CreateOutgoingMessage(
            transaction.StartedByMessageId,
            "AccountingPointCharacteristics",
            "E65",
            transaction.NewEnergySupplierId,
            _marketActivityRecordParser.From(marketActivityRecord));
    }
}
