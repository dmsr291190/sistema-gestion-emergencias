export enum TipoUnidad {
  Ambulancia = 0,
  Bomberos = 1,
  Patrullero = 2
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
