import { HttpErrorResponse } from '@angular/common/http';

// Convierte cualquier error del backend en un mensaje entendible para el usuario
export function mensajeError(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) return 'No se pudo conectar con el servidor. Verifica que el sistema esté encendido.';
    if (err.error?.mensaje) return err.error.mensaje;
    if (err.status === 403) return 'No tienes permiso para realizar esta acción.';
    if (err.status === 404) return 'No se encontró la información solicitada.';
  }
  return 'Ocurrió un error inesperado. Intenta de nuevo.';
}

const dos = (n: number) => n.toString().padStart(2, '0');

// Fecha para enviar al backend: 2026-11-20
export function fechaParam(d: Date): string {
  return `${d.getFullYear()}-${dos(d.getMonth() + 1)}-${dos(d.getDate())}`;
}

// Fecha y hora local para enviar al backend: 2026-11-20T09:00:00
export function fechaHoraParam(d: Date): string {
  return `${fechaParam(d)}T${dos(d.getHours())}:${dos(d.getMinutes())}:00`;
}

// Lee una fecha del backend sin que el navegador la cambie de zona horaria
export function leerFecha(valor: string): Date {
  const [f, h = '00:00:00'] = valor.split('T');
  const [y, m, d] = f.split('-').map(Number);
  const [hh, mm] = h.split(':').map(Number);
  return new Date(y, m - 1, d, hh, mm);
}

export function edad(fechaNacimiento: string): number {
  const n = leerFecha(fechaNacimiento);
  const hoy = new Date();
  let e = hoy.getFullYear() - n.getFullYear();
  if (hoy.getMonth() < n.getMonth() || (hoy.getMonth() === n.getMonth() && hoy.getDate() < n.getDate())) e--;
  return e;
}

export function iniciales(nombre: string): string {
  return nombre
    .replace(/^Dr\(a\)\.\s*/, '')
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map(p => p[0].toUpperCase())
    .join('');
}

export function saludo(): string {
  const h = new Date().getHours();
  return h < 12 ? 'Buenos días' : h < 19 ? 'Buenas tardes' : 'Buenas noches';
}


// Nombre del rol para mostrar en pantalla
export function nombreRol(rol: string | null | undefined): string {
  const nombres: Record<string, string> = { Recepcion: 'Recepción', Medico: 'Médico', Enfermeria: 'Enfermería' };
  return rol ? nombres[rol] ?? rol : '';
}

// Estados de cita a los que se puede pasar desde cada estado (igual que en MS_Agenda)
export const TRANSICIONES: Record<string, string[]> = {
  PROGRAMADA: ['CONFIRMADA', 'EN_ESPERA', 'CANCELADA', 'NO_ASISTIO'],
  CONFIRMADA: ['EN_ESPERA', 'CANCELADA', 'NO_ASISTIO'],
  EN_ESPERA: ['CANCELADA', 'NO_ASISTIO'],
  EN_ATENCION: [],
  ATENDIDA: [],
  CANCELADA: [],
  NO_ASISTIO: []
};


// "viernes, 20 de noviembre" -> "Viernes, 20 de noviembre"
export function mayuscula(texto: string): string {
  return texto.charAt(0).toUpperCase() + texto.slice(1);
}
