import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, finalize, throwError } from 'rxjs';
import { ErroApi } from '../models/empreendimento.model';
import { LoadingService } from '../services/loading.service';
import { ToastService } from '../services/toast.service';

/**
 * Interceptor funcional (padrão Angular 17+) que:
 * 1. Ativa/desativa o indicador de carregamento global em toda chamada HTTP.
 * 2. Traduz erros da API (formato padronizado pelo middleware do backend)
 *    em notificações toast legíveis pelo usuário.
 */
export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const loading = inject(LoadingService);
  const toast = inject(ToastService);

  loading.iniciar();

  return next(req).pipe(
    catchError((erro: HttpErrorResponse) => {
      const corpo = erro.error as ErroApi | undefined;

      if (erro.status === 0) {
        toast.erro('Não foi possível conectar ao servidor. Verifique sua conexão.');
      } else if (corpo?.mensagem) {
        toast.erro(corpo.mensagem);
      } else {
        toast.erro('Ocorreu um erro inesperado. Tente novamente.');
      }

      return throwError(() => erro);
    }),
    finalize(() => loading.finalizar())
  );
};
