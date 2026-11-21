import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { Select } from 'primeng/select';
import { InputText } from 'primeng/inputtext';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Skeleton } from 'primeng/skeleton';
import {
  AuditoriaService, NOMBRE_SERVICIO, RegistroAuditoria, descripcionAmigable, estiloAccion, leerCambios, nombreCampo, valorCampo
} from '../../core/auditoria.service';
import { fechaParam, mayuscula, nombreRol } from '../../core/utils';
import { RegistroAuditoriaItem } from '../../shared/registro-auditoria';

type Alerta = 'todo' | 'datos' | 'consultas' | 'fallidos' | 'denegados';

const LOTE = 150;

@Component({
  selector: 'app-auditoria',
  imports: [FormsModule, Button, DatePicker, Select, InputText, IconField, InputIcon, Skeleton, RegistroAuditoriaItem],
  template: `
    <div class="pagina-encabezado">
      <div>
        <h1><i class="pi pi-shield titulo-icono"></i> Auditoría</h1>
        <p>Todo lo que pasa en el sistema: quién lo hizo, cuándo, sobre qué y desde dónde. Nadie puede modificarla ni borrarla.</p>
      </div>
      <div class="acciones">
        <p-button label="Actualizar" icon="pi pi-refresh" severity="secondary" [outlined]="true" (onClick)="cargar()" [loading]="cargando()" />
        <p-button label="Exportar a Excel" icon="pi pi-file-excel" (onClick)="exportar()" [disabled]="!filtrados().length" />
      </div>
    </div>

    <!-- ============ Alertas y resumen (también filtran) ============ -->
    <div class="alertas">
      @for (a of alertas(); track a.clave) {
        <button type="button" class="tarjeta alerta" [class.elegida]="alerta() === a.clave" [class.peligro]="a.peligro && a.valor > 0"
                (click)="alerta.set(alerta() === a.clave ? 'todo' : a.clave)">
          <div class="icono" [style.background]="a.fondo" [style.color]="a.color"><i [class]="a.icono"></i></div>
          <div>
            <span>{{ a.titulo }}</span>
            <strong>{{ a.valor }}</strong>
          </div>
          @if (a.peligro && a.valor > 0) { <em class="pulso"></em> }
        </button>
      }
    </div>

    <!-- ============ Filtros ============ -->
    <div class="tarjeta filtros">
      <div class="campo rango">
        <label>Periodo</label>
        <p-datepicker [(ngModel)]="rango" selectionMode="range" dateFormat="dd/mm/yy" [showIcon]="true" [readonlyInput]="true"
                      [maxDate]="hoy" appendTo="body" (onClose)="cargar()" placeholder="Elige fechas" />
        <div class="rapidos">
          @for (r of rapidos; track r.dias) {
            <button type="button" [class.activo]="diasRapido() === r.dias" (click)="ultimosDias(r.dias)">{{ r.texto }}</button>
          }
        </div>
      </div>
      <div class="campo">
        <label>Persona</label>
        <p-select [options]="personas()" [ngModel]="persona()" (ngModelChange)="persona.set($event)" optionLabel="texto" optionValue="valor"
                  [showClear]="true" placeholder="Todas" [filter]="true" appendTo="body" />
      </div>
      <div class="campo">
        <label>Módulo</label>
        <p-select [options]="modulos" [ngModel]="modulo()" (ngModelChange)="modulo.set($event)" optionLabel="texto" optionValue="valor"
                  [showClear]="true" placeholder="Todos" appendTo="body" />
      </div>
      <div class="campo">
        <label>Tipo</label>
        <p-select [options]="tipos" [ngModel]="tipo()" (ngModelChange)="tipo.set($event)" optionLabel="texto" optionValue="valor"
                  [showClear]="true" placeholder="Todos" appendTo="body" />
      </div>
      <div class="campo buscar">
        <label>Buscar</label>
        <p-iconfield>
          <p-inputicon class="pi pi-search" />
          <input pInputText [ngModel]="texto()" (ngModelChange)="texto.set($event)" placeholder="Paciente, usuario, IP, campo..." />
        </p-iconfield>
      </div>
    </div>

    @if (sinRespuesta().length) {
      <div class="aviso"><i class="pi pi-exclamation-triangle"></i> No respondieron: <b>{{ sinRespuesta().join(', ') }}</b>. Enciéndelos y pulsa Actualizar para ver todo.</div>
    }

    <!-- ============ Línea de tiempo ============ -->
    @if (cargando()) {
      @for (i of [1, 2, 3, 4, 5]; track i) { <p-skeleton height="84px" borderRadius="14px" styleClass="mb" /> }
    } @else if (filtrados().length === 0) {
      <div class="tarjeta vacio"><i class="pi pi-inbox"></i>No hay movimientos con estos filtros.</div>
    } @else {
      <p class="conteo">{{ filtrados().length }} movimientos{{ filtrados().length > visibles().length ? ' · mostrando ' + visibles().length : '' }}</p>
      @for (g of grupos(); track g.dia) {
        <section class="dia">
          <h3><span>{{ g.titulo }}</span><em>{{ g.registros.length }}</em></h3>
          <div class="lista">
            @for (r of g.registros; track r.servicio + r.id) {
              <app-registro-auditoria [registro]="r" />
            }
          </div>
        </section>
      }
      @if (filtrados().length > visibles().length) {
        <div class="mas"><p-button label="Ver más movimientos" icon="pi pi-angle-down" severity="secondary" [outlined]="true" (onClick)="limite.set(limite() + lote)" /></div>
      }
    }
  `,
  styles: `
    .titulo-icono { color: var(--mc-primario); margin-right: .25rem; }
    .acciones { display: flex; gap: .6rem; flex-wrap: wrap; }

    .alertas { display: grid; grid-template-columns: repeat(auto-fit, minmax(190px, 1fr)); gap: 1rem; margin-bottom: 1.25rem; }
    .alerta { position: relative; display: flex; align-items: center; gap: .9rem; cursor: pointer; text-align: left; font: inherit; color: inherit;
      transition: transform .15s, box-shadow .15s, border-color .15s;
      &:hover { transform: translateY(-2px); box-shadow: 0 10px 24px rgba(16, 24, 40, .08); }
      &.elegida { border-color: var(--mc-primario); box-shadow: 0 0 0 3px rgba(13, 148, 136, .15); }
      &.peligro { border-color: #fecaca; background: linear-gradient(135deg, #fff, #fff1f2); }
      .icono { width: 44px; height: 44px; border-radius: 12px; display: grid; place-items: center; font-size: 1.15rem; }
      span { color: var(--mc-suave); font-size: .82rem; } strong { display: block; font-size: 1.55rem; } }
    .pulso { position: absolute; top: 12px; right: 12px; width: 10px; height: 10px; border-radius: 50%; background: #ef4444; animation: latir 1.6s infinite; }
    @keyframes latir { 0% { box-shadow: 0 0 0 0 rgba(239, 68, 68, .6); } 70% { box-shadow: 0 0 0 10px rgba(239, 68, 68, 0); } 100% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0); } }

    .filtros { display: grid; grid-template-columns: 1.5fr 1fr 1fr 1fr 1.3fr; gap: 1rem; margin-bottom: 1rem; align-items: start; }
    :host ::ng-deep .filtros p-datepicker, :host ::ng-deep .filtros .p-datepicker, :host ::ng-deep .filtros p-select,
    :host ::ng-deep .filtros p-iconfield, :host ::ng-deep .filtros input { width: 100%; }
    .rapidos { display: flex; gap: .35rem; flex-wrap: wrap; margin-top: .4rem; }
    .rapidos button { border: 1px solid var(--mc-borde); background: #fff; border-radius: 999px; padding: .2rem .6rem; font: inherit; font-size: .75rem; cursor: pointer; color: var(--mc-suave);
      &:hover { border-color: var(--mc-primario); color: var(--mc-primario-oscuro); }
      &.activo { background: #f0fdfa; border-color: var(--mc-primario); color: var(--mc-primario-oscuro); font-weight: 600; } }

    .aviso { margin-bottom: 1rem; padding: .7rem 1rem; border-radius: 12px; background: #fffbeb; color: #92400e; border: 1px solid #fde68a; }
    .conteo { color: var(--mc-suave); font-size: .85rem; margin: 0 0 .6rem; }
    .dia { margin-bottom: 1.25rem; }
    .dia h3 { display: flex; align-items: center; gap: .6rem; font-size: .9rem; color: var(--mc-primario-oscuro); margin-bottom: .6rem; position: sticky; top: 64px; z-index: 2;
      background: var(--mc-fondo); padding: .35rem 0;
      em { font-style: normal; font-size: .72rem; background: #e2e8f0; color: #475569; border-radius: 999px; padding: .05rem .5rem; } }
    .lista { display: flex; flex-direction: column; gap: .55rem; }
    .mas { display: flex; justify-content: center; margin: 1rem 0 2rem; }
    :host ::ng-deep .mb { margin-bottom: .6rem; }

    @media (max-width: 1200px) { .filtros { grid-template-columns: repeat(2, minmax(0, 1fr)); } .rango, .buscar { grid-column: 1 / -1; } }
    @media (max-width: 640px) { .filtros { grid-template-columns: 1fr; } .alertas { grid-template-columns: repeat(2, minmax(0, 1fr)); gap: .7rem; } }
  `
})
export class Auditoria {
  private auditoria = inject(AuditoriaService);
  private toast = inject(MessageService);

  protected hoy = new Date();
  protected lote = LOTE;
  protected rapidos = [{ dias: 1, texto: 'Hoy' }, { dias: 7, texto: '7 días' }, { dias: 30, texto: '30 días' }, { dias: 90, texto: '3 meses' }];
  protected modulos = Object.entries(NOMBRE_SERVICIO).map(([valor, texto]) => ({ valor, texto }));
  protected tipos = [
    { valor: 'Datos', texto: 'Cambios de datos' },
    { valor: 'Consulta', texto: 'Consultas de información sensible' },
    { valor: 'Seguridad', texto: 'Seguridad y accesos' }
  ];

  protected rango: Date[] | null = null;
  protected diasRapido = signal<number | null>(7);
  protected cargando = signal(true);
  protected registros = signal<RegistroAuditoria[]>([]);
  protected sinRespuesta = signal<string[]>([]);

  protected alerta = signal<Alerta>('todo');
  protected persona = signal<string | null>(null);
  protected modulo = signal<string | null>(null);
  protected tipo = signal<string | null>(null);
  protected texto = signal('');
  protected limite = signal(LOTE);

  // Personas que aparecen en el periodo (incluye usuarios inexistentes de intentos fallidos)
  protected personas = computed(() => {
    const mapa = new Map<string, string>();
    for (const r of this.registros()) {
      const clave = r.usuarioLogin ?? '(sin sesión)';
      if (!mapa.has(clave)) mapa.set(clave, r.usuarioNombre ? `${r.usuarioNombre} (@${clave})` : clave === '(sin sesión)' ? 'Sin sesión' : `@${clave}`);
    }
    return [...mapa.entries()].map(([valor, texto]) => ({ valor, texto })).sort((a, b) => a.texto.localeCompare(b.texto));
  });

  protected alertas = computed(() => {
    const r = this.registros();
    const cuenta = (a: Alerta) => r.filter(x => this.coincideAlerta(x, a)).length;
    return [
      { clave: 'todo' as Alerta, titulo: 'Movimientos', valor: r.length, icono: 'pi pi-list', color: '#0d9488', fondo: '#ccfbf1', peligro: false },
      { clave: 'datos' as Alerta, titulo: 'Cambios de datos', valor: cuenta('datos'), icono: 'pi pi-pencil', color: '#b45309', fondo: '#fef3c7', peligro: false },
      { clave: 'consultas' as Alerta, titulo: 'Consultas sensibles', valor: cuenta('consultas'), icono: 'pi pi-eye', color: '#1d4ed8', fondo: '#dbeafe', peligro: false },
      { clave: 'fallidos' as Alerta, titulo: 'Inicios de sesión fallidos', valor: cuenta('fallidos'), icono: 'pi pi-times-circle', color: '#b91c1c', fondo: '#fee2e2', peligro: true },
      { clave: 'denegados' as Alerta, titulo: 'Intentos sin permiso', valor: cuenta('denegados'), icono: 'pi pi-ban', color: '#b91c1c', fondo: '#fee2e2', peligro: true }
    ];
  });

  protected filtrados = computed(() => {
    const q = this.normalizar(this.texto());
    return this.registros().filter(r => {
      if (!this.coincideAlerta(r, this.alerta())) return false;
      if (this.persona() && (r.usuarioLogin ?? '(sin sesión)') !== this.persona()) return false;
      if (this.modulo() && r.servicio !== this.modulo()) return false;
      if (this.tipo() && r.categoria !== this.tipo()) return false;
      if (!q) return true;
      const en = [r.descripcion, descripcionAmigable(r), r.usuarioNombre, r.usuarioLogin, r.ip, r.dispositivo, r.cambios, r.ruta]
        .filter(Boolean).map(v => this.normalizar(v!)).join(' ');
      return en.includes(q);
    });
  });

  protected visibles = computed(() => this.filtrados().slice(0, this.limite()));

  // Agrupados por día: "Hoy", "Ayer", "Lunes, 17 de noviembre"
  protected grupos = computed(() => {
    const grupos: { dia: string; titulo: string; registros: RegistroAuditoria[] }[] = [];
    for (const r of this.visibles()) {
      const dia = r.fecha.slice(0, 10);
      let g = grupos.at(-1);
      if (!g || g.dia !== dia) grupos.push(g = { dia, titulo: this.tituloDia(dia), registros: [] });
      g.registros.push(r);
    }
    return grupos;
  });

  constructor() {
    this.ultimosDias(7);
  }

  ultimosDias(dias: number): void {
    const hasta = new Date();
    const desde = new Date(hasta.getFullYear(), hasta.getMonth(), hasta.getDate() - (dias - 1));
    this.rango = [desde, hasta];
    this.diasRapido.set(dias);
    this.cargar(true);
  }

  cargar(desdeRapido = false): void {
    if (!desdeRapido) this.diasRapido.set(null);
    const [desde, hasta] = this.rango ?? [];
    if (!desde) return;
    const fin = hasta ?? desde;
    const diaSiguiente = new Date(fin.getFullYear(), fin.getMonth(), fin.getDate() + 1);

    this.cargando.set(true);
    this.limite.set(LOTE);
    this.auditoria.buscar({ desde: fechaParam(desde), hasta: fechaParam(diaSiguiente), max: 1000 }).subscribe({
      next: r => {
        this.registros.set(r.registros);
        this.sinRespuesta.set(r.sinRespuesta.map(s => NOMBRE_SERVICIO[mayuscula(s)] ?? s));
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }

  // CSV con ";" y BOM para que Excel en español lo abra con acentos y columnas correctas
  exportar(): void {
    const celda = (v: unknown) => `"${String(v ?? '').replace(/"/g, '""')}"`;
    const encabezado = ['Fecha', 'Hora', 'Usuario', 'Nombre', 'Nivel', 'Módulo', 'Tipo', 'Acción', 'Descripción', 'Resultado', 'IP', 'Dispositivo', 'Cambios'];
    const filas = this.filtrados().map(r => {
      const [f, h] = r.fecha.split('T');
      const cambios = leerCambios(r).map(c => `${nombreCampo(c.campo)}: ${valorCampo(c.antes)} → ${valorCampo(c.despues)}`).join(' | ');
      return [f.split('-').reverse().join('/'), h?.slice(0, 8), r.usuarioLogin, r.usuarioNombre, nombreRol(r.rol), NOMBRE_SERVICIO[r.servicio] ?? r.servicio,
      r.categoria, estiloAccion(r).texto, descripcionAmigable(r), r.exito ? 'Correcto' : 'Fallido', r.ip, r.dispositivo, cambios].map(celda).join(';');
    });
    const csv = '\uFEFF' + [encabezado.map(celda).join(';'), ...filas].join('\r\n');

    const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
    const a = document.createElement('a');
    a.href = url;
    a.download = `auditoria_${fechaParam(new Date())}.csv`;
    a.click();
    URL.revokeObjectURL(url);
    this.toast.add({ severity: 'success', summary: 'Auditoría exportada', detail: `${filas.length} movimientos` });
  }

  private coincideAlerta(r: RegistroAuditoria, a: Alerta): boolean {
    switch (a) {
      case 'datos': return r.categoria === 'Datos';
      case 'consultas': return r.categoria === 'Consulta';
      case 'fallidos': return r.accion === 'InicioSesionFallido' || (r.accion === 'PrimerIngreso' && !r.exito);
      case 'denegados': return r.accion === 'AccesoDenegado' || r.accion === 'SesionInvalida';
      default: return true;
    }
  }

  private tituloDia(dia: string): string {
    const hoy = fechaParam(new Date());
    const ayer = fechaParam(new Date(Date.now() - 86_400_000));
    if (dia === hoy) return 'Hoy';
    if (dia === ayer) return 'Ayer';
    const [y, m, d] = dia.split('-').map(Number);
    return mayuscula(new Date(y, m - 1, d).toLocaleDateString('es', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }));
  }

  private normalizar(t: string): string {
    return t.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
  }
}
