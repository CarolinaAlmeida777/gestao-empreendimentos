export type StatusEmpreendimento = 'Ativo' | 'Inativo';

export interface Empreendimento {
  id: string;
  nome: string;
  cnpj: string;
  endereco: string | null;
  status: StatusEmpreendimento;
  dataCriacao: string;
  dataAtualizacao: string | null;
}

export interface CriarEmpreendimentoPayload {
  nome: string;
  cnpj: string;
  endereco: string | null;
}

export interface AtualizarEmpreendimentoPayload {
  nome: string;
  cnpj: string;
  endereco: string | null;
}

export type OrdenarPor = 'nome' | 'dataCriacao';

export interface FiltroEmpreendimento {
  nome?: string;
  status?: StatusEmpreendimento;
  ordenarPor: OrdenarPor;
  decrescente: boolean;
  pagina: number;
  tamanhoPagina: number;
}

export interface ResultadoPaginado<T> {
  itens: T[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
}

/** Formato de erro retornado pelo middleware global da API. */
export interface ErroApi {
  status: number;
  titulo: string;
  mensagem: string;
  timestamp: string;
}
