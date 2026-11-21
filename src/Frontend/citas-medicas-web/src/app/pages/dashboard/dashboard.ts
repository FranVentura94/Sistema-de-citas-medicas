import { Component, DestroyRef, computed, effect, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { Button } from 'primeng/button';
import { Skeleton } from 'primeng/skeleton';
import { Dialog } from 'primeng/dialog';
import { CitaDetalle } from '../../shared/cita-detalle';
import { AuthService } from '../../core/auth.service';
import { PerfilService } from '../../core/perfil.service';
import { AgendaService, DirectoryService } from '../../core/api.services';
import { Cita, HorarioMedico, Medico, ResumenCitas } from '../../core/models';
import { lanzarConfeti } from '../../core/confeti';
import { fechaParam, leerFecha, mayuscula, saludo } from '../../core/utils';

// Una frase distinta cada día para arrancar con buena energía
const FRASES = [
  'Una sonrisa en recepción es el primer tratamiento del día.',
  'Confirmar las citas un día antes reduce las inasistencias.',
  'Cuidar de otros empieza por cuidarte a ti: no olvides hidratarte.',
  'Un expediente completo hoy es un mejor diagnóstico mañana.',
  'La puntualidad también es una forma de cuidar al paciente.',
  'Escuchar con atención es parte de la consulta.',
  'Pequeñas pausas durante el día mantienen la mente enfocada.',
  'Cada paciente bien atendido es la mejor publicidad de la clínica.',
  'El trabajo en equipo hace que todo fluya mejor.',
  'Explicar con calma da tranquilidad al paciente.'
];

const PENDIENTES = ['PROGRAMADA', 'CONFIRMADA', 'EN_ESPERA'];

@Component({
  selector: 'app-dashboard',
  imports: [DatePipe, RouterLink, Button, Skeleton, Dialog, CitaDetalle],
  template: `
    <!-- ============ Celebraciones ============ -->
    @if (celebracion(); as c) {
      <div class="celebracion">
        <span class="emoji">{{ c.emoji }}</span>
        <div>
          <strong>{{ c.titulo }}</strong>
          <p>{{ c.texto }}</p>
        </div>
        <button type="button" (click)="cerrarCelebracion()" aria-label="Cerrar"><i class="pi pi-times"></i></button>
      </div>
    }

    <!-- ============ Saludo según la hora ============ -->
    <section class="hero {{ momento }}">
      <div class="decoracion" aria-hidden="true">
        @if (momento === 'noche') {
          <div class="luna"></div>
          @for (e of estrellas; track $index) {
            <span class="estrella" [style.left.%]="e.x" [style.top.%]="e.y" [style.animation-delay.s]="e.d"></span>
          }
        } @else {
          <div class="sol"></div>
          <div class="nube n1"></div>
          <div class="nube n2"></div>
        }
      </div>

      <div class="hero-contenido">
        <div class="texto">
          <span class="momento-etiqueta"><i [class]="iconoMomento"></i> {{ saludo }}</span>
          <h1>{{ saludo }}, {{ perfil.tratamiento() }} <span class="mano">👋</span></h1>
          <p class="resumen-dia">{{ resumenDelDia() }}</p>
          <div class="acciones">
            @for (a of acciones(); track a.ruta) {
              <p-button [label]="a.texto" [icon]="a.icono" [routerLink]="a.ruta" [styleClass]="$first ? 'boton-blanco' : 'boton-claro'" [outlined]="!$first" />
            }
          </div>
        </div>

        <div class="reloj">
          <strong>{{ ahora() | date: 'h:mm' }}<small>{{ ahora() | date: 'a' }}</small></strong>
          <span>{{ fechaLarga }}</span>
          <div class="frase"><i class="pi pi-sparkles"></i> {{ frase }}</div>
        </div>
      </div>
    </section>

    <!-- ============ Enfermería: médicos que asiste ============ -->
    @if (esEnfermeria()) {
      <div class="tarjeta mb-tarjeta">
        <div class="tarjeta-titulo"><i class="pi pi-users"></i> Médicos a los que asistes</div>
        @if (perfil.perfil()?.medicosAsignados?.length) {
          <div class="asignados">
            @for (m of perfil.perfil()!.medicosAsignados; track m.medicoID) {
              <div class="asignado">
                <div class="avatar-medico"><i class="pi pi-heart-fill"></i></div>
                <div><strong>{{ m.nombre }}</strong><span>Tu equipo de hoy</span></div>
              </div>
            }
          </div>
        } @else {
          <div class="vacio"><i class="pi pi-user-plus"></i>Aún no tienes médicos asignados. Pídele al administrador que te asigne.</div>
        }
      </div>
    }

      <!-- ============ Indicadores ============ -->
      <div class="kpis">
        @for (k of kpis(); track k.titulo; let i = $index) {
          <button type="button" class="tarjeta kpi" [style.animation-delay.ms]="i * 60" (click)="abrirKpi(k.clave)" [title]="'Ver ' + k.titulo.toLowerCase()">
            <div class="icono" [style.background]="k.fondo" [style.color]="k.color"><i [class]="k.icono"></i></div>
            <div>
              <span>{{ k.titulo }}</span>
              @if (cargando()) { <p-skeleton width="3rem" height="1.8rem" /> } @else { <strong>{{ animado(k.valor) }}</strong> }
            </div>
            <i class="pi pi-angle-right flecha"></i>
          </button>
        }
      </div>

      <!-- ============ Próxima cita (médico) ============ -->
      @if (esMedico() && proxima(); as p) {
        <div class="proxima">
          <div class="proxima-hora">
            <span>Tu próxima cita</span>
            <strong>{{ leer(p.fechaHoraInicio) | date: 'h:mm a' }}</strong>
            <em>{{ faltan(p) }}</em>
          </div>
          <div class="proxima-detalle">
            <strong>{{ p.pacienteNombre }}</strong>
            <span>{{ p.motivoConsulta || 'Consulta' }} · {{ p.consultorioNombre }}</span>
          </div>
          <p-button label="Ver mis citas" icon="pi pi-arrow-right" iconPos="right" routerLink="/mis-citas" [text]="true" />
        </div>
      }

      <div class="columnas">
        <!-- ============ Citas de hoy ============ -->
        <div class="tarjeta">
          <div class="tarjeta-titulo"><i class="pi pi-clock"></i> Citas de hoy</div>

          @if (cargando()) {
            @for (i of [1, 2, 3]; track i) { <p-skeleton height="3.5rem" class="mb-2" /> }
          } @else if (citas().length === 0) {
            <div class="vacio"><i class="pi pi-calendar-times"></i>No hay citas para hoy.</div>
          } @else {
            <ul class="lista-citas">
              @for (c of citas(); track c.citaId; let i = $index) {
                <li [style.animation-delay.ms]="i * 50" [class.siguiente]="c.citaId === proxima()?.citaId" (click)="detalle.abrir(c)" title="Ver detalle">
                  <div class="hora">
                    <strong>{{ leer(c.fechaHoraInicio) | date: 'HH:mm' }}</strong>
                    <span>{{ leer(c.fechaHoraFin) | date: 'HH:mm' }}</span>
                  </div>
                  <div class="detalle">
                    <strong>{{ c.pacienteNombre }}</strong>
                    <span>{{ c.medicoNombre }} · {{ c.especialidadNombre }} · {{ c.consultorioNombre }}</span>
                  </div>
                  <span class="estado" [style.color]="c.estadoColor" [style.background]="c.estadoColor + '1a'">{{ c.estadoNombre }}</span>
                  <i class="pi pi-angle-right flecha"></i>
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
                  <div class="pista"><div class="relleno" [style.width.%]="r.total ? (e.cantidad * 100 * progreso()) / r.total : 0" [style.background]="e.color"></div></div>
                </div>
              }
            </div>
          } @else {
            @for (i of [1, 2, 3, 4]; track i) { <p-skeleton height="1.5rem" class="mb-3" /> }
          }
        </div>
      </div>

    <!-- ============ Lista de citas al hacer clic en una tarjeta ============ -->
    <p-dialog [visible]="!!listaKpi()" (visibleChange)="!$event && listaKpi.set(null)" [modal]="true" [draggable]="false"
              [style]="{ width: '640px' }" [breakpoints]="{ '700px': '96vw' }" [header]="listaKpi()?.titulo ?? ''">
      @if (listaKpi(); as l) {
        @if (l.citas.length === 0) {
          <div class="vacio"><i class="pi pi-inbox"></i>No hay citas en esta lista.</div>
        } @else {
          <ul class="lista-citas">
            @for (c of l.citas; track c.citaId) {
              <li (click)="listaKpi.set(null); detalle.abrir(c)">
                <div class="hora"><strong>{{ leer(c.fechaHoraInicio) | date: 'HH:mm' }}</strong><span>{{ leer(c.fechaHoraFin) | date: 'HH:mm' }}</span></div>
                <div class="detalle"><strong>{{ c.pacienteNombre }}</strong><span>{{ c.medicoNombre }} · {{ c.consultorioNombre }}</span></div>
                <span class="estado" [style.color]="c.estadoColor" [style.background]="c.estadoColor + '1a'">{{ c.estadoNombre }}</span>
                <i class="pi pi-angle-right flecha"></i>
              </li>
            }
          </ul>
        }
      }
    </p-dialog>

    <!-- ============ Médicos y su horario ============ -->
    <p-dialog [visible]="dialogoMedicos()" (visibleChange)="dialogoMedicos.set($event)" [modal]="true" [draggable]="false"
              [style]="{ width: '820px' }" [breakpoints]="{ '860px': '96vw' }" header="Médicos y horarios de atención">
      <p class="texto-suave intro-medicos">Así sabes cuándo atiende cada médico antes de agendar. Los horarios los configura el Administrador.</p>
      <div class="medicos">
        @for (m of medicosActivos(); track m.medicoID) {
          <div class="medico" [class.disponible]="estadoHoy(m.medicoID).ahora">
            <div class="medico-cabeza">
              <div class="avatar-m">{{ m.nombres.charAt(0) }}{{ m.apellidos.charAt(0) }}</div>
              <div class="medico-nombre">
                <strong>{{ m.nombreCompleto }}</strong>
                <span>{{ especialidadesDe(m) }}</span>
              </div>
              <span class="hoy" [class.si]="estadoHoy(m.medicoID).ahora" [class.no]="!estadoHoy(m.medicoID).hoy">
                {{ estadoHoy(m.medicoID).texto }}
              </span>
            </div>
            <div class="semana">
              @for (d of dias; track d.valor) {
                <div class="dia" [class.vacio-dia]="bloques(m.medicoID, d.valor).length === 0" [class.es-hoy]="d.valor === hoyDia">
                  <b>{{ d.corto }}</b>
                  @for (h of bloques(m.medicoID, d.valor); track h.horarioMedicoId) {
                    <span>{{ h.horaInicio }}–{{ h.horaFin }}</span>
                  } @empty { <span class="libre">—</span> }
                </div>
              }
            </div>
          </div>
        } @empty {
          <div class="vacio"><i class="pi pi-id-card"></i>No hay médicos activos.</div>
        }
      </div>
      <ng-template #footer>
        @if (puedeAgendar()) { <p-button label="Agendar cita" icon="pi pi-calendar-plus" (onClick)="dialogoMedicos.set(false)" routerLink="/agendar" /> }
      </ng-template>
    </p-dialog>

    <app-cita-detalle #detalle (actualizada)="recargar()" />
  `,
  styles: `
    /* ---------- Celebración ---------- */
    .celebracion {
      display: flex; align-items: center; gap: 1rem; padding: 1rem 1.25rem; margin-bottom: 1.25rem; border-radius: 16px; color: #fff;
      background: linear-gradient(120deg, #db2777, #9333ea 55%, #6366f1); box-shadow: 0 10px 30px rgba(147, 51, 234, .3);
      animation: caer .5s cubic-bezier(.2, 1.4, .4, 1) both;
      .emoji { font-size: 2.2rem; animation: rebote 1.2s ease-in-out infinite; }
      div { flex: 1; } strong { font-size: 1.1rem; } p { margin: .15rem 0 0; color: #fce7f3; }
      button { border: none; background: rgba(255,255,255,.18); color: #fff; width: 32px; height: 32px; border-radius: 50%; cursor: pointer; }
    }
    @keyframes caer { from { opacity: 0; transform: translateY(-16px) scale(.97); } }
    @keyframes rebote { 0%, 100% { transform: translateY(0) rotate(-6deg); } 50% { transform: translateY(-6px) rotate(6deg); } }

    /* ---------- Hero ---------- */
    .hero {
      position: relative; overflow: hidden; border-radius: 22px; margin-bottom: 1.5rem; color: #fff; padding: 2rem 2.25rem;
      box-shadow: 0 14px 34px rgba(13, 148, 136, .25); animation: entrar .5s ease both;
    }
    .hero.manana { background: linear-gradient(120deg, #0d9488 0%, #14b8a6 50%, #f59e0b 135%); }
    .hero.tarde { background: linear-gradient(120deg, #0f766e 0%, #0891b2 60%, #38bdf8 120%); }
    .hero.noche { background: linear-gradient(120deg, #1e1b4b 0%, #312e81 55%, #0f766e 140%); box-shadow: 0 14px 34px rgba(49, 46, 129, .35); }
    @keyframes entrar { from { opacity: 0; transform: translateY(10px); } }

    .decoracion { position: absolute; inset: 0; pointer-events: none; }
    .sol { position: absolute; right: 8%; top: -60px; width: 190px; height: 190px; border-radius: 50%;
      background: radial-gradient(circle, #fde68a 0%, #fbbf24 45%, rgba(251, 191, 36, 0) 70%); animation: brillar 5s ease-in-out infinite; }
    .tarde .sol { top: auto; bottom: -90px; background: radial-gradient(circle, #fed7aa 0%, #fb923c 45%, rgba(251, 146, 60, 0) 70%); }
    @keyframes brillar { 50% { transform: scale(1.08); opacity: .85; } }
    .nube { position: absolute; height: 34px; border-radius: 40px; background: rgba(255,255,255,.22); animation: nube 26s linear infinite; }
    .nube::before { content: ''; position: absolute; width: 50%; height: 160%; left: 18%; bottom: 30%; border-radius: 50%; background: inherit; }
    .n1 { width: 120px; top: 28%; right: -140px; } .n2 { width: 80px; top: 62%; right: -140px; animation-delay: -13s; animation-duration: 34s; }
    @keyframes nube { to { transform: translateX(-120vw); } }
    .luna { position: absolute; right: 10%; top: 26px; width: 78px; height: 78px; border-radius: 50%; box-shadow: -18px 10px 0 0 #fef3c7; transform: rotate(-20deg); filter: drop-shadow(0 0 18px rgba(254, 243, 199, .55)); }
    .estrella { position: absolute; width: 3px; height: 3px; border-radius: 50%; background: #fff; animation: titilar 2.6s ease-in-out infinite; }
    @keyframes titilar { 50% { opacity: .15; transform: scale(.6); } }

    .hero-contenido { position: relative; display: flex; justify-content: space-between; gap: 2rem; flex-wrap: wrap; align-items: flex-end; }
    .texto { flex: 1 1 420px; }
    .momento-etiqueta { display: inline-flex; align-items: center; gap: .4rem; font-size: .75rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase;
      background: rgba(255,255,255,.18); padding: .3rem .7rem; border-radius: 999px; backdrop-filter: blur(4px); }
    h1 { font-size: 2rem; margin-top: .7rem; line-height: 1.2; }
    .mano { display: inline-block; animation: saludar 2.4s ease-in-out 1 .4s; transform-origin: 70% 70%; }
    @keyframes saludar { 0%, 60%, 100% { transform: rotate(0); } 10%, 30% { transform: rotate(16deg); } 20%, 40% { transform: rotate(-8deg); } }
    .resumen-dia { margin: .6rem 0 1.25rem; font-size: 1.05rem; color: rgba(255,255,255,.88); max-width: 620px; }
    .acciones { display: flex; gap: .75rem; flex-wrap: wrap; }
    :host ::ng-deep .hero .boton-blanco { background: #fff; color: #0f766e; border-color: #fff; font-weight: 600; }
    :host ::ng-deep .hero .boton-blanco:hover { background: #f0fdfa !important; color: #0f766e !important; }
    :host ::ng-deep .hero .boton-claro { color: #fff !important; border-color: rgba(255,255,255,.6) !important; background: rgba(255,255,255,.08) !important; }

    .reloj { text-align: right; min-width: 240px;
      strong { display: block; font-size: 3rem; font-weight: 300; line-height: 1; letter-spacing: -.02em; small { font-size: 1rem; margin-left: .3rem; font-weight: 500; } }
      span { display: block; margin-top: .35rem; color: rgba(255,255,255,.85); } }
    .frase { margin-top: .9rem; max-width: 300px; margin-left: auto; font-size: .82rem; line-height: 1.4; padding: .65rem .8rem; border-radius: 12px;
      background: rgba(255,255,255,.14); backdrop-filter: blur(6px); text-align: left; i { margin-right: .3rem; } }

    /* ---------- Indicadores ---------- */
    .kpis { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
    .kpi { display: flex; align-items: center; gap: 1rem; animation: entrar .4s ease both; transition: transform .15s, box-shadow .15s, border-color .15s;
      font: inherit; color: inherit; text-align: left; cursor: pointer; width: 100%;
      &:hover { transform: translateY(-3px); box-shadow: 0 12px 26px rgba(16, 24, 40, .1); border-color: #99f6e4; }
      &:hover .flecha { opacity: 1; transform: translateX(0); } }
    .flecha { margin-left: auto; color: var(--mc-primario); opacity: 0; transform: translateX(-4px); transition: all .15s; }
    .lista-citas li { cursor: pointer; } .lista-citas li:hover .flecha { opacity: 1; transform: translateX(0); }
    .mb-tarjeta { margin-bottom: 1.5rem; }
    .intro-medicos { margin: 0 0 1rem; }
    .medicos { display: flex; flex-direction: column; gap: .8rem; }
    .medico { border: 1px solid var(--mc-borde); border-radius: 14px; padding: .8rem 1rem; transition: border-color .15s;
      &.disponible { border-color: #86efac; background: linear-gradient(135deg, #fff, #f0fdf4); } }
    .medico-cabeza { display: flex; align-items: center; gap: .75rem; margin-bottom: .6rem; }
    .avatar-m { width: 40px; height: 40px; border-radius: 12px; display: grid; place-items: center; color: #fff; font-weight: 700;
      background: linear-gradient(135deg, #0d9488, #14b8a6); }
    .medico-nombre { flex: 1; strong { display: block; } span { font-size: .78rem; color: var(--mc-suave); } }
    .hoy { font-size: .74rem; font-weight: 700; border-radius: 999px; padding: .2rem .65rem; background: #fef3c7; color: #b45309; }
    .hoy.si { background: #dcfce7; color: #15803d; } .hoy.no { background: #f1f5f9; color: #64748b; }
    .semana { display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); gap: .35rem; }
    .dia { background: #f0fdfa; border-radius: 10px; padding: .35rem .3rem; text-align: center; font-size: .72rem; display: flex; flex-direction: column; gap: .1rem;
      b { font-size: .74rem; color: var(--mc-primario-oscuro); } span { color: #334155; } .libre { color: #cbd5e1; }
      &.vacio-dia { background: #f8fafc; } &.es-hoy { outline: 2px solid var(--mc-primario); } }
    @media (max-width: 640px) { .semana { grid-template-columns: repeat(4, minmax(0, 1fr)); } }
    .kpi .icono { width: 48px; height: 48px; border-radius: 12px; display: grid; place-items: center; font-size: 1.3rem; }
    .kpi span { color: var(--mc-suave); font-size: .85rem; }
    .kpi strong { display: block; font-size: 1.7rem; }

    /* ---------- Próxima cita ---------- */
    .proxima { display: flex; align-items: center; gap: 1.25rem; padding: 1rem 1.25rem; margin-bottom: 1.5rem; border-radius: 16px;
      background: linear-gradient(120deg, #f0fdfa, #ecfeff); border: 1px solid #99f6e4; animation: entrar .4s ease both; flex-wrap: wrap; }
    .proxima-hora { padding-right: 1.25rem; border-right: 1px solid #99f6e4;
      span { display: block; font-size: .75rem; color: var(--mc-suave); text-transform: uppercase; letter-spacing: .05em; }
      strong { font-size: 1.5rem; color: var(--mc-primario-oscuro); }
      em { display: block; font-style: normal; font-size: .8rem; color: #0891b2; font-weight: 600; } }
    .proxima-detalle { flex: 1; strong { display: block; font-size: 1.05rem; } span { color: var(--mc-suave); font-size: .85rem; } }

    /* ---------- Listas ---------- */
    .columnas { display: grid; grid-template-columns: 1.6fr 1fr; gap: 1.25rem; align-items: start; }
    .lista-citas { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: .6rem; }
    .lista-citas li { display: flex; align-items: center; gap: 1rem; padding: .75rem 1rem; border-radius: 12px; background: #f8fafb; border: 1px solid var(--mc-borde);
      animation: entrar .35s ease both; transition: transform .15s, background .15s;
      &:hover { transform: translateX(3px); background: #f0fdfa; }
      &.siguiente { border-color: #5eead4; background: #f0fdfa; box-shadow: inset 3px 0 0 #14b8a6; } }
    .hora { min-width: 56px; text-align: center; strong { display: block; font-size: 1.05rem; color: var(--mc-primario-oscuro); } span { font-size: .75rem; color: var(--mc-suave); } }
    .detalle { flex: 1; min-width: 0; strong { display: block; } span { font-size: .8rem; color: var(--mc-suave); } }

    .barras { display: flex; flex-direction: column; gap: .9rem; }
    .etiqueta { display: flex; justify-content: space-between; font-size: .85rem; margin-bottom: .3rem; }
    .pista { height: 8px; border-radius: 99px; background: #eef2f4; overflow: hidden; }
    .relleno { height: 100%; border-radius: 99px; transition: width .6s ease; }
    :host ::ng-deep .mb-2 { margin-bottom: .5rem; }
    :host ::ng-deep .mb-3 { margin-bottom: .9rem; }

    /* ---------- Enfermería ---------- */
    .asignados { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: .9rem; }
    .asignado { display: flex; align-items: center; gap: .9rem; padding: 1rem; border-radius: 14px; background: #fdf2f8; border: 1px solid #fbcfe8;
      strong { display: block; } span { font-size: .8rem; color: var(--mc-suave); } }
    .avatar-medico { width: 44px; height: 44px; border-radius: 12px; display: grid; place-items: center; color: #fff; background: linear-gradient(135deg, #ec4899, #be185d); }

    @media (max-width: 1100px) { .columnas { grid-template-columns: 1fr; } }
    @media (max-width: 700px) {
      .hero { padding: 1.5rem; } h1 { font-size: 1.5rem; }
      .reloj { text-align: left; min-width: 0; strong { font-size: 2.2rem; } } .frase { margin-left: 0; }
    }
    @media (prefers-reduced-motion: reduce) { * { animation: none !important; } }
  `
})
export class Dashboard {
  private auth = inject(AuthService);
  protected perfil = inject(PerfilService);
  private agenda = inject(AgendaService);
  private directory = inject(DirectoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  protected saludo = saludo();
  protected momento = PerfilService.momento();
  protected iconoMomento = this.momento === 'noche' ? 'pi pi-moon' : 'pi pi-sun';
  protected fechaLarga = mayuscula(new Date().toLocaleDateString('es', { weekday: 'long', day: 'numeric', month: 'long' }));
  protected frase = FRASES[Math.floor(Date.now() / 86_400_000) % FRASES.length];
  protected estrellas = Array.from({ length: 28 }, () => ({ x: Math.random() * 100, y: Math.random() * 100, d: Math.random() * 3 }));
  protected leer = leerFecha;

  protected ahora = signal(new Date());
  protected cargando = signal(true);
  protected progreso = signal(0); // 0 → 1 para animar los números
  protected resumen = signal<ResumenCitas | null>(null);
  protected citas = signal<Cita[]>([]);
  protected totalPacientes = signal(0);
  protected totalMedicos = signal(0);
  private esBienvenida = signal(this.route.snapshot.queryParamMap.has('bienvenida'));
  private celebracionCerrada = signal(false);

  protected esMedico = computed(() => this.auth.tieneRol('Medico'));
  protected esEnfermeria = computed(() => this.auth.tieneRol('Enfermeria'));
  protected puedeAgendar = computed(() => this.auth.tieneRol('Administrador', 'Recepcion', 'Medico'));

  // Ventanas que se abren al hacer clic en las tarjetas
  protected listaKpi = signal<{ titulo: string; citas: Cita[] } | null>(null);
  protected dialogoMedicos = signal(false);
  protected medicos = signal<Medico[]>([]);
  protected horarios = signal<HorarioMedico[]>([]);
  protected medicosActivos = computed(() => this.medicos().filter(m => m.activo));
  protected hoyDia = new Date().getDay();
  protected dias = [
    { valor: 1, corto: 'Lun' }, { valor: 2, corto: 'Mar' }, { valor: 3, corto: 'Mié' }, { valor: 4, corto: 'Jue' },
    { valor: 5, corto: 'Vie' }, { valor: 6, corto: 'Sáb' }, { valor: 0, corto: 'Dom' }
  ];

  private cuenta(...codigos: string[]): number {
    return this.resumen()?.porEstado.filter(e => codigos.includes(e.codigo)).reduce((t, e) => t + e.cantidad, 0) ?? 0;
  }

  protected proxima = computed<Cita | null>(() => {
    const ahora = this.ahora().getTime();
    return this.citas()
      .filter(c => PENDIENTES.includes(c.estadoCodigo) && leerFecha(c.fechaHoraFin).getTime() > ahora)
      .sort((a, b) => a.fechaHoraInicio.localeCompare(b.fechaHoraInicio))[0] ?? null;
  });

  // Una línea que resume el día según quién entra
  protected resumenDelDia = computed(() => {
    const rol = this.auth.usuario()?.rol;

    if (rol === 'Enfermeria') {
      const asignados = this.perfil.perfil()?.medicosAsignados ?? [];
      return asignados.length
        ? `Hoy asistes a ${asignados.map(m => m.nombre).join(' y ')}. ¡Que tengas un excelente turno!`
        : 'Qué bueno tenerte en el equipo. ¡Que tengas un excelente turno!';
    }
    if (this.cargando()) return 'Preparando el resumen de tu día…';

    const total = this.resumen()?.total ?? 0;
    const pendientes = this.cuenta(...PENDIENTES);
    const p = this.proxima();

    if (rol === 'Medico') {
      if (!pendientes) return total ? `¡Excelente trabajo! Ya atendiste a todos tus pacientes de hoy.` : 'Hoy no tienes pacientes agendados. ¡Buen momento para ponerte al día! ☕';
      const hora = p ? leerFecha(p.fechaHoraInicio).toLocaleTimeString('es', { hour: 'numeric', minute: '2-digit' }) : '';
      return `Tienes ${pendientes} ${pendientes === 1 ? 'paciente' : 'pacientes'} por atender hoy.` + (p ? ` El siguiente es ${p.pacienteNombre} a las ${hora}.` : '');
    }
    if (rol === 'Recepcion') {
      return total
        ? `Hoy hay ${total} ${total === 1 ? 'cita' : 'citas'} en la clínica y ${pendientes} ${pendientes === 1 ? 'paciente' : 'pacientes'} por llegar.`
        : 'Aún no hay citas para hoy. ¡Agenda la primera del día!';
    }
    return `Así va la clínica hoy: ${total} ${total === 1 ? 'cita' : 'citas'}, ${pendientes} pendientes y ${this.totalMedicos()} médicos activos.`;
  });

  protected acciones = computed(() => {
    const rol = this.auth.usuario()?.rol;
    if (rol === 'Medico') return [{ texto: 'Ver mis citas', icono: 'pi pi-calendar-clock', ruta: '/mis-citas' }, { texto: 'Pacientes', icono: 'pi pi-users', ruta: '/pacientes' }];
    if (rol === 'Enfermeria') return [];
    if (rol === 'Administrador') return [
      { texto: 'Agendar cita', icono: 'pi pi-calendar-plus', ruta: '/agendar' },
      { texto: 'Personal', icono: 'pi pi-id-card', ruta: '/admin/personal' },
      { texto: 'Horarios', icono: 'pi pi-clock', ruta: '/admin/horarios' }
    ];
    return [{ texto: 'Agendar cita', icono: 'pi pi-calendar-plus', ruta: '/agendar' }, { texto: 'Pacientes', icono: 'pi pi-users', ruta: '/pacientes' }];
  });

  protected kpis = computed(() => {
    const base = [
      { clave: 'hoy', titulo: 'Citas de hoy', valor: this.resumen()?.total ?? 0, icono: 'pi pi-calendar', color: '#0d9488', fondo: '#ccfbf1' },
      { clave: 'pendientes', titulo: 'Pendientes', valor: this.cuenta(...PENDIENTES), icono: 'pi pi-hourglass', color: '#d97706', fondo: '#fef3c7' },
      { clave: 'atendidas', titulo: 'Atendidas', valor: this.cuenta('ATENDIDA'), icono: 'pi pi-check-circle', color: '#059669', fondo: '#d1fae5' }
    ];
    return this.esMedico()
      ? [...base, { clave: 'atencion', titulo: 'En atención', valor: this.cuenta('EN_ATENCION'), icono: 'pi pi-heart', color: '#7c3aed', fondo: '#ede9fe' }]
      : [
        ...base,
        { clave: 'pacientes', titulo: 'Pacientes', valor: this.totalPacientes(), icono: 'pi pi-users', color: '#2563eb', fondo: '#dbeafe' },
        { clave: 'medicos', titulo: 'Médicos', valor: this.totalMedicos(), icono: 'pi pi-id-card', color: '#db2777', fondo: '#fce7f3' }
      ];
  });

  // Bienvenida, cumpleaños o aniversario en la clínica
  protected celebracion = computed(() => {
    if (this.celebracionCerrada()) return null;
    const nombre = this.perfil.primerNombre();
    if (this.esBienvenida()) {
      return { emoji: '🎉', titulo: `¡${this.perfil.bienvenida()} al equipo, ${nombre}!`, texto: 'Tu cuenta quedó lista y tu contraseña es solo tuya. Este es tu espacio de trabajo.' };
    }
    if (this.perfil.esCumpleanos()) {
      return { emoji: '🎂', titulo: `¡Feliz cumpleaños, ${nombre}!`, texto: 'Todo el equipo te desea un día increíble. ¡Gracias por ser parte de la clínica!' };
    }
    const anios = this.perfil.aniosEnClinica();
    if (anios > 0) {
      return { emoji: '🏆', titulo: `¡Hoy cumples ${anios} ${anios === 1 ? 'año' : 'años'} con nosotros, ${nombre}!`, texto: 'Gracias por tu dedicación y por cuidar de nuestros pacientes.' };
    }
    return null;
  });

  constructor() {
    // Reloj en vivo
    const reloj = setInterval(() => this.ahora.set(new Date()), 15_000);
    inject(DestroyRef).onDestroy(() => clearInterval(reloj));

    // Confeti al aparecer una celebración (una vez por día y persona)
    let confetiLanzado = false;
    effect(() => {
      const c = this.celebracion();
      if (c && !confetiLanzado && this.debeCelebrar(c.emoji)) {
        confetiLanzado = true;
        setTimeout(() => lanzarConfeti(), 400);
      }
    });

    // Quitar ?bienvenida=1 de la dirección para que no se repita al recargar
    if (this.esBienvenida()) this.router.navigate([], { queryParams: {}, replaceUrl: true });

    this.cargarDatos();
  }

  private cargarDatos(): void {
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
        this.medicos.set(r.medicos);
        this.totalMedicos.set(r.medicos.filter(m => m.activo).length);
        this.cargando.set(false);
        this.animarNumeros();
      },
      error: () => this.cargando.set(false)
    });
  }

  // Los números suben de 0 a su valor en menos de un segundo
  private animarNumeros(): void {
    const inicio = performance.now();
    const paso = (t: number) => {
      const x = Math.min(1, (t - inicio) / 900);
      this.progreso.set(1 - Math.pow(1 - x, 3));
      if (x < 1) requestAnimationFrame(paso);
    };
    requestAnimationFrame(paso);
  }

  protected animado(valor: number): number {
    return Math.round(valor * this.progreso());
  }

  protected faltan(c: Cita): string {
    const min = Math.round((leerFecha(c.fechaHoraInicio).getTime() - this.ahora().getTime()) / 60_000);
    if (min <= 0) return 'Es ahora';
    if (min < 60) return `En ${min} min`;
    const h = Math.floor(min / 60), m = min % 60;
    return m ? `En ${h} h ${m} min` : `En ${h} h`;
  }

  cerrarCelebracion(): void {
    this.celebracionCerrada.set(true);
  }

  // Guarda en este navegador que ya se celebró hoy (si no se puede, celebra igual)
  private debeCelebrar(tipo: string): boolean {
    if (tipo === '🎉') return true;
    const clave = `medicitas.celebrado.${this.auth.usuario()?.usuarioId}.${fechaParam(new Date())}`;
    try {
      if (localStorage.getItem(clave)) return false;
      localStorage.setItem(clave, '1');
    } catch { /* sin almacenamiento: se celebra */ }
    return true;
  }

  // ===================== Clic en las tarjetas =====================
  abrirKpi(clave: string): void {
    const citas = this.citas();
    switch (clave) {
      case 'hoy': return this.listaKpi.set({ titulo: 'Citas de hoy', citas });
      case 'pendientes': return this.listaKpi.set({ titulo: 'Citas pendientes de hoy', citas: citas.filter(c => PENDIENTES.includes(c.estadoCodigo)) });
      case 'atendidas': return this.listaKpi.set({ titulo: 'Pacientes atendidos hoy', citas: citas.filter(c => c.estadoCodigo === 'ATENDIDA') });
      case 'atencion': return this.listaKpi.set({ titulo: 'En atención ahora', citas: citas.filter(c => c.estadoCodigo === 'EN_ATENCION') });
      case 'pacientes': this.router.navigate(['/pacientes']); return;
      case 'medicos':
        if (this.medicos().length === 0) this.directory.medicos().subscribe(m => this.medicos.set(m));
        this.agenda.horarios().subscribe(h => this.horarios.set(h));
        this.dialogoMedicos.set(true);
        return;
    }
  }

  recargar(): void {
    this.cargarDatos();
  }

  protected bloques(medicoId: number, dia: number): HorarioMedico[] {
    return this.horarios().filter(h => h.medicoId === medicoId && h.diaSemana === dia).sort((a, b) => a.horaInicio.localeCompare(b.horaInicio));
  }

  protected especialidadesDe(m: Medico): string {
    return m.especialidades.map(e => e.nombre).join(' · ') || 'Sin especialidad';
  }

  // "Atendiendo ahora" / "Hoy 08:00–12:00" / "Hoy no atiende"
  protected estadoHoy(medicoId: number): { hoy: boolean; ahora: boolean; texto: string } {
    const hoy = this.bloques(medicoId, this.hoyDia);
    if (hoy.length === 0) return { hoy: false, ahora: false, texto: 'Hoy no atiende' };
    const ahora = this.ahora();
    const hhmm = `${ahora.getHours().toString().padStart(2, '0')}:${ahora.getMinutes().toString().padStart(2, '0')}`;
    const enTurno = hoy.some(h => h.horaInicio <= hhmm && hhmm < h.horaFin);
    return enTurno
      ? { hoy: true, ahora: true, texto: 'Disponible ahora' }
      : { hoy: true, ahora: false, texto: 'Hoy ' + hoy.map(h => `${h.horaInicio}–${h.horaFin}`).join(', ') };
  }
}
