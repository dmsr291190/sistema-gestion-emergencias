// FR-117: roles ampliados (MVP + Ampliación Operativa Nacional).
export const ROLES_DISPONIBLES = [
  'Operador',
  'Supervisor',
  'Administrador',
  'CoordinadorLogistico',
  'JefeDeUnidad',
  'UnidadDeRespuesta',
  'Visualizador'
] as const;

export type Rol = (typeof ROLES_DISPONIBLES)[number];

export interface UsuarioAdmin {
  id: string;
  userName: string;
  email: string | null;
  nombreCompleto: string;
  institucionId: number | null;
  roles: string[];
  bloqueado: boolean;
  requiereCambioPassword: boolean;
  ultimoAcceso: string | null;
  intentosFallidos: number;
  creadoPorId: string | null;
  modificadoPorId: string | null;
}

export interface CrearUsuarioRequest {
  userName: string;
  nombreCompleto: string;
  password: string;
  institucionId: number | null;
  roles: string[];
}

export interface EditarUsuarioRequest {
  nombreCompleto: string;
  institucionId: number | null;
  roles: string[];
}
