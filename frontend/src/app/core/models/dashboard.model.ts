export interface DashboardIndicadores {
  emergenciasPorEstado: Record<string, number>;
  emergenciasPorPrioridad: Record<string, number>;
  unidadesDisponibles: number;
  unidadesOcupadas: number;
  unidadesFueraDeServicio: number;
  // FR-127 (ampliación 002, US8)
  emergenciasPorAmbito: Record<string, number>;
  tiempoPromedioAtencionMinutos: number | null;
  personalDesplegado: number;
  recursosMovilizados: number;
}
