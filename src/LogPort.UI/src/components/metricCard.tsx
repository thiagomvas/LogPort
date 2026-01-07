import '../styles/metricCard.css'

interface MetricCardProps {
  name: string;
  last1s: number;
  last10s: number;
  last1m: number;
  sparkline: number[];
}

export default function MetricCard({ name, last1s, last10s, last1m, sparkline }: MetricCardProps) {
  const max = Math.max(...sparkline, last1s, last10s, last1m, 1);
  return (
    <div className="metric-card-dd">
      <div className="metric-name">{name}</div>
      <div className="metric-sparkline">
        {sparkline.map((v, i) => (
          <div
            key={i}
            className="sparkline-bar-dd"
            style={{ height: `${(v / max) * 100}%` }}
          />
        ))}
      </div>
      <div className="metric-values-dd">
        <span>{last1s}/s</span>
        <span>{last10s}/10s</span>
        <span>{last1m}/1m</span>
      </div>
    </div>
  );
}