import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'empreendimentos',
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/empreendimentos/lista/lista-empreendimentos.component').then(
            (m) => m.ListaEmpreendimentosComponent
          )
      },
      {
        path: 'novo',
        loadComponent: () =>
          import('./features/empreendimentos/formulario/formulario-empreendimento.component').then(
            (m) => m.FormularioEmpreendimentoComponent
          )
      },
      {
        path: ':id/editar',
        loadComponent: () =>
          import('./features/empreendimentos/formulario/formulario-empreendimento.component').then(
            (m) => m.FormularioEmpreendimentoComponent
          )
      }
    ]
  },
  { path: '', redirectTo: 'empreendimentos', pathMatch: 'full' },
  { path: '**', redirectTo: 'empreendimentos' }
];
