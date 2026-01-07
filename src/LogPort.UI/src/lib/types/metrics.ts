export type MetricCounter = {
  last1s: number;
  last10s: number;
  last1m: number;
  buckets?: number[] | null;
};

export type MetricHistogram = {
  counts: number[];
  boundaries: number[];
};

export type MetricSnapshot = {
  counters: Record<string, MetricCounter>;
  histograms: Record<string, MetricHistogram>;
};