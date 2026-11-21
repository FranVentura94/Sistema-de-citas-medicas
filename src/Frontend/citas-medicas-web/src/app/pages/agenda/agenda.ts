import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { Button } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { Select } from 'primeng/select';
import { Menu } from 'primeng/menu';
import { AgendaService, DirectoryService } from '../../core/api.services';
import { AuthService } from '../../core/auth.service';
import { CitaDetalle } from '../../shared/cita-detalle';
import { Cita, EstadoCita, Medico } from '../../core/models';
import { TRANSICIONES, fechaParam, leerFecha, mensajeError } from '../../core/utils';

const ACCIONES: Record<string, { texto: string; icono: string }> = {
  CONFIRMADA: { texto: 'Confirmar asistencia', icono: 'pi pi-check' },
  EN_ESPERA: { texto: 'Paciente en sala de espera', icono: 'pi pi-hourglass' },
  NO_ASISTIO: { texto: 'Marcar como no asistió', icono: 'pi pi-user-minus' },
  CANCELADA: { texto: 'Cancelar cita', icono: 'pi pi-times' }
};

@Component({
  selector: 'app-agenda',
  imports: [DatePipe, FormsModule, RouterLink, TableModule, Button, DatePicker, Select, Menu, CitaDetalle],
  template: `
    <div class="pagina-encabezado">
      <div>
        <h1>Agenda del día</h1>
        <p class="capital">{{ fecha() | date: 'EEEE d \\'de\\' MMMM \\'de\\' y' }}</p>
      </div>
      @if (!esEnfermeria()) {
        <p-button label="Agendar cita" icon="pi pi-calendar-plus" routerLink="/agendar" />
      }
    </div>
    @if (aviso()) { <div class="aviso-rol"><i class="pi pi-info-circle"></i> {{ aviso() }}</div> }

    <!-- ============ Filtros ============ -->
    <div class="tarjeta filtros">
      <div class="dias">
        <p-button icon="pi pi-chevron-left" [text]="true" [rounded]="true" (onClick)="moverDia(-1)" />
        <p-datepicker [ngModel]="fecha()" (ngModelChange)="cambiarFecha($event)" dateFormat="dd/mm/yy" [showIcon]="true" />
        <p-button icon="pi pi-chevron-right" [text]="true" [rounded]="true" (onClick)="moverDia(1)" />
        <p-button label="Hoy" [outlined]="true" size="small" (onClick)="irHoy()" />
      </div>
      <p-select [options]="medicosVisibles()" [ngModel]="medicoId()" (ngModelChange)="medicoId.set($event)" optionLabel="nombreCompleto"
                optionValue="medicoID" [showClear]="true" placeholder="Todos los médicos" />
      <p-select [options]="estados()" [ngModel]="estado()" (ngModelChange)="estado.set($event)" optionLabel="nombre"
                optionValue="codigo" [showClear]="true" placeholder="Todos los estados" />
    </div>

    <!-- ============ Contadores por estado ============ -->
    <div class="contadores">
      @for (e of estados(); track e.codigo) {
        <button type="button" class="contador" [class.activo]="estado() === e.codigo" (click)="estado.set(estado() === e.codigo ? null : e.codigo)">
          <span class="punto" [style.background]="e.color"></span> {{ e.nombre }} <strong>{{ conteo(e.codigo) }}</strong>
        </button>
      }
    </div>

    <div class="tarjeta sin-padding">
      <p-table [value]="filtradas()" [loading]="cargando()" [rowHover]="true">
        <ng-template #header>
          <tr><th>Hora</th><th>Paciente</th><th>Médico</th><th>Consultorio</th><th>Motivo</th><th>Estado</th><th></th></tr>
        </ng-template>
        <ng-template #body let-c>
          <tr class="fila" (click)="detalle.abrir(c)">
            <td class="hora">{{ leer(c.fechaHoraInicio) | date: 'HH:mm' }}<span>{{ leer(c.fechaHoraFin) | date: 'HH:mm' }}</span></td>
            <td><strong>{{ c.pacienteNombre }}</strong><br /><small class="texto-suave">DUI {{ c.pacienteDocumento }}</small></td>
            <td>{{ c.medicoNombre }}<br /><small class="texto-suave">{{ c.especialidadNombre }}</small></td>
            <td>{{ c.consultorioNombre }}</td>
            <td class="motivo">{{ c.motivoConsulta || '—' }}</td>
            <td><span class="estado" [style.color]="c.estadoColor" [style.background]="c.estadoColor + '1a'">{{ c.estadoNombre }}</span></td>
            <td class="derecha">
              @if (acciones(c).length) {
                <p-button icon="pi pi-ellipsis-v" [rounded]="true" [text]="true" severity="secondary"
                          (onClick)="abrirMenu(menu, $event, c); $event.stopPropagation()" (click)="$event.stopPropagation()" />
              }
            </td>
          </tr>
        </ng-template>
        <ng-template #emptymessage>
          <tr><td colspan="7"><div class="vacio"><i class="pi pi-calendar-times"></i>No hay citas con estos filtros.</div></td></tr>
        </ng-template>
      </p-table>
    </div>

    <p-menu #menu [model]="menuItems()" [popup]="true" appendTo="body" />
    <app-cita-detalle #detalle (actualizada)="reemplazar($event)" />
  `,
  styles: `
    .capital::first-letter { text-transform: uppercase; }
    .filtros { display: flex; gap: 1rem; flex-wrap: wrap; align-items: center; margin-bottom: 1rem; padding: .9rem 1.25rem; }
    .dias { display: flex; align-items: center; gap: .25rem; margin-right: auto; }
    .contadores { display: flex; gap: .5rem; flex-wrap: wrap; margin-bottom: 1rem; }
    .contador {
      display: inline-flex; align-items: center; gap: .45rem; padding: .45rem .85rem; border-radius: 999px; cursor: pointer; font: inherit; font-size: .82rem;
      border: 1px solid var(--mc-borde); background: #fff;
      strong { margin-left: .2rem; }
      &.activo { border-color: var(--mc-primario); box-shadow: 0 0 0 3px #ccfbf1; }
    }
    .punto { width: 9px; height: 9px; border-radius: 50%; }
    .sin-padding { padding: 0; overflow: hidden; }
    .hora { font-weight: 700; color: var(--mc-primario-oscuro); font-size: 1rem; span { display: block; font-size: .75rem; font-weight: 400; color: var(--mc-suave); } }
    .motivo { max-width: 220px; color: var(--mc-suave); }
    .derecha { text-align: right; }
    .fila { cursor: pointer; }
    .aviso-rol { margin: -.5rem 0 1rem; font-size: .85rem; color: #1d4ed8; background: #eff6ff; border-radius: 10px; padding: .55rem .8rem; }
  `
})
export class Agenda {
  private agenda = inject(AgendaService);
  private directory = inject(DirectoryService);
  private toast = inject(MessageService);
  private confirmacion = inject(ConfirmationService);
  private route = inject(ActivatedRoute);
  private auth = inject(AuthService);

  protected leer = leerFecha;
  protected fecha = signal(this.fechaInicial());
  protected citas = signal<Cita[]>([]);
  protected medicos = signal<Medico[]>([]);
  protected estados = signal<EstadoCita[]>([]);
  protected medicoId = signal<number | null>(null);
  protected estado = signal<string | null>(null);
  protected cargando = signal(true);
  protected menuItems = signal<MenuItem[]>([]);
  private permitidos = signal<number[] | null>(null);   // null = todos los médicos

  protected esEnfermeria = computed(() => this.auth.tieneRol('Enfermeria'));
  protected medicosVisibles = computed(() => {
    const ids = this.permitidos();
    return ids ? this.medicos().filter(m => ids.includes(m.medicoID)) : this.medicos();
  });
  protected aviso = computed(() =>
    this.auth.tieneRol('Medico') ? 'Ves tus citas y las de las agendas que otros médicos te compartieron.'
      : this.esEnfermeria() ? 'Ves las citas de los médicos a los que asistes. Haz clic en un paciente para tomarle los signos (triaje).' : null);

  protected filtradas = computed(() =>
    this.citas().filter(c =>
      (!this.medicoId() || c.medicoId === this.medicoId()) && (!this.estado() || c.estadoCodigo === this.estado())));

  constructor() {
    this.directory.medicos().subscribe(m => this.medicos.set(m));
    this.agenda.estados().subscribe(e => this.estados.set(e));
    this.agenda.medicosPermitidos().subscribe(p => this.permitidos.set(p.length ? p.map(x => x.medicoId) : null));
    this.cargar();
  }

  // Abre el día que manda "Agendar cita" (?fecha=2026-11-23) o el día de hoy
  private fechaInicial(): Date {
    const f = this.route.snapshot.queryParamMap.get('fecha');
    return f ? leerFecha(f) : new Date();
  }

  irHoy(): void {
    this.cambiarFecha(new Date());
  }

  conteo(codigo: string): number {
    return this.citas().filter(c => c.estadoCodigo === codigo && (!this.medicoId() || c.medicoId === this.medicoId())).length;
  }

  cambiarFecha(f: Date): void {
    this.fecha.set(f);
    this.cargar();
  }

  moverDia(dias: number): void {
    const f = new Date(this.fecha());
    f.setDate(f.getDate() + dias);
    this.cambiarFecha(f);
  }

  cargar(): void {
    const f = this.fecha();
    const desde = fechaParam(f);
    const hasta = fechaParam(new Date(f.getFullYear(), f.getMonth(), f.getDate() + 1));
    this.cargando.set(true);
    this.agenda.citas({ desde, hasta }).subscribe({
      next: c => { this.citas.set(c); this.cargando.set(false); },
      error: err => { this.cargando.set(false); this.toast.add({ severity: 'error', summary: 'Error', detail: mensajeError(err) }); }
    });
  }

  acciones(c: Cita): string[] {
    const todas = TRANSICIONES[c.estadoCodigo] ?? [];
    return this.esEnfermeria() ? todas.filter(e => e !== 'CANCELADA') : todas;
  }

  reemplazar(actualizada: Cita): void {
    this.citas.update(lista => lista.map(x => (x.citaId === actualizada.citaId ? actualizada : x)));
  }

  abrirMenu(menu: Menu, evento: Event, c: Cita): void {
    this.menuItems.set(this.acciones(c).map(codigo => ({
      label: ACCIONES[codigo].texto,
      icon: ACCIONES[codigo].icono,
      styleClass: codigo === 'CANCELADA' ? 'opcion-peligro' : undefined,
      command: () => this.cambiarEstado(c, codigo)
    })));
    menu.toggle(evento);
  }

  private cambiarEstado(c: Cita, codigo: string): void {
    const ejecutar = () => this.agenda.cambiarEstado(c.citaId, codigo).subscribe({
      next: actualizada => {
        this.citas.update(lista => lista.map(x => (x.citaId === actualizada.citaId ? actualizada : x)));
        this.toast.add({ severity: 'success', summary: 'Cita actualizada', detail: `${actualizada.pacienteNombre}: ${actualizada.estadoNombre}` });
      },
      error: err => this.toast.add({ severity: 'error', summary: 'No se pudo actualizar', detail: mensajeError(err) })
    });

    if (codigo === 'CANCELADA' || codigo === 'NO_ASISTIO') {
      this.confirmacion.confirm({
        header: codigo === 'CANCELADA' ? 'Cancelar cita' : 'Paciente no asistió',
        message: `¿Seguro que deseas marcar la cita de <b>${c.pacienteNombre}</b> como "${codigo === 'CANCELADA' ? 'Cancelada' : 'No asistió'}"? El horario quedará libre.`,
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Sí, continuar',
        rejectLabel: 'No',
        acceptButtonStyleClass: 'p-button-danger',
        rejectButtonStyleClass: 'p-button-text p-button-secondary',
        accept: ejecutar
      });
    } else {
      ejecutar();
    }
  }
}
