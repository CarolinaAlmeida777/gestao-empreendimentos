import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastContainerComponent } from './shared/components/toast/toast-container.component';
import { LoadingBarComponent } from './shared/components/loading/loading-bar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastContainerComponent, LoadingBarComponent],
  template: `
    <app-loading-bar />

    <header class="topo">
      <div class="topo__marca">
        <span class="topo__logo" aria-hidden="true">◆</span>
        <span class="topo__nome">monitori<span class="topo__sufixo">gestão</span></span>
      </div>
    </header>

    <main>
      <router-outlet />
    </main>

    <app-toast-container />
  `,
  styles: [`
    .topo {
      background: var(--cor-grafite);
      padding: 14px var(--espaco-lg);
    }

    .topo__marca {
      max-width: 1180px;
      margin: 0 auto;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .topo__logo {
      color: var(--cor-terracota);
      font-size: 1rem;
    }

    .topo__nome {
      font-family: var(--fonte-display);
      font-size: 1.05rem;
      font-weight: 700;
      color: var(--cor-branco);
      letter-spacing: -0.01em;
    }

    .topo__sufixo {
      font-weight: 400;
      color: var(--cor-cinza-pedra);
      margin-left: 4px;
    }
  `]
})
export class AppComponent {}
