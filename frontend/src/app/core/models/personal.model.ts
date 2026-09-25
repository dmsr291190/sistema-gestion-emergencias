export interface Personal {
  id: number;
  nombres: string;
  apellidos: string;
  documento: string;
  institucionId: number | null;
  especialidad: string | null;
  funcion: string | null;
  certificaciones: string | null;
  disponible: boolean;
  unidadRespuestaId: number;
}

export interface CrearPersonalRequest {
  nombres: string;
  apellidos: string;
  documento: string;
  institucionId: number | null;
  especialidad?: string | null;
  funcion?: string | null;
  certificaciones?: string | null;
  disponible: boolean;
}
