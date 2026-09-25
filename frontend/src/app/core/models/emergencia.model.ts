export enum Prioridad {
  Baja = 0,
  Media = 1,
  Alta = 2,
  Critica = 3
}

export enum EstadoEmergencia {
  Reportada = 0,
  Validada = 1,
  Despachada = 2,
  EnRuta = 3,
  EnElLugar = 4,
  Atendida = 5,
  Cerrada = 6
}

// FR-105: mismos 4 valores que el backend (Domain.Enums.Ambito).
export enum Ambito {
  Terrestre = 0,
  Maritimo = 1,
  Aereo = 2,
  Mixto = 3
}

export interface TipoEmergenciaResumen {
  id: number;
  nombre: string;
  ambito: Ambito;
  icono: string | null;
  color: string | null;
}

// FR-104: ubicacion completa devuelta para usuarios autenticados (coordenada
// exacta, FR-115a) — no confundir con la ubicacion redondeada de la vista pública.
export interface Ubicacion {
  departamento: string | null;
  provincia: string | null;
  distrito: string | null;
  centroPoblado: string | null;
  direccion: string | null;
  referencia: string | null;
  latitud: number;
  longitud: number;
  ambito: Ambito;
  sinDireccionFormal: boolean;
}

export interface Emergencia {
  id: number;
  tipoEmergencia: TipoEmergenciaResumen | null;
  descripcion: string;
  ubicacion: Ubicacion;
  afectados: number;
  heridos: number;
  desaparecidos: number;
  fallecidos: number;
  evacuados: number;
  prioridad: Prioridad;
  estado: EstadoEmergencia;
  fechaHoraReporte: string;
  reportanteNombre: string;
  reportanteContacto?: string | null;
}

export interface EventoAuditoria {
  id: number;
  tipoEvento: string;
  estadoAnterior?: string | null;
  estadoNuevo?: string | null;
  usuarioId?: string | null;
  fechaHora: string;
}

export enum EstadoAsignacion {
  Despachada = 0,
  EnRuta = 1,
  EnElLugar = 2,
  Atendida = 3
}

export interface AsignacionResumen {
  id: number;
  unidadId: number;
  unidadIdentificador: string;
  unidadTipo: number;
  estadoAsignacion: EstadoAsignacion;
  fechaHoraAsignacion: string;
}

export interface EmergenciaDetalle extends Emergencia {
  asignaciones: AsignacionResumen[];
  timeline: EventoAuditoria[];
}

export interface UbicacionInput {
  departamento?: string | null;
  provincia?: string | null;
  distrito?: string | null;
  centroPoblado?: string | null;
  direccion?: string | null;
  referencia?: string | null;
  latitud: number;
  longitud: number;
  ambito: Ambito;
  sinDireccionFormal: boolean;
}

export interface CrearEmergenciaRequest {
  tipoEmergenciaId: number;
  descripcion: string;
  ubicacion: UbicacionInput;
  afectados: number;
  heridos: number;
  desaparecidos: number;
  fallecidos: number;
  evacuados: number;
  prioridad: Prioridad;
  reportanteNombre: string;
  reportanteContacto?: string;
}
