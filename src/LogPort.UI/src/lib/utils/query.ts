import type { LogQueryParameters } from "../types/log";

export const paramsToQueryString = (params: LogQueryParameters) => {
  const qs = new URLSearchParams();

  if (params.from) {qs.set('from', params.from.toISOString());}
  if (params.to) {qs.set('to', params.to.toISOString());}
  if (params.level) {qs.set('level', params.level);}
  if (params.search) {qs.set('search', params.search);}
  if (params.serviceName) {qs.set('service', params.serviceName);}
  if (params.hostname) {qs.set('host', params.hostname);}
  if (params.environment) {qs.set('env', params.environment);}

  return qs.toString();
};

export const queryStringToParams = (): Partial<LogQueryParameters> => {
  const qs = new URLSearchParams(window.location.search);

  return {
    from: qs.get('from') ? new Date(qs.get('from')!) : undefined,
    to: qs.get('to') ? new Date(qs.get('to')!) : undefined,
    level: qs.get('level') || '',
    search: qs.get('search') || '',
    serviceName: qs.get('service') || '',
    hostname: qs.get('host') || '',
    environment: qs.get('env') || '',
  };
};
