import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SolversOverview } from './pages/solvers-overview/solvers-overview';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SolversOverview],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('web');
}
