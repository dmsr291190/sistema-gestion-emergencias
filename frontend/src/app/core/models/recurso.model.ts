export enum CategoriaRecurso {
  Equipos = 0,
  Herramientas = 1,
  Viveres = 2,
  Liquidos = 3,
  Estructuras = 4,
  MaterialMedico = 5,
  EquipoRescate = 6
}

export interface Recurso {
  id: number;
  codigo: string;
  nombre: string;
  categoria: CategoriaRecurso;
  unidadMedida: string | null;
  cantidad: number;
  cantidadDisponible: number;
  cantidadMinima: number;
  unidadRespuestaId: number;
  bajoStock: boolean;
}

export interface CrearRecursoRequest {
  codigo: string;
  nombre: string;
  categoria: CategoriaRecurso;
  unidadMedida?: string | null;
  cantidad: number;
  cantidadDisponible: number;
  cantidadMinima: number;
}
