# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

module "snet_internal_private_endpoints" {
  address_prefixes = ["10.140.98.0/27"]
}

module "snet_external_private_endpoints" {
  address_prefixes = ["10.140.98.32/28"]
}

module "vnet_integrations_functions" {
  address_prefixes = ["10.140.98.48/28"]
}