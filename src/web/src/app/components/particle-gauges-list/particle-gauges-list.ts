import { Component, inject, Input, OnDestroy, signal } from '@angular/core';
import { Gauges } from '../../services/gauges';
import { GaugeResult } from '../../model';

@Component({
  selector: 'app-particle-gauges-list',
  imports: [],
  templateUrl: './particle-gauges-list.html',
  styleUrl: './particle-gauges-list.css',
})
export class ParticleGaugesList implements OnDestroy {
  private gaugesService = inject(Gauges);
  @Input({ required: true }) state!: string;
  protected readonly allGauges = this.gaugesService.metadata;
  protected gaugeResult = signal<{ [key: string]: GaugeResult[] }>({});

  private enabledGauges: string[] = [];
  private readonly UPDATE_MS = 200;
  private intervalRef: number;

  constructor() {
    this.intervalRef = setInterval(() => {
      if (this.enabledGauges.length == 0) return;
      fetch('api/gauge/', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ gaugeIds: this.enabledGauges, state: this.state }),
      })
        .then((result) => result.json() as Promise<{ gaugeId: string; result: GaugeResult[] }[]>)
        .then((result) => {
          this.gaugeResult.update((gr) => ({
            ...gr,
            ...Object.fromEntries(result.map((r) => [r.gaugeId, r.result])),
          }));
        });
    }, this.UPDATE_MS);
  }

  ngOnDestroy(): void {
    clearInterval(this.intervalRef);
  }

  protected onToggleGauge(enabled: boolean, id: string) {
    if (enabled) this.enabledGauges.push(id);
    else if (this.enabledGauges.includes(id)) {
      this.enabledGauges = this.enabledGauges.filter((g) => g != id);
      this.gaugeResult.update((gr) => ({ ...gr, [id]: [] }));
    }
  }
}
