import { Component, inject } from '@angular/core';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  template: `
    <div class="toast-container" role="status" aria-live="polite">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast" [class.toast--erro]="toast.tipo === 'erro'" [class.toast--sucesso]="toast.tipo === 'sucesso'">
          <span class="toast__icone" aria-hidden="true">
            @if (toast.tipo === 'sucesso') {
              ✓
            } @else {
              !
            }
          </span>
          <span class="toast__mensagem">{{ toast.mensagem }}</span>
          <button
            type="button"
            class="toast__fechar"
            (click)="toastService.remover(toast.id)"
            aria-label="Fechar notificação"
          >×</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: var(--espaco-lg);
      right: var(--espaco-lg);
      z-index: 1000;
      display: flex;
      flex-direction: column;
      gap: var(--espaco-sm);
      max-width: 380px;
    }

    .toast {
      display: flex;
      align-items: flex-start;
      gap: var(--espaco-sm);
      padding: var(--espaco-md);
      border-radius: var(--raio-md);
      background: var(--cor-grafite);
      color: var(--cor-branco);
      box-shadow: var(--sombra-elevada);
      font-size: 0.9rem;
      line-height: 1.4;
      animation: deslizar-entrada 0.25s ease-out;
    }

    .toast--sucesso {
      background: var(--cor-oliva);
    }

    .toast--erro {
      background: var(--cor-erro);
    }

    .toast__icone {
      flex-shrink: 0;
      width: 20px;
      height: 20px;
      border-radius: 50%;
      background: rgba(255, 255, 255, 0.2);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      font-weight: 700;
    }

    .toast__mensagem {
      flex: 1;
    }

    .toast__fechar {
      background: none;
      border: none;
      color: rgba(255, 255, 255, 0.8);
      font-size: 1.1rem;
      line-height: 1;
      cursor: pointer;
      padding: 0;
    }

    .toast__fechar:hover {
      color: var(--cor-branco);
    }

    @keyframes deslizar-entrada {
      from { opacity: 0; transform: translateX(16px); }
      to { opacity: 1; transform: translateX(0); }
    }
  `]
})
export class ToastContainerComponent {
  toastService = inject(ToastService);
}
