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
using System.Threading.Tasks;
using Energinet.DataHub.MarketRoles.ActorRegistrySync.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MarketRoles.ActorRegistrySync;

public class SyncActors : IDisposable
{
    // private static IEnumerable<UserActor>? _userActors;
    // private static IEnumerable<Actor>? _actors;
    private readonly ActorSyncService _actorSyncService;

    public SyncActors()
    {
        _actorSyncService = new ActorSyncService();
    }

    [FunctionName("SyncActors")]
    public async Task RunAsync([TimerTrigger("%TIMER_TRIGGER%")] TimerInfo someTimer, ILogger log)
    {
        log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
        await SyncActorsFromExternalSourceToDbAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _actorSyncService.Dispose();
        }
    }

    private async Task SyncActorsFromExternalSourceToDbAsync()
    {
        var actors = (await _actorSyncService.GetActorsAsync().ConfigureAwait(false)).ToList();
        var energySuppliers = ActorSyncService.MapActorsToEnergySuppliers(actors).ToList();
        var supplierRegistrations = ActorSyncService.FilterObsoleteSupplierRegistrations(await _actorSyncService.GetSupplierRegistrationsAsync().ConfigureAwait(false), energySuppliers);

        await _actorSyncService.DatabaseCleanUpAsync().ConfigureAwait(false);
        await _actorSyncService.InsertActorsAsync(actors).ConfigureAwait(false);
        await _actorSyncService.InsertEnergySuppliersAsync(energySuppliers).ConfigureAwait(false);
        await _actorSyncService.InsertSupplierRegistrationsAsync(supplierRegistrations).ConfigureAwait(false);

        await _actorSyncService.CommitTransactionAsync().ConfigureAwait(false);
    }
}
