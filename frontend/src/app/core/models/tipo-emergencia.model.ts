import { Ambito, Prioridad } from './emergencia.model';

export interface TipoEmergencia {
  id: number;
  nombre: string;
  ambito: Ambito;
  icono: string | null;
  color: string | null;
  prioridadPorDefecto: Prioridad;
  activo: boolean;
}

export interface CrearTipoEmergenciaRequest {
  nombre: string;
  ambito: Ambito;
  icono?: string | null;
  color?: string | null;
  prioridadPorDefecto: Prioridad;
}

export interface EditarTipoEmergenciaRequest {
  nombre: string;
  ambito: Ambito;
  icono?: string | null;
  color?: string | null;
  prioridadPorDefecto: Prioridad;
  activo: boolean;
}
