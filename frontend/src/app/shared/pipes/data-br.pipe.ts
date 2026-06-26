import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'dataBr',
  standalone: true
})
export class DataBrPipe implements PipeTransform {
  transform(valor: string | null | undefined): string {
    if (!valor) return '—';

    const data = new Date(valor);
    if (isNaN(data.getTime())) return '—';

    return data.toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
