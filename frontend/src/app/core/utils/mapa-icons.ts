import * as L from 'leaflet';
import { iconSubset } from '../../icons/icon-subset';

// FR-106: icono propio por tipo de emergencia/unidad (no solo color). Los iconos
// de @coreui/icons vienen como [viewBox, svgPathMarkup]; se incrustan en un
// L.DivIcon con un fondo de color, en vez de depender de <c-icon> (Angular) que
// no puede renderizarse dentro de un marcador Leaflet.
export function crearIconoMarcador(nombreIcono: string | null | undefined, color: string | null | undefined): L.DivIcon {
  const entrada = nombreIcono ? (iconSubset as unknown as Record<string, [string, string]>)[nombreIcono] : undefined;
  const [viewBox, path] = entrada ?? iconSubset.cilWarning;
  const fondo = color ?? '#6c757d';

  const html = `
    <div style="
      background:${fondo};
      width:28px; height:28px; border-radius:50%;
      display:flex; align-items:center; justify-content:center;
      box-shadow:0 0 0 2px white, 0 1px 3px rgba(0,0,0,.4);
    ">
      <svg viewBox="${viewBox}" width="16" height="16" fill="white">${path}</svg>
    </div>
  `;

  return L.divIcon({
    html,
    className: 'sige-marcador-icono',
    iconSize: [28, 28],
    iconAnchor: [14, 14],
    popupAnchor: [0, -14]
  });
}
