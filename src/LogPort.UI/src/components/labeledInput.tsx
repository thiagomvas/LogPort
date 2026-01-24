import React from 'react';

import '../styles/components/labeledInput.css';

type LabeledInputProps = {
  label: string;
  description?: string;
  value: string | number | boolean;
  type?: 'text' | 'number' | 'checkbox' | 'password';
  onChange: (value: string | number | boolean) => void;
  min?: number;
  max?: number;
  step?: number;
};

export default function LabeledInput({
  label,
  description,
  value,
  type = 'text',
  onChange,
  min,
  max,
  step,
}: LabeledInputProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (type === 'number') onChange(Number(e.target.value));
    else if (type === 'checkbox') onChange(e.target.checked);
    else onChange(e.target.value);
  };

  return (
    <div className={`labeled-input ${type === 'checkbox' ? 'checkbox' : ''}`}>
  <label>
    {type !== 'checkbox' && <div className="input-label">{label}</div>}
    <input
      type={type}
      value={type === 'checkbox' ? undefined : value as string | number}
      checked={type === 'checkbox' ? Boolean(value) : undefined}
      onChange={handleChange}
      min={min}
      max={max}
      step={step}
    />
    {type === 'checkbox' && <span className="checkbox-label">{label}</span>}
  </label>
  {description && <div className="input-description">{description}</div>}
</div>

  );
}
