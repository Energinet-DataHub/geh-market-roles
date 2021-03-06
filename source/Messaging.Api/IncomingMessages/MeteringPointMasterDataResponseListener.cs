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
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.RequestResponse.Response;
using Messaging.Application.Configuration;
using Messaging.Application.MasterData;
using Messaging.Application.Transactions.MoveIn;
using Messaging.Infrastructure.Configuration.DataAccess;
using Messaging.Infrastructure.Configuration.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Messaging.Api.IncomingMessages;

public class MeteringPointMasterDataResponseListener
{
    private readonly ILogger<MeteringPointMasterDataResponseListener> _logger;
    private readonly ISerializer _serializer;
    private readonly ICommandScheduler _commandScheduler;
    private readonly B2BContext _context;

    public MeteringPointMasterDataResponseListener(
        ILogger<MeteringPointMasterDataResponseListener> logger,
        ISerializer serializer,
        ICommandScheduler commandScheduler,
        B2BContext context)
    {
        _logger = logger;
        _serializer = serializer;
        _commandScheduler = commandScheduler;
        _context = context;
    }

    [Function("MeteringPointMasterDataResponseListener")]
    public async Task RunAsync([ServiceBusTrigger("%METERING_POINT_MASTER_DATA_RESPONSE_QUEUE_NAME%", Connection = "SERVICE_BUS_CONNECTION_STRING_FOR_INTEGRATION_EVENTS_LISTENER")] byte[] data, FunctionContext context)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var metadata = GetMetaData(context);
        var masterDataContent = GetMasterDataContent(MasterDataRequestResponse.Parser.ParseFrom(data));

        var forwardMeteringPointMasterData = new ForwardMeteringPointMasterData(
            metadata.TransactionId ?? throw new InvalidOperationException("Service bus metadata property TransactionId is missing"),
            masterDataContent);
        await _commandScheduler.EnqueueAsync(forwardMeteringPointMasterData).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);
        _logger.LogInformation($"Master data response received: {data}");
    }

    private static MasterDataContent GetMasterDataContent(MasterDataRequestResponse masterData)
    {
        var address = new Application.MasterData.Address(
            masterData.Address.StreetName,
            StreetCode: masterData.Address.StreetCode,
            PostCode: masterData.Address.PostCode,
            City: masterData.Address.City,
            CountryCode: masterData.Address.CountryCode,
            CitySubDivision: masterData.Address.CitySubDivision,
            Floor: masterData.Address.Floor,
            Room: masterData.Address.Room,
            BuildingNumber: masterData.Address.BuildingNumber,
            MunicipalityCode: masterData.Address.MunicipalityCode,
            IsActualAddress: masterData.Address.IsActualAddress,
            GeoInfoReference: Guid.Parse(masterData.Address.GeoInfoReference),
            LocationDescription: masterData.Address.LocationDescription);

        return new MasterDataContent(
            masterData.GsrnNumber,
            address,
            new Application.MasterData.Series(masterData.Series.Product, masterData.Series.UnitType),
            new Application.MasterData.GridAreaDetails(masterData.GridAreaDetails.Code, masterData.GridAreaDetails.ToCode, masterData.GridAreaDetails.FromCode),
            masterData.ConnectionState,
            masterData.MeteringMethod,
            masterData.ReadingPeriodicity,
            masterData.Type,
            masterData.MaximumCurrent,
            masterData.MaximumPower,
            masterData.PowerPlantGsrnNumber,
            masterData.EffectiveDate.ToDateTime(),
            masterData.MeterNumber,
            masterData.Capacity,
            masterData.AssetType,
            masterData.SettlementMethod,
            masterData.ScheduledMeterReadingDate,
            masterData.ProductionObligation,
            masterData.NetSettlementGroup,
            masterData.DisconnetionType,
            masterData.ConnectionType,
            masterData.ParentRelatedMeteringPoint,
            masterData.GridOperatorId);
    }

    private MasterDataResponseMetadata GetMetaData(FunctionContext context)
    {
        context.BindingContext.BindingData.TryGetValue("UserProperties", out var metadata);

        if (metadata is null)
        {
            throw new InvalidOperationException($"Service bus metadata must be specified as User Properties attributes");
        }

        return _serializer.Deserialize<MasterDataResponseMetadata>(metadata.ToString() ?? throw new InvalidOperationException());
    }
}
