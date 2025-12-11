import { Component, computed, inject } from '@angular/core';
import { Solvers } from '../../services/solvers';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-solver-details',
  imports: [],
  templateUrl: './solver-details.html',
  styleUrl: './solver-details.css',
})
export class SolverDetails {
  private readonly solverService = inject(Solvers);
  private readonly route = inject(ActivatedRoute);

  protected readonly solver = computed(() => {
    const solverId = this.route.snapshot.paramMap.get('solver');
    if (!solverId) return null;
    return this.solverService.metadata().find((s) => s.displayName == solverId) ?? null;
  });

  protected readonly parameters = computed(() => {
    const params = this.solver()?.parameters ?? {};
    return Object.keys(params).map((key) => {
      return {
        key,
        data: params[key],
      };
    });
  });
}
