import { Component, computed, inject, output, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Drawer } from 'primeng/drawer';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputNumber } from 'primeng/inputnumber';
import { Textarea } from 'primeng/textarea';
import { Skeleton } from 'primeng/skeleton';
import { AuthService } from '../core/auth.service';
import { AgendaService, ClinicalService } from '../core/api.services';
import { Atencion, Cita, SignosVitales, Triaje } from '../core/models';
import { TRANSICIONES, iniciales, leerFecha, mensajeError, nombreRol } from '../core/utils';
import { Historial } from './historial';

const ACCIONES: Record<string, { texto: string; icono: string; severidad: 'secondary' | 'danger' | 'info' | 'warn' }> = {
  CONFIRMADA: { texto: 'Confirmar asistencia', icono: 'pi pi-check', severidad: 'info' },
  EN_ESPERA: { texto: 'Llegó: sala de espera', icono: 'pi pi-hourglass', severidad: 'warn' },
  NO_ASISTIO: { texto: 'No asistió', icono: 'pi pi-user-minus', severidad: 'secondary' },
  CANCELADA: { texto: 'Cancelar cita', icono: 'pi pi-times', severidad: 'danger' }
};

const PERMITIDAS_ENFERMERIA = ['CONFIRMADA', 'EN_ESPERA', 'NO_ASISTIO'];
const PUEDE_TRIAJE = ['PROGRAMADA', 'CONFIRMADA', 'EN_ESPERA'];
const PUEDE_ATENDER = ['PROGRAMADA', 'CONFIRMADA', 'EN_ESPERA', 'EN_ATENCION'];

type FormTriaje = SignosVitales & { notas: string };

// Panel lateral con todo sobre una cita: paciente, triaje, atención y acciones según quién mira.
// Uso: <app-cita-detalle #detalle (actualizada)="..." />  y luego  detalle.abrir(cita)
@Component({
  selector: 'app-cita-detalle',
  imports: [DatePipe, FormsModule, Drawer, Button, Dialog, InputNumber, Textarea, Skeleton, Historial],
  template: `
    <p-drawer [visible]="visible()" (visibleChange)="visible.set($event)" position="right" [modal]="true"
              styleClass="drawer-cita" [style]="{ width: '480px', maxWidth: '100vw' }">
      <ng-template #header>
        @if (cita(); as c) {
          <div class="cabecera">
            <div class="avatar">{{ ini(c.pacienteNombre) }}</div>
            <div>
              <h2>{{ c.pacienteNombre }}</h2>
              <span>DUI {{ c.pacienteDocumento }}</span>
            </div>
          </div>
        }
      </ng-template>
      @if (cita(); as c) {

        <!-- ============ La cita ============ -->
        <div class="bloque cita" [style.--c]="c.estadoColor">
          <div class="fila-hora">
            <div class="hora">
              <strong>{{ leer(c.fechaHoraInicio) | date: 'h:mm a' }}</strong>
              <span>{{ leer(c.fechaHoraInicio) | date: "EEEE d 'de' MMMM" }}</span>
            </div>
            <span class="estado" [style.color]="c.estadoColor" [style.background]="c.estadoColor + '1a'">{{ c.estadoNombre }}</span>
          </div>
          <ul class="datos">
            <li><i class="pi pi-user"></i><span>Médico</span><b>{{ c.medicoNombre }}</b></li>
            <li><i class="pi pi-heart"></i><span>Especialidad</span><b>{{ c.especialidadNombre || '—' }}</b></li>
            <li><i class="pi pi-building"></i><span>Consultorio</span><b>{{ c.consultorioNombre }}</b></li>
            <li><i class="pi pi-comment"></i><span>Motivo</span><b>{{ c.motivoConsulta || '—' }}</b></li>
            @if (c.notas) { <li><i class="pi pi-file-edit"></i><span>Notas</span><b>{{ c.notas }}</b></li> }
          </ul>
          @if (esCompartida()) {
            <div class="aviso-compartida"><i class="pi pi-share-alt"></i> Cita de una agenda que te compartieron. Puedes gestionarla, pero atenderla es solo de su médico.</div>
          }
        </div>

        <!-- ============ Acciones ============ -->
        @if (accionesEstado().length || puedeAtender() || puedeTriaje()) {
          <div class="acciones">
            @if (puedeAtender()) {
              <p-button label="Atender" icon="pi pi-play" (onClick)="atender()" [loading]="ocupado()" styleClass="w-full" />
            }
            @if (puedeTriaje()) {
              <p-button [label]="triaje() ? 'Corregir triaje' : 'Tomar signos (triaje)'" icon="pi pi-heart-fill" severity="help"
                        [outlined]="!!triaje()" (onClick)="abrirTriaje()" styleClass="w-full" />
            }
            <div class="estados">
              @for (a of accionesEstado(); track a) {
                <p-button [label]="acciones[a].texto" [icon]="acciones[a].icono" [severity]="acciones[a].severidad" [outlined]="true" size="small"
                          (onClick)="cambiarEstado(a)" [disabled]="ocupado()" />
              }
            </div>
          </div>
        }

        <!-- ============ Seguimiento ============ -->
        <div class="bloque">
          <h3><i class="pi pi-list-check"></i> Seguimiento</h3>
          <ol class="linea-tiempo">
            <li class="hecho"><b>Agendada</b><span>{{ leer(c.fechaCreacion) | date: "d MMM, h:mm a" }}</span></li>
            @if (cargando()) {
              <li><p-skeleton width="70%" /></li>
            } @else {
              <li [class.hecho]="!!triaje()">
                <b>Triaje (signos vitales)</b>
                @if (triaje(); as t) { <span>{{ t.registradoPorNombre }} · {{ leer(t.fechaRegistro) | date: 'h:mm a' }}</span> }
                @else { <span class="pendiente">{{ verClinica() ? 'Pendiente' : '—' }}</span> }
              </li>
              <li [class.hecho]="!!atencion()">
                <b>Consulta</b>
                @if (atencion(); as a) { <span>{{ a.medicoNombre }} · {{ leer(a.fechaInicio) | date: 'h:mm a' }}{{ a.estado === 'FINALIZADA' ? ' · Finalizada' : ' · En curso' }}</span> }
                @else { <span class="pendiente">{{ c.estadoCodigo === 'ATENDIDA' ? 'Atendida' : 'Pendiente' }}</span> }
              </li>
            }
          </ol>
        </div>

        <!-- ============ Triaje ============ -->
        @if (triaje(); as t) {
          <div class="bloque">
            <h3><i class="pi pi-heart-fill"></i> Signos vitales <small>por {{ t.registradoPorNombre }}</small></h3>
            <div class="signos">
              @if (t.signos.presionSistolica) { <div><span>Presión</span><b>{{ t.signos.presionSistolica }}/{{ t.signos.presionDiastolica }}</b><em>mmHg</em></div> }
              @if (t.signos.frecuenciaCardiaca) { <div><span>Pulso</span><b>{{ t.signos.frecuenciaCardiaca }}</b><em>lpm</em></div> }
              @if (t.signos.temperatura) { <div><span>Temp.</span><b>{{ t.signos.temperatura }}</b><em>°C</em></div> }
              @if (t.signos.saturacionOxigeno) { <div><span>SpO₂</span><b>{{ t.signos.saturacionOxigeno }}</b><em>%</em></div> }
              @if (t.signos.frecuenciaRespiratoria) { <div><span>Resp.</span><b>{{ t.signos.frecuenciaRespiratoria }}</b><em>rpm</em></div> }
              @if (t.signos.peso) { <div><span>Peso</span><b>{{ t.signos.peso }}</b><em>kg</em></div> }
              @if (t.signos.talla) { <div><span>Talla</span><b>{{ t.signos.talla }}</b><em>cm</em></div> }
              @if (t.signos.imc) { <div><span>IMC</span><b>{{ t.signos.imc }}</b><em></em></div> }
            </div>
            @if (t.notas) { <p class="nota"><i class="pi pi-comment"></i> {{ t.notas }}</p> }
          </div>
        }

        <!-- ============ Atención ============ -->
        @if (atencion(); as a) {
          <div class="bloque">
            <h3><i class="pi pi-briefcase"></i> Consulta</h3>
            <ul class="datos">
              <li><i class="pi pi-user"></i><span>Atendió</span><b>{{ a.medicoNombre }}</b></li>
              @if (a.signosTomadosPor) { <li><i class="pi pi-heart"></i><span>Signos</span><b>{{ a.signosTomadosPor }}</b></li> }
            </ul>
            @if (a.diagnosticos.length) {
              <div class="chips">
                @for (d of a.diagnosticos; track d.diagnosticoId) { <span class="chip" [class.principal]="d.tipo === 'PRINCIPAL'">{{ d.codigo }} · {{ d.descripcion }}</span> }
              </div>
            }
            @if (a.prescripcion?.detalles?.length) {
              <p class="texto-suave receta"><i class="pi pi-file"></i> Receta con {{ a.prescripcion!.detalles.length }} medicamento(s)</p>
            }
          </div>
        }

        <!-- ============ Accesos rápidos ============ -->
        <div class="atajos">
          @if (verClinica()) {
            <button type="button" (click)="ir(['/expediente', c.pacienteId])"><i class="pi pi-folder-open"></i> Expediente</button>
          }
          @if (atencion() && (esDueno() || esAdmin())) {
            <button type="button" (click)="ir(['/atencion', atencion()!.atencionId])"><i class="pi pi-eye"></i> Ver consulta</button>
          }
          @if (puedeAgendar()) {
            <button type="button" (click)="ir(['/agendar'], { pacienteId: c.pacienteId })"><i class="pi pi-calendar-plus"></i> Otra cita</button>
          }
          @if (esAdmin()) {
            <button type="button" (click)="verHistorial(historial, c)"><i class="pi pi-history"></i> Historial</button>
          }
        </div>
      }
    </p-drawer>

    <app-historial #historial />

    <!-- ============ Triaje ============ -->
    <p-dialog [visible]="dialogoTriaje()" (visibleChange)="dialogoTriaje.set($event)" [modal]="true" [draggable]="false"
              [style]="{ width: '560px' }" [breakpoints]="{ '600px': '96vw' }" header="Triaje: signos vitales">
      <p class="texto-suave intro">Al guardar, el paciente queda <b>en sala de espera</b> y el médico verá estos signos al abrir la consulta.</p>
      <div class="formulario">
        <div class="campo"><label>Presión sistólica (mmHg)</label><p-inputnumber [(ngModel)]="form.presionSistolica" [min]="50" [max]="260" placeholder="120" /></div>
        <div class="campo"><label>Presión diastólica (mmHg)</label><p-inputnumber [(ngModel)]="form.presionDiastolica" [min]="30" [max]="160" placeholder="80" /></div>
        <div class="campo"><label>Frecuencia cardíaca (lpm)</label><p-inputnumber [(ngModel)]="form.frecuenciaCardiaca" [min]="20" [max]="250" placeholder="72" /></div>
        <div class="campo"><label>Frecuencia respiratoria (rpm)</label><p-inputnumber [(ngModel)]="form.frecuenciaRespiratoria" [min]="5" [max]="80" placeholder="16" /></div>
        <div class="campo"><label>Temperatura (°C)</label><p-inputnumber [(ngModel)]="form.temperatura" [min]="30" [max]="45" [minFractionDigits]="1" [maxFractionDigits]="1" placeholder="36.5" /></div>
        <div class="campo"><label>Saturación O₂ (%)</label><p-inputnumber [(ngModel)]="form.saturacionOxigeno" [min]="50" [max]="100" placeholder="98" /></div>
        <div class="campo"><label>Peso (kg)</label><p-inputnumber [(ngModel)]="form.peso" [min]="0.5" [max]="400" [maxFractionDigits]="2" placeholder="70" /></div>
        <div class="campo"><label>Talla (cm)</label><p-inputnumber [(ngModel)]="form.talla" [min]="30" [max]="250" [maxFractionDigits]="1" placeholder="165" /></div>
        <div class="campo completo"><label>Notas</label><textarea pTextarea [(ngModel)]="form.notas" rows="2" placeholder="Ej.: refiere dolor de cabeza desde ayer"></textarea></div>
      </div>
      <ng-template #footer>
        <p-button label="Cancelar" severity="secondary" [text]="true" (onClick)="dialogoTriaje.set(false)" />
        <p-button label="Guardar triaje" icon="pi pi-check" (onClick)="guardarTriaje()" [loading]="ocupado()" />
      </ng-template>
    </p-dialog>
  `,
  styles: `
    .cabecera { display: flex; align-items: center; gap: .8rem; h2 { font-size: 1.15rem; } span { font-size: .8rem; color: var(--mc-suave); } }
    .avatar { width: 46px; height: 46px; border-radius: 14px; display: grid; place-items: center; font-weight: 700; color: #fff;
      background: linear-gradient(135deg, #0d9488, #14b8a6); }
    .bloque { border: 1px solid var(--mc-borde); border-radius: 14px; padding: .9rem 1rem; margin-bottom: .9rem; background: #fff;
      h3 { font-size: .9rem; display: flex; gap: .45rem; align-items: center; color: var(--mc-primario-oscuro); margin-bottom: .6rem;
        small { font-weight: 400; color: var(--mc-suave); font-size: .75rem; } } }
    .bloque.cita { border-left: 4px solid var(--c); }
    .fila-hora { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: .6rem; }
    .hora strong { display: block; font-size: 1.5rem; color: var(--mc-primario-oscuro); }
    .hora span { display: block; font-size: .8rem; color: var(--mc-suave); }
    .hora span::first-letter { text-transform: uppercase; }
    .datos { list-style: none; margin: 0; padding: 0; display: grid; gap: .4rem; font-size: .85rem;
      li { display: grid; grid-template-columns: 18px 95px 1fr; align-items: start; gap: .35rem; }
      i { color: #94a3b8; font-size: .8rem; margin-top: .2rem; } span { color: var(--mc-suave); } b { font-weight: 600; } }
    .aviso-compartida { margin-top: .7rem; font-size: .78rem; background: #eff6ff; color: #1d4ed8; border-radius: 10px; padding: .5rem .7rem; }
    .acciones { display: flex; flex-direction: column; gap: .5rem; margin-bottom: .9rem; }
    :host ::ng-deep .acciones .w-full { width: 100%; }
    .estados { display: flex; flex-wrap: wrap; gap: .4rem; }
    .linea-tiempo { list-style: none; margin: 0; padding: 0 0 0 1rem; border-left: 2px solid #e2e8f0; display: grid; gap: .7rem; font-size: .84rem;
      li { position: relative; display: flex; flex-direction: column; }
      li::before { content: ''; position: absolute; left: calc(-1rem - 7px); top: 3px; width: 12px; height: 12px; border-radius: 50%; background: #e2e8f0; border: 2px solid #fff; }
      li.hecho::before { background: var(--mc-primario); }
      span { color: var(--mc-suave); font-size: .78rem; } .pendiente { color: #b45309; } }
    .signos { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: .45rem;
      div { background: #f0fdfa; border-radius: 10px; padding: .45rem .5rem; text-align: center; }
      span { display: block; font-size: .7rem; color: var(--mc-suave); } b { font-size: 1.05rem; color: #0f3d3e; } em { font-style: normal; font-size: .68rem; color: var(--mc-suave); margin-left: .15rem; } }
    .nota { margin: .6rem 0 0; font-size: .82rem; color: #475569; }
    .chips { display: flex; flex-wrap: wrap; gap: .35rem; margin-top: .55rem; }
    .chip { font-size: .74rem; background: #f1f5f9; border-radius: 999px; padding: .15rem .55rem; color: #334155; }
    .chip.principal { background: #ccfbf1; color: #0f766e; font-weight: 600; }
    .receta { margin: .55rem 0 0; font-size: .8rem; }
    .atajos { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: .5rem; }
    .atajos button { display: flex; align-items: center; gap: .45rem; justify-content: center; padding: .65rem; border-radius: 12px; cursor: pointer;
      border: 1px solid var(--mc-borde); background: #fff; font: inherit; font-size: .84rem; color: var(--mc-texto); transition: all .15s;
      &:hover { border-color: var(--mc-primario); color: var(--mc-primario-oscuro); background: #f0fdfa; } }
    .intro { margin: 0 0 1rem; }
    :host ::ng-deep .formulario .p-inputnumber, :host ::ng-deep .formulario p-inputnumber, :host ::ng-deep .formulario textarea { width: 100%; }
  `
})
export class CitaDetalle {
  private auth = inject(AuthService);
  private agenda = inject(AgendaService);
  private clinical = inject(ClinicalService);
  private router = inject(Router);
  private toast = inject(MessageService);
  private confirmacion = inject(ConfirmationService);

  readonly actualizada = output<Cita>();

  protected acciones = ACCIONES;
  protected leer = leerFecha;
  protected ini = iniciales;
  protected nombreRol = nombreRol;

  protected visible = signal(false);
  protected cita = signal<Cita | null>(null);
  protected triaje = signal<Triaje | null>(null);
  protected atencion = signal<Atencion | null>(null);
  protected cargando = signal(false);
  protected ocupado = signal(false);
  protected dialogoTriaje = signal(false);
  protected form: FormTriaje = this.formVacio();

  private rol = computed(() => this.auth.usuario()?.rol ?? '');
  protected esAdmin = computed(() => this.rol() === 'Administrador');
  protected esDueno = computed(() => this.rol() === 'Medico' && this.cita()?.medicoId === this.auth.usuario()?.medicoId);
  protected esCompartida = computed(() => this.rol() === 'Medico' && !!this.cita() && !this.esDueno());
  protected verClinica = computed(() => ['Medico', 'Administrador', 'Enfermeria'].includes(this.rol()));
  protected puedeAgendar = computed(() => ['Administrador', 'Recepcion', 'Medico'].includes(this.rol()));
  protected puedeAtender = computed(() => this.esDueno() && PUEDE_ATENDER.includes(this.cita()?.estadoCodigo ?? ''));
  protected puedeTriaje = computed(() =>
    ['Enfermeria', 'Medico', 'Administrador'].includes(this.rol()) && PUEDE_TRIAJE.includes(this.cita()?.estadoCodigo ?? ''));

  protected accionesEstado = computed(() => {
    const c = this.cita();
    if (!c) return [];
    const posibles = (TRANSICIONES[c.estadoCodigo] ?? []).filter(e => ACCIONES[e]);
    return this.rol() === 'Enfermeria' ? posibles.filter(e => PERMITIDAS_ENFERMERIA.includes(e)) : posibles;
  });

  abrir(c: Cita): void {
    this.cita.set(c);
    this.triaje.set(null);
    this.atencion.set(null);
    this.visible.set(true);
    this.cargarClinica(c);
  }

  private cargarClinica(c: Cita): void {
    if (!this.verClinica()) return;
    this.cargando.set(true);
    let pendientes = 2;
    const listo = () => { if (--pendientes === 0) this.cargando.set(false); };

    this.clinical.triaje(c.citaId).subscribe({ next: t => { this.triaje.set(t); listo(); }, error: () => listo() });

    if (this.rol() !== 'Enfermeria' && ['EN_ATENCION', 'ATENDIDA'].includes(c.estadoCodigo)) {
      this.clinical.porCita(c.citaId).subscribe({ next: a => { this.atencion.set(a); listo(); }, error: () => listo() });
    } else {
      listo();
    }
  }

  cambiarEstado(codigo: string): void {
    const c = this.cita()!;
    const ejecutar = () => {
      this.ocupado.set(true);
      this.agenda.cambiarEstado(c.citaId, codigo).subscribe({
        next: actualizada => {
          this.ocupado.set(false);
          this.cita.set(actualizada);
          this.actualizada.emit(actualizada);
          this.toast.add({ severity: 'success', summary: 'Cita actualizada', detail: `${actualizada.pacienteNombre}: ${actualizada.estadoNombre}` });
        },
        error: err => { this.ocupado.set(false); this.error(err); }
      });
    };

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

  atender(): void {
    const c = this.cita()!;
    this.ocupado.set(true);
    this.clinical.iniciar(c.citaId).subscribe({
      next: a => { this.ocupado.set(false); this.visible.set(false); this.router.navigate(['/atencion', a.atencionId]); },
      error: err => { this.ocupado.set(false); this.error(err); }
    });
  }

  abrirTriaje(): void {
    const t = this.triaje();
    this.form = t ? { ...this.formVacio(), ...t.signos, notas: t.notas ?? '' } : this.formVacio();
    this.dialogoTriaje.set(true);
  }

  guardarTriaje(): void {
    const c = this.cita()!;
    const f = this.form;
    this.ocupado.set(true);
    this.clinical.registrarTriaje({
      citaId: c.citaId,
      presionSistolica: f.presionSistolica, presionDiastolica: f.presionDiastolica,
      frecuenciaCardiaca: f.frecuenciaCardiaca, frecuenciaRespiratoria: f.frecuenciaRespiratoria,
      temperatura: f.temperatura, saturacionOxigeno: f.saturacionOxigeno, peso: f.peso, talla: f.talla,
      notas: f.notas?.trim() || null
    }).subscribe({
      next: t => {
        this.ocupado.set(false);
        this.triaje.set(t);
        this.dialogoTriaje.set(false);
        this.toast.add({ severity: 'success', summary: 'Triaje guardado', detail: `${c.pacienteNombre} quedó listo para consulta.` });
        // La cita pasó a "En espera": se refresca
        this.agenda.cita(c.citaId).subscribe(actualizada => { this.cita.set(actualizada); this.actualizada.emit(actualizada); });
      },
      error: err => { this.ocupado.set(false); this.error(err); }
    });
  }

  verHistorial(historial: Historial, c: Cita): void {
    historial.abrir(c.pacienteNombre, [
      { servicio: 'directory', entidad: 'Paciente', entidadId: c.pacienteId },
      { servicio: 'clinical', entidad: 'Paciente', entidadId: c.pacienteId },
      { servicio: 'agenda', entidad: 'Cita', entidadId: c.citaId }
    ]);
  }

  ir(ruta: unknown[], queryParams?: Record<string, unknown>): void {
    this.visible.set(false);
    this.router.navigate(ruta, queryParams ? { queryParams } : {});
  }

  private formVacio(): FormTriaje {
    return { presionSistolica: null, presionDiastolica: null, frecuenciaCardiaca: null, frecuenciaRespiratoria: null,
      temperatura: null, saturacionOxigeno: null, peso: null, talla: null, notas: '' };
  }

  private error(err: unknown): void {
    this.toast.add({ severity: 'error', summary: 'No se pudo completar', detail: mensajeError(err) });
  }
}
