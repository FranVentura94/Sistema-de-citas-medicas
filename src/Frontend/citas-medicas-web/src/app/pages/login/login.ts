import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Message } from 'primeng/message';
import { AuthService } from '../../core/auth.service';
import { APP_NAME, APP_SUBTITULO } from '../../core/config';
import { mensajeError, saludo } from '../../core/utils';

@Component({
  selector: 'app-login',
  imports: [FormsModule, Button, InputText, Password, IconField, InputIcon, Message],
  template: `
    <div class="login">
      <!-- ============ Panel de presentación ============ -->
      <section class="presentacion">
        <div class="marca">
          <div class="logo"><i class="pi pi-heart-fill"></i></div>
          <span>{{ appName }}</span>
        </div>

        <div class="mensaje">
          <h1>Cuidamos de tus pacientes,<br />nosotros organizamos tu agenda.</h1>
          <p>{{ subtitulo }}: agenda inteligente, expediente clínico y recetas en un solo lugar.</p>

          <ul>
            <li><i class="pi pi-calendar-plus"></i> Citas sin cruces de horario</li>
            <li><i class="pi pi-file-edit"></i> Expediente clínico con historial</li>
            <li><i class="pi pi-shield"></i> Acceso seguro por roles</li>
          </ul>
        </div>

        <small>© {{ anio }} {{ appName }} · Arquitectura de microservicios</small>
      </section>

      <!-- ============ Formulario ============ -->
      <section class="formulario-login">
        @if (paso() === 'login') {
        <form class="caja" (ngSubmit)="ingresar()" #f="ngForm">
          <h2>{{ saludo }} <span class="mano">👋</span></h2>
          <p class="texto-suave">Ingresa tus credenciales para continuar</p>

          @if (expirada) {
            <p-message severity="warn" icon="pi pi-clock">Tu sesión expiró. Vuelve a iniciar sesión.</p-message>
          }
          @if (error()) {
            <p-message severity="error" icon="pi pi-times-circle">{{ error() }}</p-message>
          }

          <div class="campo">
            <label for="usuario">Usuario</label>
            <p-iconfield>
              <p-inputicon class="pi pi-user" />
              <input pInputText id="usuario" name="usuario" [(ngModel)]="usuario" required autocomplete="username" placeholder="Tu usuario" />
            </p-iconfield>
          </div>

          <div class="campo">
            <label for="clave">Contraseña</label>
            <p-password inputId="clave" name="clave" [(ngModel)]="clave" required [feedback]="false" [toggleMask]="true"
                        placeholder="Tu contraseña" autocomplete="current-password" />
          </div>

          <p-button type="submit" label="Iniciar sesión" icon="pi pi-sign-in" [loading]="cargando()" [disabled]="!f.valid" styleClass="w-full" />

          <!-- Accesos rápidos para la demostración (quitar en producción) -->
          <div class="demo">
            <span>Usuarios de demostración</span>
            <div class="chips">
              @for (d of demo; track d.usuario) {
                <button type="button" (click)="usarDemo(d.usuario, d.clave)"><i [class]="d.icono"></i> {{ d.texto }}</button>
              }
            </div>
          </div>
        </form>
        } @else {
        <!-- ============ Primer ingreso: crear su propia contraseña ============ -->
        <form class="caja aparecer" (ngSubmit)="crearPassword()">
          <div class="escudo"><i class="pi pi-shield"></i></div>
          <h2>¡Hola! Es tu primer ingreso</h2>
          <span class="usuario-chip"><i class="pi pi-user"></i> {{ usuario }}</span>
          <p class="texto-suave">Crea tu contraseña personal. <b>Solo tú la conocerás:</b> ni el administrador puede verla.</p>

          @if (error()) {
            <p-message severity="error" icon="pi pi-times-circle">{{ error() }}</p-message>
          }

          <div class="campo">
            <label for="nueva">Nueva contraseña</label>
            <p-password inputId="nueva" name="nueva" [(ngModel)]="nueva" [feedback]="false" [toggleMask]="true"
                        placeholder="Escribe tu nueva contraseña" autocomplete="new-password" />
          </div>

          <div class="fuerza">
            <div class="pista"><div class="relleno" [style.width.%]="fuerza() * 25" [style.background]="colorFuerza()"></div></div>
            <span [style.color]="colorFuerza()">{{ textoFuerza() }}</span>
          </div>

          <div class="campo">
            <label for="confirmar">Confírmala</label>
            <p-password inputId="confirmar" name="confirmar" [(ngModel)]="confirmar" [feedback]="false" [toggleMask]="true"
                        placeholder="Escríbela otra vez" autocomplete="new-password" />
          </div>

          <ul class="requisitos">
            @for (r of requisitos(); track r.texto) {
              <li [class.cumple]="r.ok"><i [class]="r.ok ? 'pi pi-check-circle' : 'pi pi-circle'"></i> {{ r.texto }}</li>
            }
          </ul>

          <p-button type="submit" label="Crear mi contraseña y entrar" icon="pi pi-lock" [loading]="cargando()"
                    [disabled]="!todoCumple()" styleClass="w-full" />
          <button type="button" class="volver" (click)="volverAlLogin()"><i class="pi pi-arrow-left"></i> Volver al inicio de sesión</button>
        </form>
        }
      </section>
    </div>
  `,
  styles: `
    .login { display: grid; grid-template-columns: 1.1fr 1fr; min-height: 100vh; background: #fff; }

    .presentacion {
      position: relative; overflow: hidden; color: #e6fffb; padding: 3rem; display: flex; flex-direction: column; justify-content: space-between;
      background: radial-gradient(circle at 20% 20%, #14b8a6 0%, transparent 45%), radial-gradient(circle at 80% 80%, #0e7490 0%, transparent 50%), #0f3d3e;
    }
    .marca { display: flex; align-items: center; gap: .75rem; font-size: 1.4rem; font-weight: 700; color: #fff; }
    .logo { width: 46px; height: 46px; border-radius: 14px; display: grid; place-items: center; background: rgba(255,255,255,.15); backdrop-filter: blur(6px); }
    .mensaje h1 { font-size: 2.3rem; line-height: 1.2; color: #fff; margin-bottom: 1rem; }
    .mensaje p { font-size: 1.05rem; color: #b8e6e1; max-width: 480px; }
    .mensaje ul { list-style: none; padding: 0; margin: 2rem 0 0; display: grid; gap: .9rem; }
    .mensaje li { display: flex; align-items: center; gap: .75rem; font-size: 1rem; }
    .mensaje li i { width: 36px; height: 36px; border-radius: 10px; display: grid; place-items: center; background: rgba(255,255,255,.12); }
    small { color: #8fbcb9; }

    .formulario-login { display: grid; place-items: center; padding: 2rem; }
    .caja { width: 100%; max-width: 400px; display: flex; flex-direction: column; gap: 1.1rem; }
    .caja h2 { font-size: 1.8rem; }
    .caja p { margin: -.6rem 0 .4rem; }
    :host ::ng-deep .caja .p-iconfield, :host ::ng-deep .caja .p-password, :host ::ng-deep .caja .p-password input, :host ::ng-deep .caja input { width: 100%; }
    :host ::ng-deep .w-full { width: 100%; padding: .8rem; }

    .demo { margin-top: .5rem; padding-top: 1.25rem; border-top: 1px dashed var(--mc-borde); }
    .demo span { font-size: .8rem; color: var(--mc-suave); }
    .chips { display: flex; gap: .5rem; flex-wrap: wrap; margin-top: .6rem; }
    .chips button {
      border: 1px solid var(--mc-borde); background: #f8fafc; border-radius: 999px; padding: .45rem .85rem; cursor: pointer;
      font: inherit; font-size: .82rem; color: var(--mc-texto); display: inline-flex; gap: .4rem; align-items: center;
      &:hover { border-color: var(--mc-primario); color: var(--mc-primario-oscuro); background: #f0fdfa; }
    }

    .mano { display: inline-block; animation: saludar 2.2s ease-in-out 1; transform-origin: 70% 70%; }
    @keyframes saludar { 0%, 60%, 100% { transform: rotate(0); } 10%, 30% { transform: rotate(16deg); } 20%, 40% { transform: rotate(-8deg); } }
    .presentacion::before, .presentacion::after {
      content: ''; position: absolute; border-radius: 50%; filter: blur(40px); opacity: .45; animation: flotar 12s ease-in-out infinite alternate;
    }
    .presentacion::before { width: 320px; height: 320px; background: #2dd4bf; top: -80px; right: -60px; }
    .presentacion::after { width: 260px; height: 260px; background: #0ea5e9; bottom: -60px; left: 20%; animation-delay: -6s; }
    .presentacion > * { position: relative; z-index: 1; }
    @keyframes flotar { to { transform: translate(-40px, 30px) scale(1.15); } }

    .aparecer { animation: entrar .35s ease both; }
    @keyframes entrar { from { opacity: 0; transform: translateY(12px); } }
    .escudo { width: 56px; height: 56px; border-radius: 16px; display: grid; place-items: center; font-size: 1.6rem; color: #fff;
      background: linear-gradient(135deg, #14b8a6, #0e7490); box-shadow: 0 10px 24px rgba(13, 148, 136, .35); }
    .usuario-chip { align-self: flex-start; display: inline-flex; gap: .4rem; align-items: center; font-size: .82rem; font-weight: 600;
      background: #f0fdfa; color: var(--mc-primario-oscuro); border: 1px solid #99f6e4; border-radius: 999px; padding: .25rem .7rem; margin-top: -.5rem; }
    .fuerza { display: flex; align-items: center; gap: .75rem; margin-top: -.4rem; font-size: .8rem; font-weight: 600; }
    .fuerza .pista { flex: 1; height: 6px; border-radius: 99px; background: #eef2f4; overflow: hidden; }
    .fuerza .relleno { height: 100%; border-radius: 99px; transition: width .3s, background .3s; }
    .requisitos { list-style: none; margin: 0; padding: .8rem 1rem; border-radius: 12px; background: #f8fafc; display: grid; gap: .4rem; font-size: .85rem; color: var(--mc-suave); }
    .requisitos li { display: flex; align-items: center; gap: .5rem; transition: color .2s; }
    .requisitos li.cumple { color: #15803d; font-weight: 600; }
    .volver { border: none; background: none; color: var(--mc-suave); cursor: pointer; font: inherit; display: inline-flex; gap: .4rem; align-items: center; justify-content: center;
      &:hover { color: var(--mc-primario-oscuro); } }

    @media (max-width: 900px) {
      .login { grid-template-columns: 1fr; }
      .presentacion { display: none; }
    }
  `
})
export class Login {
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  protected appName = APP_NAME;
  protected subtitulo = APP_SUBTITULO;
  protected anio = new Date().getFullYear();
  protected expirada = this.route.snapshot.queryParamMap.has('expirada');

  protected saludo = saludo();
  protected usuario = '';
  protected clave = '';
  protected cargando = signal(false);
  protected error = signal<string | null>(null);

  // Primer ingreso (entró con contraseña temporal)
  protected paso = signal<'login' | 'crear'>('login');
  protected nueva = '';
  protected confirmar = '';

  protected demo = [
    { texto: 'Recepción', usuario: 'recepcion', clave: 'Recepcion123*', icono: 'pi pi-id-card' },
    { texto: 'Médico', usuario: 'medico', clave: 'Medico123*', icono: 'pi pi-heart' },
    { texto: 'Administrador', usuario: 'admin', clave: 'Admin123*', icono: 'pi pi-cog' }
  ];

  usarDemo(usuario: string, clave: string): void {
    this.usuario = usuario;
    this.clave = clave;
    this.ingresar();
  }

  ingresar(): void {
    if (!this.usuario || !this.clave) return;
    this.cargando.set(true);
    this.error.set(null);

    this.auth.login(this.usuario.trim(), this.clave).subscribe({
      next: r => {
        if (r.requiereCambioPassword) {
          // Contraseña temporal correcta: ahora debe crear la suya
          this.cargando.set(false);
          this.nueva = '';
          this.confirmar = '';
          this.paso.set('crear');
          return;
        }
        const volver = this.route.snapshot.queryParamMap.get('volver');
        this.router.navigateByUrl(volver || '/dashboard');
      },
      error: err => {
        this.error.set(mensajeError(err));
        this.cargando.set(false);
      }
    });
  }

  // ===================== Primer ingreso =====================
  requisitos(): { texto: string; ok: boolean }[] {
    const n = this.nueva;
    return [
      { texto: 'Al menos 8 caracteres', ok: n.length >= 8 },
      { texto: 'Una letra mayúscula y una minúscula', ok: /[A-ZÁÉÍÓÚÑ]/.test(n) && /[a-záéíóúñ]/.test(n) },
      { texto: 'Al menos un número', ok: /\d/.test(n) },
      { texto: 'Diferente a la contraseña temporal', ok: n.length > 0 && n !== this.clave },
      { texto: 'Las dos contraseñas coinciden', ok: n.length > 0 && n === this.confirmar }
    ];
  }

  todoCumple(): boolean {
    return this.requisitos().every(r => r.ok);
  }

  // 0 a 4 según qué tan segura es
  fuerza(): number {
    const n = this.nueva;
    if (!n) return 0;
    let puntos = 0;
    if (n.length >= 8) puntos++;
    if (/[A-Z]/.test(n) && /[a-z]/.test(n)) puntos++;
    if (/\d/.test(n)) puntos++;
    if (n.length >= 12 || /[^A-Za-z0-9]/.test(n)) puntos++;
    return puntos;
  }

  colorFuerza(): string {
    return ['#cbd5e1', '#ef4444', '#f59e0b', '#22c55e', '#0d9488'][this.fuerza()];
  }

  textoFuerza(): string {
    return ['', 'Débil', 'Regular', 'Buena', '¡Excelente!'][this.fuerza()];
  }

  crearPassword(): void {
    if (!this.todoCumple()) return;
    this.cargando.set(true);
    this.error.set(null);

    this.auth.primerIngreso(this.usuario.trim(), this.clave, this.nueva).subscribe({
      next: () => this.router.navigate(['/dashboard'], { queryParams: { bienvenida: 1 } }),
      error: err => {
        this.error.set(mensajeError(err));
        this.cargando.set(false);
      }
    });
  }

  volverAlLogin(): void {
    this.paso.set('login');
    this.clave = '';
    this.error.set(null);
  }
}
