import React from "react";
import type { LogEntry } from "../lib/types/logEntry";

interface LogRowProps {
    log: LogEntry;
}

const levelColors: Record<string, string> = {
    error: "#f87171",
    warn: "#fbbf24",
    info: "#3b82f6",
    debug: "#9ca3af",
};

export const LogRow: React.FC<LogRowProps> = ({ log }) => {
    return (
        <tr className={`row ${log.level}`}>
            <td>{new Date(log.timestamp).toLocaleString()}</td>
            <td style={{ color: levelColors[log.level] || "#000" }}>{log.level}</td>
            <td>{log.serviceName || "-"}</td>
            <td>{log.message}</td>
        </tr>
    );
};