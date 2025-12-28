import { useEffect, useState } from "react";
import { type TimeRange, type PresetValue, PRESETS } from "../lib/types/timeRange";
import "../styles/timeRangeDropdown.css";
type Props = {
  onChange?: (range: TimeRange) => void;
};

export default function TimeRangeDropdown({ onChange }: Props) {
  const [value, setValue] = useState<PresetValue>("1d");
  const [customFrom, setCustomFrom] = useState("");
  const [customTo, setCustomTo] = useState("");

  useEffect(() => {
    const now = new Date();
    let range: TimeRange | null = null;

    if (value === "custom") {
      if (customFrom && customTo) {
        range = {
          from: new Date(customFrom),
          to: new Date(customTo),
        };
      }
    } else {
      const preset = PRESETS.find(p => p.value === value);
      if (!preset?.minutes) {return;}

      const from = new Date(now);
      from.setMinutes(now.getMinutes() - preset.minutes);
      range = { from, to: now };
    }

    if (range) {
      onChange?.(range);
    }
  }, [value, customFrom, customTo, onChange]);

  return (
    <div className="time-range-picker">
      <select
        className="time-range-select"
        value={value}
        onChange={e => setValue(e.target.value as PresetValue)}
      >
        {PRESETS.map(p => (
          <option key={p.value} value={p.value}>
            {p.label}
          </option>
        ))}
      </select>

      {value === "custom" && (
        <div className="custom-range">
          <input
            className="time-range-input"
            type="datetime-local"
            value={customFrom}
            onChange={e => setCustomFrom(e.target.value)}
          />
          <input
            className="time-range-input"
            type="datetime-local"
            value={customTo}
            onChange={e => setCustomTo(e.target.value)}
          />
        </div>
      )}
    </div>
  );
}
