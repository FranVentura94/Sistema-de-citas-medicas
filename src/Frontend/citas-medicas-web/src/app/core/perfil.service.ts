import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { API_URL } from './config';
import { AuthService } from './auth.service';
import { MiPerfil } from './models';
import { leerFecha } from './utils';

export type Momento = 'manana' | 'tarde' | 'noche';

// Perfil de quien inició sesión y la forma de saludarlo:
//   Médico      -> "Dr. Rivas" / "Dra. López"
//   Enfermería  -> "enfermera Ana" / "enfermero Luis"
//   Los demás   -> solo su nombre: "Laura"
@Injectable({ providedIn: 'root' })
export class PerfilService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);

  readonly perfil = signal<MiPerfil | null>(null);

  // Primer nombre (de la ficha o, si no tiene ficha, del usuario)
  readonly primerNombre = computed(() => {
    const p = this.perfil();
    if (p) return p.nombres.split(' ')[0];
    return (this.auth.usuario()?.nombreCompleto ?? '').replace(/^Dr(a|\(a\))?\.\s*/, '').split(' ')[0];
  });

  // Cómo llamar a la persona en el saludo
  readonly tratamiento = computed(() => {
    const p = this.perfil();
    const rol = this.auth.usuario()?.rol;
    const genero = p?.genero ?? null;

    if (rol === 'Medico') {
      const titulo = genero === 'F' ? 'Dra.' : genero === 'M' ? 'Dr.' : 'Dr(a).';
      const apellido = p?.apellidos?.split(' ')[0]
        ?? (this.auth.usuario()?.nombreCompleto ?? '').split(' ').filter(Boolean).pop();
      return `${titulo} ${apellido ?? this.primerNombre()}`;
    }

    if (rol === 'Enfermeria' && genero) {
      return `${genero === 'F' ? 'enfermera' : 'enfermero'} ${this.primerNombre()}`;
    }

    return this.primerNombre();
  });

  // Nombre completo con título, para el menú lateral
  readonly nombreMostrado = computed(() => {
    const p = this.perfil();
    const completo = p ? `${p.nombres} ${p.apellidos ?? ''}`.trim() : this.auth.usuario()?.nombreCompleto ?? '';
    if (this.auth.usuario()?.rol === 'Medico' && p) {
      return `${p.genero === 'F' ? 'Dra.' : p.genero === 'M' ? 'Dr.' : 'Dr(a).'} ${completo}`;
    }
    return completo;
  });

  // "Bienvenida" / "Bienvenido" / "Te damos la bienvenida"
  readonly bienvenida = computed(() => {
    const g = this.perfil()?.genero;
    return g === 'F' ? 'Bienvenida' : g === 'M' ? 'Bienvenido' : 'Te damos la bienvenida';
  });

  readonly esCumpleanos = computed(() => this.mismoDia(this.perfil()?.fechaNacimiento));

  // Años cumplidos en la clínica, solo el día exacto del aniversario
  readonly aniosEnClinica = computed(() => {
    const f = this.perfil()?.fechaIngreso;
    if (!f || !this.mismoDia(f)) return 0;
    return new Date().getFullYear() - leerFecha(f).getFullYear();
  });

  cargar(): void {
    if (!this.auth.estaAutenticado()) return;
    this.perfil.set(null); // no mostrar el nombre de la sesión anterior
    this.http.get<MiPerfil>(`${API_URL}/Empleados/mi-perfil`).subscribe({
      next: p => this.perfil.set(p),
      error: () => this.perfil.set(null) // sin ficha: se saluda con el nombre del usuario
    });
  }

  limpiar(): void {
    this.perfil.set(null);
  }

  static momento(fecha = new Date()): Momento {
    const h = fecha.getHours();
    return h >= 5 && h < 12 ? 'manana' : h >= 12 && h < 19 ? 'tarde' : 'noche';
  }

  private mismoDia(valor: string | null | undefined): boolean {
    if (!valor) return false;
    const f = leerFecha(valor);
    const hoy = new Date();
    return f.getDate() === hoy.getDate() && f.getMonth() === hoy.getMonth();
  }
}
