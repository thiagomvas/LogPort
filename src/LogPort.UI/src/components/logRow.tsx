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
        <tr className={`row ${log.Level}`}>
            <td>{new Date(log.Timestamp || "N/A").toLocaleString()}</td>
            <td style={{ color: "#000" }}>{log.Level}</td>
            <td>{log.ServiceName || "-"}</td>
            <td>{log.Message}</td>
        </tr>
    );
};