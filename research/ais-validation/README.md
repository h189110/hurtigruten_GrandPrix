# Hurtigruten Coastal Express AIS Validation

Research only. This folder records which Hurtigruten Coastal Express vessels can be identified and tracked in Kystverket's real-time AIS feed. No application code was changed.

### Files

- `vessels.json`: vessel registry with IMO, MMSI, sources, and verification flags.
- `api-response-sample.json`: one real feature from `GET /api/ais/realtime/geojson`, copied unchanged. The geometry is the real two-point LineString.
- `validation-results.md`: per-vessel results from two real-time snapshots (2026-11-25, about 15:12–15:14 UTC).

### Method

- Fleet scope: the seven ships listed as Coastal Express (Vesterålen, Kong Harald, Richard With, Nordlys, Nordkapp, Nordnorge, Polarlys). Expedition ships, Signature ships, and former vessels were excluded.
- Identity: MMSI and IMO were taken from external ship registries and matched against the feed by exact MMSI and IMO.
- Real-time check: anonymous requests to the GeoJSON endpoint, no credentials. Speed, position, and `date_time_utc` were read for each matched feature.

### Limitations

- The Hurtigruten ship-locator page and the Swagger documentation could not be parsed by the fetch tool. The Swagger endpoints list was read from the raw JSON instead.
- The SOG unit (knots) is not stated in the documentation. Values are assumed to be knots and marked as such.
- Richard With and Kong Harald have only one external source each. Kong Harald's identity rests on the feed alone.
- Stationary ships report 0.0 with older timestamps. This does not confirm the vessel is stopped.
- The static-information POST endpoints were not exercised; the search was limited to the real-time feed and external registries.
