import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private contador = 0;
  private readonly _carregando = signal(false);
  readonly carregando = this._carregando.asReadonly();

  iniciar(): void {
    this.contador++;
    this._carregando.set(true);
  }

  finalizar(): void {
    this.contador = Math.max(0, this.contador - 1);
    if (this.contador === 0) {
      this._carregando.set(false);
    }
  }
}
