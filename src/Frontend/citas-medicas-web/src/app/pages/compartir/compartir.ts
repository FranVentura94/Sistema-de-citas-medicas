import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { Select } from 'primeng/select';
import { MultiSelect } from 'primeng/multiselect';
import { SelectButton } from 'primeng/selectbutton';
import { DatePicker } from 'primeng/datepicker';
import { InputText } from 'primeng/inputtext';
import { Skeleton } from 'primeng/skeleton';
import { AuthService } from '../../core/auth.service';
import { AgendaService, DirectoryService } from '../../core/api.services';
import { AccesoAgenda, Medico } from '../../core/models';
import { fechaParam, iniciales, leerFecha, mensajeError } from '../../core/utils';

const ESTADOS: Record<string, { texto: string; clase: string }> = {
  VIGENTE: { texto: 'Vigente', clase: 'verde' },
  VENCIDO: { texto: 'Vencido', clase: 'gris' },
  REVOCADO: { texto: 'Revocado', clase: 'rojo' }
};

// El médico comparte su agenda (toda o pacientes específicos) con otro médico, con vencimiento opcional
@Component({
  selector: 'app-compartir',
  imports: [DatePipe, FormsModule, Button, Dialog, Select, MultiSelect, SelectButton, DatePicker, InputText, Skeleton],
  template: `
    <div class="pagina-encabezado">
      <div>
        <h1>Compartir mi agenda</h1>
        <p>Dale a otro médico acceso a tu agenda completa o solo a pacientes específicos, por el tiempo que quieras.</p>
      </div>
      <p-button label="Compartir" icon="pi pi-share-alt" (onClick)="nuevo()" />
    </div>

    <div class="explicacion tarjeta">
      <div><i class="pi pi-eye"></i><b>Puede ver</b> las citas y el historial clínico de esos pacientes.</div>
      <div><i class="pi pi-calendar-plus"></i><b>Puede gestionar:</b> agendar, confirmar y cancelar citas.</div>
      <div><i class="pi pi-lock"></i><b>No puede atender</b> tus citas: eso sigue siendo solo tuyo.</div>
    </div>

    <div class="columnas">
      @for (lado of lados; track lado.clave) {
        <section>
          <h2><i [class]="lado.icono"></i> {{ lado.titulo }} <em>{{ vigentes(lado.clave) }}</em></h2>
          @if (cargando()) {
            <p-skeleton height="140px" borderRadius="16px" />
          } @else if (lista(lado.clave).length === 0) {
            <div class="tarjeta vacio"><i [class]="lado.icono"></i>{{ lado.vacio }}</div>
          } @else {
            @for (a of lista(lado.clave); track a.accesoAgendaId) {
              <div class="tarjeta acceso" [class.apagado]="a.estado !== 'VIGENTE'">
                <div class="cabeza">
                  <div class="avatar">{{ ini(lado.clave === 'mios' ? a.medicoAutorizadoNombre : a.medicoPropietarioNombre) }}</div>
                  <div class="quien">
                    <span>{{ lado.clave === 'mios' ? 'Compartido con' : 'Te compartió' }}</span>
                    <strong>{{ lado.clave === 'mios' ? a.medicoAutorizadoNombre : a.medicoPropietarioNombre }}</strong>
                  </div>
                  <span class="estado-pill {{ estados[a.estado].clase }}">{{ estados[a.estado].texto }}</span>
                </div>
                <div class="detalle">
                  <span class="alcance" [class.todo]="a.alcance === 'TODA'">
                    <i [class]="a.alcance === 'TODA' ? 'pi pi-calendar' : 'pi pi-users'"></i>
                    {{ a.alcance === 'TODA' ? 'Toda la agenda' : a.pacientes.length + (a.pacientes.length === 1 ? ' paciente' : ' pacientes') }}
                  </span>
                  <span><i class="pi pi-clock"></i>
                    {{ a.fechaVencimiento ? 'Hasta el ' + (leer(a.fechaVencimiento) | date: "d 'de' MMMM y") : 'Sin vencimiento' }}</span>
                </div>
                @if (a.alcance === 'PACIENTES') {
                  <div class="chips">
                    @for (p of a.pacientes; track p.pacienteId) { <span class="chip">{{ p.pacienteNombre }}</span> }
                  </div>
                }
                @if (a.motivo) { <p class="motivo"><i class="pi pi-comment"></i> {{ a.motivo }}</p> }
                <div class="pie">
                  <small class="texto-suave">Desde el {{ leer(a.fechaCreacion) | date: "d MMM y" }}{{ a.fechaRevocacion ? ' · revocado el ' + (leer(a.fechaRevocacion) | date: 'd MMM') : '' }}</small>
                  @if (lado.clave === 'mios' && a.estado === 'VIGENTE') {
                    <p-button label="Revocar" icon="pi pi-ban" severity="danger" [text]="true" size="small" (onClick)="revocar(a)" />
                  }
                </div>
              </div>
            }
          }
        </section>
      }
    </div>

    <!-- ============ Compartir ============ -->
    <p-dialog [visible]="dialogo()" (visibleChange)="dialogo.set($event)" [modal]="true" [draggable]="false"
              [style]="{ width: '560px' }" [breakpoints]="{ '600px': '96vw' }" header="Compartir mi agenda">
      <div class="formulario">
        <div class="campo completo">
          <label>¿Con qué médico? *</label>
          <p-select [options]="otrosMedicos()" [(ngModel)]="form.medicoId" optionLabel="nombreCompleto" optionValue="medicoID"
                    [filter]="true" placeholder="Elige un médico" appendTo="body" />
        </div>
        <div class="campo completo">
          <label>¿Qué compartes? *</label>
          <p-selectbutton [options]="alcances" [(ngModel)]="form.alcance" optionLabel="texto" optionValue="valor" [allowEmpty]="false" />
        </div>
        @if (form.alcance === 'PACIENTES') {
          <div class="campo completo">
            <label>Pacientes *</label>
            <p-multiselect [options]="misPacientes()" [(ngModel)]="form.pacienteIds" optionLabel="pacienteNombre" optionValue="pacienteId"
                           display="chip" [filter]="true" placeholder="Elige uno o varios" appendTo="body"
                           emptyMessage="Aún no tienes pacientes con citas contigo" />
          </div>
        }
        <div class="campo">
          <label>Vence (opcional)</label>
          <p-datepicker [(ngModel)]="form.vence" [minDate]="manana" dateFormat="dd/mm/yy" [showIcon]="true" [showClear]="true"
                        appendTo="body" placeholder="Sin vencimiento" />
        </div>
        <div class="campo">
          <label>Motivo (opcional)</label>
          <input pInputText [(ngModel)]="form.motivo" placeholder="Vacaciones, interconsulta..." />
        </div>
      </div>
      <small class="texto-suave nota"><i class="pi pi-info-circle"></i> Si ya compartiste con ese médico, se actualiza lo que le compartiste.</small>
      <ng-template #footer>
        <p-button label="Cancelar" severity="secondary" [text]="true" (onClick)="dialogo.set(false)" />
        <p-button label="Compartir" icon="pi pi-share-alt" (onClick)="guardar()" [loading]="guardando()" />
      </ng-template>
    </p-dialog>
  `,
  styles: `
    .explicacion { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 1rem; margin-bottom: 1.25rem; font-size: .85rem; color: #475569;
      div { display: flex; gap: .5rem; align-items: flex-start; } i { color: var(--mc-primario); margin-top: .15rem; } b { color: var(--mc-texto); margin-right: .2rem; } }
    .columnas { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; align-items: start; }
    section { display: flex; flex-direction: column; gap: .8rem; }
    section h2 { font-size: 1rem; display: flex; align-items: center; gap: .5rem; color: var(--mc-primario-oscuro);
      em { font-style: normal; font-size: .72rem; background: #ccfbf1; color: #0f766e; border-radius: 999px; padding: .05rem .5rem; } }
    .acceso { display: flex; flex-direction: column; gap: .65rem; transition: transform .15s, box-shadow .15s; animation: entrar .3s ease both;
      &:hover { transform: translateY(-2px); box-shadow: 0 10px 24px rgba(16, 24, 40, .08); }
      &.apagado { opacity: .6; } }
    @keyframes entrar { from { opacity: 0; transform: translateY(6px); } }
    .cabeza { display: flex; align-items: center; gap: .75rem; }
    .avatar { width: 44px; height: 44px; border-radius: 14px; display: grid; place-items: center; color: #fff; font-weight: 700;
      background: linear-gradient(135deg, #6366f1, #0d9488); }
    .quien { flex: 1; span { display: block; font-size: .72rem; color: var(--mc-suave); } strong { font-size: .98rem; } }
    .estado-pill { font-size: .72rem; font-weight: 700; border-radius: 999px; padding: .2rem .6rem; }
    .estado-pill.verde { background: #dcfce7; color: #15803d; } .estado-pill.gris { background: #f1f5f9; color: #64748b; } .estado-pill.rojo { background: #fee2e2; color: #b91c1c; }
    .detalle { display: flex; flex-wrap: wrap; gap: .5rem 1rem; font-size: .82rem; color: #475569; i { margin-right: .25rem; color: #94a3b8; } }
    .alcance { font-weight: 600; color: #6d28d9; } .alcance.todo { color: #0f766e; } .alcance i { color: inherit !important; }
    .chips { display: flex; flex-wrap: wrap; gap: .35rem; }
    .chip { font-size: .74rem; background: #f5f3ff; color: #5b21b6; border-radius: 999px; padding: .15rem .55rem; }
    .motivo { margin: 0; font-size: .82rem; color: #475569; }
    .pie { display: flex; justify-content: space-between; align-items: center; border-top: 1px dashed var(--mc-borde); padding-top: .5rem; }
    .nota { display: block; margin-top: .8rem; }
    :host ::ng-deep .formulario p-datepicker, :host ::ng-deep .formulario .p-datepicker, :host ::ng-deep .formulario p-multiselect,
    :host ::ng-deep .formulario .p-multiselect, :host ::ng-deep .formulario input { width: 100%; }
    :host ::ng-deep .formulario .p-selectbutton { display: flex; } :host ::ng-deep .formulario .p-togglebutton { flex: 1; }
    @media (max-width: 960px) { .columnas, .explicacion { grid-template-columns: 1fr; } }
  `
})
export class Compartir {
  private agenda = inject(AgendaService);
  private directory = inject(DirectoryService);
  private auth = inject(AuthService);
  private toast = inject(MessageService);
  private confirmacion = inject(ConfirmationService);

  protected estados = ESTADOS;
  protected leer = leerFecha;
  protected ini = (n: string) => iniciales(n.replace(/^Dra?\.\s*|^Dr\(a\)\.\s*/, ''));
  protected manana = new Date(Date.now() + 86_400_000);
  protected alcances = [{ texto: 'Toda mi agenda', valor: 'TODA' }, { texto: 'Pacientes específicos', valor: 'PACIENTES' }];
  protected lados = [
    { clave: 'mios' as const, titulo: 'Lo que yo compartí', icono: 'pi pi-send', vacio: 'Todavía no has compartido tu agenda con nadie.' },
    { clave: 'recibidos' as const, titulo: 'Lo que me compartieron', icono: 'pi pi-inbox', vacio: 'Nadie te ha compartido su agenda todavía.' }
  ];

  protected cargando = signal(true);
  protected guardando = signal(false);
  protected dialogo = signal(false);
  protected mios = signal<AccesoAgenda[]>([]);
  protected recibidos = signal<AccesoAgenda[]>([]);
  protected medicos = signal<Medico[]>([]);
  protected misPacientes = signal<{ pacienteId: number; pacienteNombre: string }[]>([]);
  protected form = { medicoId: null as number | null, alcance: 'TODA' as 'TODA' | 'PACIENTES', pacienteIds: [] as number[], vence: null as Date | null, motivo: '' };

  protected otrosMedicos = computed(() => this.medicos().filter(m => m.activo && m.medicoID !== this.auth.usuario()?.medicoId));

  constructor() {
    this.cargar();
    this.directory.medicos().subscribe(m => this.medicos.set(m));
    this.agenda.misPacientes().subscribe(p => this.misPacientes.set(p));
  }

  lista(lado: 'mios' | 'recibidos'): AccesoAgenda[] {
    return lado === 'mios' ? this.mios() : this.recibidos();
  }

  vigentes(lado: 'mios' | 'recibidos'): number {
    return this.lista(lado).filter(a => a.estado === 'VIGENTE').length;
  }

  cargar(): void {
    this.agenda.compartidosPorMi().subscribe({ next: a => { this.mios.set(a); this.cargando.set(false); }, error: err => { this.cargando.set(false); this.error(err); } });
    this.agenda.compartidosConmigo().subscribe({ next: a => this.recibidos.set(a), error: err => this.error(err) });
  }

  nuevo(): void {
    this.form = { medicoId: null, alcance: 'TODA', pacienteIds: [], vence: null, motivo: '' };
    this.dialogo.set(true);
  }

  guardar(): void {
    const f = this.form;
    if (!f.medicoId) return this.aviso('Elige el médico con quien vas a compartir.');
    if (f.alcance === 'PACIENTES' && f.pacienteIds.length === 0) return this.aviso('Elige al menos un paciente.');

    this.guardando.set(true);
    this.agenda.compartir({
      medicoAutorizadoId: f.medicoId, alcance: f.alcance, pacienteIds: f.alcance === 'PACIENTES' ? f.pacienteIds : [],
      fechaVencimiento: f.vence ? `${fechaParam(f.vence)}T23:59:59` : null, motivo: f.motivo.trim() || null
    }).subscribe({
      next: a => {
        this.guardando.set(false);
        this.dialogo.set(false);
        this.toast.add({ severity: 'success', summary: 'Agenda compartida', detail: `${a.medicoAutorizadoNombre} ya tiene acceso.` });
        this.cargar();
      },
      error: err => { this.guardando.set(false); this.error(err); }
    });
  }

  revocar(a: AccesoAgenda): void {
    this.confirmacion.confirm({
      header: 'Revocar acceso',
      message: `<b>${a.medicoAutorizadoNombre}</b> dejará de ver tu agenda y el historial de esos pacientes de inmediato. ¿Continuar?`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sí, revocar',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text p-button-secondary',
      accept: () => this.agenda.revocar(a.accesoAgendaId).subscribe({
        next: () => { this.toast.add({ severity: 'success', summary: 'Acceso revocado' }); this.cargar(); },
        error: err => this.error(err)
      })
    });
  }

  private aviso(detalle: string): void {
    this.toast.add({ severity: 'warn', summary: 'Faltan datos', detail: detalle });
  }

  private error(err: unknown): void {
    this.toast.add({ severity: 'error', summary: 'No se pudo completar', detail: mensajeError(err) });
  }
}
