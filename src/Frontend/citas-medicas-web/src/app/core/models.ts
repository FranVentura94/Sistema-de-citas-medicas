// ===================== Identity =====================
export interface UsuarioInfo {
  usuarioId: number;
  nombreUsuario: string;
  nombreCompleto: string;
  email: string;
  rol: string;
  medicoId: number | null;
  permisos: string[];
}

// Si requiereCambioPassword = true, no hay token: la persona debe crear su contraseña primero
export interface LoginResponse {
  token: string | null;
  expiraEn: string | null;
  usuario: UsuarioInfo | null;
  requiereCambioPassword: boolean;
}

// Datos de quien inició sesión, para saludarlo por su nombre
export interface MiPerfil {
  empleadoID: number;
  nombres: string;
  apellidos: string | null;
  genero: 'F' | 'M' | null;
  cargo: string;
  fechaNacimiento: string | null;
  fechaIngreso: string | null;
  medicosAsignados: { medicoID: number; nombre: string }[];
}

// ===================== Directory =====================
export interface Paciente {
  pacienteID: number;
  codigoPaciente: string;
  tipoDocumento: string;
  numeroDocumento: string;
  nombres: string;
  apellidos: string;
  fechaNacimiento: string;
  sexo: string;
  estadoCivil: string | null;
  telefono: string | null;
  telefonoSecundario: string | null;
  email: string | null;
  direccion: string | null;
  ciudad: string | null;
  pais: string | null;
  ocupacion: string | null;
  tipoSangre: string | null;
  activo: boolean;
  fechaRegistro: string;
}

export interface Especialidad {
  especialidadID: number;
  codigo: string;
  nombre: string;
  descripcion: string | null;
}

export interface Medico {
  medicoID: number;
  usuarioID: number | null;
  nombres: string;
  apellidos: string;
  nombreCompleto: string;
  numeroColegiatura: string;
  telefono: string;
  email: string;
  activo: boolean;
  especialidades: { especialidadID: number; nombre: string; principal: boolean }[];
}

// ===================== Agenda =====================
export interface Cita {
  citaId: number;
  pacienteId: number;
  pacienteNombre: string;
  pacienteDocumento: string;
  medicoId: number;
  medicoNombre: string;
  especialidadId: number | null;
  especialidadNombre: string | null;
  consultorioId: number;
  consultorioNombre: string;
  fechaHoraInicio: string;
  fechaHoraFin: string;
  estadoCodigo: string;
  estadoNombre: string;
  estadoColor: string;
  motivoConsulta: string | null;
  notas: string | null;
  fechaCreacion: string;
}

export interface EspacioDisponible {
  inicio: string;
  fin: string;
  disponible: boolean;
  consultorioId: number;
  consultorioNombre: string;
}

export interface EstadoCita {
  codigo: string;
  nombre: string;
  color: string;
}

export interface ResumenCitas {
  fecha: string;
  total: number;
  porEstado: { codigo: string; nombre: string; color: string; cantidad: number }[];
}

// ===================== Clinical =====================
export interface SignosVitales {
  presionSistolica: number | null;
  presionDiastolica: number | null;
  frecuenciaCardiaca: number | null;
  frecuenciaRespiratoria: number | null;
  temperatura: number | null;
  saturacionOxigeno: number | null;
  peso: number | null;
  talla: number | null;
  imc?: number | null;
  fechaRegistro?: string;
}

export interface NotaMedica {
  notaMedicaId: number;
  tipo: string;
  contenido: string;
  autor: string | null;
  fechaRegistro: string;
}

export interface AtencionDiagnostico {
  diagnosticoId: number;
  codigo: string;
  descripcion: string;
  tipo: string;
  observacion: string | null;
}

export interface RecetaDetalle {
  medicamentoId: number;
  medicamento: string;
  presentacion: string;
  concentracion: string;
  dosis: string;
  frecuencia: string;
  duracion: string;
  via: string;
  indicaciones: string | null;
}

export interface Receta {
  prescripcionId: number;
  fecha: string;
  indicacionesGenerales: string | null;
  detalles: RecetaDetalle[];
}

export interface Atencion {
  atencionId: number;
  citaId: number;
  pacienteId: number;
  pacienteNombre: string;
  numeroExpediente: string;
  medicoId: number;
  medicoNombre: string;
  especialidadNombre: string | null;
  fechaInicio: string;
  fechaFin: string | null;
  estado: 'EN_PROCESO' | 'FINALIZADA';
  motivoConsulta: string | null;
  observaciones: string | null;
  signosVitales: SignosVitales | null;
  notas: NotaMedica[];
  diagnosticos: AtencionDiagnostico[];
  prescripcion: Receta | null;
  signosTomadosPor?: string | null;
}

export interface AtencionResumen {
  atencionId: number;
  citaId: number;
  fechaInicio: string;
  medicoNombre: string;
  especialidadNombre: string | null;
  estado: string;
  motivoConsulta: string | null;
  diagnosticos: string[];
}

export interface Expediente {
  expedienteClinicoId: number;
  pacienteId: number;
  pacienteNombre: string;
  numeroExpediente: string;
  fechaApertura: string;
  alergias: string | null;
  antecedentesPersonales: string | null;
  antecedentesFamiliares: string | null;
  observaciones: string | null;
  atenciones: AtencionResumen[];
}

export interface Diagnostico {
  diagnosticoId: number;
  codigo: string;
  descripcion: string;
}

export interface Medicamento {
  medicamentoId: number;
  nombre: string;
  principioActivo: string;
  presentacion: string;
  concentracion: string;
}

// ===================== Triaje (preconsulta de enfermería) =====================
export interface Triaje {
  triajeId: number;
  citaId: number;
  pacienteId: number;
  pacienteNombre: string;
  medicoId: number;
  medicoNombre: string;
  signos: SignosVitales;
  notas: string | null;
  registradoPorNombre: string;
  registradoPorRol: string;
  fechaRegistro: string;
}

// ===================== Agendas compartidas =====================
export interface AccesoAgenda {
  accesoAgendaId: number;
  medicoPropietarioId: number;
  medicoPropietarioNombre: string;
  medicoAutorizadoId: number;
  medicoAutorizadoNombre: string;
  alcance: 'TODA' | 'PACIENTES';
  fechaVencimiento: string | null;
  motivo: string | null;
  estado: 'VIGENTE' | 'VENCIDO' | 'REVOCADO';
  fechaCreacion: string;
  fechaRevocacion: string | null;
  pacientes: { pacienteId: number; pacienteNombre: string }[];
}

// Agendas que puede ver/gestionar quien inició sesión (lista vacía = todas)
export interface MedicoPermitido {
  medicoId: number;
  propio: boolean;
  agendaCompleta: boolean;
  pacienteIds: number[];
}

export interface HorarioMedico {
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

