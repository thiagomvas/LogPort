import React, { type ReactNode } from 'react';

import '../styles/components/section.css';

type Props = {
  title: string;
  description?: string;
  children: ReactNode;
};

export default function Section({ title, description, children }: Props) {
  return (
    <section className="section">
      <h3>{title}</h3>
      {description && <p className="section-desc">{description}</p>}
      <div className="section-content">{children}</div>
    </section>
  );
}
