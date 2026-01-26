import { useState, useEffect } from 'react';
import '../styles/configPage.css';
import Grid from '../components/grid';
import Section from '../components/section';
import LabeledInput from '../components/labeledInput';
import type { LogPortConfig } from '../lib/types/config';
import { getConfig, saveConfig } from '../lib/services/config.service';
import ExtractorInput from '../components/extractorInput';

export default function ConfigurationPage() {
  const [config, setConfig] = useState<LogPortConfig>();
  const [status, setStatus] = useState<'idle' | 'saving' | 'success' | 'error'>('idle');

useEffect(() => {
  setStatus('idle');
  getConfig()
    .then(data => {
      // normalize extractors
      const normalizedExtractors = data.extractors.map(ext => ({
        ...ext,
        extractionMode: ext.extractionMode.toLowerCase() as 'json' | 'regex',
      })) as (typeof data.extractors);
      setConfig({ ...data, extractors: normalizedExtractors });
    })
    .catch(err => {
      console.error('Failed to fetch config', err);
      setStatus('error');
    });
}, []);


  const handleChange = <K extends keyof LogPortConfig>(key: K, value: LogPortConfig[K]) => {
    setConfig(prev => prev ? { ...prev, [key]: value } : undefined);
    setStatus('idle');
  };

  const handleNestedChange = <K extends keyof LogPortConfig, NK extends keyof LogPortConfig[K]>(
    parent: K,
    key: NK,
    value: LogPortConfig[K][NK]
  ) => {
    setConfig(prev => prev ? ({
      ...prev,
      [parent]: { ...(prev[parent] as any), [key]: value },
    }) : undefined);
    setStatus('idle');
  };

  const handleSave = () => {
    setStatus('saving');
    saveConfig(config!)
      .then(() => setStatus('success'))
      .catch(err => {
        console.error('Failed to save config', err);
        setStatus('error');
      });

  };

  if (!config) {
    return <div>Loading configuration...</div>;
  }

  return (
    <div className="config-page">
      <h2>Agent Configuration</h2>
      <p>Some changes are applied immediately, while some require a restart. To ensure your new configuration is applied, restart the agent, since it is not guaranteed that the changes will be applied without an actual restart.</p>

      <Section title="General Settings">
        <Grid>
          <LabeledInput
            label="Port"
            type="number"
            value={config.port}
            description="The port on which the agent will listen for incoming connections."
            onChange={v => handleChange('port', v as number)}
          />
          <LabeledInput
            label="Upstream URL"
            type="text"
            value={config.upstreamUrl ?? ''}
            description="The URL the agent will relay data to if mode is Relay."
            onChange={v => handleChange('upstreamUrl', v as string || undefined)}
          />
          <LabeledInput
            label="Batch Size"
            type="number"
            value={config.batchSize}
            description="Number of logs processed in each background batch."
            onChange={v => handleChange('batchSize', v as number)}
          />
          <LabeledInput
            label="Flush Interval (ms)"
            type="number"
            value={config.flushIntervalMs}
            description="Time interval in milliseconds for processing log batches."
            onChange={v => handleChange('flushIntervalMs', v as number)}
          />
        </Grid>
      </Section>

      <Section title="Log Extractors">
  {config.extractors.map((ext, idx) => (
    <ExtractorInput
      key={idx}
      index={idx}
      extractor={ext}
      onChange={(i, newVal) => {
        const newExtractors = [...config.extractors];
        newExtractors[i] = newVal;
        setConfig(prev => prev ? ({ ...prev, extractors: newExtractors }) : undefined);
      }}
      onRemove={i => {
        const newExtractors = config.extractors.filter((_, index) => index !== i);
        setConfig(prev => prev ? ({ ...prev, extractors: newExtractors }) : undefined);
      }}
    />
  ))}

  <button
    className="btn btn-primary"
    onClick={() =>
      setConfig(prev => prev ? ({
        ...prev,
        extractors: [
          ...prev.extractors,
          { type: 'json', messageProperty: '', levelProperty: '', timestampProperty: '', extractionMode: 'json', serviceName: '' },
        ],
      }) : undefined)
    }
  >
    Add JSON Extractor
  </button>
  <button
    className="btn btn-primary"
    style={{ marginLeft: '8px' }}
    onClick={() =>
      setConfig(prev => prev ? ({
        ...prev,
        extractors: [
          ...prev.extractors,
          { type: 'regex', pattern: '', messageGroup: '', levelGroup: '', timestampGroup: '', extractionMode: 'regex', serviceName: '' },
        ],
      }) : undefined)
    }
  >
    Add Regex Extractor
  </button>
</Section>

      <Section title="Authentication">
        <Grid>
          <LabeledInput
            label="Admin Login"
            type="text"
            value={config.adminLogin}
            description="Username for HTTP Basic authentication."
            onChange={v => handleChange('adminLogin', v as string)}
          />
          <LabeledInput
            label="Admin Password"
            type="password"
            value={config.adminPassword}
            description="Password for HTTP Basic authentication."
            onChange={v => handleChange('adminPassword', v as string)}
          />
          <LabeledInput
            label="API Secret"
            type="text"
            value={config.apiSecret}
            description="Shared secret for API token authentication."
            onChange={v => handleChange('apiSecret', v as string)}
          />
        </Grid>
      </Section>

      <Section title="Postgres">
        <Grid>
          <LabeledInput
            label="Use Postgres"
            type="checkbox"
            value={config.postgres.use}
            description="Enable PostgreSQL as the log storage backend."
            onChange={v => handleNestedChange('postgres', 'use', v as boolean)}
          />
          <LabeledInput
            label="Host"
            type="text"
            value={config.postgres.host}
            description="PostgreSQL host address."
            onChange={v => handleNestedChange('postgres', 'host', v as string)}
          />
          <LabeledInput
            label="Port"
            type="number"
            value={config.postgres.port}
            description="PostgreSQL port number."
            onChange={v => handleNestedChange('postgres', 'port', v as number)}
          />
          <LabeledInput
            label="Database"
            type="text"
            value={config.postgres.database}
            description="Database name to store logs."
            onChange={v => handleNestedChange('postgres', 'database', v as string)}
          />
          <LabeledInput
            label="Username"
            type="text"
            value={config.postgres.username}
            description="PostgreSQL user."
            onChange={v => handleNestedChange('postgres', 'username', v as string)}
          />
          <LabeledInput
            label="Password"
            type="password"
            value={config.postgres.password}
            description="PostgreSQL password."
            onChange={v => handleNestedChange('postgres', 'password', v as string)}
          />
          <LabeledInput
            label="Partition Length (days)"
            type="number"
            value={config.postgres.partitionLength}
            description="Length of log table partitions in days."
            onChange={v => handleNestedChange('postgres', 'partitionLength', v as number)}
          />
        </Grid>
      </Section>

      <Section title="Docker">
        <Grid>
          <LabeledInput
            label="Use Docker"
            type="checkbox"
            value={config.docker.use}
            description="Enable Docker module to collect container logs."
            onChange={v => handleNestedChange('docker', 'use', v as boolean)}
          />
          <LabeledInput
            label="Socket Path"
            type="text"
            value={config.docker.socketPath}
            description="Path to Docker socket."
            onChange={v => handleNestedChange('docker', 'socketPath', v as string)}
          />
          <LabeledInput
            label="Watch All Containers"
            type="checkbox"
            value={config.docker.watchAllContainers}
            description="If enabled, every container is monitored (can be high load)."
            onChange={v => handleNestedChange('docker', 'watchAllContainers', v as boolean)}
          />
        </Grid>
      </Section>

      <Section title="Cache">
        <Grid>
          <LabeledInput
            label="Use Redis"
            type="checkbox"
            value={config.cache.useRedis}
            description="Enable Redis caching for metrics and logs."
            onChange={v => handleNestedChange('cache', 'useRedis', v as boolean)}
          />
          <LabeledInput
            label="Redis Connection String"
            type="text"
            value={config.cache.redisConnectionString ?? ''}
            description="Connection string for Redis cache."
            onChange={v => handleNestedChange('cache', 'redisConnectionString', v as string || undefined)}
          />
          <LabeledInput
            label="Default Expiration (ms)"
            type="text"
            value={config.cache.defaultExpiration}
            description="Default expiration time for cached entries."
            onChange={v => handleNestedChange('cache', 'defaultExpiration', v as string)}
          />
        </Grid>
      </Section>

      <div className="dashboard-actions">
        <button className="btn btn-primary" onClick={handleSave} disabled={status === 'saving'}>
          {status === 'saving' ? 'Saving…' : 'Save Configuration'}
        </button>
        {status === 'success' && (
          <span style={{ marginLeft: '12px', color: 'var(--accent-success)' }}>Saved successfully!</span>
        )}
        {status === 'error' && (
          <span style={{ marginLeft: '12px', color: 'var(--color-error)' }}>Failed to save.</span>
        )}
      </div>
    </div>
  );
}
