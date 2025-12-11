import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SolversOverview } from './pages/solvers-overview/solvers-overview';
import { NavigationMenu } from './components/navigation-menu/navigation-menu';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SolversOverview, NavigationMenu],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('web');
}
