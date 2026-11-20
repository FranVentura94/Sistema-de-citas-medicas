import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { Button } from 'primeng/button';
import { Avatar } from 'primeng/avatar';
import { Tooltip } from 'primeng/tooltip';
import { AuthService } from '../core/auth.service';
import { APP_NAME, APP_SUBTITULO, Rol } from '../core/config';
import { iniciales, mayuscula } from '../core/utils';

interface OpcionMenu {
  texto: string;
  icono: string;
  ruta: string;
  roles: Rol[];
}

// Menú lateral: cada usuario ve solo las opciones de su rol
const MENU: OpcionMenu[] = [
  { texto: 'Inicio', icono: 'pi pi-home', ruta: '/dashboard', roles: ['Administrador', 'Recepcion', 'Medico'] },
  { texto: 'Mis citas', icono: 'pi pi-calendar-clock', ruta: '/mis-citas', roles: ['Medico'] },
  { texto: 'Agendar cita', icono: 'pi pi-calendar-plus', ruta: '/agendar', roles: ['Administrador', 'Recepcion'] },
  { texto: 'Agenda del día', icono: 'pi pi-calendar', ruta: '/agenda', roles: ['Administrador', 'Recepcion'] },
  { texto: 'Pacientes', icono: 'pi pi-users', ruta: '/pacientes', roles: ['Administrador', 'Recepcion', 'Medico'] }
];

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Button, Avatar, Tooltip],
  template: `
    <div class="contenedor" [class.menu-abierto]="menuAbierto()">
      <!-- ============ Menú lateral ============ -->
      <aside class="lateral">
        <div class="marca">
          <div class="logo"><i class="pi pi-heart-fill"></i></div>
          <div>
            <strong>{{ appName }}</strong>
            <span>{{ subtitulo }}</span>
          </div>
        </div>

        <nav>
          <span class="seccion">Menú principal</span>
          @for (op of menu(); track op.ruta) {
            <a [routerLink]="op.ruta" routerLinkActive="activo" (click)="menuAbierto.set(false)">
              <i [class]="op.icono"></i>
              <span>{{ op.texto }}</span>
            </a>
          }
        </nav>

        <div class="usuario">
          <p-avatar [label]="iniciales()" shape="circle" class="avatar" />
          <div class="datos">
            <strong>{{ usuario()?.nombreCompleto }}</strong>
            <span>{{ rolTexto() }}</span>
          </div>
          <p-button icon="pi pi-sign-out" [rounded]="true" [text]="true" severity="secondary"
                    pTooltip="Cerrar sesión" tooltipPosition="top" (onClick)="auth.logout()" />
        </div>
      </aside>

      <div class="fondo-oscuro" (click)="menuAbierto.set(false)"></div>

      <!-- ============ Contenido ============ -->
      <main class="principal">
        <header class="barra">
          <p-button icon="pi pi-bars" [text]="true" severity="secondary" class="boton-menu" (onClick)="menuAbierto.set(true)" />
          <div class="fecha"><i class="pi pi-calendar"></i> {{ hoy }}</div>
        </header>

        <section class="pagina">
          <router-outlet />
        </section>
      </main>
    </div>
  `,
  styles: `
    .contenedor { display: flex; min-height: 100vh; }

    .lateral {
      width: 260px; flex-shrink: 0; position: fixed; inset: 0 auto 0 0; z-index: 30;
      display: flex; flex-direction: column;
      background: linear-gradient(180deg, #0f3d3e 0%, #0b2a2b 100%); color: #e2f1f0;
      transition: transform .25s ease;
    }
    .marca {
      display: flex; align-items: center; gap: .75rem; padding: 1.5rem 1.25rem;
      strong { display: block; font-size: 1.2rem; color: #fff; }
      span { font-size: .72rem; color: #8fbcb9; }
    }
    .logo {
      width: 42px; height: 42px; border-radius: 12px; display: grid; place-items: center;
      background: linear-gradient(135deg, #2dd4bf, #0d9488); color: #fff; font-size: 1.2rem;
      box-shadow: 0 6px 16px rgba(45, 212, 191, .35);
    }
    nav { flex: 1; padding: .5rem .75rem; display: flex; flex-direction: column; gap: .25rem; }
    .seccion { font-size: .7rem; text-transform: uppercase; letter-spacing: .08em; color: #6b9a97; padding: .75rem .75rem .5rem; }
    nav a {
      display: flex; align-items: center; gap: .85rem; padding: .75rem .9rem; border-radius: 10px;
      color: #cfe6e4; text-decoration: none; font-weight: 500; transition: background .15s;
      i { font-size: 1.05rem; }
      &:hover { background: rgba(255, 255, 255, .06); color: #fff; }
      &.activo { background: rgba(45, 212, 191, .16); color: #5eead4; box-shadow: inset 3px 0 0 #2dd4bf; }
    }
    .usuario {
      display: flex; align-items: center; gap: .75rem; padding: 1rem 1.1rem; margin: .75rem;
      background: rgba(255, 255, 255, .06); border-radius: 12px;
      .datos { flex: 1; min-width: 0; }
      strong { display: block; color: #fff; font-size: .85rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
      span { font-size: .75rem; color: #8fbcb9; }
    }
    :host ::ng-deep .avatar { background: #2dd4bf; color: #0b2a2b; font-weight: 700; }

    .principal { flex: 1; margin-left: 260px; min-width: 0; }
    .barra {
      height: 64px; display: flex; align-items: center; justify-content: space-between; padding: 0 2rem;
      background: rgba(255, 255, 255, .8); backdrop-filter: blur(8px); border-bottom: 1px solid var(--mc-borde);
      position: sticky; top: 0; z-index: 10;
    }
    .fecha { color: var(--mc-suave); display: flex; gap: .5rem; align-items: center; margin-left: auto; }
    .boton-menu { display: none; }
    .pagina { padding: 2rem; max-width: 1400px; }
    .fondo-oscuro { display: none; }

    @media (max-width: 1024px) {
      .lateral { transform: translateX(-100%); }
      .menu-abierto .lateral { transform: none; }
      .menu-abierto .fondo-oscuro { display: block; position: fixed; inset: 0; background: rgba(0, 0, 0, .4); z-index: 20; }
      .principal { margin-left: 0; }
      .boton-menu { display: inline-flex; }
      .pagina { padding: 1.25rem; }
      .barra { padding: 0 1rem; }
    }
  `
})
export class Layout {
  protected auth = inject(AuthService);
  private router = inject(Router);

  protected appName = APP_NAME;
  protected subtitulo = APP_SUBTITULO;
  protected menuAbierto = signal(false);
  protected usuario = this.auth.usuario;
  protected hoy = mayuscula(new Date().toLocaleDateString('es', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' }));

  protected menu = computed(() => MENU.filter(op => this.auth.tieneRol(...op.roles)));
  protected iniciales = computed(() => iniciales(this.usuario()?.nombreCompleto ?? '?'));
  protected rolTexto = computed(() => {
    const rol = this.usuario()?.rol;
    return rol === 'Recepcion' ? 'Recepción' : rol === 'Medico' ? 'Médico' : rol ?? '';
  });

  constructor() {
    // En celular, cerrar el menú al navegar
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => this.menuAbierto.set(false));
  }
}
