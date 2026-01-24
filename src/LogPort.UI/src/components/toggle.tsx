import React from 'react';
import Card from './card';

type Props = {
  label: string;
  checked: boolean;
  onChange: (value: boolean) => void;
};

export default function Toggle({ label, checked, onChange }: Props) {
  return (
    <Card>
      <label style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '0.9rem' }}>
        <input type="checkbox" checked={checked} onChange={e => onChange(e.target.checked)} />
        {label}
      </label>
    </Card>
  );
}