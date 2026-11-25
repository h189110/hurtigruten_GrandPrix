import { useEffect, useRef, type CSSProperties } from 'react';
import { AnimatePresence, motion, useAnimate, useReducedMotion } from 'motion/react';
import type { Ship } from '../../types/ship';
import { LeaderCrown } from './LeaderCrown';
import { RankIndicator } from './RankIndicator';
import { ShipStatusBadge } from './ShipStatus';

type ShipCardProps = {
  ship: Ship;
  totalVessels: number;
  imageUrl: string | undefined;
};

const SPRING = { type: 'spring', stiffness: 100, damping: 20 } as const;

export function ShipCard({ ship, totalVessels, imageUrl }: ShipCardProps) {
  const reduceMotion = useReducedMotion();
  const [scope, animate] = useAnimate<HTMLLIElement>();
  const previousRank = useRef(ship.rank);

  useEffect(() => {
    if (previousRank.current === ship.rank) return;
    previousRank.current = ship.rank;
    if (ship.rank === null || reduceMotion || !scope.current) return;

    animate(
      scope.current,
      { outlineColor: ['rgba(37, 39, 34, 0.45)', 'rgba(37, 39, 34, 0)'] },
      { duration: 1.4, ease: 'easeOut' },
    );
  }, [ship.rank, reduceMotion, animate, scope]);

  const imageStyle = imageUrl
    ? ({ '--ship-image': `url("${imageUrl}")` } as CSSProperties)
    : undefined;

  const speed = ship.speedKnots;

  return (
    <motion.li
      ref={scope}
      layout
      transition={reduceMotion ? { duration: 0 } : SPRING}
      className={`ship-card ship-card--${ship.status.toLowerCase()}${imageUrl ? '' : ' ship-card--no-image'}`}
      style={imageStyle}
    >
      <AnimatePresence initial={false}>
        {ship.rank === 1 && (
          <LeaderCrown key="leader-crown" reduceMotion={reduceMotion ?? false} />
        )}
      </AnimatePresence>
      <div className="ship-card__content">
        <RankIndicator rank={ship.rank} total={totalVessels} />
        <h3 className="ship-card__name">{ship.name}</h3>
        <p className="ship-card__speed">
          {speed !== null ? (
            <>
              <span className="ship-card__speed-value">{speed.toFixed(1)}</span>
              <span className="ship-card__speed-unit">kn</span>
            </>
          ) : (
            <span className="ship-card__speed-missing">No speed reading</span>
          )}
        </p>
        <ShipStatusBadge status={ship.status} dataAgeSeconds={ship.dataAgeSeconds} />
      </div>
    </motion.li>
  );
}
