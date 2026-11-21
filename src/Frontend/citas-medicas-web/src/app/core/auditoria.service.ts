import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, forkJoin, map, of } from 'rxjs';
import { API_URL } from './config';

// ===================== Auditoría (solo Administrador) =====================
export type ServicioAuditoria = 'identity' | 'directory' | 'agenda' | 'clinical';
export const SERVICIOS: ServicioAuditoria[] = ['identity', 'directory', 'agenda', 'clinical'];

export interface RegistroAuditoria {
  id: number;
  fecha: string;
  servicio: string;
  categoria: 'Datos' | 'Seguridad' | 'Consulta';
  accion: string;
  descripcion: string;
  entidad: string | null;
  entidadId: string | null;
  cambios: string | null;
  usuarioId: number | null;
  usuarioLogin: string | null;
  usuarioNombre: string | null;
  rol: string | null;
  ip: string | null;
  dispositivo: string | null;
  ruta: string | null;
  exito: boolean;
}

export interface CambioCampo {
  campo: string;
  antes: unknown;
  despues: unknown;
}

export interface FiltroAuditoria {
  desde?: string | null;
  hasta?: string | null;
  categoria?: string | null;
  entidad?: string | null;
  entidadId?: string | null;
  usuarioId?: number | null;
  max?: number;
}

export interface ResultadoAuditoria {
  registros: RegistroAuditoria[];
  sinRespuesta: ServicioAuditoria[];   // servicios apagados o con error
}

// Una búsqueda en un servicio concreto (para el historial de un registro)
export interface ConsultaHistorial {
  servicio: ServicioAuditoria;
  entidad: string;
  entidadId: string | number;
}

@Injectable({ providedIn: 'root' })
export class AuditoriaService {
  private http = inject(HttpClient);

  // Busca en los 4 microservicios a la vez y junta todo, del más reciente al más antiguo
  buscar(filtro: FiltroAuditoria, servicios: ServicioAuditoria[] = SERVICIOS): Observable<ResultadoAuditoria> {
    const llamadas = servicios.map(s =>
      this.http.get<RegistroAuditoria[]>(`${API_URL}/auditoria/${s}`, { params: this.params(filtro) }).pipe(
        map(registros => ({ s, registros, ok: true })),
        catchError(() => of({ s, registros: [] as RegistroAuditoria[], ok: false }))
      ));

    return forkJoin(llamadas).pipe(map(respuestas => ({
      registros: respuestas.flatMap(r => r.registros).sort((a, b) => b.fecha.localeCompare(a.fecha)),
      sinRespuesta: respuestas.filter(r => !r.ok).map(r => r.s)
    })));
  }

  // Historial de un registro (ej. un paciente en Directory y en Clinical)
  historial(consultas: ConsultaHistorial[]): Observable<ResultadoAuditoria> {
    return forkJoin(consultas.map(c =>
      this.buscar({ entidad: c.entidad, entidadId: String(c.entidadId), max: 500 }, [c.servicio]))).pipe(
        map(partes => ({
          registros: partes.flatMap(p => p.registros).sort((a, b) => b.fecha.localeCompare(a.fecha)),
          sinRespuesta: [...new Set(partes.flatMap(p => p.sinRespuesta))]
        })));
  }

  private params(f: FiltroAuditoria): HttpParams {
    let p = new HttpParams();
    for (const [k, v] of Object.entries(f)) {
      if (v !== null && v !== undefined && v !== '') p = p.set(k, String(v));
    }
    return p;
  }
}

// ===================== Textos para mostrar =====================
export const NOMBRE_SERVICIO: Record<string, string> = {
  Identity: 'Accesos', Directory: 'Personal y pacientes', Agenda: 'Agenda', Clinical: 'Clínica'
};

export const NOMBRE_ENTIDAD: Record<string, string> = {
  Paciente: 'Paciente', Empleado: 'Empleado', Medico: 'Médico', Especialidad: 'Especialidad',
  MedicoEspecialidad: 'Especialidad del médico', EnfermeraMedico: 'Asignación de enfermería',
  Usuario: 'Usuario', Rol: 'Nivel de acceso', RolPermiso: 'Permiso del nivel',
  Cita: 'Cita', HorarioMedico: 'Horario', Consultorio: 'Consultorio',
  Atencion: 'Atención médica', SignosVitales: 'Signos vitales', NotaMedica: 'Nota médica',
  AtencionDiagnostico: 'Diagnóstico', Prescripcion: 'Receta', PrescripcionDetalle: 'Medicamento recetado',
  ExpedienteClinico: 'Expediente clínico', EstadoCita: 'Estado de cita', Diagnostico: 'Catálogo de diagnósticos',
  Medicamento: 'Catálogo de medicamentos', Permiso: 'Permiso'
};

export interface EstiloAccion { icono: string; color: string; fondo: string; texto: string }

export function estiloAccion(r: RegistroAuditoria): EstiloAccion {
  if (!r.exito) return { icono: 'pi pi-times-circle', color: '#b91c1c', fondo: '#fee2e2', texto: 'Fallido' };
  switch (r.accion) {
    case 'Crear': return { icono: 'pi pi-plus', color: '#15803d', fondo: '#dcfce7', texto: 'Creó' };
    case 'Editar': return { icono: 'pi pi-pencil', color: '#b45309', fondo: '#fef3c7', texto: 'Editó' };
    case 'Eliminar': return { icono: 'pi pi-trash', color: '#b91c1c', fondo: '#fee2e2', texto: 'Eliminó' };
    case 'Consultar': return { icono: 'pi pi-eye', color: '#1d4ed8', fondo: '#dbeafe', texto: 'Consultó' };
    case 'InicioSesion': return { icono: 'pi pi-sign-in', color: '#0f766e', fondo: '#ccfbf1', texto: 'Entró' };
    case 'CierreSesion': return { icono: 'pi pi-sign-out', color: '#475569', fondo: '#f1f5f9', texto: 'Salió' };
    case 'PrimerIngreso': return { icono: 'pi pi-shield', color: '#0f766e', fondo: '#ccfbf1', texto: 'Primer ingreso' };
    case 'ContrasenaTemporal': return { icono: 'pi pi-key', color: '#6d28d9', fondo: '#ede9fe', texto: 'Contraseña' };
    case 'AccesoDenegado': return { icono: 'pi pi-ban', color: '#b91c1c', fondo: '#fee2e2', texto: 'Sin permiso' };
    case 'SesionInvalida': return { icono: 'pi pi-exclamation-triangle', color: '#b91c1c', fondo: '#fee2e2', texto: 'Sesión inválida' };
    default: return { icono: 'pi pi-circle', color: '#475569', fondo: '#f1f5f9', texto: r.accion };
  }
}

// "Editó HorarioMedico #17" -> "Editó Horario #17"
export function descripcionAmigable(r: RegistroAuditoria): string {
  if (!r.entidad || !NOMBRE_ENTIDAD[r.entidad]) return r.descripcion;
  return r.descripcion.replace(` ${r.entidad} #`, ` ${NOMBRE_ENTIDAD[r.entidad]} #`);
}

export function leerCambios(r: RegistroAuditoria): CambioCampo[] {
  if (!r.cambios) return [];
  try { return JSON.parse(r.cambios) as CambioCampo[]; } catch { return []; }
}

// "PasswordTemporalVence" -> "Password temporal vence"
export function nombreCampo(campo: string): string {
  const texto = campo.replace(/ID$/, 'Id').replace(/([a-záéíóú])([A-Z])/g, '$1 $2').toLowerCase();
  return texto.charAt(0).toUpperCase() + texto.slice(1);
}

export function valorCampo(v: unknown): string {
  if (v === null || v === undefined || v === '') return '—';
  if (v === true) return 'Sí';
  if (v === false) return 'No';
  if (typeof v === 'string' && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(v)) {
    const [f, h] = v.split('T');
    const [y, m, d] = f.split('-');
    return h.startsWith('00:00:00') ? `${d}/${m}/${y}` : `${d}/${m}/${y} ${h.slice(0, 5)}`;
  }
  return String(v);
}
