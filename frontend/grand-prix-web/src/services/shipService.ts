import { getJson } from './apiClient';
import type { Ship, ShipStatus } from '../types/ship';

export function fetchShips(): Promise<Ship[]> {
  return getJson<Ship[]>('/api/ships');
}

export function fetchShipStatus(): Promise<ShipStatus> {
  return getJson<ShipStatus>('/api/ships/status');
}
