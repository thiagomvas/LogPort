export type TimeRange = {
    from: Date;
    to: Date;
}


export type PresetValue =
  | "5m"
  | "15m"
  | "30m"
  | "1h"
  | "2h"
  | "6h"
  | "12h"
  | "1d"
  | "custom";

type Preset = {
  label: string;
  value: PresetValue;
  minutes?: number;
};

export const PRESETS: Preset[] = [
  { label: "Last 5 minutes", value: "5m", minutes: 5 },
  { label: "Last 15 minutes", value: "15m", minutes: 15 },
  { label: "Last 30 minutes", value: "30m", minutes: 30 },
  { label: "Last 1 hour", value: "1h", minutes: 60 },
  { label: "Last 2 hours", value: "2h", minutes: 120 },
  { label: "Last 6 hours", value: "6h", minutes: 360 },
  { label: "Last 12 hours", value: "12h", minutes: 720 },
  { label: "Last 1 day", value: "1d", minutes: 1440 },
  { label: "Custom", value: "custom" },
];
