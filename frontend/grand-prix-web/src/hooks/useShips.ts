import { useEffect, useState } from 'react';
import { fetchShips, fetchShipStatus } from '../services/shipService';
import type { Ship, ShipStatus } from '../types/ship';

const REFRESH_INTERVAL_MS = 30_000;

export interface ShipsState {
  ships: Ship[];
  status: ShipStatus | null;
  isLoading: boolean;
  error: string | null;
  isStale: boolean;
  lastUpdatedAt: Date | null;
}

const initialState: ShipsState = {
  ships: [],
  status: null,
  isLoading: true,
  error: null,
  isStale: false,
  lastUpdatedAt: null,
};

export function useShips(): ShipsState {
  const [state, setState] = useState<ShipsState>(initialState);

  useEffect(() => {
    let cancelled = false;

    async function refresh() {
      try {
        const [ships, status] = await Promise.all([fetchShips(), fetchShipStatus()]);
        if (cancelled) return;
        setState({
          ships,
          status,
          isLoading: false,
          error: null,
          isStale: false,
          lastUpdatedAt: new Date(),
        });
      } catch (caught) {
        if (cancelled) return;
        const message = caught instanceof Error ? caught.message : 'Unknown error';
        // Keep the last successfully received data and mark it as possibly outdated.
        setState((previous) => ({
          ...previous,
          isLoading: false,
          error: message,
          isStale: previous.lastUpdatedAt !== null,
        }));
      }
    }

    refresh();
    const timerId = window.setInterval(refresh, REFRESH_INTERVAL_MS);

    return () => {
      cancelled = true;
      window.clearInterval(timerId);
    };
  }, []);

  return state;
}
