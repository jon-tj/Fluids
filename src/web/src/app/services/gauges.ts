import { Injectable, signal } from '@angular/core';
import { GaugeResult, GaugeMetadata } from '../model';

@Injectable({
  providedIn: 'root',
})
export class Gauges {
  readonly metadata = signal<GaugeMetadata[]>([]);

  constructor() {
    this.loadMetadata();
  }

  loadMetadata(): void {
    fetch('api/gauge')
      .then((res) => res.json())
      .then((data) => this.metadata.set(data));
  }

  requestUpdate(gauge: string, state: string): Promise<GaugeResult[]> {
    return fetch(`api/gauge/${gauge}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ state }),
    })
      .then((res) => res.json())
      .then((data) => data);
  }
}
