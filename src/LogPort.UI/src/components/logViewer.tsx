import React, { useState, useEffect, useRef } from "react";
import type { LogEntry } from "../lib/types/log";
import { LogRow } from "./logRow";

interface LogViewerProps {
    logs: LogEntry[];
}

export const LogViewer: React.FC<LogViewerProps> = ({ logs }) => {
    const [filter, setFilter] = useState("");
    const scrollRef = useRef<HTMLDivElement>(null);

    const filteredLogs = logs.filter(
        (log) =>
            log.message?.toLowerCase().includes(filter.toLowerCase()) ||
            log.serviceName?.toLowerCase().includes(filter.toLowerCase()) ||
            log.level?.toLowerCase().includes(filter.toLowerCase())
    );

    // Auto-scroll to bottom on new logs
    useEffect(() => {
        scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight, behavior: "smooth" });
    }, [filteredLogs.length]);

    return (
        <div className="fullscreen" style={{ display: "flex", flexDirection: "column", padding: "1rem" }}>
            <input
                type="text"
                placeholder="Filter by message, service, or level..."
                value={filter}
                onChange={(e) => setFilter(e.target.value)}
                style={{ marginBottom: "1rem", padding: "0.5rem", width: "100%" }}
            />
            <div ref={scrollRef} style={{ flex: 1, overflowY: "auto" }}>
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
                        {filteredLogs.map((log, index) => (
                            <LogRow key={(log.timestamp?.toString() ?? "unknown") + index} log={log} />
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};
