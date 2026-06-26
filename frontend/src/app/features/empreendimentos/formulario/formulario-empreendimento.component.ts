import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EmpreendimentoService } from '../../../core/services/empreendimento.service';
import { ToastService } from '../../../core/services/toast.service';
import { StatusEmpreendimento } from '../../../core/models/empreendimento.model';
import { aplicarMascaraCnpj, cnpjValidator, somenteDigitos } from '../../../shared/validators/cnpj.validator';

@Component({
  selector: 'app-formulario-empreendimento',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './formulario-empreendimento.component.html',
  styleUrl: './formulario-empreendimento.component.scss'
})
export class FormularioEmpreendimentoComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly empreendimentoService = inject(EmpreendimentoService);
  private readonly toastService = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.minLength(3)]],
    cnpj: ['', [Validators.required, cnpjValidator()]],
    endereco: ['']
  });

  readonly modoEdicao = signal(false);
  readonly carregandoInicial = signal(false);
  readonly salvando = signal(false);
  readonly statusAtual = signal<StatusEmpreendimento>('Ativo');

  private empreendimentoId: string | null = null;

  ngOnInit(): void {
    // Aplica a máscara visual de CNPJ conforme o usuário digita.
    this.form.controls.cnpj.valueChanges.subscribe((valor) => {
      const mascarado = aplicarMascaraCnpj(valor);
      if (mascarado !== valor) {
        this.form.controls.cnpj.setValue(mascarado, { emitEvent: false });
      }
    });

    this.empreendimentoId = this.route.snapshot.paramMap.get('id');

    if (this.empreendimentoId) {
      this.modoEdicao.set(true);
      this.carregarParaEdicao(this.empreendimentoId);
    }
  }

  campoInvalido(nome: 'nome' | 'cnpj'): boolean {
    const controle = this.form.controls[nome];
    return controle.invalid && (controle.dirty || controle.touched);
  }

  mensagemErro(nome: 'nome' | 'cnpj'): string {
    const controle = this.form.controls[nome];

    if (controle.hasError('required')) return 'Este campo é obrigatório.';
    if (nome === 'nome' && controle.hasError('minlength')) return 'O nome deve possuir pelo menos 3 caracteres.';
    if (nome === 'cnpj' && controle.hasError('cnpjInvalido')) return 'CNPJ inválido. Verifique os números digitados.';

    return 'Valor inválido.';
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const valores = this.form.getRawValue();
    const payload = {
      nome: valores.nome.trim(),
      cnpj: somenteDigitos(valores.cnpj),
      endereco: valores.endereco?.trim() || null
    };

    this.salvando.set(true);

    const operacao = this.modoEdicao() && this.empreendimentoId
      ? this.empreendimentoService.atualizar(this.empreendimentoId, payload)
      : this.empreendimentoService.criar(payload);

    operacao.subscribe({
      next: (resultado) => {
        this.toastService.sucesso(
          this.modoEdicao()
            ? `"${resultado.nome}" foi atualizado com sucesso.`
            : `"${resultado.nome}" foi cadastrado com sucesso.`
        );
        this.router.navigate(['/empreendimentos']);
      },
      error: () => this.salvando.set(false)
    });
  }

  private carregarParaEdicao(id: string): void {
    this.carregandoInicial.set(true);

    this.empreendimentoService.obterPorId(id).subscribe({
      next: (empreendimento) => {
        this.statusAtual.set(empreendimento.status);

        this.form.patchValue({
          nome: empreendimento.nome,
          cnpj: empreendimento.cnpj,
          endereco: empreendimento.endereco ?? ''
        });

        // Regra de negócio: empreendimentos inativos não podem ser editados.
        if (empreendimento.status === 'Inativo') {
          this.form.disable();
          this.toastService.erro('Este empreendimento está inativo e não pode ser editado.');
        }

        this.carregandoInicial.set(false);
      },
      error: () => {
        this.carregandoInicial.set(false);
        this.router.navigate(['/empreendimentos']);
      }
    });
  }
}
