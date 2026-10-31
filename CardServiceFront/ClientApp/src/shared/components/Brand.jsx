import { Link as RouterLink } from 'react-router-dom';

export function Brand({ light = false }) {
  return <RouterLink to="/" className={`brand ${light ? 'brand-light' : ''}`} aria-label="Tuya Card Flow, inicio">
    <span className="brand-word">tuya<span className="brand-dot">.</span></span>
    <span className="brand-divider" /><span className="brand-caption">CARD FLOW</span>
  </RouterLink>;
}
