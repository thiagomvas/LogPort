
const dateTimeFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
  hour: "2-digit",
  minute: "2-digit",
});

export const formatDateTime = (date: Date): string => {
  return dateTimeFormatter.format(date);
};

export const timeAgo = (date: Date | string): string => {
  const d = typeof date === "string" ? new Date(date) : date;
  const seconds = Math.floor((Date.now() - d.getTime()) / 1000);

  if (seconds < 60) { return `${seconds}s ago`; }
  if (seconds < 3600) { return `${Math.floor(seconds / 60)}m ago`; }
  if (seconds < 86400) { return `${Math.floor(seconds / 3600)}h ago`; }
  return `${Math.floor(seconds / 86400)}d ago`;
};

