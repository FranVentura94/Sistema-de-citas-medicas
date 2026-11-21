import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable, forkJoin, map, of, switchMap } from 'rxjs';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { MultiSelect } from 'primeng/multiselect';
import { DatePicker } from 'primeng/datepicker';
import { Checkbox } from 'primeng/checkbox';
import { SelectButton } from 'primeng/selectbutton';
import { Textarea } from 'primeng/textarea';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Skeleton } from 'primeng/skeleton';
import { Historial } from '../../shared/historial';
import { AdminService, Empleado, EmpleadoDatos, RolSistema, UsuarioAdmin } from '../../core/admin.service';
import { DirectoryService } from '../../core/api.services';
import { AuthService } from '../../core/auth.service';
import { Especialidad, Medico } from '../../core/models';
import { fechaParam, iniciales, leerFecha, mensajeError, nombreRol } from '../../core/utils';

// ---------- Cargos con su color e ícono ----------
const CARGOS: Record<string, { color: string; fondo: string; icono: string }> = {
  'Médico': { color: '#0f766e', fondo: '#ccfbf1', icono: 'pi pi-heart' },
  'Enfermería': { color: '#be185d', fondo: '#fce7f3', icono: 'pi pi-plus-circle' },
  'Recepción': { color: '#1d4ed8', fondo: '#dbeafe', icono: 'pi pi-phone' },
  'Administrativo': { color: '#6d28d9', fondo: '#ede9fe', icono: 'pi pi-briefcase' },
  'Otro': { color: '#475569', fondo: '#f1f5f9', icono: 'pi pi-user' }
};
const LISTA_CARGOS = Object.keys(CARGOS);

// Nivel de acceso que se sugiere según el cargo
const NIVEL_SUGERIDO: Record<string, string> = {
  'Médico': 'Medico', 'Enfermería': 'Enfermeria', 'Recepción': 'Recepcion', 'Administrativo': 'Recepcion', 'Otro': 'Recepcion'
};

// ---------- Estado del acceso al portal ----------
type EstadoAcceso = 'activo' | 'pendiente' | 'vencida' | 'sin-contrasena' | 'desactivado' | 'sin-acceso';
const ESTADOS: Record<EstadoAcceso, { texto: string; icono: string; clase: string }> = {
  'activo': { texto: 'Con acceso', icono: 'pi pi-check-circle', clase: 'verde' },
  'pendiente': { texto: 'Esperando primer ingreso', icono: 'pi pi-hourglass', clase: 'ambar' },
  'vencida': { texto: 'Contraseña temporal vencida', icono: 'pi pi-exclamation-triangle', clase: 'rojo' },
  'sin-contrasena': { texto: 'Falta generar contraseña', icono: 'pi pi-key', clase: 'ambar' },
  'desactivado': { texto: 'Acceso desactivado', icono: 'pi pi-lock', clase: 'gris' },
  'sin-acceso': { texto: 'Sin acceso al portal', icono: 'pi pi-minus-circle', clase: 'gris' }
};

type Filtro = 'todos' | 'con-acceso' | 'pendientes' | 'sin-acceso';

interface Fila {
  emp: Empleado;
  usuario: UsuarioAdmin | null;
  estado: EstadoAcceso;
}

interface FormEmpleado {
  nombres: string;
  apellidos: string;
  identificacion: string;
  fechaNacimiento: Date | null;
  genero: 'F' | 'M' | null;
  telefono: string;
  email: string;
  direccion: string;
  cargo: string;
  fechaIngreso: Date | null;
  notas: string;
  emergenciaNombre: string;
  emergenciaParentesco: string;
  emergenciaTelefono: string;
  activo: boolean;
  numeroColegiatura: string;
  especialidadIds: number[];
  especialidadPrincipalId: number | null;
  medicosAsignados: number[];
}

@Component({
  selector: 'app-personal',
  imports: [
    DatePipe, FormsModule, Button, Dialog, InputText, Select, MultiSelect, DatePicker, Checkbox,
    SelectButton, Textarea, IconField, InputIcon, Skeleton, Historial
  ],
  template: `
    <div class="pagina-encabezado">
      <div>
        <h1>Personal de la clínica</h1>
        <p>Registra a tu equipo y decide quién puede entrar al portal y con qué nivel de acceso</p>
      </div>
      <p-button label="Registrar empleado" icon="pi pi-user-plus" (onClick)="nuevo()" />
    </div>

    <!-- ============ Resumen (también sirven de filtro) ============ -->
    <div class="resumen">
      @for (r of resumen(); track r.filtro) {
        <button type="button" class="tarjeta indicador" [class.elegido]="filtro() === r.filtro" (click)="filtro.set(r.filtro)">
          <div class="icono" [style.background]="r.fondo" [style.color]="r.color"><i [class]="r.icono"></i></div>
          <div>
            <span>{{ r.titulo }}</span>
            <strong>{{ r.valor }}</strong>
          </div>
        </button>
      }
    </div>

    <!-- ============ Búsqueda y cargos ============ -->
    <div class="tarjeta barra-filtros">
      <p-iconfield class="buscar">
        <p-inputicon class="pi pi-search" />
        <input pInputText [ngModel]="busqueda()" (ngModelChange)="busqueda.set($event)" placeholder="Buscar por nombre, usuario, teléfono o identificación" />
      </p-iconfield>
      <div class="chips">
        <button type="button" [class.activo]="cargoFiltro() === null" (click)="cargoFiltro.set(null)">Todos <b>{{ empleados().length }}</b></button>
        @for (c of cargos; track c) {
          <button type="button" [class.activo]="cargoFiltro() === c" (click)="cargoFiltro.set(c)"
                  [style.--c]="meta(c).color" [style.--f]="meta(c).fondo">
            <i [class]="meta(c).icono"></i> {{ c }} <b>{{ cuentaCargo(c) }}</b>
          </button>
        }
      </div>
    </div>

    <!-- ============ Tarjetas del personal ============ -->
    @if (cargando()) {
      <div class="rejilla">
        @for (i of [1, 2, 3, 4, 5, 6]; track i) { <p-skeleton height="190px" borderRadius="16px" /> }
      </div>
    } @else if (filas().length === 0) {
      <div class="tarjeta vacio"><i class="pi pi-users"></i>No hay personas que coincidan con el filtro.</div>
    } @else {
      <div class="rejilla">
        @for (f of filas(); track f.emp.empleadoID; let i = $index) {
          <button type="button" class="persona" [class.inactivo]="!f.emp.activo" (click)="editar(f.emp)"
                  [style.--c]="meta(f.emp.cargo).color" [style.--f]="meta(f.emp.cargo).fondo" [style.animation-delay.ms]="i * 35">
            <div class="cabeza">
              <div class="avatar">{{ ini(f.emp.nombreCompleto) }}</div>
              <div class="nombre">
                <strong>{{ titulo(f.emp) }}{{ f.emp.nombreCompleto }}</strong>
                <span class="cargo"><i [class]="meta(f.emp.cargo).icono"></i> {{ f.emp.cargo }}</span>
              </div>
              @if (!f.emp.activo) { <span class="etiqueta-inactivo">Inactivo</span> }
            </div>

            <ul class="datos">
              @if (f.emp.telefono) { <li><i class="pi pi-phone"></i>{{ f.emp.telefono }}</li> }
              @if (f.emp.email) { <li><i class="pi pi-envelope"></i>{{ f.emp.email }}</li> }
              @if (f.emp.cargo === 'Enfermería' && f.emp.medicosAsignados.length) {
                <li><i class="pi pi-link"></i>Asiste a {{ nombresAsignados(f.emp) }}</li>
              }
              @if (!f.emp.telefono && !f.emp.email) { <li class="texto-suave"><i class="pi pi-info-circle"></i>Sin datos de contacto</li> }
            </ul>

            <div class="pie">
              <span class="acceso-pill {{ estados[f.estado].clase }}">
                <i [class]="estados[f.estado].icono"></i> {{ estados[f.estado].texto }}
              </span>
              @if (f.usuario && f.estado !== 'sin-acceso') {
                <span class="usuario-tag">&#64;{{ f.usuario.nombreUsuario }} · {{ nombreRol(f.usuario.rol) }}</span>
              }
            </div>
          </button>
        }
      </div>
    }

    <!-- ============ Ficha del empleado ============ -->
    <p-dialog [visible]="dialogo()" (visibleChange)="dialogo.set($event)" [modal]="true" [draggable]="false"
              [style]="{ width: '880px' }" [breakpoints]="{ '960px': '96vw' }" styleClass="dialogo-ficha" [dismissableMask]="false">
      <ng-template #header>
        <div class="ficha-titulo" [style.--c]="meta(form.cargo).color" [style.--f]="meta(form.cargo).fondo">
          <div class="avatar grande">{{ ini(form.nombres + ' ' + form.apellidos) || '?' }}</div>
          <div>
            <h2>{{ actual() ? (form.nombres + ' ' + form.apellidos).trim() : 'Nuevo empleado' }}</h2>
            <span>{{ actual() ? 'Ficha registrada el ' + (actual()!.fechaRegistro | date: 'd MMM y') : 'Solo el nombre es obligatorio; completa lo que sepas' }}</span>
          </div>
                    @if (actual()) {
            <p-button label="Historial" icon="pi pi-history" severity="secondary" [outlined]="true" size="small" class="boton-historial"
                      (onClick)="verHistorial(historial)" />
          }
        </div>
      </ng-template>

      <div class="ficha">
        <!-- Datos personales -->
        <section>
          <h3><i class="pi pi-user"></i> Datos personales</h3>
          <div class="formulario tres">
            <div class="campo"><label>Nombres *</label><input pInputText [(ngModel)]="form.nombres" placeholder="Ana María" /></div>
            <div class="campo"><label>Apellidos</label><input pInputText [(ngModel)]="form.apellidos" placeholder="Pérez López" /></div>
            <div class="campo"><label>Identificación (DUI)</label><input pInputText [(ngModel)]="form.identificacion" placeholder="00000000-0" /></div>
            <div class="campo">
              <label>Fecha de nacimiento</label>
              <p-datepicker [(ngModel)]="form.fechaNacimiento" dateFormat="dd/mm/yy" [showIcon]="true" appendTo="body" [maxDate]="hoy" placeholder="dd/mm/aaaa" />
            </div>
            <div class="campo">
              <label>Género <small class="texto-suave">(para saludarle bien)</small></label>
              <p-selectbutton [options]="generos" [(ngModel)]="form.genero" optionLabel="texto" optionValue="valor" [allowEmpty]="true" />
            </div>
          </div>
        </section>

        <!-- Contacto -->
        <section>
          <h3><i class="pi pi-phone"></i> Contacto</h3>
          <div class="formulario tres">
            <div class="campo"><label>Teléfono</label><input pInputText [(ngModel)]="form.telefono" placeholder="(503) 0000-0000" /></div>
            <div class="campo"><label>Correo</label><input pInputText type="email" [(ngModel)]="form.email" placeholder="nombre@clinica.com" /></div>
            <div class="campo"><label>Dirección</label><input pInputText [(ngModel)]="form.direccion" placeholder="Colonia, ciudad" /></div>
          </div>
        </section>

        <!-- Trabajo -->
        <section>
          <h3><i class="pi pi-briefcase"></i> Trabajo</h3>
          <div class="cargos">
            @for (c of cargos; track c) {
              <button type="button" [class.activo]="form.cargo === c" (click)="cambiarCargo(c)"
                      [style.--c]="meta(c).color" [style.--f]="meta(c).fondo">
                <i [class]="meta(c).icono"></i>{{ c }}
              </button>
            }
          </div>

          <div class="formulario tres">
            <div class="campo">
              <label>Fecha de ingreso</label>
              <p-datepicker [(ngModel)]="form.fechaIngreso" dateFormat="dd/mm/yy" [showIcon]="true" appendTo="body" placeholder="dd/mm/aaaa" />
            </div>
            @if (form.cargo === 'Médico') {
              <div class="campo"><label>N.º de colegiatura (JVPM)</label><input pInputText [(ngModel)]="form.numeroColegiatura" placeholder="JVPM-00000" /></div>
            }
            @if (actual()) {
              <div class="campo">
                <label>Estado en la clínica</label>
                <p-selectbutton [options]="estadosEmpleado" [(ngModel)]="form.activo" optionLabel="texto" optionValue="valor" [allowEmpty]="false" />
              </div>
            }
          </div>

          @if (form.cargo === 'Médico') {
            <div class="sub-bloque">
              <div class="formulario">
                <div class="campo">
                  <label>Especialidades * <button type="button" class="enlace" (click)="nuevaEspecialidad()">+ Nueva</button></label>
                  <p-multiselect [options]="especialidades()" [(ngModel)]="form.especialidadIds" optionLabel="nombre" optionValue="especialidadID"
                                 display="chip" placeholder="Elige una o varias" appendTo="body" (onChange)="ajustarPrincipal()" [filter]="true" />
                </div>
                <div class="campo">
                  <label>Especialidad principal</label>
                  <p-select [options]="seleccionadas()" [(ngModel)]="form.especialidadPrincipalId" optionLabel="nombre"
                            optionValue="especialidadID" placeholder="La primera elegida" appendTo="body" />
                </div>
              </div>
              <small class="texto-suave"><i class="pi pi-info-circle"></i> Así aparecerá en la agenda para que le agenden citas. Sus horarios se configuran en <b>Horarios</b>.</small>
            </div>
          }

          @if (form.cargo === 'Enfermería') {
            <div class="sub-bloque rosa">
              <div class="campo">
                <label>¿A qué médicos asiste?</label>
                <p-multiselect [options]="medicos()" [(ngModel)]="form.medicosAsignados" optionLabel="nombreCompleto" optionValue="medicoID"
                               display="chip" placeholder="Elige uno o varios médicos" appendTo="body" [filter]="true" />
              </div>
              <small class="texto-suave"><i class="pi pi-info-circle"></i> Verá la agenda de estos médicos y podrá tomar signos vitales a sus pacientes.</small>
            </div>
          }

          <div class="campo">
            <label>Notas</label>
            <textarea pTextarea [(ngModel)]="form.notas" rows="2" placeholder="Turno, observaciones, etc."></textarea>
          </div>
        </section>

        <!-- Emergencia -->
        <section>
          <h3><i class="pi pi-heart"></i> Contacto de emergencia</h3>
          <div class="formulario tres">
            <div class="campo"><label>Nombre del contacto</label><input pInputText [(ngModel)]="form.emergenciaNombre" placeholder="María López" /></div>
            <div class="campo"><label>Parentesco</label><input pInputText [(ngModel)]="form.emergenciaParentesco" placeholder="Esposa" /></div>
            <div class="campo"><label>Teléfono</label><input pInputText [(ngModel)]="form.emergenciaTelefono" placeholder="(503) 0000-0000" /></div>
          </div>
        </section>

        <!-- ============ Acceso al portal ============ -->
        <section class="acceso" [class.encendido]="acceso.conAcceso">
          <label class="check-acceso">
            <p-checkbox [(ngModel)]="acceso.conAcceso" [binary]="true" [disabled]="esYoMismo()" (onChange)="alCambiarAcceso()" inputId="conAcceso" />
            <span>Con acceso al portal web</span>
            @if (usuarioActual(); as u) {
              <span class="acceso-pill {{ estados[estadoDe(u)].clase }}"><i [class]="estados[estadoDe(u)].icono"></i> {{ estados[estadoDe(u)].texto }}</span>
            }
          </label>
          <small class="texto-suave">
            @if (esYoMismo()) { Es tu propio usuario: no puedes quitarte el acceso. }
            @else { Sin marcar, la persona queda solo como registro de personal y no puede iniciar sesión. }
          </small>

          @if (acceso.conAcceso) {
            <div class="separador"></div>

            @if (usuarioActual(); as u) {
              @if (!esYoMismo() && u.activo) {
                <div class="generar">
                  <p-button label="Generar contraseña temporal" icon="pi pi-key" severity="secondary" [outlined]="true"
                            [loading]="generando()" (onClick)="generarTemporal(u)" [class.pulso]="resaltarGenerar()" />
                  <small class="texto-suave">
                    Díctala ahora: no se vuelve a mostrar. La persona la usa una vez y define la suya al entrar.
                    <b>Nadie puede ver la contraseña que elija.</b>
                  </small>
                </div>
              } @else if (!u.activo) {
                <small class="aviso"><i class="pi pi-info-circle"></i> Guarda los cambios para reactivar su acceso; después podrás generarle una contraseña temporal.</small>
              }
              @if (u.debeCambiarPassword && u.passwordTemporalVence) {
                <small class="texto-suave"><i class="pi pi-clock"></i> Su contraseña temporal vence el {{ leer(u.passwordTemporalVence) | date: "EEEE d 'de' MMMM, h:mm a" }}.</small>
              }
              @if (u.ultimoAcceso) {
                <small class="texto-suave"><i class="pi pi-sign-in"></i> Último ingreso: {{ leer(u.ultimoAcceso) | date: "d 'de' MMMM, h:mm a" }}</small>
              }
            } @else {
              <small class="aviso"><i class="pi pi-info-circle"></i> Al guardar se crea su acceso. Luego, aquí mismo, generas la contraseña temporal para dictársela.</small>
            }

            <div class="formulario">
              <div class="campo">
                <label>Usuario</label>
                <p-iconfield>
                  <p-inputicon class="pi pi-at" />
                  <input pInputText [(ngModel)]="acceso.nombreUsuario" [readonly]="!!usuarioActual()" placeholder="hlopez" autocomplete="off" />
                </p-iconfield>
                @if (!usuarioActual()) { <small class="texto-suave">Minúsculas, sin espacios. El usuario no se puede cambiar después.</small> }
              </div>
              <div class="campo">
                <label>Nivel de acceso</label>
                <p-select [options]="nivelesDisponibles()" [(ngModel)]="acceso.rolId" optionLabel="etiqueta" optionValue="rolId"
                          appendTo="body" [disabled]="esYoMismo()" placeholder="Elige un nivel">
                  <ng-template #item let-r>
                    <div class="nivel-opcion"><strong>{{ r.etiqueta }}</strong><small>{{ r.descripcion }}</small></div>
                  </ng-template>
                </p-select>
              </div>
            </div>
          }
        </section>
      </div>

      <ng-template #footer>
        <p-button label="Cancelar" severity="secondary" [text]="true" (onClick)="dialogo.set(false)" />
        <p-button [label]="actual() ? 'Guardar cambios' : 'Registrar'" icon="pi pi-check" (onClick)="guardar()" [loading]="guardando()" />
      </ng-template>
    </p-dialog>

    <!-- ============ Contraseña temporal (se muestra UNA vez) ============ -->
    <p-dialog [visible]="!!temporal()" (visibleChange)="!$event && temporal.set(null)" [modal]="true" [draggable]="false"
              [style]="{ width: '480px' }" [breakpoints]="{ '560px': '96vw' }" [closable]="true">
      <ng-template #header><strong class="titulo-temporal"><i class="pi pi-key"></i> Contraseña temporal</strong></ng-template>
      @if (temporal(); as t) {
        <div class="temporal">
          <p>Díctale estos datos a <b>{{ t.nombre }}</b>:</p>
          <div class="credencial">
            <span>Usuario</span>
            <strong>{{ t.usuario }}</strong>
          </div>
          <div class="credencial clave">
            <span>Contraseña temporal</span>
            <strong>
              @for (g of t.password.split('-'); track $index; let ultimo = $last) {
                <em>{{ g }}</em>@if (!ultimo) {<i>-</i>}
              }
            </strong>
          </div>
          <div class="acciones-temporal">
            <p-button [label]="copiado() === 'clave' ? '¡Copiada!' : 'Copiar contraseña'" [icon]="copiado() === 'clave' ? 'pi pi-check' : 'pi pi-copy'"
                      severity="secondary" [outlined]="true" (onClick)="copiar(t.password, 'clave')" />
            <p-button [label]="copiado() === 'mensaje' ? '¡Mensaje copiado!' : 'Copiar mensaje para enviar'" [icon]="copiado() === 'mensaje' ? 'pi pi-check' : 'pi pi-send'"
                      (onClick)="copiar(mensajeTemporal(t), 'mensaje')" />
          </div>
          <ul class="avisos">
            <li><i class="pi pi-eye-slash"></i> No se vuelve a mostrar. Si se pierde, genera otra.</li>
            <li><i class="pi pi-clock"></i> Vence el {{ leer(t.vence) | date: "EEEE d 'de' MMMM 'a las' h:mm a" }}.</li>
            <li><i class="pi pi-shield"></i> Al entrar, el sistema le pedirá crear su propia contraseña. Nadie más podrá verla.</li>
          </ul>
        </div>
      }
      <ng-template #footer>
        <p-button label="Listo" icon="pi pi-check" (onClick)="temporal.set(null)" />
      </ng-template>
    </p-dialog>

        <app-historial #historial />

    <!-- ============ Nueva especialidad ============ -->
    <p-dialog [visible]="dialogoEspecialidad()" (visibleChange)="dialogoEspecialidad.set($event)" [modal]="true"
              [style]="{ width: '420px' }" header="Nueva especialidad" [draggable]="false">
      <div class="formulario">
        <div class="campo"><label>Código *</label><input pInputText [(ngModel)]="especialidad.codigo" placeholder="NEU" maxlength="20" /></div>
        <div class="campo"><label>Nombre *</label><input pInputText [(ngModel)]="especialidad.nombre" placeholder="Neurología" /></div>
        <div class="campo completo"><label>Descripción</label><input pInputText [(ngModel)]="especialidad.descripcion" /></div>
      </div>
      <ng-template #footer>
        <p-button label="Cancelar" severity="secondary" [text]="true" (onClick)="dialogoEspecialidad.set(false)" />
        <p-button label="Agregar" icon="pi pi-check" (onClick)="guardarEspecialidad()" />
      </ng-template>
    </p-dialog>
  `,
  styles: `
    /* ---------- Resumen ---------- */
    .resumen { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin-bottom: 1.25rem; }
    .indicador {
      display: flex; align-items: center; gap: 1rem; cursor: pointer; text-align: left; font: inherit; color: inherit;
      transition: transform .15s, box-shadow .15s, border-color .15s;
      &:hover { transform: translateY(-2px); box-shadow: 0 10px 24px rgba(16, 24, 40, .08); }
      &.elegido { border-color: var(--mc-primario); box-shadow: 0 0 0 3px rgba(13, 148, 136, .15); }
      .icono { width: 46px; height: 46px; border-radius: 12px; display: grid; place-items: center; font-size: 1.25rem; }
      span { color: var(--mc-suave); font-size: .85rem; }
      strong { display: block; font-size: 1.6rem; }
    }

    /* ---------- Filtros ---------- */
    .barra-filtros { display: flex; flex-direction: column; gap: .9rem; margin-bottom: 1.25rem; }
    :host ::ng-deep .buscar, :host ::ng-deep .buscar input { width: 100%; }
    .chips { display: flex; flex-wrap: wrap; gap: .5rem; }
    .chips button {
      --c: var(--mc-primario-oscuro); --f: #f0fdfa;
      border: 1px solid var(--mc-borde); background: #fff; border-radius: 999px; padding: .4rem .85rem; cursor: pointer;
      font: inherit; font-size: .85rem; display: inline-flex; align-items: center; gap: .4rem; color: var(--mc-texto); transition: all .15s;
      b { background: #f1f5f9; border-radius: 999px; padding: 0 .45rem; font-size: .75rem; }
      &:hover { border-color: var(--c); color: var(--c); }
      &.activo { background: var(--f); border-color: var(--c); color: var(--c); font-weight: 600; b { background: #fff; } }
    }

    /* ---------- Tarjetas de personas ---------- */
    .rejilla { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1rem; }
    .persona {
      --c: var(--mc-primario); --f: #f0fdfa;
      position: relative; overflow: hidden; text-align: left; font: inherit; color: inherit; cursor: pointer;
      display: flex; flex-direction: column; gap: .85rem; padding: 1.15rem 1.15rem 1rem;
      background: #fff; border: 1px solid var(--mc-borde); border-radius: 16px; box-shadow: var(--mc-sombra);
      transition: transform .18s ease, box-shadow .18s ease, border-color .18s;
      animation: aparecer .35s ease both;
      &::before { content: ''; position: absolute; inset: 0 0 auto; height: 4px; background: linear-gradient(90deg, var(--c), color-mix(in srgb, var(--c) 40%, white)); }
      &:hover { transform: translateY(-4px); box-shadow: 0 14px 30px rgba(16, 24, 40, .12); border-color: color-mix(in srgb, var(--c) 40%, white); }
      &.inactivo { opacity: .6; filter: grayscale(.5); }
    }
    @keyframes aparecer { from { opacity: 0; transform: translateY(8px); } }
    .cabeza { display: flex; align-items: center; gap: .8rem; }
    .avatar {
      width: 48px; height: 48px; flex-shrink: 0; border-radius: 14px; display: grid; place-items: center; font-weight: 700; font-size: 1rem;
      color: #fff; background: linear-gradient(135deg, var(--c), color-mix(in srgb, var(--c) 55%, white));
      box-shadow: 0 6px 14px color-mix(in srgb, var(--c) 30%, transparent);
    }
    .avatar.grande { width: 56px; height: 56px; font-size: 1.2rem; border-radius: 16px; }
    .nombre { flex: 1; min-width: 0; strong { display: block; font-size: 1rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; } }
    .cargo { display: inline-flex; align-items: center; gap: .3rem; margin-top: .2rem; font-size: .75rem; font-weight: 600; color: var(--c); background: var(--f); padding: .15rem .55rem; border-radius: 999px; }
    .etiqueta-inactivo { font-size: .7rem; background: #e2e8f0; color: #475569; border-radius: 6px; padding: .15rem .45rem; font-weight: 600; }
    .datos { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: .35rem; font-size: .83rem; color: #475569;
      li { display: flex; align-items: center; gap: .5rem; overflow: hidden; white-space: nowrap; text-overflow: ellipsis; }
      i { color: #94a3b8; font-size: .8rem; } }
    .pie { margin-top: auto; display: flex; flex-wrap: wrap; align-items: center; gap: .4rem .6rem; padding-top: .75rem; border-top: 1px dashed var(--mc-borde); }
    .usuario-tag { font-size: .75rem; color: var(--mc-suave); }

    .acceso-pill { display: inline-flex; align-items: center; gap: .35rem; font-size: .75rem; font-weight: 600; padding: .25rem .6rem; border-radius: 999px; }
    .acceso-pill.verde { background: #dcfce7; color: #15803d; }
    .acceso-pill.ambar { background: #fef3c7; color: #b45309; }
    .acceso-pill.rojo { background: #fee2e2; color: #b91c1c; }
    .acceso-pill.gris { background: #f1f5f9; color: #64748b; }

    /* ---------- Ficha ---------- */
    .boton-historial { margin-left: auto; margin-right: .5rem; }
    .ficha-titulo { --c: var(--mc-primario); display: flex; align-items: center; gap: 1rem; flex: 1;      h2 { font-size: 1.25rem; } span { font-size: .82rem; color: var(--mc-suave); } }
    .ficha { display: flex; flex-direction: column; gap: 1.1rem; }
    .ficha section { border: 1px solid var(--mc-borde); border-radius: 14px; padding: 1rem 1.1rem; display: flex; flex-direction: column; gap: .9rem; }
    .ficha h3 { font-size: .95rem; display: flex; align-items: center; gap: .5rem; color: var(--mc-primario-oscuro); }
    .formulario.tres { grid-template-columns: repeat(3, minmax(0, 1fr)); }
    :host ::ng-deep .ficha .campo p-datepicker, :host ::ng-deep .ficha .campo .p-datepicker,
    :host ::ng-deep .ficha .campo p-multiselect, :host ::ng-deep .ficha .campo .p-multiselect,
    :host ::ng-deep .ficha .campo p-iconfield, :host ::ng-deep .ficha .campo input, :host ::ng-deep .ficha .campo textarea { width: 100%; }
    :host ::ng-deep .ficha .p-selectbutton { display: flex; } :host ::ng-deep .ficha .p-selectbutton .p-togglebutton { flex: 1; }
    .cargos { display: flex; flex-wrap: wrap; gap: .5rem; }
    .cargos button {
      --c: var(--mc-primario); --f: #f0fdfa;
      display: inline-flex; align-items: center; gap: .45rem; padding: .55rem .95rem; border-radius: 12px; cursor: pointer; font: inherit; font-weight: 500;
      border: 1.5px solid var(--mc-borde); background: #fff; color: var(--mc-texto); transition: all .15s;
      &:hover { border-color: var(--c); color: var(--c); }
      &.activo { border-color: var(--c); background: var(--f); color: var(--c); font-weight: 700; box-shadow: 0 4px 12px color-mix(in srgb, var(--c) 18%, transparent); }
    }
    .sub-bloque { background: #f0fdfa; border: 1px solid #99f6e4; border-radius: 12px; padding: .9rem; display: flex; flex-direction: column; gap: .6rem; }
    .sub-bloque.rosa { background: #fdf2f8; border-color: #fbcfe8; }
    .enlace { border: none; background: none; color: var(--mc-primario); cursor: pointer; font: inherit; font-size: .8rem; font-weight: 600; padding: 0 0 0 .4rem; }

    /* ---------- Acceso al portal (como en tu portal) ---------- */
    .acceso { background: #f8fafc; transition: background .2s, border-color .2s; }
    .acceso.encendido { background: #eff6ff; border-color: #bfdbfe; }
    .check-acceso { display: flex; align-items: center; gap: .65rem; font-weight: 600; cursor: pointer; flex-wrap: wrap; }
    .separador { border-top: 1px dashed #bfdbfe; }
    .generar { display: flex; flex-direction: column; align-items: flex-start; gap: .5rem; }
    .aviso { display: flex; gap: .45rem; align-items: flex-start; color: #1d4ed8; }
    :host ::ng-deep .pulso .p-button { animation: pulso 1.4s ease-in-out 4; border-color: var(--mc-primario); color: var(--mc-primario-oscuro); }
    @keyframes pulso { 0%, 100% { box-shadow: 0 0 0 0 rgba(13, 148, 136, .45); } 50% { box-shadow: 0 0 0 10px rgba(13, 148, 136, 0); } }
    .nivel-opcion { display: flex; flex-direction: column; small { color: var(--mc-suave); } }

    /* ---------- Contraseña temporal ---------- */
    .titulo-temporal { display: flex; gap: .5rem; align-items: center; font-size: 1.15rem; i { color: var(--mc-primario); } }
    .temporal { display: flex; flex-direction: column; gap: .9rem; p { margin: 0; } }
    .credencial { border: 1px solid var(--mc-borde); border-radius: 12px; padding: .7rem 1rem; span { display: block; font-size: .75rem; color: var(--mc-suave); } strong { font-size: 1.15rem; } }
    .credencial.clave { background: linear-gradient(135deg, #f0fdfa, #ecfeff); border-color: #5eead4;
      strong { display: flex; align-items: center; gap: .25rem; font-family: 'Consolas', 'Cascadia Mono', monospace; font-size: 1.9rem; letter-spacing: .08em; color: #0f3d3e; }
      em { font-style: normal; background: #fff; border-radius: 8px; padding: .05rem .4rem; box-shadow: 0 1px 3px rgba(0,0,0,.08); }
      i { font-style: normal; color: #94a3b8; } }
    .acciones-temporal { display: flex; gap: .5rem; flex-wrap: wrap; }
    .avisos { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: .45rem; font-size: .83rem; color: #475569;
      i { color: var(--mc-primario); margin-right: .4rem; } }

    @media (max-width: 760px) { .formulario.tres { grid-template-columns: 1fr; } }
    @media (max-width: 640px) {
      .resumen { grid-template-columns: repeat(2, minmax(0, 1fr)); gap: .75rem; }
      .indicador { flex-direction: column; align-items: flex-start; gap: .5rem; padding: .9rem; strong { font-size: 1.35rem; } }
    }
  `
})
export class Personal {
  private admin = inject(AdminService);
  private directory = inject(DirectoryService);
  private auth = inject(AuthService);
  private toast = inject(MessageService);
  private confirmacion = inject(ConfirmationService);

  protected cargos = LISTA_CARGOS;
  protected estados = ESTADOS;
  protected nombreRol = nombreRol;
  protected leer = leerFecha;
  protected hoy = new Date();
  protected generos = [{ texto: 'Femenino', valor: 'F' }, { texto: 'Masculino', valor: 'M' }];
  protected estadosEmpleado = [{ texto: 'Activo', valor: true }, { texto: 'Inactivo', valor: false }];

  // ---------- Datos ----------
  protected empleados = signal<Empleado[]>([]);
  protected usuarios = signal<UsuarioAdmin[]>([]);
  protected roles = signal<RolSistema[]>([]);
  protected especialidades = signal<Especialidad[]>([]);
  protected medicos = signal<Medico[]>([]);
  protected cargando = signal(true);

  // ---------- Filtros ----------
  protected filtro = signal<Filtro>('todos');
  protected cargoFiltro = signal<string | null>(null);
  protected busqueda = signal('');

  // ---------- Ficha abierta ----------
  protected dialogo = signal(false);
  protected actual = signal<Empleado | null>(null);
  protected guardando = signal(false);
  protected generando = signal(false);
  protected resaltarGenerar = signal(false);
  protected form: FormEmpleado = this.formVacio();
  protected acceso = { conAcceso: false, nombreUsuario: '', rolId: null as number | null };

  // ---------- Contraseña temporal recién generada ----------
  protected temporal = signal<{ nombre: string; usuario: string; password: string; vence: string } | null>(null);
  protected copiado = signal<'clave' | 'mensaje' | null>(null);

  protected dialogoEspecialidad = signal(false);
  protected especialidad = { codigo: '', nombre: '', descripcion: '' };

  private usuarioPorId = computed(() => new Map(this.usuarios().map(u => [u.usuarioId, u])));

  protected todasLasFilas = computed<Fila[]>(() => this.empleados().map(emp => {
    const usuario = emp.usuarioID ? this.usuarioPorId().get(emp.usuarioID) ?? null : null;
    return { emp, usuario, estado: usuario ? this.estadoDe(usuario) : 'sin-acceso' };
  }));

  protected filas = computed(() => {
    const texto = this.normalizar(this.busqueda());
    return this.todasLasFilas().filter(f => {
      if (this.cargoFiltro() && f.emp.cargo !== this.cargoFiltro()) return false;
      if (!this.coincideFiltro(f.estado, this.filtro())) return false;
      if (!texto) return true;
      const en = [f.emp.nombreCompleto, f.emp.telefono, f.emp.identificacion, f.emp.email, f.usuario?.nombreUsuario]
        .filter(Boolean).map(v => this.normalizar(v!)).join(' ');
      return en.includes(texto);
    });
  });

  protected resumen = computed(() => {
    const filas = this.todasLasFilas();
    const cuenta = (f: Filtro) => filas.filter(x => this.coincideFiltro(x.estado, f)).length;
    return [
      { filtro: 'todos' as Filtro, titulo: 'Personal registrado', valor: filas.length, icono: 'pi pi-users', color: '#0d9488', fondo: '#ccfbf1' },
      { filtro: 'con-acceso' as Filtro, titulo: 'Con acceso al portal', valor: cuenta('con-acceso'), icono: 'pi pi-check-circle', color: '#16a34a', fondo: '#dcfce7' },
      { filtro: 'pendientes' as Filtro, titulo: 'Pendientes de ingresar', valor: cuenta('pendientes'), icono: 'pi pi-hourglass', color: '#d97706', fondo: '#fef3c7' },
      { filtro: 'sin-acceso' as Filtro, titulo: 'Sin acceso', valor: cuenta('sin-acceso'), icono: 'pi pi-lock', color: '#64748b', fondo: '#f1f5f9' }
    ];
  });

  protected usuarioActual = computed(() => {
    const id = this.actual()?.usuarioID;
    return id ? this.usuarioPorId().get(id) ?? null : null;
  });

  protected esYoMismo = computed(() => !!this.usuarioActual() && this.usuarioActual()!.usuarioId === this.auth.usuario()?.usuarioId);

  constructor() {
    this.cargar();
  }

  // ===================== Carga =====================
  cargar(): void {
    forkJoin({
      empleados: this.admin.empleados(),
      usuarios: this.admin.usuarios(),
      roles: this.admin.roles(),
      especialidades: this.directory.especialidades(),
      medicos: this.directory.medicos()
    }).subscribe({
      next: r => {
        this.empleados.set(r.empleados);
        this.usuarios.set(r.usuarios);
        this.roles.set(r.roles);
        this.especialidades.set(r.especialidades);
        this.medicos.set(r.medicos.filter(m => m.activo));
        this.cargando.set(false);
      },
      error: err => { this.cargando.set(false); this.error(err); }
    });
  }

  // ===================== Abrir ficha =====================
  nuevo(): void {
    this.actual.set(null);
    this.form = this.formVacio();
    this.acceso = { conAcceso: false, nombreUsuario: '', rolId: null };
    this.resaltarGenerar.set(false);
    this.dialogo.set(true);
  }

  editar(e: Empleado): void {
    this.actual.set(e);
    this.form = {
      nombres: e.nombres, apellidos: e.apellidos ?? '', identificacion: e.identificacion ?? '',
      fechaNacimiento: e.fechaNacimiento ? leerFecha(e.fechaNacimiento) : null, genero: e.genero,
      telefono: e.telefono ?? '', email: e.email ?? '', direccion: e.direccion ?? '',
      cargo: e.cargo, fechaIngreso: e.fechaIngreso ? leerFecha(e.fechaIngreso) : null, notas: e.notas ?? '',
      emergenciaNombre: e.emergenciaNombre ?? '', emergenciaParentesco: e.emergenciaParentesco ?? '', emergenciaTelefono: e.emergenciaTelefono ?? '',
      activo: e.activo, numeroColegiatura: e.numeroColegiatura ?? '',
      especialidadIds: [...e.especialidadIds], especialidadPrincipalId: e.especialidadPrincipalId,
      medicosAsignados: e.medicosAsignados.map(m => m.medicoID)
    };
    const u = this.usuarioActual();
    this.acceso = {
      conAcceso: !!u && u.activo,
      nombreUsuario: u?.nombreUsuario ?? '',
      rolId: u?.rolId ?? this.rolSugerido(e.cargo)
    };
    this.resaltarGenerar.set(false);
    this.dialogo.set(true);
  }

  // ===================== Interacciones de la ficha =====================
  cambiarCargo(cargo: string): void {
    this.form.cargo = cargo;
    if (!this.usuarioActual()) this.acceso.rolId = this.rolSugerido(cargo);
    // El nivel Médico solo existe para quien tiene cargo de Médico
    if (cargo !== 'Médico' && this.rolNombre(this.acceso.rolId) === 'Medico') this.acceso.rolId = this.rolSugerido(cargo);
  }

  alCambiarAcceso(): void {
    if (this.acceso.conAcceso && !this.usuarioActual()) {
      if (!this.acceso.nombreUsuario) this.acceso.nombreUsuario = this.sugerirUsuario();
      if (!this.acceso.rolId) this.acceso.rolId = this.rolSugerido(this.form.cargo);
    }
  }

  nivelesDisponibles(): (RolSistema & { etiqueta: string })[] {
    return this.roles()
      .filter(r => r.nombre !== 'Medico' || this.form.cargo === 'Médico')
      .map(r => ({ ...r, etiqueta: nombreRol(r.nombre) }));
  }

  seleccionadas(): Especialidad[] {
    return this.especialidades().filter(e => this.form.especialidadIds.includes(e.especialidadID));
  }

  ajustarPrincipal(): void {
    if (!this.form.especialidadIds.includes(this.form.especialidadPrincipalId ?? -1)) {
      this.form.especialidadPrincipalId = this.form.especialidadIds[0] ?? null;
    }
  }

  // ===================== Guardar ficha + acceso =====================
  guardar(): void {
    const f = this.form;
    if (!f.nombres.trim()) return this.aviso('Falta el nombre', 'Escribe al menos el nombre de la persona.');
    if (f.cargo === 'Médico' && f.especialidadIds.length === 0) return this.aviso('Falta la especialidad', 'Un médico necesita al menos una especialidad.');
    if (this.acceso.conAcceso && !this.usuarioActual()) {
      if (!/^[a-z0-9._-]{4,30}$/.test(this.acceso.nombreUsuario.trim().toLowerCase()))
        return this.aviso('Revisa el usuario', 'De 4 a 30 caracteres: letras, números, punto o guion, sin espacios.');
      if (!this.acceso.rolId) return this.aviso('Falta el nivel de acceso', 'Elige qué nivel de acceso tendrá.');
    }

    const datos: EmpleadoDatos = {
      nombres: f.nombres.trim(), apellidos: this.vacio(f.apellidos), identificacion: this.vacio(f.identificacion),
      fechaNacimiento: f.fechaNacimiento ? fechaParam(f.fechaNacimiento) : null, genero: f.genero,
      telefono: this.vacio(f.telefono), email: this.vacio(f.email), direccion: this.vacio(f.direccion),
      cargo: f.cargo, fechaIngreso: f.fechaIngreso ? fechaParam(f.fechaIngreso) : null, notas: this.vacio(f.notas),
      emergenciaNombre: this.vacio(f.emergenciaNombre), emergenciaParentesco: this.vacio(f.emergenciaParentesco),
      emergenciaTelefono: this.vacio(f.emergenciaTelefono), activo: f.activo,
      numeroColegiatura: this.vacio(f.numeroColegiatura), especialidadIds: f.especialidadIds,
      especialidadPrincipalId: f.especialidadPrincipalId, medicosAsignados: f.medicosAsignados
    };

    const id = this.actual()?.empleadoID;
    const esNuevo = !id;
    this.guardando.set(true);

    (id ? this.admin.actualizarEmpleado(id, datos) : this.admin.crearEmpleado(datos)).pipe(
      switchMap(emp => {
        // La ficha ya quedó guardada: si el acceso falla, se puede reintentar sin duplicarla
        this.ponerEmpleado(emp);
        return this.sincronizarAcceso(emp);
      })
    ).subscribe({
      next: resultado => {
        this.guardando.set(false);
        const nombre = this.form.nombres.trim();

        if (resultado === 'creado') {
          // Se queda abierta para generar la contraseña temporal aquí mismo (como en tu portal)
          this.resaltarGenerar.set(true);
          this.toast.add({ severity: 'success', summary: `Acceso creado para ${nombre}`, detail: 'Ahora genera su contraseña temporal para dictársela.', life: 6000 });
          return;
        }

        this.dialogo.set(false);
        const detalle = resultado === 'desactivado' ? 'Se le quitó el acceso al portal.' : esNuevo ? 'Quedó registrado en el personal.' : 'Los cambios se guardaron.';
        this.toast.add({ severity: 'success', summary: esNuevo ? `¡${nombre} ya es parte del equipo!` : `Ficha de ${nombre} actualizada`, detail: detalle });
      },
      error: err => { this.guardando.set(false); this.error(err); }
    });
  }

  // Crea, actualiza o desactiva el usuario del portal según la casilla
  private sincronizarAcceso(emp: Empleado): Observable<'creado' | 'actualizado' | 'desactivado' | 'nada'> {
    const u = emp.usuarioID ? this.usuarioPorId().get(emp.usuarioID) ?? null : null;
    const rolId = this.acceso.rolId!;
    const medicoId = this.rolNombre(rolId) === 'Medico' ? emp.medicoID : null;
    const base = { nombreCompleto: emp.nombreCompleto, email: emp.email };

    if (this.acceso.conAcceso && !u) {
      return this.admin.crearUsuario({ ...base, nombreUsuario: this.acceso.nombreUsuario.trim().toLowerCase(), rolId, medicoId }).pipe(
        switchMap(nuevo => this.admin.enlazarUsuario(emp.empleadoID, nuevo.usuarioId).pipe(
          map(enlazado => {
            this.usuarios.update(lista => [...lista, nuevo]);
            this.ponerEmpleado(enlazado);
            return 'creado' as const;
          })
        ))
      );
    }

    if (u && (this.acceso.conAcceso || u.activo)) {
      const activo = this.acceso.conAcceso;
      return this.admin.actualizarUsuario(u.usuarioId, { ...base, rolId: activo ? rolId : u.rolId, medicoId: activo ? medicoId : u.medicoId, activo }).pipe(
        map(actualizado => {
          this.usuarios.update(lista => lista.map(x => x.usuarioId === actualizado.usuarioId ? actualizado : x));
          return activo ? 'actualizado' as const : 'desactivado' as const;
        })
      );
    }

    return of('nada' as const);
  }

  // ===================== Contraseña temporal =====================
  generarTemporal(u: UsuarioAdmin): void {
    const nombre = this.form.nombres.trim();
    const generar = () => {
      this.generando.set(true);
      this.admin.generarPasswordTemporal(u.usuarioId).subscribe({
        next: t => {
          this.generando.set(false);
          this.resaltarGenerar.set(false);
          this.copiado.set(null);
          this.usuarios.update(lista => lista.map(x => x.usuarioId === u.usuarioId
            ? { ...x, tieneContrasena: true, debeCambiarPassword: true, passwordTemporalVence: t.venceEn } : x));
          this.temporal.set({ nombre, usuario: u.nombreUsuario, password: t.passwordTemporal, vence: t.venceEn });
        },
        error: err => { this.generando.set(false); this.error(err); }
      });
    };

    // Si ya tiene su propia contraseña, avisar que dejará de funcionar
    if (u.tieneContrasena && !u.debeCambiarPassword) {
      this.confirmacion.confirm({
        header: 'Restablecer contraseña',
        message: `<b>${nombre}</b> ya tiene su propia contraseña. Si generas una temporal, la actual <b>dejará de funcionar</b> y deberá crear una nueva al entrar. ¿Continuar?`,
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Sí, generar',
        rejectLabel: 'Cancelar',
        rejectButtonStyleClass: 'p-button-text p-button-secondary',
        accept: generar
      });
    } else {
      generar();
    }
  }

  mensajeTemporal(t: { nombre: string; usuario: string; password: string; vence: string }): string {
    const vence = leerFecha(t.vence).toLocaleString('es', { weekday: 'long', day: 'numeric', month: 'long', hour: 'numeric', minute: '2-digit' });
    return `Hola ${t.nombre.split(' ')[0]}, ya tienes acceso al portal de la clínica.\n\n` +
      `Página: ${location.origin}\nUsuario: ${t.usuario}\nContraseña temporal: ${t.password}\n\n` +
      `Al entrar, el sistema te pedirá crear tu propia contraseña. La temporal vence el ${vence}.`;
  }

  copiar(texto: string, que: 'clave' | 'mensaje'): void {
    navigator.clipboard.writeText(texto).then(
      () => { this.copiado.set(que); setTimeout(() => this.copiado.set(null), 2500); },
      () => this.aviso('No se pudo copiar', 'Selecciona el texto y cópialo manualmente.')
    );
  }
  // ===================== Historial de cambios =====================
  verHistorial(historial: Historial): void {
    const e = this.actual();
    if (!e) return;
    historial.abrir(e.nombreCompleto, [
      { servicio: 'directory', entidad: 'Empleado', entidadId: e.empleadoID },
      ...(e.usuarioID ? [{ servicio: 'identity' as const, entidad: 'Usuario', entidadId: e.usuarioID }] : []),
      ...(e.medicoID ? [{ servicio: 'directory' as const, entidad: 'Medico', entidadId: e.medicoID }] : [])
    ]);
  }


  // ===================== Especialidades =====================
  nuevaEspecialidad(): void {
    this.especialidad = { codigo: '', nombre: '', descripcion: '' };
    this.dialogoEspecialidad.set(true);
  }

  guardarEspecialidad(): void {
    const e = this.especialidad;
    if (!e.codigo.trim() || !e.nombre.trim()) return this.aviso('Faltan datos', 'El código y el nombre son obligatorios.');
    this.admin.crearEspecialidad({ codigo: e.codigo, nombre: e.nombre, descripcion: this.vacio(e.descripcion) }).subscribe({
      next: nueva => {
        this.especialidades.update(l => [...l, nueva].sort((a, b) => a.nombre.localeCompare(b.nombre)));
        this.form.especialidadIds = [...this.form.especialidadIds, nueva.especialidadID];
        this.ajustarPrincipal();
        this.dialogoEspecialidad.set(false);
        this.toast.add({ severity: 'success', summary: 'Especialidad agregada', detail: nueva.nombre });
      },
      error: err => this.error(err)
    });
  }

  // ===================== Ayudantes =====================
  protected meta(cargo: string) { return CARGOS[cargo] ?? CARGOS['Otro']; }
  protected ini(nombre: string) { return iniciales(nombre.trim()); }
  protected cuentaCargo(cargo: string) { return this.empleados().filter(e => e.cargo === cargo).length; }
  protected titulo(e: Empleado) { return e.cargo === 'Médico' ? (e.genero === 'F' ? 'Dra. ' : e.genero === 'M' ? 'Dr. ' : 'Dr(a). ') : ''; }
  protected nombresAsignados(e: Empleado) { return e.medicosAsignados.map(m => m.nombre).join(', '); }

  protected estadoDe(u: UsuarioAdmin): EstadoAcceso {
    if (!u.activo) return 'desactivado';
    if (!u.tieneContrasena) return 'sin-contrasena';
    if (u.debeCambiarPassword) {
      return u.passwordTemporalVence && leerFecha(u.passwordTemporalVence) < new Date() ? 'vencida' : 'pendiente';
    }
    return 'activo';
  }

  private coincideFiltro(estado: EstadoAcceso, filtro: Filtro): boolean {
    switch (filtro) {
      case 'con-acceso': return estado === 'activo';
      case 'pendientes': return estado === 'pendiente' || estado === 'vencida' || estado === 'sin-contrasena';
      case 'sin-acceso': return estado === 'sin-acceso' || estado === 'desactivado';
      default: return true;
    }
  }

  private ponerEmpleado(emp: Empleado): void {
    this.actual.set(emp);
    this.empleados.update(lista => lista.some(x => x.empleadoID === emp.empleadoID)
      ? lista.map(x => x.empleadoID === emp.empleadoID ? emp : x)
      : [...lista, emp].sort((a, b) => a.nombreCompleto.localeCompare(b.nombreCompleto)));
    // Si es médico nuevo, que aparezca para asignarlo a enfermeras
    if (emp.medicoID && !this.medicos().some(m => m.medicoID === emp.medicoID)) {
      this.directory.medicos().subscribe(m => this.medicos.set(m.filter(x => x.activo)));
    }
  }

  private rolSugerido(cargo: string): number | null {
    return this.roles().find(r => r.nombre === NIVEL_SUGERIDO[cargo])?.rolId ?? null;
  }

  private rolNombre(rolId: number | null): string | undefined {
    return this.roles().find(r => r.rolId === rolId)?.nombre;
  }

  // "Héctor López" -> "hlopez" (y "hlopez2" si ya existe)
  private sugerirUsuario(): string {
    const limpio = (t: string) => this.normalizar(t).replace(/[^a-z0-9]/g, '');
    const nombre = limpio(this.form.nombres.split(' ')[0] ?? '');
    const apellido = limpio(this.form.apellidos.split(' ')[0] ?? '');
    let base = apellido ? `${nombre.charAt(0)}${apellido}` : nombre;
    if (base.length < 4) base = (base + 'user').slice(0, 4);
    const usados = new Set(this.usuarios().map(u => u.nombreUsuario));
    let sugerido = base, n = 2;
    while (usados.has(sugerido)) sugerido = `${base}${n++}`;
    return sugerido;
  }

  private normalizar(t: string): string {
    return t.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim();
  }

  private vacio(t: string | null | undefined): string | null {
    return t && t.trim() ? t.trim() : null;
  }

  private formVacio(): FormEmpleado {
    return {
      nombres: '', apellidos: '', identificacion: '', fechaNacimiento: null, genero: null, telefono: '', email: '', direccion: '',
      cargo: 'Recepción', fechaIngreso: new Date(), notas: '', emergenciaNombre: '', emergenciaParentesco: '', emergenciaTelefono: '',
      activo: true, numeroColegiatura: '', especialidadIds: [], especialidadPrincipalId: null, medicosAsignados: []
    };
  }

  private aviso(titulo: string, detalle: string): void {
    this.toast.add({ severity: 'warn', summary: titulo, detail: detalle });
  }

  private error(err: unknown): void {
    this.toast.add({ severity: 'error', summary: 'No se pudo completar', detail: mensajeError(err) });
  }
}
