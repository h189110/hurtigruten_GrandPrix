import type { FreshnessStatus } from '../../types/ship';

type ShipStatusProps = {
  status: FreshnessStatus;
  dataAgeSeconds: number | null;
};

const LABELS: Record<FreshnessStatus, string> = {
  Fresh: 'FRESH AIS',
  Delayed: 'DELAYED AIS',
  Stale: 'STALE AIS',
  Unavailable: 'UNAVAILABLE',
};

function formatAge(seconds: number | null): string {
  if (seconds === null) return 'No observation';
  if (seconds < 60) return `Updated ${seconds} seconds ago`;
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `Updated ${minutes} min ago`;
  return `Updated ${Math.floor(minutes / 60)} h ago`;
}

export function ShipStatusBadge({ status, dataAgeSeconds }: ShipStatusProps) {
  // Fresh is the default state, so only the age is shown for it.
  if (status === 'Fresh') {
    return (
      <p className="ship-status ship-status--fresh">
        <span className="ship-status__age">{formatAge(dataAgeSeconds)}</span>
      </p>
    );
  }

  return (
    <p className={`ship-status ship-status--${status.toLowerCase()}`}>
      <span className="ship-status__dot" aria-hidden="true" />
      <span className="ship-status__label">{LABELS[status]}</span>
      <span className="ship-status__separator" aria-hidden="true">·</span>
      <span className="ship-status__age">{formatAge(dataAgeSeconds)}</span>
    </p>
  );
}
