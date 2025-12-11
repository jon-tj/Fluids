import { Routes } from '@angular/router';
import { SolversOverview } from './pages/solvers-overview/solvers-overview';
import { SolverDetails } from './pages/solver-details/solver-details';

export const routes: Routes = [
  {
    path: '',
    component: SolversOverview,
  },
  {
    path: 'solvers/:solver',
    component: SolverDetails,
  },
];
