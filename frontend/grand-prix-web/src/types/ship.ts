export type FreshnessStatus = 'Fresh' | 'Delayed' | 'Stale' | 'Unavailable';

export interface Ship {
  name: string;
  mmsi: number;
  imo: number;
  speedKnots: number | null;
  latitude: number | null;
  longitude: number | null;
  courseOverGround: number | null;
  trueHeading: number | null;
  sourceTimestampUtc: string | null;
  dataAgeSeconds: number | null;
  status: FreshnessStatus;
  rank: number | null;
}

export interface ShipStatus {
  upstreamReachable: boolean;
  upstreamStatus: string;
  lastSuccessfulFetchUtc: string | null;
  lastAttemptedFetchUtc: string | null;
  pollingIntervalSeconds: number;
  featuresReceived: number;
  matchedVesselCount: number;
  vesselsDetected: number;
  identityMismatchCount: number;
  freshCount: number;
  delayedCount: number;
  staleCount: number;
  unavailableCount: number;
  lastError: string | null;
}
