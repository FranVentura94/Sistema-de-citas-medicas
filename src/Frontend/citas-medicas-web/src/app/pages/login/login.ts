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
import { mensajeError } from '../../core/utils';

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
        <form class="caja" (ngSubmit)="ingresar()" #f="ngForm">
          <h2>Bienvenido 👋</h2>
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

  protected usuario = '';
  protected clave = '';
  protected cargando = signal(false);
  protected error = signal<string | null>(null);

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
      next: () => {
        const volver = this.route.snapshot.queryParamMap.get('volver');
        this.router.navigateByUrl(volver || '/dashboard');
      },
      error: err => {
        this.error.set(mensajeError(err));
        this.cargando.set(false);
      }
    });
  }
}
