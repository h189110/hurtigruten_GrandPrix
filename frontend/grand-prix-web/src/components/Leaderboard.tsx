import { useMemo } from 'react';
import { LayoutGroup } from 'motion/react';
import { useShips } from '../hooks/useShips';
import { SHIP_IMAGES } from '../assets/shipImages';
import { ShipCard } from './leaderboard/ShipCard';
import type { FreshnessStatus, Ship } from '../types/ship';

const UNRANKED_ORDER: Record<FreshnessStatus, number> = {
  Fresh: 0,
  Delayed: 0,
  Stale: 1,
  Unavailable: 2,
};

function sortForLeaderboard(ships: Ship[]): Ship[] {
  return [...ships].sort((a, b) => {
    const aRanked = a.rank !== null;
    const bRanked = b.rank !== null;
    if (aRanked && bRanked) return a.rank! - b.rank!;
    if (aRanked !== bRanked) return aRanked ? -1 : 1;
    const byStatus = UNRANKED_ORDER[a.status] - UNRANKED_ORDER[b.status];
    if (byStatus !== 0) return byStatus;
    return a.name.localeCompare(b.name);
  });
}

export function Leaderboard() {
  const { ships, isLoading, error, isStale } = useShips();

  const sortedShips = useMemo(() => sortForLeaderboard(ships), [ships]);
  const hasRanked = ships.some((ship) => ship.rank !== null);

  return (
    <section className="leaderboard" aria-labelledby="leaderboard-title">
      <h2 id="leaderboard-title" className="visually-hidden">
        Leaderboard
      </h2>

      {isLoading && <p className="leaderboard-empty">Loading live AIS data</p>}

      {error && (
        <p className="leaderboard-warning" role="alert">
          Could not reach the backend ({error}).
          {isStale && ' Showing the last received data, which may be outdated.'}
        </p>
      )}

      {!isLoading && !error && ships.length === 0 && (
        <p className="leaderboard-empty">No vessels available yet.</p>
      )}

      {!isLoading && ships.length > 0 && !hasRanked && (
        <p className="leaderboard-empty">No vessels have fresh observations right now.</p>
      )}

      {sortedShips.length > 0 && (
        <LayoutGroup>
          <ol className="leaderboard-list">
            {sortedShips.map((ship) => (
              <ShipCard
                key={ship.mmsi}
                ship={ship}
                totalVessels={ships.length}
                imageUrl={SHIP_IMAGES[ship.mmsi]}
              />
            ))}
          </ol>
        </LayoutGroup>
      )}
    </section>
  );
}
