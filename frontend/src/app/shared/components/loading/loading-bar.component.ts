import { Component, inject } from '@angular/core';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-loading-bar',
  standalone: true,
  template: `
    @if (loadingService.carregando()) {
      <div class="loading-bar" role="progressbar" aria-label="Carregando"></div>
    }
  `,
  styles: [`
    .loading-bar {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 3px;
      z-index: 1100;
      background: linear-gradient(
        90deg,
        var(--cor-terracota) 0%,
        var(--cor-terracota-escura) 50%,
        var(--cor-terracota) 100%
      );
      background-size: 200% 100%;
      animation: progresso 1.1s ease-in-out infinite;
    }

    @keyframes progresso {
      0% { background-position: 200% 0; }
      100% { background-position: -200% 0; }
    }
  `]
})
export class LoadingBarComponent {
  loadingService = inject(LoadingService);
}
