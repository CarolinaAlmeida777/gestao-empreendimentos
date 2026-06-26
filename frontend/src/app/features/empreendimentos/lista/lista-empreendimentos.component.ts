import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { Empreendimento, FiltroEmpreendimento, OrdenarPor, ResultadoPaginado, StatusEmpreendimento } from '../../../core/models/empreendimento.model';
import { EmpreendimentoService } from '../../../core/services/empreendimento.service';
import { LoadingService } from '../../../core/services/loading.service';
import { ToastService } from '../../../core/services/toast.service';
import { DataBrPipe } from '../../../shared/pipes/data-br.pipe';

@Component({
  selector: 'app-lista-empreendimentos',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, DataBrPipe],
  templateUrl: './lista-empreendimentos.component.html',
  styleUrl: './lista-empreendimentos.component.scss'
})
export class ListaEmpreendimentosComponent implements OnInit {
  private readonly empreendimentoService = inject(EmpreendimentoService);
  private readonly toastService = inject(ToastService);
  readonly loadingService = inject(LoadingService);

  readonly resultado = signal<ResultadoPaginado<Empreendimento> | null>(null);
  readonly filtroNome = signal<string>('');
  readonly filtroStatus = signal<StatusEmpreendimento | undefined>(undefined);
  readonly ordenarPor = signal<OrdenarPor>('nome');
  readonly decrescente = signal(false);
  readonly pagina = signal(1);
  readonly itemParaInativar = signal<Empreendimento | null>(null);

  private readonly TAMANHO_PAGINA = 10;
  private readonly busca$ = new Subject<string>();

  ngOnInit(): void {
    this.busca$.pipe(debounceTime(350), distinctUntilChanged()).subscribe((termo) => {
      this.filtroNome.set(termo);
      this.pagina.set(1);
      this.carregar();
    });

    this.carregar();
  }

  onFiltroNomeChange(valor: string): void {
    this.busca$.next(valor);
  }

  onFiltroStatusChange(valor: StatusEmpreendimento | undefined): void {
    this.filtroStatus.set(valor);
    this.pagina.set(1);
    this.carregar();
  }

  onOrdenarPorChange(valor: OrdenarPor): void {
    this.ordenarPor.set(valor);
    this.carregar();
  }

  alternarDirecao(): void {
    this.decrescente.set(!this.decrescente());
    this.carregar();
  }

  irParaPagina(pagina: number): void {
    this.pagina.set(pagina);
    this.carregar();
  }

  confirmarInativacao(item: Empreendimento): void {
    this.itemParaInativar.set(item);
  }

  cancelarInativacao(): void {
    this.itemParaInativar.set(null);
  }

  inativar(): void {
    const item = this.itemParaInativar();
    if (!item) return;

    this.empreendimentoService.inativar(item.id).subscribe({
      next: () => {
        this.toastService.sucesso(`"${item.nome}" foi inativado com sucesso.`);
        this.itemParaInativar.set(null);
        this.carregar();
      },
      error: () => this.itemParaInativar.set(null)
    });
  }

  private carregar(): void {
    const filtro: FiltroEmpreendimento = {
      nome: this.filtroNome() || undefined,
      status: this.filtroStatus(),
      ordenarPor: this.ordenarPor(),
      decrescente: this.decrescente(),
      pagina: this.pagina(),
      tamanhoPagina: this.TAMANHO_PAGINA
    };

    this.empreendimentoService.listar(filtro).subscribe({
      next: (res) => this.resultado.set(res)
    });
  }
}
