import { Injectable, signal } from '@angular/core';

export type TipoToast = 'sucesso' | 'erro';

export interface Toast {
  id: number;
  tipo: TipoToast;
  mensagem: string;
}

let proximoId = 1;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _toasts = signal<Toast[]>([]);
  readonly toasts = this._toasts.asReadonly();

  sucesso(mensagem: string, duracaoMs = 4000): void {
    this.mostrar('sucesso', mensagem, duracaoMs);
  }

  erro(mensagem: string, duracaoMs = 6000): void {
    this.mostrar('erro', mensagem, duracaoMs);
  }

  remover(id: number): void {
    this._toasts.update((lista) => lista.filter((t) => t.id !== id));
  }

  private mostrar(tipo: TipoToast, mensagem: string, duracaoMs: number): void {
    const toast: Toast = { id: proximoId++, tipo, mensagem };
    this._toasts.update((lista) => [...lista, toast]);
    setTimeout(() => this.remover(toast.id), duracaoMs);
  }
}
