type RankIndicatorProps = {
  rank: number | null;
  total: number;
};

export function RankIndicator({ rank, total }: RankIndicatorProps) {
  if (rank === null) {
    return <span className="rank rank--unranked">Not ranked</span>;
  }

  const position = String(rank).padStart(2, '0');
  const of = String(total).padStart(2, '0');

  return (
    <span className={`rank${rank <= 3 ? ` rank--top rank--${rank}` : ''}`}>
      {position} <span className="rank__total">/ {of}</span>
    </span>
  );
}
