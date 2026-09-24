export interface DashboardIndicadores {
  emergenciasPorEstado: Record<string, number>;
  emergenciasPorPrioridad: Record<string, number>;
  unidadesDisponibles: number;
  unidadesOcupadas: number;
  unidadesFueraDeServicio: number;
}
