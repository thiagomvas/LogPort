import React, { useState } from "react";
import type { LogEntry } from "../lib/types/log";
import "../styles/logs.css";

interface LogRowProps {
    log: LogEntry;
}

export const LogRow: React.FC<LogRowProps> = ({ log }) => {
    const [expanded, setExpanded] = useState(false);

    const toggleExpand = () => setExpanded((prev) => !prev);

    // Helper to render metadata nicely
    const renderMetadata = (metadata?: Record<string, any>) => {
        if (!metadata || Object.keys(metadata).length === 0) return <span>-</span>;
        return (
            <ul className="metadata-list">
                {Object.entries(metadata).map(([key, value]) => (
                    <li key={key}>
                        <strong>{key}:</strong> {JSON.stringify(value)}
                    </li>
                ))}
            </ul>
        );
    };

    return (
        <>
            <tr
                className={`row`}
                onClick={toggleExpand}
                style={{ cursor: "pointer" }}
            >
                <td className={`${log.level?.toLowerCase()}`}>{log.level}</td>
                <td>{new Date(log.timestamp || "N/A").toLocaleString()}</td>
                <td>{log.serviceName || "-"}</td>
                <td>{log.message}</td>
            </tr>
            {expanded && (
                <tr className="row-expanded">
                    <td colSpan={4} style={{ padding: "0.75rem 1rem" }}>
                        <div className="log-details">
                            {log.traceId && (
                                <div>
                                    <strong>Trace ID:</strong> {log.traceId}
                                </div>
                            )}
                            {log.spanId && (
                                <div>
                                    <strong>Span ID:</strong> {log.spanId}
                                </div>
                            )}
                            {log.hostname && (
                                <div>
                                    <strong>Host:</strong> {log.hostname}
                                </div>
                            )}
                            {log.environment && (
                                <div>
                                    <strong>Environment:</strong> {log.environment}
                                </div>
                            )}
                            {log.metadata && (
                                <div>
                                    <strong>Metadata:</strong>
                                    {renderMetadata(log.metadata)}
                                </div>
                            )}
                        </div>
                    </td>
                </tr>
            )}
        </>
    );
};
