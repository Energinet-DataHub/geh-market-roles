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
using System.Threading.Tasks;
using Processing.Application.AccountingPoints;
using Processing.Application.Customers.GetCustomerMasterData;
using Processing.Application.MoveIn;
using Processing.Domain.EnergySuppliers;
using Processing.Domain.MeteringPoints;
using Xunit;

namespace Processing.IntegrationTests.Application.Customers.GetCustomerMasterData
{
    public class GetCustomerMasterDataTests : TestHost
    {
        public GetCustomerMasterDataTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Get_customer_master_data_test()
        {
            var processId = await GivenAMoveInProcessHasBeenStarted().ConfigureAwait(false);

            var query = new GetCustomerMasterDataQuery(Guid.Parse(processId));
            var result = await QueryAsync(query).ConfigureAwait(false);

            Assert.Equal(result.Data?.RegisteredByProcessId.ToString(), processId);
            Assert.Equal(SampleData.CustomerId, result.Data?.CustomerId);
            Assert.Equal(SampleData.CustomerName, result.Data?.CustomerName);
            Assert.Equal(SampleData.CustomerIdType, result.Data?.CustomerIdType);
            Assert.Equal("01/01/0001 00:00:00", result.Data?.ElectricalHeatingEffectiveDate.ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public async Task Return_process_id_must_exist()
        {
            var processId = Guid.NewGuid();

            var query = new GetCustomerMasterDataQuery(processId);
            var result = await QueryAsync(query).ConfigureAwait(false);

            Assert.NotEmpty(result.Error);
        }

        private async Task<string> GivenAMoveInProcessHasBeenStarted()
        {
            GetService<IEnergySupplierRepository>().Add(new EnergySupplier(EnergySupplierId.New(), GlnNumber.Create(SampleData.EnergySupplierNumber)));
            await InvokeCommandAsync(new CreateAccountingPoint(SampleData.MeteringPointId, SampleData.GsrnNumber, MeteringPointType.Consumption.Name)).ConfigureAwait(false);
            var result = await SendRequestAsync(new MoveInRequest(new Consumer(SampleData.CustomerName, SampleData.CustomerId, SampleData.CustomerIdType), SampleData.EnergySupplierNumber, SampleData.GsrnNumber, SampleData.MoveInDate));
            return result.ProcessId;
        }
    }
}
