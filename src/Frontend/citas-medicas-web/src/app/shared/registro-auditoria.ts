import { Component, computed, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import {
  NOMBRE_SERVICIO, RegistroAuditoria, descripcionAmigable, estiloAccion, leerCambios, nombreCampo, valorCampo
} from '../core/auditoria.service';
import { nombreRol } from '../core/utils';

// Un movimiento de la auditoría: quién, qué, cuándo, desde dónde y (si aplica) el antes/después
@Component({
  selector: 'app-registro-auditoria',
  imports: [DatePipe],
  template: `
    @let r = registro();
    <div class="registro" [class.fallido]="!r.exito" [class.abierto]="abierto()">
      <div class="icono" [style.color]="estilo().color" [style.background]="estilo().fondo"><i [class]="estilo().icono"></i></div>

      <div class="cuerpo">
        <div class="linea">
          <strong class="quien">{{ r.usuarioNombre || r.usuarioLogin || 'Alguien sin sesión' }}</strong>
          @if (r.rol) { <span class="rol">{{ nombreRol(r.rol) }}</span> }
          <span class="hora">{{ fecha() | date: (conFecha() ? "d MMM, h:mm:ss a" : 'h:mm:ss a') }}</span>
        </div>
        <p class="que">{{ texto() }}</p>
        <div class="chips">
          <span class="chip accion" [style.color]="estilo().color" [style.background]="estilo().fondo">{{ estilo().texto }}</span>
          <span class="chip"><i class="pi pi-box"></i> {{ modulo() }}</span>
          @if (r.dispositivo) { <span class="chip"><i class="pi pi-desktop"></i> {{ r.dispositivo }}</span> }
          @if (r.ip) { <span class="chip"><i class="pi pi-globe"></i> {{ r.ip }}</span> }
          @if (cambios().length) {
            <button type="button" class="ver" (click)="abierto.set(!abierto())">
              <i [class]="abierto() ? 'pi pi-chevron-up' : 'pi pi-chevron-down'"></i>
              {{ abierto() ? 'Ocultar' : 'Ver' }} {{ cambios().length }} {{ cambios().length === 1 ? 'campo' : 'campos' }}
            </button>
          }
        </div>

        @if (abierto()) {
          <table class="cambios">
            <thead><tr><th>Campo</th><th>Antes</th><th>Después</th></tr></thead>
            <tbody>
              @for (c of cambios(); track c.campo) {
                <tr>
                  <td>{{ nombreCampo(c.campo) }}</td>
                  <td><span class="antes" [class.vacio]="c.antes === null">{{ valor(c.antes) }}</span></td>
                  <td><span class="despues" [class.vacio]="c.despues === null">{{ valor(c.despues) }}</span></td>
                </tr>
              }
            </tbody>
          </table>
          @if (r.ruta) { <small class="ruta">{{ r.ruta }}</small> }
        }
      </div>
    </div>
  `,
  styles: `
    .registro { display: flex; gap: .85rem; padding: .85rem 1rem; border-radius: 14px; border: 1px solid var(--mc-borde); background: #fff;
      transition: box-shadow .15s, border-color .15s; animation: entrar .3s ease both;
      &:hover { box-shadow: 0 8px 20px rgba(16, 24, 40, .07); }
      &.fallido { border-color: #fecaca; background: #fff7f7; }
      &.abierto { border-color: #99f6e4; } }
    @keyframes entrar { from { opacity: 0; transform: translateY(6px); } }
    .icono { width: 38px; height: 38px; flex-shrink: 0; border-radius: 12px; display: grid; place-items: center; font-size: .95rem; }
    .cuerpo { flex: 1; min-width: 0; }
    .linea { display: flex; align-items: center; gap: .5rem; flex-wrap: wrap; }
    .quien { font-size: .92rem; }
    .rol { font-size: .7rem; font-weight: 600; color: var(--mc-suave); background: #f1f5f9; border-radius: 6px; padding: .1rem .4rem; }
    .hora { margin-left: auto; font-size: .78rem; color: var(--mc-suave); white-space: nowrap; }
    .que { margin: .2rem 0 .45rem; font-size: .88rem; color: #334155; word-break: break-word; }
    .chips { display: flex; flex-wrap: wrap; gap: .35rem; align-items: center; }
    .chip { display: inline-flex; align-items: center; gap: .3rem; font-size: .72rem; color: #64748b; background: #f8fafc; border: 1px solid var(--mc-borde);
      border-radius: 999px; padding: .12rem .55rem; i { font-size: .7rem; } }
    .chip.accion { border: none; font-weight: 700; }
    .ver { border: none; background: none; color: var(--mc-primario); font: inherit; font-size: .78rem; font-weight: 600; cursor: pointer; display: inline-flex; gap: .3rem; align-items: center; }
    .cambios { width: 100%; margin-top: .7rem; border-collapse: collapse; font-size: .82rem; animation: entrar .2s ease both;
      th { text-align: left; font-weight: 600; color: var(--mc-suave); font-size: .72rem; text-transform: uppercase; letter-spacing: .04em; padding: .35rem .5rem; border-bottom: 1px solid var(--mc-borde); }
      td { padding: .4rem .5rem; border-bottom: 1px dashed #eef2f4; vertical-align: top; word-break: break-word; }
      td:first-child { font-weight: 600; color: #334155; width: 30%; } }
    .antes { color: #b91c1c; background: #fef2f2; border-radius: 6px; padding: .05rem .35rem; text-decoration: line-through; }
    .despues { color: #15803d; background: #f0fdf4; border-radius: 6px; padding: .05rem .35rem; font-weight: 600; }
    .antes.vacio, .despues.vacio { text-decoration: none; background: none; color: #cbd5e1; font-weight: 400; }
    .ruta { display: block; margin-top: .45rem; font-family: Consolas, monospace; font-size: .72rem; color: #94a3b8; }
  `
})
export class RegistroAuditoriaItem {
  readonly registro = input.required<RegistroAuditoria>();
  readonly conFecha = input(false);

  protected abierto = signal(false);
  protected nombreRol = nombreRol;
  protected nombreCampo = nombreCampo;
  protected valor = valorCampo;

  protected estilo = computed(() => estiloAccion(this.registro()));
  protected texto = computed(() => descripcionAmigable(this.registro()));
  protected cambios = computed(() => leerCambios(this.registro()));
  // "2026-11-20T16:38:10.123" (hora local del servidor, con segundos)
  protected fecha = computed(() => new Date(this.registro().fecha));
  protected modulo = computed(() => NOMBRE_SERVICIO[this.registro().servicio] ?? this.registro().servicio);
}
