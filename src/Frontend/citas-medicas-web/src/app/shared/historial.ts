import { Component, inject, signal } from '@angular/core';
import { Dialog } from 'primeng/dialog';
import { Button } from 'primeng/button';
import { Skeleton } from 'primeng/skeleton';
import { AuditoriaService, ConsultaHistorial, RegistroAuditoria } from '../core/auditoria.service';
import { RegistroAuditoriaItem } from './registro-auditoria';

// Ventana "Historial de cambios" de un registro (paciente, empleado...).
// Uso: <app-historial #historial />  y luego  historial.abrir('María Hernández', [{ servicio: 'directory', entidad: 'Paciente', entidadId: 3 }])
@Component({
  selector: 'app-historial',
  imports: [Dialog, Button, Skeleton, RegistroAuditoriaItem],
  template: `
    <p-dialog [visible]="visible()" (visibleChange)="visible.set($event)" [modal]="true" [draggable]="false"
              [style]="{ width: '760px' }" [breakpoints]="{ '800px': '96vw' }">
      <ng-template #header>
        <div class="titulo">
          <div class="icono"><i class="pi pi-history"></i></div>
          <div>
            <h2>Historial de {{ titulo() }}</h2>
            <span>Todo lo que se ha hecho con este registro, quién y cuándo</span>
          </div>
        </div>
      </ng-template>

      @if (cargando()) {
        @for (i of [1, 2, 3]; track i) { <p-skeleton height="80px" borderRadius="14px" styleClass="mb" /> }
      } @else if (registros().length === 0) {
        <div class="vacio"><i class="pi pi-inbox"></i>Todavía no hay movimientos registrados.</div>
      } @else {
        <div class="lista">
          @for (r of registros(); track r.servicio + r.id) {
            <app-registro-auditoria [registro]="r" [conFecha]="true" />
          }
        </div>
      }
      @if (sinRespuesta().length) {
        <small class="aviso"><i class="pi pi-exclamation-triangle"></i> No respondió: {{ sinRespuesta().join(', ') }}. Puede faltar información.</small>
      }

      <ng-template #footer>
        <p-button label="Cerrar" severity="secondary" [text]="true" (onClick)="visible.set(false)" />
      </ng-template>
    </p-dialog>
  `,
  styles: `
    .titulo { display: flex; align-items: center; gap: .85rem; h2 { font-size: 1.15rem; } span { font-size: .8rem; color: var(--mc-suave); } }
    .titulo .icono { width: 44px; height: 44px; border-radius: 14px; display: grid; place-items: center; color: #fff; font-size: 1.1rem;
      background: linear-gradient(135deg, #6366f1, #0d9488); }
    .lista { display: flex; flex-direction: column; gap: .6rem; }
    :host ::ng-deep .mb { margin-bottom: .6rem; }
    .aviso { display: block; margin-top: .75rem; color: #b45309; }
  `
})
export class Historial {
  private auditoria = inject(AuditoriaService);

  protected visible = signal(false);
  protected titulo = signal('');
  protected cargando = signal(false);
  protected registros = signal<RegistroAuditoria[]>([]);
  protected sinRespuesta = signal<string[]>([]);

  abrir(titulo: string, consultas: ConsultaHistorial[]): void {
    this.titulo.set(titulo);
    this.registros.set([]);
    this.sinRespuesta.set([]);
    this.cargando.set(true);
    this.visible.set(true);

    this.auditoria.historial(consultas).subscribe({
      next: r => { this.registros.set(r.registros); this.sinRespuesta.set(r.sinRespuesta); this.cargando.set(false); },
      error: () => this.cargando.set(false)
    });
  }
}
