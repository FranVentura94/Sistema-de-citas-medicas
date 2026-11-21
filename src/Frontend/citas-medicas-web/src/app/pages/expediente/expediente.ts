import { Component, computed, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Location } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Textarea } from 'primeng/textarea';
import { Skeleton } from 'primeng/skeleton';
import { AuthService } from '../../core/auth.service';
import { ClinicalService, DirectoryService } from '../../core/api.services';
import { Expediente, Paciente } from '../../core/models';
import { edad, iniciales, leerFecha, mensajeError } from '../../core/utils';

@Component({
  selector: 'app-expediente',
  imports: [DatePipe, FormsModule, RouterLink, Button, Textarea, Skeleton],
  template: `
    <div class="pagina-encabezado">
      <div>
        <h1>Expediente clínico</h1>
        <p>Historial médico del paciente</p>
      </div>
      <p-button label="Volver" icon="pi pi-arrow-left" severity="secondary" [text]="true" (onClick)="volver()" />
    </div>

    <div class="contenido">
      <!-- ============ Ficha del paciente ============ -->
      <aside class="tarjeta ficha">
        @if (paciente(); as p) {
          <div class="avatar" [class.f]="p.sexo === 'F'">{{ ini(p.nombres + ' ' + p.apellidos) }}</div>
          <h2>{{ p.nombres }} {{ p.apellidos }}</h2>
          <span class="exp">{{ expediente()?.numeroExpediente || 'Sin expediente' }}</span>
          <dl>
            <dt>Documento</dt><dd>{{ p.tipoDocumento }} {{ p.numeroDocumento }}</dd>
            <dt>Edad</dt><dd>{{ edad(p.fechaNacimiento) }} años</dd>
            <dt>Sexo</dt><dd>{{ p.sexo === 'F' ? 'Femenino' : 'Masculino' }}</dd>
            <dt>Tipo de sangre</dt><dd>{{ p.tipoSangre || '—' }}</dd>
            <dt>Teléfono</dt><dd>{{ p.telefono || '—' }}</dd>
            <dt>Ciudad</dt><dd>{{ p.ciudad || '—' }}</dd>
          </dl>
        } @else {
          <p-skeleton shape="circle" size="5rem" /><p-skeleton height="8rem" />
        }
      </aside>

      <div class="columna">
        <!-- ============ Antecedentes ============ -->
        <div class="tarjeta">
          <div class="tarjeta-titulo"><i class="pi pi-exclamation-circle"></i> Antecedentes y alergias</div>
          @if (sinAcceso()) {
            <div class="vacio bloqueado"><i class="pi pi-lock"></i>{{ sinAcceso() }}</div>
          } @else if (sinExpediente()) {
            <div class="vacio"><i class="pi pi-folder"></i>No se pudo abrir el expediente de este paciente.</div>
          } @else {
            <div class="formulario">
              <div class="campo completo alergias">
                <label><i class="pi pi-ban"></i> Alergias</label>
                <textarea pTextarea rows="2" [(ngModel)]="form.alergias" placeholder="Ej: penicilina, mariscos..."></textarea>
              </div>
              <div class="campo">
                <label>Antecedentes personales</label>
                <textarea pTextarea rows="3" [(ngModel)]="form.antecedentesPersonales"></textarea>
              </div>
              <div class="campo">
                <label>Antecedentes familiares</label>
                <textarea pTextarea rows="3" [(ngModel)]="form.antecedentesFamiliares"></textarea>
              </div>
            </div>
            @if (puedeEditar()) {
              <div class="pie"><p-button label="Guardar antecedentes" icon="pi pi-save" (onClick)="guardar()" [loading]="guardando()" /></div>
            }
          }
        </div>

        <!-- ============ Historial ============ -->
        <div class="tarjeta">
          <div class="tarjeta-titulo"><i class="pi pi-history"></i> Historial de atenciones</div>
          @for (a of expediente()?.atenciones ?? []; track a.atencionId) {
            <a class="atencion" [routerLink]="['/atencion', a.atencionId]">
              <div class="fecha">
                <strong>{{ leer(a.fechaInicio) | date: 'd' }}</strong>
                <span>{{ leer(a.fechaInicio) | date: 'MMM y' }}</span>
              </div>
              <div class="detalle">
                <strong>{{ a.especialidadNombre }} · {{ a.medicoNombre }}</strong>
                <span>{{ a.motivoConsulta || 'Sin motivo' }}</span>
                <div class="dx">@for (d of a.diagnosticos; track d) { <span>{{ d }}</span> }</div>
              </div>
              <i class="pi pi-chevron-right"></i>
            </a>
          } @empty {
            <div class="vacio"><i class="pi pi-inbox"></i>Este paciente todavía no tiene atenciones.</div>
          }
        </div>
      </div>
    </div>
  `,
  styles: `
    .contenido { display: grid; grid-template-columns: 300px 1fr; gap: 1.25rem; align-items: start; }
    .columna { display: flex; flex-direction: column; gap: 1.25rem; }
    .ficha { text-align: center; display: flex; flex-direction: column; align-items: center; gap: .5rem; position: sticky; top: 84px; }
    .avatar { width: 84px; height: 84px; border-radius: 50%; display: grid; place-items: center; font-size: 1.8rem; font-weight: 700; background: #dbeafe; color: #1d4ed8;
      &.f { background: #fce7f3; color: #be185d; } }
    .ficha h2 { font-size: 1.2rem; }
    .exp { font-family: Consolas, monospace; background: #e0f2f1; color: #0f766e; padding: .2rem .6rem; border-radius: 6px; font-weight: 700; }
    dl { width: 100%; display: grid; grid-template-columns: auto 1fr; gap: .5rem; text-align: left; margin-top: 1rem; }
    dt { color: var(--mc-suave); font-size: .85rem; } dd { margin: 0; font-weight: 600; text-align: right; }
    .alergias label { color: #b91c1c; }
    .pie { display: flex; justify-content: flex-end; margin-top: 1rem; }

    .atencion { display: flex; align-items: center; gap: 1rem; padding: .9rem 1rem; border-radius: 12px; border: 1px solid var(--mc-borde);
      text-decoration: none; color: inherit; margin-bottom: .6rem; transition: all .15s;
      &:hover { border-color: var(--mc-primario); background: #f0fdfa; } }
    .fecha { min-width: 60px; text-align: center; strong { display: block; font-size: 1.4rem; color: var(--mc-primario-oscuro); } span { font-size: .75rem; color: var(--mc-suave); text-transform: capitalize; } }
    .detalle { flex: 1; strong { display: block; } > span { font-size: .85rem; color: var(--mc-suave); } }
    .dx { display: flex; gap: .4rem; flex-wrap: wrap; margin-top: .35rem; span { font-size: .75rem; background: #f1f5f9; padding: .15rem .5rem; border-radius: 6px; } }

    @media (max-width: 1000px) { .contenido { grid-template-columns: 1fr; } .ficha { position: static; } }
  `
})
export class ExpedientePage {
  // Viene de la ruta /expediente/:pacienteId
  readonly pacienteId = input.required<string>();

  private clinical = inject(ClinicalService);
  private directory = inject(DirectoryService);
  private toast = inject(MessageService);
  private location = inject(Location);

  protected leer = leerFecha;
  protected edad = edad;
  protected ini = iniciales;
  protected paciente = signal<Paciente | null>(null);
  protected expediente = signal<Expediente | null>(null);
  protected sinExpediente = signal(false);
  protected sinAcceso = signal<string | null>(null);
  private auth = inject(AuthService);
  // Enfermería consulta el expediente (alergias, antecedentes) pero no lo modifica
  protected puedeEditar = computed(() => this.auth.tieneRol('Medico', 'Administrador'));
  protected guardando = signal(false);
  protected form = { alergias: '', antecedentesPersonales: '', antecedentesFamiliares: '', observaciones: '' };

  ngOnInit(): void {
    const id = Number(this.pacienteId());
    this.directory.paciente(id).subscribe({
      next: p => this.paciente.set(p),
      error: err => this.toast.add({ severity: 'error', summary: 'Error', detail: mensajeError(err) })
    });
    this.clinical.expediente(id).subscribe({
      next: e => this.mostrar(e),
      error: (err: HttpErrorResponse) => {
        if (err.status === 403) this.sinAcceso.set(mensajeError(err));
        else if (err.status === 404) this.sinExpediente.set(true);
        else this.toast.add({ severity: 'error', summary: 'Error', detail: mensajeError(err) });
      }
    });
  }

  private mostrar(e: Expediente): void {
    this.form = {
      alergias: e.alergias ?? '',
      antecedentesPersonales: e.antecedentesPersonales ?? '',
      antecedentesFamiliares: e.antecedentesFamiliares ?? '',
      observaciones: e.observaciones ?? ''
    };
    this.expediente.set(e);
  }

  guardar(): void {
    this.guardando.set(true);
    this.clinical.actualizarExpediente(Number(this.pacienteId()), this.form).subscribe({
      next: e => {
        this.guardando.set(false);
        this.mostrar(e);
        this.toast.add({ severity: 'success', summary: 'Expediente actualizado' });
      },
      error: err => {
        this.guardando.set(false);
        this.toast.add({ severity: 'error', summary: 'No se pudo guardar', detail: mensajeError(err) });
      }
    });
  }

  volver(): void {
    this.location.back();
  }
}
