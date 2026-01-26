import LabeledInput from './labeledInput';
import type {
  BaseLogEntryExtractorConfig,
  JsonLogEntryExtractorConfig,
  RegexLogEntryExtractorConfig,
} from '../lib/types/config';

type Props = {
  extractor: JsonLogEntryExtractorConfig | RegexLogEntryExtractorConfig;
  index: number;
  onChange: (index: number, newValue: JsonLogEntryExtractorConfig | RegexLogEntryExtractorConfig) => void;
  onRemove: (index: number) => void;
};

export default function ExtractorInput({ extractor, index, onChange, onRemove }: Props) {
  const handleFieldChange = (field: string, value: any) => {
    onChange(index, { ...extractor, [field]: value } as typeof extractor);
  };

  const handleModeChange = (newMode: 'json' | 'regex') => {
    if (newMode === 'json') {
      onChange(index, {
        extractionMode: 'json',
        serviceName: extractor.serviceName || '',
        messageProperty: '',
        levelProperty: '',
        timestampProperty: '',
      });
    } else {
      onChange(index, {
        extractionMode: 'regex',
        serviceName: extractor.serviceName || '',
        pattern: '',
        messageGroup: '',
        levelGroup: '',
        timestampGroup: '',
      });
    }
  };

  return (
    <div className="extractor-card">
      <select
        value={extractor.extractionMode}
        onChange={e => handleModeChange(e.target.value as 'json' | 'regex')}
      >
        <option value="json">JSON Extractor</option>
        <option value="regex">Regex Extractor</option>
      </select>

      <LabeledInput
        label="Service Name"
        value={extractor.serviceName}
        onChange={v => handleFieldChange('serviceName', v as string)}
      />

      {extractor.extractionMode === 'json' && (
        <>
          <LabeledInput
            label="Message Property"
            value={(extractor as JsonLogEntryExtractorConfig).messageProperty}
            onChange={v => handleFieldChange('messageProperty', v as string)}
          />
          <LabeledInput
            label="Level Property"
            value={(extractor as JsonLogEntryExtractorConfig).levelProperty}
            onChange={v => handleFieldChange('levelProperty', v as string)}
          />
          <LabeledInput
            label="Timestamp Property"
            value={(extractor as JsonLogEntryExtractorConfig).timestampProperty}
            onChange={v => handleFieldChange('timestampProperty', v as string)}
          />
        </>
      )}

      {extractor.extractionMode === 'regex' && (
        <>
          <LabeledInput
            label="Pattern"
            value={(extractor as RegexLogEntryExtractorConfig).pattern}
            onChange={v => handleFieldChange('pattern', v as string)}
          />
          <LabeledInput
            label="Message Group"
            value={(extractor as RegexLogEntryExtractorConfig).messageGroup}
            onChange={v => handleFieldChange('messageGroup', v as string)}
          />
          <LabeledInput
            label="Level Group"
            value={(extractor as RegexLogEntryExtractorConfig).levelGroup}
            onChange={v => handleFieldChange('levelGroup', v as string)}
          />
          <LabeledInput
            label="Timestamp Group"
            value={(extractor as RegexLogEntryExtractorConfig).timestampGroup}
            onChange={v => handleFieldChange('timestampGroup', v as string)}
          />
        </>
      )}

      <button className="btn btn-danger" onClick={() => onRemove(index)}>
        Remove Extractor
      </button>
    </div>
  );
}
