import React, { useState } from "react";
import type { LogEntry } from "../lib/types/logEntry";
import { LogRow } from "./logRow";

interface LogViewerProps {
    logs: LogEntry[];
}

export const LogViewer: React.FC<LogViewerProps> = ({ logs }) => {
    const [filter, setFilter] = useState<string>("");

    const filteredLogs = logs.filter(
        (log) =>
            log.message.toLowerCase().includes(filter.toLowerCase()) ||
            (log.serviceName?.toLowerCase().includes(filter.toLowerCase()) ?? false) ||
            (log.level.toLowerCase().includes(filter.toLowerCase()))
    );

    return (
        <div className="fullscreen" style={{ padding: "1rem" }}>
            <input
                type="text"
                placeholder="Filter by message, service, or level..."
                value={filter}
                onChange={(e) => setFilter(e.target.value)}
                style={{ marginBottom: "1rem", padding: "0.5rem", width: "100%" }}
            />
            <table style={{ width: "100%", borderCollapse: "collapse" }}>
                <thead>
                    <tr>
                        <th>Timestamp</th>
                        <th>Level</th>
                        <th>Service</th>
                        <th>Message</th>
                    </tr>
                </thead>
                <tbody>
                    {filteredLogs.map((log, idx) => (
                        <LogRow key={idx} log={log} />
                    ))}
                </tbody>
            </table>
        </div>
    );
};