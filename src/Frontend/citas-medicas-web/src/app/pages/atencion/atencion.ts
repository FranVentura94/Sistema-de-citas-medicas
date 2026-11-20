import { Component, computed, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Tabs, TabList, Tab, TabPanels, TabPanel } from 'primeng/tabs';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { Textarea } from 'primeng/textarea';
import { Skeleton } from 'primeng/skeleton';
import { Tooltip } from 'primeng/tooltip';
import { ClinicalService, RecetaLinea } from '../../core/api.services';
import { APP_NAME } from '../../core/config';
import { Atencion, Diagnostico, Medicamento, SignosVitales } from '../../core/models';
import { leerFecha, mensajeError } from '../../core/utils';

const SIGNOS_VACIOS: SignosVitales = {
  presionSistolica: null, presionDiastolica: null, frecuenciaCardiaca: null, frecuenciaRespiratoria: null,
  temperatura: null, saturacionOxigeno: null, peso: null, talla: null
};

@Component({
  selector: 'app-atencion',
  imports: [DatePipe, FormsModule, RouterLink, Button, Tabs, TabList, Tab, TabPanels, TabPanel, InputNumber, InputText, Select, Textarea, Skeleton, Tooltip],
  template: `
    @if (atencion(); as a) {
      <!-- ============ Encabezado del paciente ============ -->
      <div class="tarjeta cabecera">
        <div class="paciente">
          <div class="avatar"><i class="pi pi-user"></i></div>
          <div>
            <h1>{{ a.pacienteNombre }}</h1>
            <p>
              <span><i class="pi pi-folder"></i> {{ a.numeroExpediente }}</span>
              <span><i class="pi pi-calendar"></i> {{ leer(a.fechaInicio) | date: 'd MMM y, HH:mm' }}</span>
              <span><i class="pi pi-heart"></i> {{ a.especialidadNombre }}</span>
            </p>
            <p class="motivo"><b>Motivo:</b> {{ a.motivoConsulta || 'No registrado' }}</p>
          </div>
        </div>
        <div class="acciones">
          @if (finalizada()) {
            <span class="estado" style="color:#059669; background:#d1fae5">Atención finalizada</span>
          } @else {
            <span class="estado" style="color:#7c3aed; background:#ede9fe">En atención</span>
          }
          <p-button label="Expediente" icon="pi pi-folder-open" severity="secondary" [outlined]="true" [routerLink]="['/expediente', a.pacienteId]" />
          @if (!finalizada()) {
            <p-button label="Finalizar atención" icon="pi pi-check-circle" severity="success" (onClick)="finalizar()" [loading]="finalizando()" />
          }
        </div>
      </div>

      <!-- ============ Pestañas ============ -->
      <div class="tarjeta">
        <p-tabs value="signos">
          <p-tablist>
            <p-tab value="signos"><i class="pi pi-heart-fill"></i> Signos vitales @if (a.signosVitales) { <i class="pi pi-check ok"></i> }</p-tab>
            <p-tab value="notas"><i class="pi pi-file-edit"></i> Notas ({{ a.notas.length }})</p-tab>
            <p-tab value="diagnosticos"><i class="pi pi-search-plus"></i> Diagnóstico ({{ a.diagnosticos.length }})</p-tab>
            <p-tab value="receta"><i class="pi pi-receipt"></i> Receta @if (a.prescripcion) { <i class="pi pi-check ok"></i> }</p-tab>
          </p-tablist>

          <p-tabpanels>
            <!-- ---------- Signos vitales ---------- -->
            <p-tabpanel value="signos">
              <div class="signos">
                @for (s of camposSignos; track s.campo) {
                  <div class="signo">
                    <label><i [class]="s.icono"></i> {{ s.texto }}</label>
                    <p-inputnumber [(ngModel)]="signos[s.campo]" [suffix]="' ' + s.unidad" [minFractionDigits]="0"
                                   [maxFractionDigits]="s.decimales" [disabled]="finalizada()" [placeholder]="s.ejemplo" />
                  </div>
                }
                <div class="signo imc" [style.borderColor]="imc().color">
                  <label><i class="pi pi-calculator"></i> IMC</label>
                  <strong [style.color]="imc().color">{{ imc().valor ?? '—' }}</strong>
                  <span>{{ imc().texto }}</span>
                </div>
              </div>
              @if (!finalizada()) {
                <div class="pie"><p-button label="Guardar signos vitales" icon="pi pi-save" (onClick)="guardarSignos()" [loading]="guardando() === 'signos'" /></div>
              }
            </p-tabpanel>

            <!-- ---------- Notas ---------- -->
            <p-tabpanel value="notas">
              @if (!finalizada()) {
                <div class="nueva-nota">
                  <p-select [options]="tiposNota" [(ngModel)]="notaTipo" optionLabel="texto" optionValue="valor" />
                  <textarea pTextarea rows="3" [(ngModel)]="notaTexto" placeholder="Escribe la evolución, valoración, hallazgos u observaciones..."></textarea>
                  <p-button label="Agregar nota" icon="pi pi-plus" (onClick)="agregarNota()" [disabled]="!notaTexto.trim()" [loading]="guardando() === 'nota'" />
                </div>
              }
              <div class="linea-tiempo">
                @for (n of a.notas; track n.notaMedicaId) {
                  <div class="nota">
                    <span class="tipo">{{ textoTipoNota(n.tipo) }}</span>
                    <p>{{ n.contenido }}</p>
                    <small>{{ n.autor }} · {{ leer(n.fechaRegistro) | date: 'HH:mm' }}</small>
                  </div>
                } @empty {
                  <div class="vacio"><i class="pi pi-file"></i>Todavía no hay notas.</div>
                }
              </div>
            </p-tabpanel>

            <!-- ---------- Diagnósticos ---------- -->
            <p-tabpanel value="diagnosticos">
              @if (!finalizada()) {
                <div class="nuevo-dx">
                  <p-select [options]="catalogoDx()" [(ngModel)]="dxId" optionValue="diagnosticoId" [filter]="true"
                            filterBy="codigo,descripcion" placeholder="Buscar diagnóstico CIE-10" class="dx-select" appendTo="body">
                    <ng-template #selectedItem let-d>{{ d.codigo }} · {{ d.descripcion }}</ng-template>
                    <ng-template #item let-d><b class="codigo">{{ d.codigo }}</b> {{ d.descripcion }}</ng-template>
                  </p-select>
                  <p-select [options]="tiposDx" [(ngModel)]="dxTipo" />
                  <input pInputText [(ngModel)]="dxObservacion" placeholder="Observación (opcional)" />
                  <p-button label="Agregar" icon="pi pi-plus" (onClick)="agregarDiagnostico()" [disabled]="!dxId" [loading]="guardando() === 'dx'" />
                </div>
              }
              <div class="diagnosticos">
                @for (d of a.diagnosticos; track d.diagnosticoId) {
                  <div class="dx" [class.principal]="d.tipo === 'PRINCIPAL'">
                    <span class="codigo">{{ d.codigo }}</span>
                    <div>
                      <strong>{{ d.descripcion }}</strong>
                      <small>{{ d.tipo === 'PRINCIPAL' ? 'Diagnóstico principal' : 'Diagnóstico secundario' }}{{ d.observacion ? ' · ' + d.observacion : '' }}</small>
                    </div>
                    @if (!finalizada()) {
                      <p-button icon="pi pi-trash" [rounded]="true" [text]="true" severity="danger" pTooltip="Quitar" (onClick)="quitarDiagnostico(d.diagnosticoId)" />
                    }
                  </div>
                } @empty {
                  <div class="vacio"><i class="pi pi-search"></i>Agrega al menos un diagnóstico para poder finalizar.</div>
                }
              </div>
            </p-tabpanel>

            <!-- ---------- Receta ---------- -->
            <p-tabpanel value="receta">
              @if (!finalizada()) {
                <div class="receta-editor">
                  @for (l of lineas; track $index) {
                    <div class="linea">
                      <p-select [options]="catalogoMed()" [(ngModel)]="l.medicamentoId" optionValue="medicamentoId" [filter]="true"
                                filterBy="nombre,principioActivo" placeholder="Medicamento" class="med-select" appendTo="body">
                        <ng-template #selectedItem let-m>{{ m.nombre }} {{ m.concentracion }}</ng-template>
                        <ng-template #item let-m>{{ m.nombre }} <small class="texto-suave">{{ m.presentacion }} {{ m.concentracion }}</small></ng-template>
                      </p-select>
                      <input pInputText [(ngModel)]="l.dosis" placeholder="Dosis (1 tableta)" />
                      <input pInputText [(ngModel)]="l.frecuencia" placeholder="Frecuencia (cada 8 h)" />
                      <input pInputText [(ngModel)]="l.duracion" placeholder="Duración (5 días)" />
                      <p-select [options]="vias" [(ngModel)]="l.via" appendTo="body" />
                      <p-button icon="pi pi-times" [rounded]="true" [text]="true" severity="danger" (onClick)="quitarLinea($index)" [disabled]="lineas.length === 1" />
                    </div>
                  }
                  <p-button label="Agregar medicamento" icon="pi pi-plus" [text]="true" (onClick)="agregarLinea()" />
                  <div class="campo">
                    <label>Indicaciones generales</label>
                    <textarea pTextarea rows="2" [(ngModel)]="indicaciones" placeholder="Ej: abundantes líquidos, reposo..."></textarea>
                  </div>
                  <div class="pie">
                    @if (a.prescripcion) { <p-button label="Imprimir receta" icon="pi pi-print" severity="secondary" [outlined]="true" (onClick)="imprimir()" /> }
                    <p-button label="Guardar receta" icon="pi pi-save" (onClick)="guardarReceta()" [loading]="guardando() === 'receta'" />
                  </div>
                </div>
              } @else if (a.prescripcion; as r) {
                <table class="tabla-receta">
                  <tr><th>Medicamento</th><th>Dosis</th><th>Frecuencia</th><th>Duración</th><th>Vía</th></tr>
                  @for (d of r.detalles; track $index) {
                    <tr><td><b>{{ d.medicamento }}</b> {{ d.concentracion }}</td><td>{{ d.dosis }}</td><td>{{ d.frecuencia }}</td><td>{{ d.duracion }}</td><td>{{ d.via }}</td></tr>
                  }
                </table>
                <p><b>Indicaciones:</b> {{ r.indicacionesGenerales || '—' }}</p>
                <div class="pie"><p-button label="Imprimir receta" icon="pi pi-print" (onClick)="imprimir()" /></div>
              } @else {
                <div class="vacio"><i class="pi pi-receipt"></i>Esta atención no tiene receta.</div>
              }
            </p-tabpanel>
          </p-tabpanels>
        </p-tabs>
      </div>

      <!-- ============ Receta para imprimir ============ -->
      @if (a.prescripcion; as r) {
        <div class="solo-impresion receta-impresa">
          <h2>{{ appName }} — Receta médica</h2>
          <p><b>Paciente:</b> {{ a.pacienteNombre }} &nbsp; <b>Expediente:</b> {{ a.numeroExpediente }}</p>
          <p><b>Médico:</b> {{ a.medicoNombre }} ({{ a.especialidadNombre }}) &nbsp; <b>Fecha:</b> {{ leer(r.fecha) | date: 'd/MM/y' }}</p>
          <p><b>Diagnóstico:</b> @for (d of a.diagnosticos; track d.diagnosticoId) { {{ d.codigo }} {{ d.descripcion }}; }</p>
          <hr />
          @for (d of r.detalles; track $index) {
            <p>℞ <b>{{ d.medicamento }} {{ d.concentracion }}</b> ({{ d.presentacion }}) — {{ d.dosis }} {{ d.frecuencia }} por {{ d.duracion }}, vía {{ d.via }}</p>
          }
          <p><b>Indicaciones:</b> {{ r.indicacionesGenerales }}</p>
          <br /><br />
          <p>_____________________________<br />{{ a.medicoNombre }}</p>
        </div>
      }
    } @else {
      <p-skeleton height="8rem" class="mb" />
      <p-skeleton height="20rem" />
    }
  `,
  styles: `
    .cabecera { display: flex; justify-content: space-between; align-items: center; gap: 1rem; flex-wrap: wrap; margin-bottom: 1.25rem;
      background: linear-gradient(120deg, #f0fdfa, #fff); }
    .paciente { display: flex; gap: 1rem; align-items: center;
      h1 { font-size: 1.5rem; }
      p { margin: .3rem 0 0; color: var(--mc-suave); display: flex; gap: 1rem; flex-wrap: wrap; font-size: .85rem; }
      .motivo { display: block; color: var(--mc-texto); } }
    .avatar { width: 58px; height: 58px; border-radius: 16px; display: grid; place-items: center; background: var(--mc-primario); color: #fff; font-size: 1.5rem; }
    .acciones { display: flex; align-items: center; gap: .75rem; flex-wrap: wrap; }
    :host ::ng-deep .p-tab { display: flex; gap: .5rem; align-items: center; }
    .ok { color: #10b981; font-size: .8rem; }

    .signos { display: grid; grid-template-columns: repeat(auto-fill, minmax(210px, 1fr)); gap: 1rem; padding-top: .5rem; }
    .signo { border: 1px solid var(--mc-borde); border-radius: 12px; padding: .9rem; display: flex; flex-direction: column; gap: .5rem;
      label { font-size: .82rem; font-weight: 600; color: var(--mc-suave); i { color: var(--mc-primario); } } }
    :host ::ng-deep .signo .p-inputnumber, :host ::ng-deep .signo input { width: 100%; }
    .imc { border-width: 2px; strong { font-size: 1.8rem; } span { font-size: .8rem; color: var(--mc-suave); } }
    .pie { display: flex; justify-content: flex-end; gap: .5rem; margin-top: 1.25rem; }

    .nueva-nota { display: grid; grid-template-columns: 200px 1fr auto; gap: .75rem; align-items: start; margin: .5rem 0 1.5rem; }
    .linea-tiempo { display: flex; flex-direction: column; gap: .75rem; border-left: 2px solid #ccfbf1; padding-left: 1.25rem; }
    .nota { position: relative; background: #f8fafb; border-radius: 10px; padding: .8rem 1rem;
      &::before { content: ''; position: absolute; left: -1.62rem; top: 1rem; width: 12px; height: 12px; border-radius: 50%; background: var(--mc-primario); border: 2px solid #fff; }
      p { margin: .35rem 0; white-space: pre-line; } small { color: var(--mc-suave); } }
    .tipo { font-size: .72rem; font-weight: 700; text-transform: uppercase; color: var(--mc-primario-oscuro); letter-spacing: .05em; }

    .nuevo-dx { display: grid; grid-template-columns: 1fr 160px 1fr auto; gap: .75rem; margin: .5rem 0 1.5rem; }
    :host ::ng-deep .dx-select, :host ::ng-deep .med-select { width: 100%; }
    .diagnosticos { display: flex; flex-direction: column; gap: .6rem; }
    .dx { display: flex; align-items: center; gap: 1rem; padding: .8rem 1rem; border: 1px solid var(--mc-borde); border-radius: 12px;
      div { flex: 1; } strong { display: block; } small { color: var(--mc-suave); }
      &.principal { border-color: #5eead4; background: #f0fdfa; } }
    .codigo { font-family: Consolas, monospace; font-weight: 700; background: #e0f2f1; color: #0f766e; padding: .25rem .5rem; border-radius: 6px; }

    .receta-editor { display: flex; flex-direction: column; gap: .75rem; padding-top: .5rem; }
    .linea { display: grid; grid-template-columns: 1.6fr 1fr 1fr 1fr 120px auto; gap: .5rem; align-items: center; }
    .tabla-receta { width: 100%; border-collapse: collapse; margin: .5rem 0 1rem;
      th, td { text-align: left; padding: .6rem; border-bottom: 1px solid var(--mc-borde); } th { color: var(--mc-suave); font-size: .82rem; } }
    .receta-impresa { font-family: Georgia, serif; color: #000; h2 { margin-bottom: 1rem; } }
    :host ::ng-deep .mb { margin-bottom: 1rem; }

    @media (max-width: 1000px) { .nueva-nota, .nuevo-dx, .linea { grid-template-columns: 1fr; } }
  `
})
export class AtencionPage {
  // Viene de la ruta /atencion/:id (withComponentInputBinding)
  readonly id = input.required<string>();

  private clinical = inject(ClinicalService);
  private toast = inject(MessageService);
  private confirmacion = inject(ConfirmationService);
  private router = inject(Router);

  protected appName = APP_NAME;
  protected leer = leerFecha;
  protected atencion = signal<Atencion | null>(null);
  protected catalogoDx = signal<Diagnostico[]>([]);
  protected catalogoMed = signal<Medicamento[]>([]);
  protected guardando = signal<string | null>(null);
  protected finalizando = signal(false);
  protected finalizada = computed(() => this.atencion()?.estado === 'FINALIZADA');

  // ---------- Signos vitales ----------
  protected signos: SignosVitales = { ...SIGNOS_VACIOS };
  protected camposSignos: { campo: keyof SignosVitales; texto: string; unidad: string; icono: string; decimales: number; ejemplo: string }[] = [
    { campo: 'presionSistolica', texto: 'Presión sistólica', unidad: 'mmHg', icono: 'pi pi-arrow-up', decimales: 0, ejemplo: '120' },
    { campo: 'presionDiastolica', texto: 'Presión diastólica', unidad: 'mmHg', icono: 'pi pi-arrow-down', decimales: 0, ejemplo: '80' },
    { campo: 'frecuenciaCardiaca', texto: 'Frecuencia cardíaca', unidad: 'lpm', icono: 'pi pi-heart', decimales: 0, ejemplo: '75' },
    { campo: 'frecuenciaRespiratoria', texto: 'Frecuencia respiratoria', unidad: 'rpm', icono: 'pi pi-sync', decimales: 0, ejemplo: '16' },
    { campo: 'temperatura', texto: 'Temperatura', unidad: '°C', icono: 'pi pi-sun', decimales: 1, ejemplo: '36.5' },
    { campo: 'saturacionOxigeno', texto: 'Saturación O₂', unidad: '%', icono: 'pi pi-cloud', decimales: 0, ejemplo: '98' },
    { campo: 'peso', texto: 'Peso', unidad: 'kg', icono: 'pi pi-box', decimales: 2, ejemplo: '70' },
    { campo: 'talla', texto: 'Talla', unidad: 'cm', icono: 'pi pi-arrows-v', decimales: 1, ejemplo: '170' }
  ];

  // El IMC se recalcula cada vez que se escribe el peso o la talla
  protected imc() {
    const { peso, talla } = this.signos;
    if (!peso || !talla) return { valor: null, texto: 'Ingresa peso y talla', color: '#94a3b8' };
    const v = Math.round((peso / Math.pow(talla / 100, 2)) * 10) / 10;
    if (v < 18.5) return { valor: v, texto: 'Bajo peso', color: '#0ea5e9' };
    if (v < 25) return { valor: v, texto: 'Peso normal', color: '#10b981' };
    if (v < 30) return { valor: v, texto: 'Sobrepeso', color: '#f59e0b' };
    return { valor: v, texto: 'Obesidad', color: '#ef4444' };
  }

  // ---------- Notas ----------
  protected tiposNota = [
    { texto: 'Evolución', valor: 'EVOLUCION' }, { texto: 'Valoración', valor: 'VALORACION' },
    { texto: 'Hallazgo', valor: 'HALLAZGO' }, { texto: 'Observación', valor: 'OBSERVACION' }
  ];
  protected notaTipo = 'VALORACION';
  protected notaTexto = '';

  // ---------- Diagnósticos ----------
  protected tiposDx = ['PRINCIPAL', 'SECUNDARIO'];
  protected dxId: number | null = null;
  protected dxTipo = 'PRINCIPAL';
  protected dxObservacion = '';

  // ---------- Receta ----------
  protected vias = ['Oral', 'Sublingual', 'Tópica', 'Inhalada', 'Intramuscular', 'Intravenosa', 'Oftálmica', 'Ótica'];
  protected lineas: RecetaLinea[] = [this.lineaVacia()];
  protected indicaciones = '';

  ngOnInit(): void {
    this.clinical.atencion(Number(this.id())).subscribe({
      next: a => this.mostrar(a),
      error: err => {
        this.toast.add({ severity: 'error', summary: 'Error', detail: mensajeError(err) });
        this.router.navigate(['/mis-citas']);
      }
    });
    this.clinical.diagnosticos().subscribe(d => this.catalogoDx.set(d));
    this.clinical.medicamentos().subscribe(m => this.catalogoMed.set(m));
  }

  private mostrar(a: Atencion): void {
    this.signos = a.signosVitales ? { ...a.signosVitales } : { ...SIGNOS_VACIOS };
    if (a.prescripcion) {
      this.lineas = a.prescripcion.detalles.map(d => ({
        medicamentoId: d.medicamentoId, dosis: d.dosis, frecuencia: d.frecuencia, duracion: d.duracion, via: d.via, indicaciones: d.indicaciones
      }));
      this.indicaciones = a.prescripcion.indicacionesGenerales ?? '';
    }
    this.atencion.set(a);
  }

  // Envuelve cada guardado: muestra el spinner, actualiza la pantalla y avisa
  private guardar(clave: string, peticion: ReturnType<ClinicalService['atencion']>, exito: string, despues?: () => void): void {
    this.guardando.set(clave);
    peticion.subscribe({
      next: a => {
        this.guardando.set(null);
        this.atencion.set(a);
        this.toast.add({ severity: 'success', summary: exito });
        despues?.();
      },
      error: err => {
        this.guardando.set(null);
        this.toast.add({ severity: 'error', summary: 'No se pudo guardar', detail: mensajeError(err) });
      }
    });
  }

  guardarSignos(): void {
    this.guardar('signos', this.clinical.guardarSignos(this.atencion()!.atencionId, this.signos), 'Signos vitales guardados');
  }

  textoTipoNota(tipo: string): string {
    return this.tiposNota.find(t => t.valor === tipo)?.texto ?? tipo;
  }

  agregarNota(): void {
    this.guardar('nota', this.clinical.agregarNota(this.atencion()!.atencionId, this.notaTipo, this.notaTexto), 'Nota agregada',
      () => (this.notaTexto = ''));
  }

  agregarDiagnostico(): void {
    if (!this.dxId) return;
    this.guardar('dx', this.clinical.agregarDiagnostico(this.atencion()!.atencionId, {
      diagnosticoId: this.dxId, tipo: this.dxTipo, observacion: this.dxObservacion || null
    }), 'Diagnóstico agregado', () => {
      this.dxId = null;
      this.dxObservacion = '';
      this.dxTipo = 'SECUNDARIO';
    });
  }

  quitarDiagnostico(diagnosticoId: number): void {
    this.guardar('dx', this.clinical.quitarDiagnostico(this.atencion()!.atencionId, diagnosticoId), 'Diagnóstico quitado');
  }

  private lineaVacia(): RecetaLinea {
    return { medicamentoId: null, dosis: '', frecuencia: '', duracion: '', via: 'Oral', indicaciones: null };
  }

  agregarLinea(): void {
    this.lineas = [...this.lineas, this.lineaVacia()];
  }

  quitarLinea(i: number): void {
    this.lineas = this.lineas.filter((_, idx) => idx !== i);
  }

  guardarReceta(): void {
    const completas = this.lineas.filter(l => l.medicamentoId);
    if (completas.length === 0) {
      this.toast.add({ severity: 'warn', summary: 'Receta vacía', detail: 'Agrega al menos un medicamento.' });
      return;
    }
    this.guardar('receta', this.clinical.guardarReceta(this.atencion()!.atencionId, {
      indicacionesGenerales: this.indicaciones || null,
      detalles: completas
    }), 'Receta guardada');
  }

  imprimir(): void {
    window.print();
  }

  finalizar(): void {
    const a = this.atencion()!;
    if (a.diagnosticos.length === 0) {
      this.toast.add({ severity: 'warn', summary: 'Falta el diagnóstico', detail: 'Registra al menos un diagnóstico antes de finalizar.' });
      return;
    }

    this.confirmacion.confirm({
      header: 'Finalizar atención',
      message: `Se cerrará la atención de <b>${a.pacienteNombre}</b> y la cita quedará como <b>Atendida</b>. Después ya no se podrá modificar. ¿Continuar?`,
      icon: 'pi pi-check-circle',
      acceptLabel: 'Sí, finalizar',
      rejectLabel: 'Cancelar',
      rejectButtonStyleClass: 'p-button-text p-button-secondary',
      accept: () => {
        this.finalizando.set(true);
        this.clinical.finalizar(a.atencionId, null).subscribe({
          next: r => {
            this.finalizando.set(false);
            this.atencion.set(r);
            this.toast.add({ severity: 'success', summary: 'Atención finalizada', detail: 'La cita quedó como Atendida.', life: 5000 });
          },
          error: err => {
            this.finalizando.set(false);
            this.toast.add({ severity: 'error', summary: 'No se pudo finalizar', detail: mensajeError(err) });
          }
        });
      }
    });
  }
}
