import { Component, computed, inject } from '@angular/core';
import { Solvers } from '../../services/solvers';
import { ActivatedRoute } from '@angular/router';
import { ParameterField } from '../../components/parameter-field/parameter-field';
import { SolverSimulation } from '../../components/solver-simulation/solver-simulation';

@Component({
  selector: 'app-solver-details',
  imports: [ParameterField, SolverSimulation],
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
