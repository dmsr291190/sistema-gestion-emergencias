import { Ambito, Prioridad, EstadoEmergencia } from './emergencia.model';

// FR-113, FR-114, FR-115: shape deliberadamente distinto (y mas chico) que
// Emergencia -- nunca debe crecer para incluir datos sensibles.
export interface UbicacionPublica {
  departamento: string | null;
  provincia: string | null;
  distrito: string | null;
  centroPoblado: string | null;
  latitudAproximada: number;
  longitudAproximada: number;
  ambito: Ambito;
}

export interface EmergenciaPublica {
  codigo: number;
  tipoNombre: string | null;
  tipoIcono: string | null;
  tipoColor: string | null;
  prioridad: Prioridad;
  estado: EstadoEmergencia;
  ubicacion: UbicacionPublica;
  ultimaActualizacion: string;
}

export interface EventoPublico {
  tipoEvento: string;
  estadoNuevo: string | null;
  fechaHora: string;
}

export interface EmergenciaPublicaDetalle extends EmergenciaPublica {
  timeline: EventoPublico[];
}
