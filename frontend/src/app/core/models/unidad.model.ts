export enum TipoUnidad {
  Ambulancia = 0,
  Bomberos = 1,
  Patrullero = 2,
  // Sección 6.2 del documento de origen (ampliación 002).
  VehiculoRescate = 3,
  CamionLogistico = 4,
  Embarcacion = 5,
  Helicoptero = 6,
  PuestoDeComando = 7
}

export enum EstadoOperativoUnidad {
  Disponible = 0,
  Ocupada = 1,
  FueraDeServicio = 2
}

export interface Unidad {
  id: number;
  tipo: TipoUnidad;
  identificador: string;
  estadoOperativo: EstadoOperativoUnidad;
  latitud?: number | null;
  longitud?: number | null;
}

export interface CrearUnidadRequest {
  tipo: TipoUnidad;
  identificador: string;
  latitud?: number | null;
  longitud?: number | null;
}
