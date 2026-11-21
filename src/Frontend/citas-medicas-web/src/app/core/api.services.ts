import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from './config';
import {
  AccesoAgenda, Atencion, Cita, Diagnostico, EspacioDisponible, Especialidad, EstadoCita,
  Expediente, HorarioMedico, Medicamento, Medico, MedicoPermitido, Paciente, ResumenCitas, SignosVitales, Triaje
} from './models';

// Una línea de la receta tal como la recibe MS_Clinical
export interface RecetaLinea {
  medicamentoId: number | null;
  dosis: string;
  frecuencia: string;
  duracion: string;
  via: string;
  indicaciones: string | null;
}

// Quita los parámetros vacíos para no mandar "?medicoId=null"
function params(valores: Record<string, string | number | null | undefined>): HttpParams {
  let p = new HttpParams();
  for (const [k, v] of Object.entries(valores)) {
    if (v !== null && v !== undefined && v !== '') p = p.set(k, String(v));
  }
  return p;
}

// ============ MS_Directory: pacientes, médicos y especialidades ============
@Injectable({ providedIn: 'root' })
export class DirectoryService {
  private http = inject(HttpClient);
  private url = API_URL;

  pacientes() { return this.http.get<Paciente[]>(`${this.url}/Pacientes`); }
  paciente(id: number) { return this.http.get<Paciente>(`${this.url}/Pacientes/${id}`); }
  crearPaciente(datos: Partial<Paciente>) { return this.http.post<Paciente>(`${this.url}/Pacientes`, datos); }
  actualizarPaciente(id: number, datos: Partial<Paciente>) { return this.http.put<Paciente>(`${this.url}/Pacientes/${id}`, datos); }

  medicos(especialidadId?: number | null) {
    return this.http.get<Medico[]>(`${this.url}/Medicos`, { params: params({ especialidadId }) });
  }
  especialidades() { return this.http.get<Especialidad[]>(`${this.url}/Especialidades`); }
}

// ============ MS_Agenda: horarios y citas ============
@Injectable({ providedIn: 'root' })
export class AgendaService {
  private http = inject(HttpClient);
  private url = `${API_URL}`;

  disponibilidad(medicoId: number, fecha: string) {
    return this.http.get<EspacioDisponible[]>(`${this.url}/Horarios/disponibilidad`, { params: params({ medicoId, fecha }) });
  }

  citas(filtro: { desde?: string; hasta?: string; medicoId?: number | null; pacienteId?: number | null; estado?: string | null }) {
    return this.http.get<Cita[]>(`${this.url}/Citas`, { params: params(filtro) });
  }
  misCitas(fecha: string) { return this.http.get<Cita[]>(`${this.url}/Citas/mis-citas`, { params: params({ fecha }) }); }
  crearCita(datos: { pacienteId: number; medicoId: number; especialidadId: number | null; fechaHoraInicio: string; motivoConsulta: string | null }) {
    return this.http.post<Cita>(`${this.url}/Citas`, datos);
  }
  cambiarEstado(citaId: number, estado: string, notas?: string | null) {
    return this.http.patch<Cita>(`${this.url}/Citas/${citaId}/estado`, { estado, notas });
  }
  estados() { return this.http.get<EstadoCita[]>(`${this.url}/Citas/estados`); }
  resumen(fecha: string, medicoId?: number | null) {
    return this.http.get<ResumenCitas>(`${this.url}/Citas/resumen`, { params: params({ fecha, medicoId }) });
  }
  cita(id: number) { return this.http.get<Cita>(`${this.url}/Citas/${id}`); }

  // Horarios de atención de los médicos (consulta para todos; editar es solo del Administrador)
  horarios(medicoId?: number | null) {
    return this.http.get<HorarioMedico[]>(`${this.url}/Horarios`, { params: params({ medicoId }) });
  }

  // ---------- Agendas compartidas ----------
  medicosPermitidos() { return this.http.get<MedicoPermitido[]>(`${this.url}/Accesos/medicos-permitidos`); }
  compartidosPorMi() { return this.http.get<AccesoAgenda[]>(`${this.url}/Accesos/compartidos`); }
  compartidosConmigo() { return this.http.get<AccesoAgenda[]>(`${this.url}/Accesos/recibidos`); }
  misPacientes() { return this.http.get<{ pacienteId: number; pacienteNombre: string }[]>(`${this.url}/Accesos/mis-pacientes`); }
  compartir(datos: { medicoAutorizadoId: number; alcance: 'TODA' | 'PACIENTES'; pacienteIds: number[]; fechaVencimiento: string | null; motivo: string | null }) {
    return this.http.post<AccesoAgenda>(`${this.url}/Accesos`, datos);
  }
  revocar(id: number) { return this.http.delete<AccesoAgenda>(`${this.url}/Accesos/${id}`); }
}

// ============ MS_Clinical: atención, expediente y catálogos ============
@Injectable({ providedIn: 'root' })
export class ClinicalService {
  private http = inject(HttpClient);
  private url = `${API_URL}/Atenciones`;

  iniciar(citaId: number) { return this.http.post<Atencion>(`${this.url}/iniciar`, { citaId }); }
  atencion(id: number) { return this.http.get<Atencion>(`${this.url}/${id}`); }
  porCita(citaId: number) { return this.http.get<Atencion>(`${this.url}/por-cita/${citaId}`); }

  guardarSignos(id: number, signos: SignosVitales) { return this.http.put<Atencion>(`${this.url}/${id}/signos-vitales`, signos); }
  agregarNota(id: number, tipo: string, contenido: string) { return this.http.post<Atencion>(`${this.url}/${id}/notas`, { tipo, contenido }); }
  agregarDiagnostico(id: number, d: { diagnosticoId: number; tipo: string; observacion: string | null }) {
    return this.http.post<Atencion>(`${this.url}/${id}/diagnosticos`, d);
  }
  quitarDiagnostico(id: number, diagnosticoId: number) { return this.http.delete<Atencion>(`${this.url}/${id}/diagnosticos/${diagnosticoId}`); }
  guardarReceta(id: number, receta: { indicacionesGenerales: string | null; detalles: RecetaLinea[] }) {
    return this.http.put<Atencion>(`${this.url}/${id}/prescripcion`, receta);
  }
  finalizar(id: number, observaciones: string | null) { return this.http.post<Atencion>(`${this.url}/${id}/finalizar`, { observaciones }); }

  // ---------- Triaje (preconsulta) ----------
  triaje(citaId: number) { return this.http.get<Triaje | null>(`${API_URL}/Triajes/cita/${citaId}`); }
  registrarTriaje(datos: SignosVitales & { citaId: number; notas: string | null }) {
    return this.http.post<Triaje>(`${API_URL}/Triajes`, datos);
  }

  expediente(pacienteId: number) { return this.http.get<Expediente>(`${API_URL}/expedientes/paciente/${pacienteId}`); }
  actualizarExpediente(pacienteId: number, datos: Partial<Expediente>) {
    return this.http.put<Expediente>(`${API_URL}/expedientes/paciente/${pacienteId}`, datos);
  }

  diagnosticos(buscar?: string) { return this.http.get<Diagnostico[]>(`${API_URL}/catalogos/diagnosticos`, { params: params({ buscar }) }); }
  medicamentos(buscar?: string) { return this.http.get<Medicamento[]>(`${API_URL}/catalogos/medicamentos`, { params: params({ buscar }) }); }
}
