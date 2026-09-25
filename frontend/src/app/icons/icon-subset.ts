import {
  cilAirplaneMode,
  cilBank,
  cilBell,
  cilBoatAlt,
  cilBuilding,
  cilCarAlt,
  cilFire,
  cilGroup,
  cilHome,
  cilHospital,
  cilLifeRing,
  cilLocationPin,
  cilMap,
  cilMedicalCross,
  cilMoon,
  cilRain,
  cilSettings,
  cilShieldAlt,
  cilSun,
  cilTerrain,
  cilTruck,
  cilUser,
  cilWarning
} from '@coreui/icons';

// research.md §4: catálogo de íconos por tipo de emergencia/unidad (sección 6 del
// documento de origen), usando @coreui/icons en vez de subir archivos propios.
export const iconSubset = {
  cilAirplaneMode,
  cilBank,
  cilBell,
  cilBoatAlt,
  cilBuilding,
  cilCarAlt,
  cilFire,
  cilGroup,
  cilHome,
  cilHospital,
  cilLifeRing,
  cilLocationPin,
  cilMap,
  cilMedicalCross,
  cilMoon,
  cilRain,
  cilSettings,
  cilShieldAlt,
  cilSun,
  cilTerrain,
  cilTruck,
  cilUser,
  cilWarning
};

// Ícono sugerido por tipo de emergencia (sección 6.1 del documento de origen).
// Sirve como default al crear un TipoEmergencia nuevo; el Administrador puede
// elegir otro del mismo subconjunto.
export const ICONOS_TIPO_EMERGENCIA: Record<string, string> = {
  Incendio: 'cilFire',
  EmergenciaMedica: 'cilMedicalCross',
  Accidente: 'cilCarAlt',
  EmergenciaMaritima: 'cilBoatAlt',
  HuaicoDerrumbe: 'cilTerrain',
  Inundacion: 'cilRain',
  MaterialPeligroso: 'cilWarning',
  EmergenciaAerea: 'cilAirplaneMode',
  BusquedaRescate: 'cilLifeRing'
};

// Ícono por tipo de unidad (sección 6.2 del documento de origen).
export const ICONOS_TIPO_UNIDAD: Record<string, string> = {
  Ambulancia: 'cilMedicalCross',
  Bomberos: 'cilFire',
  Patrullero: 'cilCarAlt',
  VehiculoRescate: 'cilLifeRing',
  CamionLogistico: 'cilTruck',
  Embarcacion: 'cilBoatAlt',
  Helicoptero: 'cilAirplaneMode',
  PuestoDeComando: 'cilBuilding'
};
