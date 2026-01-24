import React, { type ReactNode } from 'react';

import '../styles/components/grid.css';

type Props = {
  children: ReactNode;
};

export default function Grid({ children }: Props) {
  return <div className="grid">{children}</div>;
}
