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

export interface Emergencia {
  id: number;
  tipo: string;
  descripcion: string;
  latitud: number;
  longitud: number;
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

export interface AsignacionResumen {
  id: number;
  unidadId: number;
  unidadIdentificador: string;
  unidadTipo: number;
  estadoAsignacion: number;
  fechaHoraAsignacion: string;
}

export interface EmergenciaDetalle extends Emergencia {
  asignaciones: AsignacionResumen[];
  timeline: EventoAuditoria[];
}

export interface CrearEmergenciaRequest {
  tipo: string;
  descripcion: string;
  latitud: number;
  longitud: number;
  prioridad: Prioridad;
  reportanteNombre: string;
  reportanteContacto?: string;
}
