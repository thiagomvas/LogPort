import React, { type ReactNode } from 'react';

import '../styles/components/card.css';

type Props = {
  children: ReactNode;
};

export default function Card({ children }: Props) {
  return <div className="card">{children}</div>;
}
