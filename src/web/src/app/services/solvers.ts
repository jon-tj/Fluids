import { Injectable, signal } from '@angular/core';
import { SolverMetadata } from '../model';

@Injectable({
  providedIn: 'root',
})
export class Solvers {
  readonly metadata = signal<SolverMetadata[]>([]);

  constructor() {
    this.loadMetadata();
  }

  private loadMetadata() {
    fetch('http://localhost:5140/api/solver')
      .then((res) => res.json())
      .then((data) => this.metadata.set(data));
  }
}
