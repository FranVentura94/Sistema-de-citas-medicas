import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from './config';
import { Especialidad } from './models';

// ===================== Modelos del Panel de Administración =====================
export interface UsuarioAdmin {
  usuarioId: number;
  nombreUsuario: string;
  nombreCompleto: string;
  email: string;
  rolId: number;
  rol: string;
  medicoId: number | null;
  activo: boolean;
  tieneContrasena: boolean;          // false = aún no se le generó contraseña temporal
  debeCambiarPassword: boolean;      // true = tiene temporal y no ha entrado por primera vez
  passwordTemporalVence: string | null;
  fechaCreacion: string;
  ultimoAcceso: string | null;
}

export interface RolSistema {
  rolId: number;
  nombre: string;
  descripcion: string | null;
}

export interface PasswordTemporal {
  passwordTemporal: string;
  venceEn: string;
}

// Ficha del empleado tal como la devuelve MS_Directory
export interface Empleado {
  empleadoID: number;
  nombres: string;
  apellidos: string | null;
  nombreCompleto: string;
  identificacion: string | null;
  fechaNacimiento: string | null;
  genero: 'F' | 'M' | null;
  telefono: string | null;
  email: string | null;
  direccion: string | null;
  cargo: string;
  fechaIngreso: string | null;
  notas: string | null;
  emergenciaNombre: string | null;
  emergenciaParentesco: string | null;
  emergenciaTelefono: string | null;
  activo: boolean;
  fechaRegistro: string;
  medicoID: number | null;
  usuarioID: number | null;
  numeroColegiatura: string | null;
  especialidadIds: number[];
  especialidadPrincipalId: number | null;
  medicosAsignados: { medicoID: number; nombre: string }[];
}

// Lo que se envía al guardar la ficha (todo opcional menos el nombre)
export interface EmpleadoDatos {
  nombres: string;
  apellidos: string | null;
  identificacion: string | null;
  fechaNacimiento: string | null;
  genero: 'F' | 'M' | null;
  telefono: string | null;
  email: string | null;
  direccion: string | null;
  cargo: string;
  fechaIngreso: string | null;
  notas: string | null;
  emergenciaNombre: string | null;
  emergenciaParentesco: string | null;
  emergenciaTelefono: string | null;
  activo: boolean;
  numeroColegiatura: string | null;
  especialidadIds: number[];
  especialidadPrincipalId: number | null;
  medicosAsignados: number[];
}

export interface Horario {
  horarioMedicoId: number;
  medicoId: number;
  medicoNombre: string;
  consultorioId: number;
  consultorioNombre: string;
  diaSemana: number;
  diaNombre: string;
  horaInicio: string;
  horaFin: string;
  duracionCitaMinutos: number;
}

export interface Consultorio {
  consultorioId: number;
  codigo: string;
  nombre: string;
  ubicacion: string | null;
}

// ===================== Llamadas que solo puede hacer el Administrador =====================
@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);

  // ---------- Personal (MS_Directory) ----------
  empleados() { return this.http.get<Empleado[]>(`${API_URL}/Empleados`); }
  crearEmpleado(datos: EmpleadoDatos) { return this.http.post<Empleado>(`${API_URL}/Empleados`, datos); }
  actualizarEmpleado(id: number, datos: EmpleadoDatos) { return this.http.put<Empleado>(`${API_URL}/Empleados/${id}`, datos); }
  enlazarUsuario(empleadoId: number, usuarioID: number | null) {
    return this.http.put<Empleado>(`${API_URL}/Empleados/${empleadoId}/usuario`, { usuarioID });
  }
  crearEspecialidad(datos: { codigo: string; nombre: string; descripcion: string | null }) {
    return this.http.post<Especialidad>(`${API_URL}/Especialidades`, datos);
  }

  // ---------- Acceso al portal (MS_Identity) ----------
  usuarios() { return this.http.get<UsuarioAdmin[]>(`${API_URL}/Usuarios`); }
  roles() { return this.http.get<RolSistema[]>(`${API_URL}/Usuarios/roles`); }
  crearUsuario(datos: { nombreUsuario: string; nombreCompleto: string; email: string | null; rolId: number; medicoId: number | null }) {
    return this.http.post<UsuarioAdmin>(`${API_URL}/Usuarios`, datos);
  }
  actualizarUsuario(id: number, datos: { nombreCompleto: string; email: string | null; rolId: number; medicoId: number | null; activo: boolean }) {
    return this.http.put<UsuarioAdmin>(`${API_URL}/Usuarios/${id}`, datos);
  }
  generarPasswordTemporal(id: number) {
    return this.http.post<PasswordTemporal>(`${API_URL}/Usuarios/${id}/password-temporal`, {});
  }

  // ---------- Horarios y consultorios (MS_Agenda) ----------
  horarios(medicoId?: number | null) {
    const params = medicoId ? new HttpParams().set('medicoId', medicoId) : undefined;
    return this.http.get<Horario[]>(`${API_URL}/Horarios`, { params });
  }
  consultorios() { return this.http.get<Consultorio[]>(`${API_URL}/Horarios/consultorios`); }
  crearHorario(datos: { medicoId: number; consultorioId: number; diaSemana: number; horaInicio: string; horaFin: string; duracionCitaMinutos: number }) {
    return this.http.post<Horario>(`${API_URL}/Horarios`, datos);
  }
  eliminarHorario(id: number) { return this.http.delete<{ mensaje: string }>(`${API_URL}/Horarios/${id}`); }
}
