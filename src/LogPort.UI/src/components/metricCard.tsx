import '../styles/metricCard.css';

interface MetricCardProps {
  name: string;
  last1s: number;
  last10s: number;
  last1m: number;
  sparkline?: number[]; // Optional array for sparkline data
  timeRange: '1m' | '24h'; // Time range for display
}

export default function MetricCard({
  name,
  last1s,
  last10s,
  last1m,
  sparkline = [], // Default to an empty array if no sparkline data is passed
  timeRange,
}: MetricCardProps) {
  // Find the max value among sparkline, last1s, last10s, and last1m
  const max = Math.max(...sparkline, last1s, last10s, last1m, 1);

  // Time formatting helper function
  const formatTimeLabel = (value: number, range: '1m' | '24h') => {
    if (range === '1m') {
      return `${value}m ago`;
    } else {
      return `${value}h ago`;
    }
  };

  return (
    <div className="metric-card-dd" title={name}>
      <div className="metric-name" title={name}>
        {name}
      </div>

      {/* Render sparkline bars if sparkline data exists, else emphasize last1s */}
      {sparkline.length > 0 ? (
        <div className="metric-sparkline">
          {sparkline.map((v, i) => (
            <div
              key={i}
              className="sparkline-bar-dd"
              style={{
                height: `${(v / max) * 100}%`,
                backgroundColor: v > max * 0.7 ? '#d9534f' : '#5bc0de', // Color based on value
              }}
            />
          ))}
        </div>
      ) : (
        <div className="metric-value-emphasis">{last1s}/s</div>  
      )}

      <div className="metric-values-dd">
        {/* Render 'last1s' only if no sparkline, otherwise render all values */}
        {!sparkline.length ? (
          <span className="metric-value">{last1s}/s</span>
        ) : null}
        <span className="metric-value">{last10s}/10s</span>
        <span className="metric-value">{last1m}/1m</span>
      </div>

      {/* Time Range Display: Show the labels for time-based metrics */}
      <div className="metric-time-range">
        {timeRange === '1m' ? (
          <span className="time-label">{formatTimeLabel(0, '1m')}</span> // Current time
        ) : (
          <span className="time-label">{formatTimeLabel(0, '24h')}</span> // Current time
        )}
      </div>
    </div>
  );
}
