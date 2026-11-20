import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { DatePicker } from 'primeng/datepicker';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Tooltip } from 'primeng/tooltip';
import { Avatar } from 'primeng/avatar';
import { AuthService } from '../../core/auth.service';
import { DirectoryService } from '../../core/api.services';
import { Paciente } from '../../core/models';
import { edad, fechaParam, iniciales, leerFecha, mensajeError } from '../../core/utils';

// Modelo del formulario (fecha como Date para el calendario)
interface FormPaciente {
  pacienteID: number | null;
  codigoPaciente: string;
  tipoDocumento: string;
  numeroDocumento: string;
  nombres: string;
  apellidos: string;
  fechaNacimiento: Date | null;
  sexo: string;
  estadoCivil: string | null;
  telefono: string | null;
  telefonoSecundario: string | null;
  email: string | null;
  direccion: string | null;
  ciudad: string | null;
  pais: string | null;
  ocupacion: string | null;
  tipoSangre: string | null;
  activo: boolean;
}

const VACIO: FormPaciente = {
  pacienteID: null, codigoPaciente: '', tipoDocumento: 'DUI', numeroDocumento: '', nombres: '', apellidos: '',
  fechaNacimiento: null, sexo: '', estadoCivil: null, telefono: null, telefonoSecundario: null, email: null,
  direccion: null, ciudad: null, pais: 'El Salvador', ocupacion: null, tipoSangre: null, activo: true
};

@Component({
  selector: 'app-pacientes',
  imports: [FormsModule, TableModule, Button, Dialog, InputText, Select, DatePicker, IconField, InputIcon, Tooltip, Avatar],
  template: `
    <div class="pagina-encabezado">
      <div>
        <h1>Pacientes</h1>
        <p>{{ pacientes().length }} pacientes registrados en la clínica</p>
      </div>
      @if (puedeEditar()) {
        <p-button label="Nuevo paciente" icon="pi pi-user-plus" (onClick)="nuevo()" />
      }
    </div>

    <div class="tarjeta sin-padding">
      <p-table #tabla [value]="pacientes()" [loading]="cargando()" [paginator]="true" [rows]="8" [rowHover]="true"
               [globalFilterFields]="['nombres', 'apellidos', 'numeroDocumento', 'codigoPaciente', 'telefono', 'email']"
               currentPageReportTemplate="Mostrando {first} a {last} de {totalRecords}" [showCurrentPageReport]="true">
        <ng-template #caption>
          <p-iconfield>
            <p-inputicon class="pi pi-search" />
            <input pInputText type="text" placeholder="Buscar por nombre, DUI, código, teléfono..." class="buscador"
                   (input)="tabla.filterGlobal($any($event.target).value, 'contains')" />
          </p-iconfield>
        </ng-template>

        <ng-template #header>
          <tr>
            <th pSortableColumn="apellidos">Paciente <p-sortIcon field="apellidos" /></th>
            <th>Documento</th>
            <th pSortableColumn="fechaNacimiento">Edad <p-sortIcon field="fechaNacimiento" /></th>
            <th>Contacto</th>
            <th>Ciudad</th>
            <th>Estado</th>
            <th class="derecha">Acciones</th>
          </tr>
        </ng-template>

        <ng-template #body let-p>
          <tr>
            <td>
              <div class="persona">
                <p-avatar [label]="ini(p.nombres + ' ' + p.apellidos)" shape="circle" [class]="p.sexo === 'F' ? 'av-f' : 'av-m'" />
                <div>
                  <strong>{{ p.nombres }} {{ p.apellidos }}</strong>
                  <span>{{ p.codigoPaciente }}</span>
                </div>
              </div>
            </td>
            <td>{{ p.tipoDocumento }} {{ p.numeroDocumento }}</td>
            <td>{{ edad(p.fechaNacimiento) }} años</td>
            <td>
              <div class="contacto">
                @if (p.telefono) { <span><i class="pi pi-phone"></i> {{ p.telefono }}</span> }
                @if (p.email) { <span><i class="pi pi-envelope"></i> {{ p.email }}</span> }
              </div>
            </td>
            <td>{{ p.ciudad || '—' }}</td>
            <td>
              <span class="estado" [style.color]="p.activo ? '#059669' : '#6b7280'" [style.background]="p.activo ? '#d1fae5' : '#f1f5f9'">
                {{ p.activo ? 'Activo' : 'Inactivo' }}
              </span>
            </td>
            <td class="derecha">
              @if (puedeEditar()) {
                <p-button icon="pi pi-calendar-plus" [rounded]="true" [text]="true" pTooltip="Agendar cita" (onClick)="agendar(p)" />
                <p-button icon="pi pi-pencil" [rounded]="true" [text]="true" severity="secondary" pTooltip="Editar" (onClick)="editar(p)" />
              }
              @if (esMedico()) {
                <p-button icon="pi pi-folder-open" [rounded]="true" [text]="true" pTooltip="Expediente" (onClick)="expediente(p)" />
              }
            </td>
          </tr>
        </ng-template>

        <ng-template #emptymessage>
          <tr><td colspan="7"><div class="vacio"><i class="pi pi-users"></i>No se encontraron pacientes.</div></td></tr>
        </ng-template>
      </p-table>
    </div>

    <!-- ============ Formulario nuevo / editar ============ -->
    <p-dialog [(visible)]="dialogo" [modal]="true" [style]="{ width: '760px' }" [breakpoints]="{ '800px': '95vw' }"
              [header]="form.pacienteID ? 'Editar paciente' : 'Nuevo paciente'" [draggable]="false">
      <form #f="ngForm" class="formulario" (ngSubmit)="guardar(f)">
        <div class="campo">
          <label>Nombres *</label>
          <input pInputText name="nombres" [(ngModel)]="form.nombres" required />
        </div>
        <div class="campo">
          <label>Apellidos *</label>
          <input pInputText name="apellidos" [(ngModel)]="form.apellidos" required />
        </div>
        <div class="campo">
          <label>Tipo de documento *</label>
          <p-select name="tipoDocumento" [(ngModel)]="form.tipoDocumento" [options]="tiposDocumento" required appendTo="body" />
        </div>
        <div class="campo">
          <label>Número de documento *</label>
          <input pInputText name="numeroDocumento" [(ngModel)]="form.numeroDocumento" required placeholder="00000000-0" />
        </div>
        <div class="campo">
          <label>Fecha de nacimiento *</label>
          <p-datepicker name="fechaNacimiento" [(ngModel)]="form.fechaNacimiento" required dateFormat="dd/mm/yy"
                        [maxDate]="hoy" [showIcon]="true" appendTo="body" placeholder="dd/mm/aaaa" />
        </div>
        <div class="campo">
          <label>Sexo *</label>
          <p-select name="sexo" [(ngModel)]="form.sexo" [options]="sexos" optionLabel="label" optionValue="value" required appendTo="body" placeholder="Seleccionar" />
        </div>
        <div class="campo">
          <label>Teléfono</label>
          <input pInputText name="telefono" [(ngModel)]="form.telefono" placeholder="7000-0000" />
        </div>
        <div class="campo">
          <label>Correo electrónico</label>
          <input pInputText name="email" type="email" [(ngModel)]="form.email" email placeholder="paciente@correo.com" />
        </div>
        <div class="campo">
          <label>Estado civil</label>
          <p-select name="estadoCivil" [(ngModel)]="form.estadoCivil" [options]="estadosCiviles" [showClear]="true" appendTo="body" placeholder="Seleccionar" />
        </div>
        <div class="campo">
          <label>Tipo de sangre</label>
          <p-select name="tipoSangre" [(ngModel)]="form.tipoSangre" [options]="tiposSangre" [showClear]="true" appendTo="body" placeholder="Seleccionar" />
        </div>
        <div class="campo completo">
          <label>Dirección</label>
          <input pInputText name="direccion" [(ngModel)]="form.direccion" />
        </div>
        <div class="campo">
          <label>Ciudad</label>
          <input pInputText name="ciudad" [(ngModel)]="form.ciudad" />
        </div>
        <div class="campo">
          <label>Ocupación</label>
          <input pInputText name="ocupacion" [(ngModel)]="form.ocupacion" />
        </div>

        <div class="completo pie">
          <p-button label="Cancelar" severity="secondary" [text]="true" (onClick)="dialogo = false" />
          <p-button type="submit" label="Guardar paciente" icon="pi pi-check" [loading]="guardando()" [disabled]="!f.valid" />
        </div>
      </form>
    </p-dialog>
  `,
  styles: `
    .sin-padding { padding: 0; overflow: hidden; }
    .buscador { width: 360px; max-width: 100%; }
    .persona { display: flex; align-items: center; gap: .75rem; strong { display: block; } span { font-size: .78rem; color: var(--mc-suave); } }
    .contacto { display: flex; flex-direction: column; gap: .15rem; font-size: .82rem; i { color: var(--mc-suave); font-size: .75rem; } }
    .derecha { text-align: right; white-space: nowrap; }
    .pie { display: flex; justify-content: flex-end; gap: .5rem; margin-top: .5rem; }
    :host ::ng-deep .av-m { background: #dbeafe; color: #1d4ed8; font-weight: 600; }
    :host ::ng-deep .av-f { background: #fce7f3; color: #be185d; font-weight: 600; }
  `
})
export class Pacientes {
  private directory = inject(DirectoryService);
  private auth = inject(AuthService);
  private toast = inject(MessageService);
  private router = inject(Router);

  protected pacientes = signal<Paciente[]>([]);
  protected cargando = signal(true);
  protected guardando = signal(false);
  protected dialogo = false;
  protected form: FormPaciente = { ...VACIO };
  protected hoy = new Date();

  protected puedeEditar = computed(() => this.auth.tieneRol('Administrador', 'Recepcion'));
  protected esMedico = computed(() => this.auth.tieneRol('Medico', 'Administrador'));

  protected edad = edad;
  protected ini = iniciales;

  protected tiposDocumento = ['DUI', 'Pasaporte', 'Carnet de residente', 'Partida de nacimiento'];
  protected sexos = [{ label: 'Masculino', value: 'M' }, { label: 'Femenino', value: 'F' }];
  protected estadosCiviles = ['Soltero(a)', 'Casado(a)', 'Acompañado(a)', 'Divorciado(a)', 'Viudo(a)'];
  protected tiposSangre = ['O+', 'O-', 'A+', 'A-', 'B+', 'B-', 'AB+', 'AB-'];

  constructor() {
    this.cargar();
  }

  cargar(): void {
    this.directory.pacientes().subscribe({
      next: p => { this.pacientes.set(p); this.cargando.set(false); },
      error: err => { this.cargando.set(false); this.toast.add({ severity: 'error', summary: 'Error', detail: mensajeError(err) }); }
    });
  }

  nuevo(): void {
    this.form = { ...VACIO };
    this.dialogo = true;
  }

  editar(p: Paciente): void {
    this.form = { ...p, fechaNacimiento: leerFecha(p.fechaNacimiento) };
    this.dialogo = true;
  }

  guardar(f: NgForm): void {
    if (f.invalid || !this.form.fechaNacimiento) return;
    this.guardando.set(true);

    const datos = { ...this.form, fechaNacimiento: fechaParam(this.form.fechaNacimiento) } as unknown as Partial<Paciente>;
    const peticion = this.form.pacienteID
      ? this.directory.actualizarPaciente(this.form.pacienteID, datos)
      : this.directory.crearPaciente(datos);

    peticion.subscribe({
      next: p => {
        this.guardando.set(false);
        this.dialogo = false;
        this.toast.add({
          severity: 'success',
          summary: this.form.pacienteID ? 'Paciente actualizado' : 'Paciente registrado',
          detail: `${p.nombres} ${p.apellidos} (${p.codigoPaciente})`
        });
        this.cargar();
      },
      error: err => {
        this.guardando.set(false);
        this.toast.add({ severity: 'error', summary: 'No se pudo guardar', detail: mensajeError(err) });
      }
    });
  }

  agendar(p: Paciente): void {
    this.router.navigate(['/agendar'], { queryParams: { pacienteId: p.pacienteID } });
  }

  expediente(p: Paciente): void {
    this.router.navigate(['/expediente', p.pacienteID]);
  }
}
