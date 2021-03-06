# Development notes for integration events

Notes regarding the development of the NuGet package bundle `IntegrationEvents`.

The bundle contains the following packages:

* `Energinet.DataHub.EnergySupplying.IntegrationEvents`

The package contains integration event classes for energy supplying domain (formerly market roles)

## Workflows

### `integration-events-publish.yml`

This workflow handles test, build, pack and publish of the bundle.

If any of the packages in the bundle has changes, both currently must be updated with regards to version.

Before publishing anything an action verifies that there is no released version existing with the current version number. This is to help detect if we forgot to update the version number in packages.

If the workflow is triggered:

* Manually (`workflow_dispatch`), a prerelease version of the packages are published.
* By `pull_request`, then the packages are not published.
* By `push` to main, the a release version of the packages are published.

## Updating integration events

If the integration event proto files are updated, building `Energinet.DataHub.MarketRoles` solution will compile them and put them in `/integrationevents/source/integrationevents/` folder.

License comments must be applied to each file if not present.
