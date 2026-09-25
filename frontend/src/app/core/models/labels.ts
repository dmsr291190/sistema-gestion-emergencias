// FR-023: mismo texto y mismo color para cada valor de estado o prioridad en todas
// las pantallas (listado, mapa, detalle, dashboard) — un solo lugar en vez de un
// objeto duplicado por componente. Los valores de color son clases utilitarias de
// Bootstrap/CoreUI (text-success, bg-danger, etc.), consumidas con [ngClass].
export const ESTADO_EMERGENCIA_LABEL: Record<number, string> = {
  0: 'Reportada',
  1: 'Validada',
  2: 'Despachada',
  3: 'En ruta',
  4: 'En el lugar',
  5: 'Atendida',
  6: 'Cerrada'
};

export const ESTADO_EMERGENCIA_COLOR: Record<number, string> = {
  0: 'secondary',
  1: 'info',
  2: 'primary',
  3: 'primary',
  4: 'primary',
  5: 'success',
  6: 'dark'
};

export const PRIORIDAD_LABEL: Record<number, string> = {
  0: 'Baja',
  1: 'Media',
  2: 'Alta',
  3: 'Critica'
};

export const PRIORIDAD_COLOR: Record<number, string> = {
  0: 'secondary',
  1: 'info',
  2: 'warning',
  3: 'danger'
};

// El endpoint del dashboard (GET /api/Dashboard/indicadores) agrupa por el nombre del
// enum (ej. "Atendida"), no por su valor numerico — se necesita el mismo color por
// nombre para que el dashboard use la paleta consistente de FR-023.
export const ESTADO_EMERGENCIA_COLOR_POR_NOMBRE: Record<string, string> = {
  Reportada: 'secondary',
  Validada: 'info',
  Despachada: 'primary',
  EnRuta: 'primary',
  EnElLugar: 'primary',
  Atendida: 'success',
  Cerrada: 'dark'
};

export const PRIORIDAD_COLOR_POR_NOMBRE: Record<string, string> = {
  Baja: 'secondary',
  Media: 'info',
  Alta: 'warning',
  Critica: 'danger'
};

export const ESTADO_ASIGNACION_LABEL: Record<number, string> = {
  0: 'Despachada',
  1: 'En ruta',
  2: 'En el lugar',
  3: 'Atendida'
};

export const TIPO_UNIDAD_LABEL: Record<number, string> = {
  0: 'Ambulancia',
  1: 'Bomberos',
  2: 'Patrullero',
  3: 'Vehículo de rescate',
  4: 'Camión logístico',
  5: 'Embarcación',
  6: 'Helicóptero',
  7: 'Puesto de comando'
};

export const ESTADO_OPERATIVO_UNIDAD_LABEL: Record<number, string> = {
  0: 'Disponible',
  1: 'Ocupada',
  2: 'Fuera de servicio'
};
