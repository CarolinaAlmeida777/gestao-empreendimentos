import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Remove qualquer caractere não numérico. */
export function somenteDigitos(valor: string | null | undefined): string {
  return (valor ?? '').replace(/\D/g, '');
}

/** Aplica a máscara visual XX.XXX.XXX/XXXX-XX enquanto o usuário digita. */
export function aplicarMascaraCnpj(valor: string): string {
  const digitos = somenteDigitos(valor).slice(0, 14);

  return digitos
    .replace(/^(\d{2})(\d)/, '$1.$2')
    .replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/\.(\d{3})(\d)/, '.$1/$2')
    .replace(/(\d{4})(\d)/, '$1-$2');
}

/**
 * Valida o dígito verificador do CNPJ no próprio formulário (front),
 * replicando o mesmo algoritmo usado na entidade de domínio do backend,
 * para dar feedback imediato ao usuário antes de chamar a API.
 */
export function cnpjValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const digitos = somenteDigitos(control.value);

    if (!digitos) {
      return null; // deixa o Validators.required cuidar do campo vazio
    }

    if (digitos.length !== 14 || !cnpjMatematicamenteValido(digitos)) {
      return { cnpjInvalido: true };
    }

    return null;
  };
}

function cnpjMatematicamenteValido(cnpj: string): boolean {
  if (new Set(cnpj).size === 1) return false;

  const calcularDigito = (base: string, pesos: number[]): number => {
    const soma = base
      .split('')
      .reduce((acc, char, i) => acc + Number(char) * pesos[i], 0);
    const resto = soma % 11;
    return resto < 2 ? 0 : 11 - resto;
  };

  const pesos1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  const pesos2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

  const digito1 = calcularDigito(cnpj.substring(0, 12), pesos1);
  const digito2 = calcularDigito(cnpj.substring(0, 12) + digito1, pesos2);

  return cnpj === cnpj.substring(0, 12) + digito1 + digito2;
}
