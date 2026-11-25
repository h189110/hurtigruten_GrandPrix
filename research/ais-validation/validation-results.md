### Validation run

- Date/time (UTC): 2026-11-25 15:12–15:14 (three real-time GeoJSON requests: about 15:12, 15:13:54, and 15:14)
- Endpoint: `GET https://kystdatahuset.kystverket.no/ws/api/ais/realtime/geojson` (anonymous, no credentials)
- Feed size: 3,736 features (first request), 3,738 features (second request)

### Field mapping (observed in the response)

- MMSI: `features[].properties.mmsi` (integer)
- Name: `features[].properties.ship_name` (string)
- IMO: `features[].properties.imo` (integer)
- SOG: `features[].properties.speed` (decimal; unit not stated in the Swagger spec, so knots is assumed, not confirmed)
- COG / heading: `properties.cog` (decimal), `properties.true_heading` (integer, 511 = not available)
- Timestamp: `properties.date_time_utc` (ISO string, UTC, no timezone suffix)
- Position: `geometry.coordinates` (LineString, `[lon, lat]`); the last point is the latest position

### Results per vessel

| Vessel | IMO | MMSI | Identity | AIS detected | SOG (latest) | Position (lat, lon) | Data age | Validation | Notes |
|---|---|---|---|---|---|---|---|---|---|
| MS Nordlys | 9048914 | 259139000 | Verified | Yes | 0.0 (15:09:04) | 62.47571, 6.15242 | ~5 min at 15:14 | Partial | Position and timestamp identical across both snapshots. Separate NORDLYS (MMSI 258208440, IMO 0) exists in the feed and was excluded. |
| MS Vesterålen | 8019368 | 258478000 | Verified | Yes | 15.5 (15:13:47) | 67.36767, 14.4101 | ~7 s at 15:13:54 | Pass | SOG changed from 15.7 to 15.5 and position moved between snapshots. |
| MS Nordnorge | 9107784 | 259371000 | Verified | Yes | 11.2 (15:13:50) | 70.27491, 31.07406 | ~4 s at 15:13:54 | Pass | SOG changed from 11.5 to 11.2 and position moved between snapshots. |
| MS Richard With | 9040429 | 258500000 | Unverified | Yes | 0.0 (15:11:47) | 62.44141, 6.27966 | ~2 min at 15:13:54 | Partial | Single external registry source. Timestamp unchanged across snapshots, so the 0.0 may be stale. |
| MS Kong Harald | 9039119 | 257200000 | Unverified | Yes | 0.0 (15:14:09) | 69.64652, 18.9605 | ~0 s at 15:14:09 | Partial | No external source found. Identity rests on the feed alone, which is not independent. |
| MS Polarlys | 9107796 | 259322000 | Verified | Yes | 0.0 (15:11:53) | 65.47594, 12.21068 | ~2 min at 15:13:54 | Partial | Timestamp unchanged across snapshots; the 0.0 may be stale. |
| MS Nordkapp | 9107772 | 259330000 | Verified | Yes | 0.0 (15:11:56) | 60.3928, 5.30996 | ~2 min at 15:13:54 | Partial | Timestamp unchanged across snapshots; the 0.0 may be stale. Other ships named "NORDKAPP" (DEEPSEA NORDKAPP, FOSNA NORDKAPP) were excluded by IMO. |

### Summary

- Vessels in scope: 7
- MMSI identifiers verified against two or more sources: 6 of 7 (Richard With has one external source)
- Vessels present in the real-time feed: 7 of 7, with matching name, MMSI, and IMO
- Vessels with valid numeric speed: 7 of 7 (all numeric; 2 show clearly changing speed)
- Vessels with valid coordinates: 7 of 7 (all within lat/lon bounds)
- Freshness: the feed exposes `date_time_utc`, so age can be calculated. Ships that are stationary report older timestamps, and the 10-minute default window means some results may be near expiry.

### Notes

- The SOG unit (knots) is not stated in the API documentation. Values are plausible for knots (Vesterålen about 15 kn, close to its service speed).
- Vessels with `speed` 0.0 and unchanged timestamps across requests were treated as stale-or-stationary, not as confirmed stopped.
