import React, { useState, useEffect, useRef } from "react";
import type { LogEntry } from "../lib/types/log";
import { LogRow } from "./logRow";
import "../styles/logs.css";
interface LogViewerProps {
    logs: LogEntry[];
}

export const LogViewer: React.FC<LogViewerProps> = ({ logs }) => {
    const scrollRef = useRef<HTMLDivElement>(null);

    // Auto-scroll to bottom on new logs
    useEffect(() => {
        scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight, behavior: "smooth" });
    }, [logs.length]);

    return (
        <div className="fullscreen" style={{ display: "flex", flexDirection: "column" }}>
            <div ref={scrollRef} style={{ flex: 1, overflowY: "auto" }}>
                <table style={{ width: "100%", borderCollapse: "collapse" }}>
                    <thead>
                        <tr>
                            <th>Severity</th>
                            <th>Timestamp</th>
                            <th>Service</th>
                            <th>Message</th>
                        </tr>
                    </thead>
                    <tbody>
                        {logs.map((log, index) => (
                            <LogRow key={(log.timestamp?.toString() ?? "unknown") + index} log={log} />
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};
