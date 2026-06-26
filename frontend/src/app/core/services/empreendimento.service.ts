import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AtualizarEmpreendimentoPayload,
  CriarEmpreendimentoPayload,
  Empreendimento,
  FiltroEmpreendimento,
  ResultadoPaginado
} from '../models/empreendimento.model';

@Injectable({ providedIn: 'root' })
export class EmpreendimentoService {
  private readonly baseUrl = `${environment.apiUrl}/empreendimentos`;

  constructor(private http: HttpClient) {}

  listar(filtro: FiltroEmpreendimento): Observable<ResultadoPaginado<Empreendimento>> {
    let params = new HttpParams()
      .set('ordenarPor', filtro.ordenarPor)
      .set('decrescente', filtro.decrescente)
      .set('pagina', filtro.pagina)
      .set('tamanhoPagina', filtro.tamanhoPagina);

    if (filtro.nome) {
      params = params.set('nome', filtro.nome);
    }
    if (filtro.status) {
      params = params.set('status', filtro.status);
    }

    return this.http.get<ResultadoPaginado<Empreendimento>>(this.baseUrl, { params });
  }

  obterPorId(id: string): Observable<Empreendimento> {
    return this.http.get<Empreendimento>(`${this.baseUrl}/${id}`);
  }

  criar(payload: CriarEmpreendimentoPayload): Observable<Empreendimento> {
    return this.http.post<Empreendimento>(this.baseUrl, payload);
  }

  atualizar(id: string, payload: AtualizarEmpreendimentoPayload): Observable<Empreendimento> {
    return this.http.put<Empreendimento>(`${this.baseUrl}/${id}`, payload);
  }

  inativar(id: string): Observable<Empreendimento> {
    return this.http.patch<Empreendimento>(`${this.baseUrl}/${id}/inativar`, {});
  }
}
