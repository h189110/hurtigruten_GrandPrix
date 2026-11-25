import type { CSSProperties, ReactNode } from 'react';
import headerShip from '../assets/ships/vesteralen.png';

type LayoutProps = {
  children: ReactNode;
};

const headerStyle = { '--header-ship': `url("${headerShip}")` } as CSSProperties;

export function Layout({ children }: LayoutProps) {
  return (
    <div className="app-shell">
      <header className="app-header" style={headerStyle}>
        <p className="app-kicker">Coastal express championship</p>
        <h1>Hurtigrute Grand Prix</h1>
      </header>
      <main className="app-main">{children}</main>
    </div>
  );
}
