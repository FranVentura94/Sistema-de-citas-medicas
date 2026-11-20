import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { API_URL, Rol } from './config';
import { LoginResponse, UsuarioInfo } from './models';

const CLAVE_SESION = 'medicitas.sesion';

interface Sesion {
  token: string;
  expiraEn: string;
  usuario: UsuarioInfo;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private sesion = signal<Sesion | null>(this.leerSesion());

  readonly usuario = computed(() => this.sesion()?.usuario ?? null);
  readonly token = computed(() => this.sesion()?.token ?? null);

  login(nombreUsuario: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${API_URL}/Auth/login`, { nombreUsuario, password }).pipe(
      tap(r => {
        const sesion: Sesion = { token: r.token, expiraEn: r.expiraEn, usuario: r.usuario };
        localStorage.setItem(CLAVE_SESION, JSON.stringify(sesion));
        this.sesion.set(sesion);
      })
    );
  }

  logout(expirada = false): void {
    localStorage.removeItem(CLAVE_SESION);
    this.sesion.set(null);
    this.router.navigate(['/login'], expirada ? { queryParams: { expirada: 1 } } : {});
  }

  estaAutenticado(): boolean {
    const s = this.sesion();
    return !!s && new Date(s.expiraEn).getTime() > Date.now();
  }

  tieneRol(...roles: Rol[]): boolean {
    const rol = this.usuario()?.rol as Rol | undefined;
    return !!rol && roles.includes(rol);
  }

  private leerSesion(): Sesion | null {
    try {
      const raw = localStorage.getItem(CLAVE_SESION);
      return raw ? (JSON.parse(raw) as Sesion) : null;
    } catch {
      return null;
    }
  }
}
