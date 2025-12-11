import { Component, inject } from '@angular/core';
import { Solvers } from '../../services/solvers';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-solvers-overview',
  imports: [RouterLink],
  templateUrl: './solvers-overview.html',
  styleUrl: './solvers-overview.css',
})
export class SolversOverview {
  private readonly solversService = inject(Solvers);
  protected readonly solvers = this.solversService.metadata;
}
