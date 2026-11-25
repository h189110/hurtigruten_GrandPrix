import { motion } from 'motion/react';

type LeaderCrownProps = {
  reduceMotion: boolean;
};

const POP_IN = { type: 'spring', stiffness: 260, damping: 22 } as const;
const DROP_OUT = { duration: 0.7, ease: 'easeIn' } as const;

export function LeaderCrown({ reduceMotion }: LeaderCrownProps) {
  return (
    <motion.div
      className="leader-crown"
      aria-hidden="true"
      initial={reduceMotion ? false : { opacity: 0, scale: 0.3, rotate: -30 }}
      animate={{ opacity: 1, scale: 1, rotate: 14 }}
      exit={
        reduceMotion
          ? { opacity: 0, transition: { duration: 0 } }
          : { y: 90, rotate: 40, opacity: 0, transition: DROP_OUT }
      }
      transition={reduceMotion ? { duration: 0 } : POP_IN}
    >
      <svg viewBox="0 0 48 32" focusable="false">
        <path d="M4 26 L8 10 L17 19 L24 5 L31 19 L40 10 L44 26 Z" />
      </svg>
      {!reduceMotion && (
        <>
          <span className="leader-crown__spark leader-crown__spark--a" />
          <span className="leader-crown__spark leader-crown__spark--b" />
          <span className="leader-crown__spark leader-crown__spark--c" />
        </>
      )}
    </motion.div>
  );
}
