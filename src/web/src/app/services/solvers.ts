import { Injectable, signal } from '@angular/core';
import { SolverMetadata } from '../model';

@Injectable({
  providedIn: 'root',
})
export class Solvers {
  readonly metadata = signal<SolverMetadata[]>([]);

  constructor() {
    this.loadMetadata(false);
  }

  loadMetadata(reset = true): void {
    fetch('api/solver?reset=' + reset)
      .then((res) => res.json())
      .then((data) => this.metadata.set(data));
  }

  requestUpdate(solver: string, state: string): Promise<string> {
    return fetch(`api/solver/${solver}/update`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ state }),
    })
      .then((res) => res.text())
      .then((data) => data);
  }
}
