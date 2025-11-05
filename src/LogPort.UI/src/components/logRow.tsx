import React from "react";
import type { LogEntry } from "../lib/types/log";
import "../styles/logs.css";

interface LogRowProps {
    log: LogEntry;
}

export const LogRow: React.FC<LogRowProps> = ({ log }) => {
    return (
        <tr className={`row`}>
            <td>{new Date(log.timestamp || "N/A").toLocaleString()}</td>
            <td className={`${log.level?.toLowerCase()}`}>{log.level}</td>
            <td>{log.serviceName || "-"}</td>
            <td>{log.message}</td>
        </tr>
    );
};