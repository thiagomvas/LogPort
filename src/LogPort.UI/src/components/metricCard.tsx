import '../styles/metricCard.css';

interface MetricCardProps {
  name: string;
  last1s: number;
  last10s: number;
  last1m: number;
  sparkline?: number[];
  timeRange: '1m' | '24h'; 
}

export default function MetricCard({
  name,
  last1s,
  last10s,
  last1m,
  sparkline = [], 
  timeRange,
}: MetricCardProps) {
  const max = Math.max(...sparkline, last1s, last10s, last1m, 1);
  const min = Math.min(...sparkline, last1s, last10s, last1m, 0);
  const timeLabel = timeRange === '1m' ? 'm' : 'h';

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

      {sparkline.length > 0 ? (
        <div className="metric-sparkline">
          {sparkline.map((v, i) => (
            <div
              key={i}
              className="sparkline-bar-dd"
              style={{
                height: `${(v / max) * 100}%`,
                backgroundColor: v > max * 0.7 ? '#d9534f' : '#5bc0de',
              }}
            />
          ))}
        </div>
      ) : (
        <div className="metric-value-emphasis">{last1s}/s</div>
      )}

      <div className="metric-values-dd">
        {!sparkline.length ? (
          <>
            <span className="metric-value">{last1s}/s</span>
            <span className="metric-value">{last10s}/10s</span>
            <span className="metric-value">{last1m}/1m</span>
          </>

        ) : (
          <>
            <span className="metric-value">{max}/{timeLabel} peak</span>
            <span className="metric-value">{min}/{timeLabel} lowest</span>
          </>
        )}

      </div>

      <div className="metric-time-range">
        {timeRange === '1m' ? (
          <span className="time-label">{formatTimeLabel(0, '1m')}</span>
        ) : (
          <span className="time-label">{formatTimeLabel(0, '24h')}</span>
        )}
      </div>
    </div>
  );
}
