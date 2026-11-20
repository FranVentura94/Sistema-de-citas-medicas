import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { Skeleton } from 'primeng/skeleton';
import { AgendaService, ClinicalService } from '../../core/api.services';
import { Cita } from '../../core/models';
import { fechaParam, iniciales, leerFecha, mensajeError } from '../../core/utils';

const ATENDIBLES = ['PROGRAMADA', 'CONFIRMADA', 'EN_ESPERA', 'EN_ATENCION'];

@Component({
  selector: 'app-mis-citas',
  imports: [DatePipe, FormsModule, Button, DatePicker, Skeleton],
  template: `
    <div class="pagina-encabezado">
      <div>
        <h1>Mis citas</h1>
        <p class="capital">{{ fecha() | date: 'EEEE d \\'de\\' MMMM' }} · {{ citas().length }} pacientes</p>
      </div>
      <div class="dias">
        <p-button icon="pi pi-chevron-left" [text]="true" [rounded]="true" (onClick)="moverDia(-1)" />
        <p-datepicker [ngModel]="fecha()" (ngModelChange)="cambiarFecha($event)" dateFormat="dd/mm/yy" [showIcon]="true" />
        <p-button icon="pi pi-chevron-right" [text]="true" [rounded]="true" (onClick)="moverDia(1)" />
      </div>
    </div>

    @if (cargando()) {
      <div class="lista">@for (i of [1, 2, 3]; track i) { <p-skeleton height="6rem" /> }</div>
    } @else if (citas().length === 0) {
      <div class="tarjeta vacio"><i class="pi pi-sun"></i>No tienes citas para este día.</div>
    } @else {
      <div class="lista">
        @for (c of citas(); track c.citaId) {
          <div class="tarjeta cita" [class.en-curso]="c.estadoCodigo === 'EN_ATENCION'">
            <div class="hora">
              <strong>{{ leer(c.fechaHoraInicio) | date: 'HH:mm' }}</strong>
              <span>{{ leer(c.fechaHoraFin) | date: 'HH:mm' }}</span>
            </div>
            <div class="avatar">{{ ini(c.pacienteNombre) }}</div>
            <div class="info">
              <strong>{{ c.pacienteNombre }}</strong>
              <span><i class="pi pi-id-card"></i> DUI {{ c.pacienteDocumento }} · <i class="pi pi-building"></i> {{ c.consultorioNombre }}</span>
              <span class="motivo"><i class="pi pi-comment"></i> {{ c.motivoConsulta || 'Sin motivo registrado' }}</span>
            </div>
            <span class="estado" [style.color]="c.estadoColor" [style.background]="c.estadoColor + '1a'">{{ c.estadoNombre }}</span>
            <div class="botones">
              <p-button icon="pi pi-folder-open" [text]="true" [rounded]="true" severity="secondary" (onClick)="expediente(c)" title="Expediente" />
              @if (atendible(c)) {
                <p-button [label]="c.estadoCodigo === 'EN_ATENCION' ? 'Continuar' : 'Atender'" icon="pi pi-play"
                          [loading]="abriendo() === c.citaId" (onClick)="atender(c)" />
              } @else if (c.estadoCodigo === 'ATENDIDA') {
                <p-button label="Ver atención" icon="pi pi-eye" [outlined]="true" (onClick)="verAtencion(c)" />
              }
            </div>
          </div>
        }
      </div>
    }
  `,
  styles: `
    .capital::first-letter { text-transform: uppercase; }
    .dias { display: flex; align-items: center; gap: .25rem; }
    .lista { display: flex; flex-direction: column; gap: .85rem; }
    .cita { display: flex; align-items: center; gap: 1.25rem; padding: 1rem 1.25rem; flex-wrap: wrap; }
    .cita.en-curso { border-color: #a78bfa; box-shadow: 0 0 0 3px #ede9fe; }
    .hora { min-width: 64px; text-align: center; padding-right: 1.25rem; border-right: 1px solid var(--mc-borde);
      strong { display: block; font-size: 1.3rem; color: var(--mc-primario-oscuro); } span { color: var(--mc-suave); font-size: .8rem; } }
    .avatar { width: 46px; height: 46px; border-radius: 50%; display: grid; place-items: center; background: #ccfbf1; color: #0f766e; font-weight: 700; }
    .info { flex: 1; min-width: 220px; display: flex; flex-direction: column; gap: .2rem;
      strong { font-size: 1.05rem; } span { font-size: .82rem; color: var(--mc-suave); } i { font-size: .75rem; } }
    .motivo { font-style: italic; }
    .botones { display: flex; gap: .5rem; align-items: center; }
  `
})
export class MisCitas {
  private agenda = inject(AgendaService);
  private clinical = inject(ClinicalService);
  private toast = inject(MessageService);
  private router = inject(Router);

  protected leer = leerFecha;
  protected ini = iniciales;
  protected fecha = signal(new Date());
  protected citas = signal<Cita[]>([]);
  protected cargando = signal(true);
  protected abriendo = signal<number | null>(null);

  constructor() {
    this.cargar();
  }

  atendible(c: Cita): boolean {
    return ATENDIBLES.includes(c.estadoCodigo);
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
    this.cargando.set(true);
    this.agenda.misCitas(fechaParam(this.fecha())).subscribe({
      next: c => { this.citas.set(c); this.cargando.set(false); },
      error: err => { this.cargando.set(false); this.toast.add({ severity: 'error', summary: 'Error', detail: mensajeError(err) }); }
    });
  }

  // Inicia la atención en MS_Clinical (la cita pasa a "En atención" en MS_Agenda)
  atender(c: Cita): void {
    this.abriendo.set(c.citaId);
    this.clinical.iniciar(c.citaId).subscribe({
      next: a => this.router.navigate(['/atencion', a.atencionId]),
      error: err => {
        this.abriendo.set(null);
        this.toast.add({ severity: 'error', summary: 'No se pudo iniciar', detail: mensajeError(err) });
      }
    });
  }

  verAtencion(c: Cita): void {
    this.clinical.porCita(c.citaId).subscribe({
      next: a => this.router.navigate(['/atencion', a.atencionId]),
      error: err => this.toast.add({ severity: 'warn', summary: 'Sin atención', detail: mensajeError(err) })
    });
  }

  expediente(c: Cita): void {
    this.router.navigate(['/expediente', c.pacienteId]);
  }
}
