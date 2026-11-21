import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { Select } from 'primeng/select';
import { DatePicker } from 'primeng/datepicker';
import { Tooltip } from 'primeng/tooltip';
import { AdminService, Consultorio, Horario } from '../../core/admin.service';
import { DirectoryService } from '../../core/api.services';
import { Medico } from '../../core/models';
import { mensajeError } from '../../core/utils';

// Lunes primero, domingo al final (0 = domingo como en .NET)
const DIAS = [
  { valor: 1, nombre: 'Lunes' }, { valor: 2, nombre: 'Martes' }, { valor: 3, nombre: 'Miércoles' },
  { valor: 4, nombre: 'Jueves' }, { valor: 5, nombre: 'Viernes' }, { valor: 6, nombre: 'Sábado' }, { valor: 0, nombre: 'Domingo' }
];

@Component({
  selector: 'app-admin-horarios',
  imports: [FormsModule, Button, Dialog, Select, DatePicker, Tooltip],
  template: `
    <div class="pagina-encabezado">
      <div>
        <h1>Horarios de atención</h1>
        <p>Días y horas en que cada médico recibe citas</p>
      </div>
      <p-button label="Agregar horario" icon="pi pi-plus" (onClick)="nuevo()" [disabled]="!medicoId()" />
    </div>

    <div class="tarjeta filtro">
      <label>Médico</label>
      <p-select [options]="medicos()" [ngModel]="medicoId()" (ngModelChange)="elegirMedico($event)" optionLabel="nombreCompleto"
                optionValue="medicoID" [filter]="true" placeholder="Selecciona un médico" class="sel-medico" />
      @if (medicoId()) {
        <span class="texto-suave">{{ horarios().length }} bloques · {{ horasSemana() }} horas por semana</span>
      }
    </div>

    @if (!medicoId()) {
      <div class="tarjeta vacio"><i class="pi pi-user"></i>Elige un médico para ver y editar sus horarios.</div>
    } @else {
      <div class="semana">
        @for (d of dias; track d.valor) {
          <div class="tarjeta dia" [class.libre]="bloques(d.valor).length === 0">
            <h3>{{ d.nombre }}</h3>
            @for (h of bloques(d.valor); track h.horarioMedicoId) {
              <div class="bloque">
                <div>
                  <strong>{{ h.horaInicio }} – {{ h.horaFin }}</strong>
                  <span>{{ h.consultorioNombre }} · citas de {{ h.duracionCitaMinutos }} min</span>
                </div>
                <p-button icon="pi pi-trash" [rounded]="true" [text]="true" severity="danger" size="small" pTooltip="Quitar" (onClick)="eliminar(h)" />
              </div>
            } @empty {
              <span class="texto-suave">No atiende</span>
            }
            <button type="button" class="agregar" (click)="nuevo(d.valor)"><i class="pi pi-plus"></i> Agregar</button>
          </div>
        }
      </div>
    }

    <!-- ============ Nuevo bloque de horario ============ -->
    <p-dialog [(visible)]="dialogo" [modal]="true" [style]="{ width: '480px' }" header="Agregar horario" [draggable]="false">
      <div class="formulario">
        <div class="campo completo">
          <label>Día *</label>
          <p-select [options]="dias" [(ngModel)]="form.diaSemana" optionLabel="nombre" optionValue="valor" appendTo="body" />
        </div>
        <div class="campo">
          <label>Desde *</label>
          <p-datepicker [(ngModel)]="form.desde" [timeOnly]="true" hourFormat="24" [stepMinute]="15" appendTo="body" placeholder="08:00" />
        </div>
        <div class="campo">
          <label>Hasta *</label>
          <p-datepicker [(ngModel)]="form.hasta" [timeOnly]="true" hourFormat="24" [stepMinute]="15" appendTo="body" placeholder="12:00" />
        </div>
        <div class="campo">
          <label>Consultorio *</label>
          <p-select [options]="consultorios()" [(ngModel)]="form.consultorioId" optionLabel="nombre" optionValue="consultorioId" appendTo="body" placeholder="Seleccionar" />
        </div>
        <div class="campo">
          <label>Duración de cada cita *</label>
          <p-select [options]="duraciones" [(ngModel)]="form.duracion" optionLabel="texto" optionValue="valor" appendTo="body" />
        </div>
      </div>
      <ng-template #footer>
        <p-button label="Cancelar" severity="secondary" [text]="true" (onClick)="dialogo = false" />
        <p-button label="Agregar" icon="pi pi-check" (onClick)="guardar()" [loading]="guardando()" />
      </ng-template>
    </p-dialog>
  `,
  styles: `
    .filtro { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; margin-bottom: 1.25rem; label { font-weight: 600; } }
    :host ::ng-deep .sel-medico { min-width: 320px; }
    .semana { display: grid; grid-template-columns: repeat(auto-fill, minmax(230px, 1fr)); gap: 1rem; }
    .dia { display: flex; flex-direction: column; gap: .6rem; padding: 1rem;
      h3 { font-size: 1rem; color: var(--mc-primario-oscuro); } }
    .dia.libre { background: #fafafa; }
    .bloque { display: flex; align-items: center; justify-content: space-between; gap: .5rem; padding: .6rem .75rem; border-radius: 10px;
      background: #f0fdfa; border: 1px solid #99f6e4;
      strong { display: block; } span { font-size: .75rem; color: var(--mc-suave); } }
    .agregar { margin-top: auto; border: 1px dashed var(--mc-borde); background: none; border-radius: 10px; padding: .5rem; cursor: pointer;
      color: var(--mc-suave); font: inherit; &:hover { border-color: var(--mc-primario); color: var(--mc-primario-oscuro); } }
    :host ::ng-deep .campo p-datepicker, :host ::ng-deep .campo p-select, :host ::ng-deep .campo input { width: 100%; }
  `
})
export class AdminHorarios {
  private admin = inject(AdminService);
  private directory = inject(DirectoryService);
  private toast = inject(MessageService);
  private confirmacion = inject(ConfirmationService);
  private route = inject(ActivatedRoute);

  protected dias = DIAS;
  protected duraciones = [15, 20, 30, 45, 60].map(v => ({ valor: v, texto: `${v} minutos` }));
  protected medicos = signal<Medico[]>([]);
  protected consultorios = signal<Consultorio[]>([]);
  protected horarios = signal<Horario[]>([]);
  protected medicoId = signal<number | null>(null);
  protected guardando = signal(false);

  protected dialogo = false;
  protected form = { diaSemana: 1, desde: null as Date | null, hasta: null as Date | null, consultorioId: null as number | null, duracion: 30 };

  protected horasSemana = computed(() =>
    Math.round(this.horarios().reduce((t, h) => t + (this.minutos(h.horaFin) - this.minutos(h.horaInicio)), 0) / 6) / 10);

  constructor() {
    this.directory.medicos().subscribe(m => this.medicos.set(m.filter(x => x.activo)));
    this.admin.consultorios().subscribe(c => this.consultorios.set(c));

    // Si viene desde la pantalla de médicos (?medicoId=2), se abre directo
    const id = Number(this.route.snapshot.queryParamMap.get('medicoId'));
    if (id) this.elegirMedico(id);
  }

  bloques(dia: number): Horario[] {
    return this.horarios().filter(h => h.diaSemana === dia).sort((a, b) => a.horaInicio.localeCompare(b.horaInicio));
  }

  elegirMedico(id: number | null): void {
    this.medicoId.set(id);
    if (id) this.cargar();
    else this.horarios.set([]);
  }

  cargar(): void {
    this.admin.horarios(this.medicoId()).subscribe({
      next: h => this.horarios.set(h),
      error: err => this.error(err)
    });
  }

  nuevo(dia = 1): void {
    const hora = (h: number) => { const d = new Date(); d.setHours(h, 0, 0, 0); return d; };
    this.form = { diaSemana: dia, desde: hora(8), hasta: hora(12), consultorioId: this.consultorios()[0]?.consultorioId ?? null, duracion: 30 };
    this.dialogo = true;
  }

  guardar(): void {
    const f = this.form;
    if (!f.desde || !f.hasta || !f.consultorioId || !this.medicoId()) {
      this.toast.add({ severity: 'warn', summary: 'Faltan datos', detail: 'Completa todos los campos.' });
      return;
    }
    this.guardando.set(true);
    this.admin.crearHorario({
      medicoId: this.medicoId()!, consultorioId: f.consultorioId, diaSemana: f.diaSemana,
      horaInicio: this.hhmm(f.desde), horaFin: this.hhmm(f.hasta), duracionCitaMinutos: f.duracion
    }).subscribe({
      next: h => {
        this.guardando.set(false);
        this.dialogo = false;
        this.toast.add({ severity: 'success', summary: 'Horario agregado', detail: `${h.diaNombre} ${h.horaInicio} – ${h.horaFin}` });
        this.cargar();
      },
      error: err => { this.guardando.set(false); this.error(err); }
    });
  }

  eliminar(h: Horario): void {
    this.confirmacion.confirm({
      header: 'Quitar horario',
      message: `¿Quitar el bloque del <b>${h.diaNombre} de ${h.horaInicio} a ${h.horaFin}</b>? Las citas ya agendadas se conservan, pero no se podrán agendar nuevas en ese bloque.`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sí, quitar',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text p-button-secondary',
      accept: () => this.admin.eliminarHorario(h.horarioMedicoId).subscribe({
        next: () => { this.toast.add({ severity: 'success', summary: 'Horario quitado' }); this.cargar(); },
        error: err => this.error(err)
      })
    });
  }

  private hhmm(d: Date): string {
    return `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`;
  }

  private minutos(hhmm: string): number {
    const [h, m] = hhmm.split(':').map(Number);
    return h * 60 + m;
  }

  private error(err: unknown): void {
    this.toast.add({ severity: 'error', summary: 'No se pudo completar', detail: mensajeError(err) });
  }
}
