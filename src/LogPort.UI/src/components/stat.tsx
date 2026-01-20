export default function Stat({ title, value, intent, subtitle }: any) {
  return (
    <div className={`stat-card ${intent ?? ''}`}>
      <span className="stat-title">{title}</span>
      <span className="stat-value">{value}</span>
      <span className="stat-subtitle">{subtitle}</span>
    </div>
  );
}