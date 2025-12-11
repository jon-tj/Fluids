export interface SolverMetadata {
  displayName: string;
  description: string;
  parameters: { [key: string]: { value: number } };
  dimensionality: number;
  domainNature: number;
}
