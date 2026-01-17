export default function KeyValueBars({ data, limit = 10 }: { data: Record<string, number>; limit?: number }) {
  const entries = Object.entries(data).sort((a, b) => b[1] - a[1]).slice(0, limit);
  const max = Math.max(...entries.map(e => e[1]), 1);
  const sum = entries.reduce((acc, [, v]) => acc + v, 0);
  return (
    <div className="kv-bars">
      {entries.map(([key, value]) => (
        <div key={key} className="kv-row">
          <span>{key || '(null)'}</span>
          <div className="bar">
            <div className="fill" style={{ width: `${(value / sum) * 100}%` }} />
          </div>
          <span>{value}</span>
        </div>
      ))}
    </div>
  );
}
