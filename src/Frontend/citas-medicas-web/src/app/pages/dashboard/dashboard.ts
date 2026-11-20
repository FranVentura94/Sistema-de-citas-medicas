import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { Button } from 'primeng/button';
import { Skeleton } from 'primeng/skeleton';
import { AuthService } from '../../core/auth.service';
import { AgendaService, DirectoryService } from '../../core/api.services';
import { Cita, ResumenCitas } from '../../core/models';
import { fechaParam, leerFecha, saludo } from '../../core/utils';

@Component({
  selector: 'app-dashboard',
  imports: [DatePipe, RouterLink, Button, Skeleton],
  template: `
    <!-- ============ Bienvenida ============ -->
    <div class="hero">
      <div>
        <h1>{{ saludo }}, {{ nombre() }}</h1>
        <p>Este es el resumen de la actividad de hoy en la clínica.</p>
      </div>
      <div class="acciones">
        @if (esMedico()) {
          <p-button label="Ver mis citas" icon="pi pi-calendar-clock" routerLink="/mis-citas" />
        } @else {
          <p-button label="Agendar cita" icon="pi pi-calendar-plus" routerLink="/agendar" />
          <p-button label="Pacientes" icon="pi pi-users" severity="secondary" [outlined]="true" routerLink="/pacientes" styleClass="boton-claro" />
        }
      </div>
    </div>

    <!-- ============ Indicadores ============ -->
    <div class="kpis">
      @for (k of kpis(); track k.titulo) {
        <div class="tarjeta kpi">
          <div class="icono" [style.background]="k.fondo" [style.color]="k.color"><i [class]="k.icono"></i></div>
          <div>
            <span>{{ k.titulo }}</span>
            @if (cargando()) { <p-skeleton width="3rem" height="1.8rem" /> } @else { <strong>{{ k.valor }}</strong> }
          </div>
        </div>
      }
    </div>

    <div class="columnas">
      <!-- ============ Próximas citas ============ -->
      <div class="tarjeta">
        <div class="tarjeta-titulo"><i class="pi pi-clock"></i> Citas de hoy</div>

        @if (cargando()) {
          @for (i of [1, 2, 3]; track i) { <p-skeleton height="3.5rem" class="mb-2" /> }
        } @else if (citas().length === 0) {
          <div class="vacio"><i class="pi pi-calendar-times"></i>No hay citas para hoy.</div>
        } @else {
          <ul class="lista-citas">
            @for (c of citas(); track c.citaId) {
              <li>
                <div class="hora">
                  <strong>{{ leer(c.fechaHoraInicio) | date: 'HH:mm' }}</strong>
                  <span>{{ leer(c.fechaHoraFin) | date: 'HH:mm' }}</span>
                </div>
                <div class="detalle">
                  <strong>{{ c.pacienteNombre }}</strong>
                  <span>{{ c.medicoNombre }} · {{ c.especialidadNombre }} · {{ c.consultorioNombre }}</span>
                </div>
                <span class="estado" [style.color]="c.estadoColor" [style.background]="c.estadoColor + '1a'">{{ c.estadoNombre }}</span>
              </li>
            }
          </ul>
        }
      </div>

      <!-- ============ Distribución por estado ============ -->
      <div class="tarjeta">
        <div class="tarjeta-titulo"><i class="pi pi-chart-bar"></i> Citas por estado</div>
        @if (resumen(); as r) {
          <div class="barras">
            @for (e of r.porEstado; track e.codigo) {
              <div class="barra">
                <div class="etiqueta"><span>{{ e.nombre }}</span><strong>{{ e.cantidad }}</strong></div>
                <div class="pista"><div class="relleno" [style.width.%]="r.total ? (e.cantidad * 100) / r.total : 0" [style.background]="e.color"></div></div>
              </div>
            }
          </div>
        } @else {
          @for (i of [1, 2, 3, 4]; track i) { <p-skeleton height="1.5rem" class="mb-3" /> }
        }
      </div>
    </div>
  `,
  styles: `
    .hero {
      display: flex; justify-content: space-between; align-items: center; gap: 1rem; flex-wrap: wrap;
      padding: 1.75rem 2rem; border-radius: 18px; margin-bottom: 1.5rem; color: #fff;
      background: linear-gradient(120deg, #0f766e 0%, #0d9488 55%, #14b8a6 100%);
      box-shadow: 0 10px 30px rgba(13, 148, 136, .25);
      h1 { font-size: 1.7rem; }
      p { margin: .35rem 0 0; color: #ccfbf1; }
    }
    .acciones { display: flex; gap: .75rem; flex-wrap: wrap; }
    :host ::ng-deep .hero .p-button:not(.boton-claro) { background: #fff; color: #0f766e; border-color: #fff; }
    :host ::ng-deep .hero .boton-claro { color: #fff !important; border-color: rgba(255,255,255,.6) !important; }

    .kpis { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
    .kpi { display: flex; align-items: center; gap: 1rem; }
    .kpi .icono { width: 48px; height: 48px; border-radius: 12px; display: grid; place-items: center; font-size: 1.3rem; }
    .kpi span { color: var(--mc-suave); font-size: .85rem; }
    .kpi strong { display: block; font-size: 1.7rem; }

    .columnas { display: grid; grid-template-columns: 1.6fr 1fr; gap: 1.25rem; align-items: start; }
    .lista-citas { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: .6rem; }
    .lista-citas li { display: flex; align-items: center; gap: 1rem; padding: .75rem 1rem; border-radius: 12px; background: #f8fafb; border: 1px solid var(--mc-borde); }
    .hora { min-width: 56px; text-align: center; strong { display: block; font-size: 1.05rem; color: var(--mc-primario-oscuro); } span { font-size: .75rem; color: var(--mc-suave); } }
    .detalle { flex: 1; min-width: 0; strong { display: block; } span { font-size: .8rem; color: var(--mc-suave); } }

    .barras { display: flex; flex-direction: column; gap: .9rem; }
    .etiqueta { display: flex; justify-content: space-between; font-size: .85rem; margin-bottom: .3rem; }
    .pista { height: 8px; border-radius: 99px; background: #eef2f4; overflow: hidden; }
    .relleno { height: 100%; border-radius: 99px; transition: width .6s ease; }
    :host ::ng-deep .mb-2 { margin-bottom: .5rem; }
    :host ::ng-deep .mb-3 { margin-bottom: .9rem; }

    @media (max-width: 1100px) { .columnas { grid-template-columns: 1fr; } }
  `
})
export class Dashboard {
  private auth = inject(AuthService);
  private agenda = inject(AgendaService);
  private directory = inject(DirectoryService);

  protected saludo = saludo();
  protected leer = leerFecha;
  protected cargando = signal(true);
  protected resumen = signal<ResumenCitas | null>(null);
  protected citas = signal<Cita[]>([]);
  protected totalPacientes = signal(0);
  protected totalMedicos = signal(0);

  protected esMedico = computed(() => this.auth.tieneRol('Medico'));
  protected nombre = computed(() => (this.auth.usuario()?.nombreCompleto ?? '').split(' ').slice(0, 2).join(' '));

  protected kpis = computed(() => {
    const cuenta = (...codigos: string[]) =>
      this.resumen()?.porEstado.filter(e => codigos.includes(e.codigo)).reduce((t, e) => t + e.cantidad, 0) ?? 0;

    const base = [
      { titulo: 'Citas de hoy', valor: this.resumen()?.total ?? 0, icono: 'pi pi-calendar', color: '#0d9488', fondo: '#ccfbf1' },
      { titulo: 'Pendientes', valor: cuenta('PROGRAMADA', 'CONFIRMADA', 'EN_ESPERA'), icono: 'pi pi-hourglass', color: '#d97706', fondo: '#fef3c7' },
      { titulo: 'Atendidas', valor: cuenta('ATENDIDA'), icono: 'pi pi-check-circle', color: '#059669', fondo: '#d1fae5' }
    ];

    return this.esMedico()
      ? [...base, { titulo: 'En atención', valor: cuenta('EN_ATENCION'), icono: 'pi pi-heart', color: '#7c3aed', fondo: '#ede9fe' }]
      : [
        ...base,
        { titulo: 'Pacientes', valor: this.totalPacientes(), icono: 'pi pi-users', color: '#2563eb', fondo: '#dbeafe' },
        { titulo: 'Médicos', valor: this.totalMedicos(), icono: 'pi pi-id-card', color: '#db2777', fondo: '#fce7f3' }
      ];
  });

  constructor() {
    const hoy = new Date();
    const fecha = fechaParam(hoy);
    const manana = fechaParam(new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate() + 1));
    const medicoId = this.auth.usuario()?.medicoId ?? null;
    const esMedico = this.esMedico();

    forkJoin({
      resumen: this.agenda.resumen(fecha, esMedico ? medicoId : null),
      citas: esMedico ? this.agenda.misCitas(fecha) : this.agenda.citas({ desde: fecha, hasta: manana }),
      pacientes: esMedico ? of([]) : this.directory.pacientes(),
      medicos: esMedico ? of([]) : this.directory.medicos()
    }).subscribe({
      next: r => {
        this.resumen.set(r.resumen);
        this.citas.set(r.citas);
        this.totalPacientes.set(r.pacientes.length);
        this.totalMedicos.set(r.medicos.length);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }
}
