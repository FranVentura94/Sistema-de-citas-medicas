import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Select } from 'primeng/select';
import { DatePicker } from 'primeng/datepicker';
import { Textarea } from 'primeng/textarea';
import { Skeleton } from 'primeng/skeleton';
import { AgendaService, DirectoryService } from '../../core/api.services';
import { EspacioDisponible, Especialidad, Medico, Paciente, MedicoPermitido } from '../../core/models';
import { fechaHoraParam, fechaParam, iniciales, leerFecha, mensajeError } from '../../core/utils';

@Component({
  selector: 'app-agendar',
  imports: [DatePipe, FormsModule, Button, Select, DatePicker, Textarea, Skeleton],
  template: `
    <div class="pagina-encabezado">
      <div>
        <h1>Agendar cita</h1>
        <p>Selecciona el paciente, el médico y un espacio disponible.</p>
      </div>
    </div>

    <div class="contenido">
      <div class="pasos">
        <!-- ============ 1. Paciente ============ -->
        <div class="tarjeta">
          <div class="tarjeta-titulo"><span class="num">1</span> Paciente</div>
          <p-select [options]="pacientes()" [ngModel]="pacienteId()" (ngModelChange)="pacienteId.set($event)"
                    optionValue="pacienteID" [filter]="true" filterBy="nombres,apellidos,numeroDocumento"
                    placeholder="Buscar paciente por nombre o DUI" class="w-full" [loading]="pacientes().length === 0">
            <ng-template #selectedItem let-p>{{ p.nombres }} {{ p.apellidos }} · {{ p.numeroDocumento }}</ng-template>
            <ng-template #item let-p>
              <div class="opcion"><strong>{{ p.nombres }} {{ p.apellidos }}</strong><span>{{ p.tipoDocumento }} {{ p.numeroDocumento }}</span></div>
            </ng-template>
          </p-select>
        </div>

        <!-- ============ 2. Especialidad y médico ============ -->
        <div class="tarjeta">
          <div class="tarjeta-titulo"><span class="num">2</span> Especialidad y médico</div>
          <div class="chips">
            @for (e of especialidades(); track e.especialidadID) {
              <button type="button" [class.activo]="especialidadId() === e.especialidadID" (click)="elegirEspecialidad(e.especialidadID)">
                {{ e.nombre }}
              </button>
            }
          </div>

          @if (especialidadId()) {
            <div class="medicos">
              @for (m of medicos(); track m.medicoID) {
                <button type="button" class="medico" [class.activo]="medico()?.medicoID === m.medicoID" (click)="elegirMedico(m)">
                  <span class="av">{{ ini(m.nombreCompleto) }}</span>
                  <div>
                    <strong>{{ m.nombreCompleto }}</strong>
                    <span>{{ nombresEspecialidades(m) }}</span>
                  </div>
                  @if (medico()?.medicoID === m.medicoID) { <i class="pi pi-check-circle"></i> }
                </button>
              } @empty {
                <p class="texto-suave">No hay médicos para esta especialidad.</p>
              }
            </div>
          }
        </div>

        <!-- ============ 3. Fecha y hora ============ -->
        <div class="tarjeta">
          <div class="tarjeta-titulo"><span class="num">3</span> Fecha y hora</div>
          @if (!medico()) {
            <p class="texto-suave">Primero elige un médico para ver sus horarios.</p>
          } @else {
            <div class="fecha-hora">
              <p-datepicker [inline]="true" [ngModel]="fecha()" (ngModelChange)="elegirFecha($event)" [minDate]="hoy" />
              <div class="espacios">
                <h4>{{ fecha() | date: 'EEEE d \\'de\\' MMMM' }}</h4>
                @if (cargandoEspacios()) {
                  <div class="rejilla">@for (i of [1,2,3,4,5,6,7,8]; track i) { <p-skeleton height="2.4rem" /> }</div>
                } @else if (espacios().length === 0) {
                  <div class="vacio"><i class="pi pi-calendar-times"></i>{{ medico()!.nombreCompleto }} no atiende este día.</div>
                } @else {
                  <div class="rejilla">
                    @for (e of espacios(); track e.inicio) {
                      <button type="button" class="hora" [disabled]="!e.disponible" [class.activo]="espacio()?.inicio === e.inicio" (click)="espacio.set(e)">
                        {{ leer(e.inicio) | date: 'HH:mm' }}
                      </button>
                    }
                  </div>
                  <div class="leyenda"><span class="libre"></span> Disponible <span class="ocupado"></span> Ocupado</div>
                }
              </div>
            </div>
          }
        </div>
      </div>

      <!-- ============ Resumen ============ -->
      <aside class="tarjeta resumen">
        <div class="tarjeta-titulo"><i class="pi pi-file-check"></i> Resumen de la cita</div>
        <dl>
          <dt>Paciente</dt><dd>{{ paciente() ? paciente()!.nombres + ' ' + paciente()!.apellidos : '—' }}</dd>
          <dt>Especialidad</dt><dd>{{ especialidadNombre() || '—' }}</dd>
          <dt>Médico</dt><dd>{{ medico()?.nombreCompleto || '—' }}</dd>
          <dt>Fecha</dt><dd>{{ espacio() ? (leer(espacio()!.inicio) | date: 'EEEE d MMM, y') : '—' }}</dd>
          <dt>Hora</dt><dd>{{ espacio() ? (leer(espacio()!.inicio) | date: 'HH:mm') + ' - ' + (leer(espacio()!.fin) | date: 'HH:mm') : '—' }}</dd>
          <dt>Consultorio</dt><dd>{{ espacio()?.consultorioNombre || '—' }}</dd>
        </dl>

        <div class="campo">
          <label>Motivo de consulta</label>
          <textarea pTextarea rows="3" [(ngModel)]="motivo" placeholder="Ej: control general, dolor de garganta..."></textarea>
        </div>

        <p-button label="Confirmar cita" icon="pi pi-check" styleClass="w-full" [disabled]="!listo()" [loading]="guardando()" (onClick)="confirmar()" />
      </aside>
    </div>
  `,
  styles: `
    .contenido { display: grid; grid-template-columns: 1fr 340px; gap: 1.25rem; align-items: start; }
    .pasos { display: flex; flex-direction: column; gap: 1.25rem; }
    .num { width: 26px; height: 26px; border-radius: 50%; display: grid; place-items: center; background: var(--mc-primario); color: #fff; font-size: .8rem; }
    :host ::ng-deep .w-full { width: 100%; }
    .opcion { display: flex; flex-direction: column; span { font-size: .78rem; color: var(--mc-suave); } }

    .chips { display: flex; flex-wrap: wrap; gap: .5rem; }
    .chips button {
      border: 1px solid var(--mc-borde); background: #fff; padding: .5rem 1rem; border-radius: 999px; cursor: pointer; font: inherit;
      &:hover { border-color: var(--mc-primario); }
      &.activo { background: var(--mc-primario); color: #fff; border-color: var(--mc-primario); }
    }
    .medicos { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: .75rem; margin-top: 1rem; }
    .medico {
      display: flex; align-items: center; gap: .75rem; text-align: left; padding: .85rem; border-radius: 12px; cursor: pointer; font: inherit;
      border: 1px solid var(--mc-borde); background: #fff; color: inherit;
      strong { display: block; } span { font-size: .78rem; color: var(--mc-suave); }
      div { flex: 1; }
      i { color: var(--mc-primario); font-size: 1.2rem; }
      &:hover { border-color: var(--mc-primario); }
      &.activo { border-color: var(--mc-primario); background: #f0fdfa; box-shadow: 0 0 0 3px #ccfbf1; }
    }
    .av { width: 40px; height: 40px; border-radius: 50%; display: grid; place-items: center; background: #ccfbf1; color: #0f766e; font-weight: 700; font-size: .85rem !important; }

    .fecha-hora { display: grid; grid-template-columns: auto 1fr; gap: 1.5rem; }
    .espacios h4 { margin-bottom: .9rem; } .espacios h4::first-letter, dd::first-letter { text-transform: uppercase; }
    .rejilla { display: grid; grid-template-columns: repeat(auto-fill, minmax(78px, 1fr)); gap: .5rem; }
    .hora {
      padding: .6rem 0; border-radius: 10px; border: 1px solid #99f6e4; background: #f0fdfa; color: #0f766e; font: inherit; font-weight: 600; cursor: pointer;
      &:hover:not(:disabled) { background: #ccfbf1; }
      &.activo { background: var(--mc-primario); color: #fff; border-color: var(--mc-primario); }
      &:disabled { background: #f1f5f9; color: #94a3b8; border-color: #e2e8f0; text-decoration: line-through; cursor: not-allowed; }
    }
    .leyenda { display: flex; align-items: center; gap: .4rem; margin-top: 1rem; font-size: .78rem; color: var(--mc-suave);
      span { width: 12px; height: 12px; border-radius: 4px; display: inline-block; margin-left: .6rem; }
      .libre { background: #ccfbf1; border: 1px solid #99f6e4; } .ocupado { background: #f1f5f9; border: 1px solid #e2e8f0; } }

    .resumen { position: sticky; top: 84px; display: flex; flex-direction: column; gap: 1rem; }
    dl { display: grid; grid-template-columns: auto 1fr; gap: .55rem 1rem; margin: 0; }
    dt { color: var(--mc-suave); font-size: .85rem; }
    dd { margin: 0; font-weight: 600; text-align: right; }

    @media (max-width: 1200px) { .contenido { grid-template-columns: 1fr; } .resumen { position: static; } }
    @media (max-width: 700px) { .fecha-hora { grid-template-columns: 1fr; } }
  `
})
export class Agendar {
  private directory = inject(DirectoryService);
  private agenda = inject(AgendaService);
  private toast = inject(MessageService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  protected hoy = new Date(new Date().setHours(0, 0, 0, 0));
  protected leer = leerFecha;
  protected ini = iniciales;

  protected pacientes = signal<Paciente[]>([]);
  protected especialidades = signal<Especialidad[]>([]);
  protected medicos = signal<Medico[]>([]);
  protected espacios = signal<EspacioDisponible[]>([]);

  protected pacienteId = signal<number | null>(null);
  protected especialidadId = signal<number | null>(null);
  protected medico = signal<Medico | null>(null);
  protected fecha = signal<Date>(new Date());
  protected espacio = signal<EspacioDisponible | null>(null);
  protected motivo = '';

  protected cargandoEspacios = signal(false);
  protected guardando = signal(false);

  protected paciente = computed(() => this.pacientes().find(p => p.pacienteID === this.pacienteId()) ?? null);
  protected especialidadNombre = computed(() => this.especialidades().find(e => e.especialidadID === this.especialidadId())?.nombre);
  protected listo = computed(() => !!this.paciente() && !!this.medico() && !!this.espacio());

  // Médico: solo puede agendar en su agenda y en las compartidas (lista vacía = todas)
  private permitidos = signal<MedicoPermitido[]>([]);
  private permitido(m: Medico): boolean {
    const lista = this.permitidos();
    if (lista.length === 0) return true;
    const p = lista.find(x => x.medicoId === m.medicoID);
    return !!p && (p.agendaCompleta || p.pacienteIds.includes(this.pacienteId() ?? -1));
  }

  constructor() {
    this.directory.pacientes().subscribe(p => {
      this.pacientes.set(p.filter(x => x.activo));
      // Si viene desde la pantalla de pacientes, se preselecciona
      const id = Number(this.route.snapshot.queryParamMap.get('pacienteId'));
      if (id) this.pacienteId.set(id);
    });
    this.directory.especialidades().subscribe(e => this.especialidades.set(e));
    this.agenda.medicosPermitidos().subscribe(p => this.permitidos.set(p));
  }

  nombresEspecialidades(m: Medico): string {
    return m.especialidades.map(e => e.nombre).join(' · ');
  }

  elegirEspecialidad(id: number): void {
    this.especialidadId.set(id);
    this.medico.set(null);
    this.espacio.set(null);
    this.medicos.set([]);
    this.directory.medicos(id).subscribe(m => this.medicos.set(m.filter(x => x.activo && this.permitido(x))));
  }

  elegirMedico(m: Medico): void {
    this.medico.set(m);
    this.cargarEspacios();
  }

  elegirFecha(f: Date): void {
    this.fecha.set(f);
    this.cargarEspacios();
  }

  private cargarEspacios(): void {
    const medico = this.medico();
    if (!medico) return;
    this.espacio.set(null);
    this.cargandoEspacios.set(true);
    this.agenda.disponibilidad(medico.medicoID, fechaParam(this.fecha())).subscribe({
      next: e => { this.espacios.set(e); this.cargandoEspacios.set(false); },
      error: err => {
        this.cargandoEspacios.set(false);
        this.toast.add({ severity: 'error', summary: 'Error', detail: mensajeError(err) });
      }
    });
  }

  confirmar(): void {
    const paciente = this.paciente();
    const medico = this.medico();
    const espacio = this.espacio();
    if (!paciente || !medico || !espacio) return;

    this.guardando.set(true);
    this.agenda.crearCita({
      pacienteId: paciente.pacienteID,
      medicoId: medico.medicoID,
      especialidadId: this.especialidadId(),
      fechaHoraInicio: fechaHoraParam(leerFecha(espacio.inicio)),
      motivoConsulta: this.motivo || null
    }).subscribe({
      next: c => {
        this.guardando.set(false);
        this.toast.add({
          severity: 'success',
          summary: '¡Cita agendada!',
          detail: `${c.pacienteNombre} con ${c.medicoNombre} en ${c.consultorioNombre}`,
          life: 5000
        });
        this.router.navigate(['/agenda'], { queryParams: { fecha: fechaParam(leerFecha(c.fechaHoraInicio)) } });
      },
      error: err => {
        this.guardando.set(false);
        this.toast.add({ severity: 'error', summary: 'No se pudo agendar', detail: mensajeError(err) });
        this.cargarEspacios();
      }
    });
  }
}
