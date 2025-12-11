export interface SolverMetadata {
  displayName: string;
  description: string;
  parameters: { [key: string]: DisplayParameter };
  dimensionality: number;
  domainNature: number;
}

export interface Unit {
  symbol: string;
  name: string;
  type: number;
}

export interface ValueWithUnit {
  value: number;
  unit: Unit;
}

export interface Interval {
  min: number;
  max: number;
}

export interface DisplayParameter extends ValueWithUnit {
  range: Interval;
  domain: number;
  reinitializeOnChange: boolean;
}
