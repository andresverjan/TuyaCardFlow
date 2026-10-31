import { Link as RouterLink } from 'react-router-dom';
import { Box, Link, Stack, Typography } from '@mui/material';
import NorthEastRounded from '@mui/icons-material/NorthEastRounded';
import CheckRounded from '@mui/icons-material/CheckRounded';
import LockOutlined from '@mui/icons-material/LockOutlined';
import { Brand } from '../components/Brand';

export function AccessLayout({ children, mode }) {
  const register = mode === 'register';
  return <div className="access-page">
    <header className="access-header"><Brand /><Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
      <Typography variant="body2" className="header-question">{register ? '¿Ya tienes un perfil?' : '¿Aún no tienes un perfil?'}</Typography>
      <Link component={RouterLink} to={register ? '/ingresar' : '/crear-perfil'}>
        {register ? 'Iniciar sesión' : 'Crear perfil'} <NorthEastRounded sx={{ fontSize: 15, verticalAlign: 'middle' }} />
      </Link>
    </Stack></header>
    <main className="access-main">
      <aside className="story-panel">
        <div className="story-text"><span className="eyebrow"><span className="tiny-dot" /> TU ESPACIO EMPIEZA AQUÍ</span>
          <Typography component="h1" sx={{ fontSize: { xs: '2.2rem', md: '3.45rem' }, fontWeight: 650, lineHeight: 1.1, letterSpacing: '-.06em', mt: 3, mb: 2.5 }}>
            Tu historia<br />puede ser tuya<span className="accent-text">.</span>
          </Typography>
          <Typography sx={{ color: '#70707d', maxWidth: 310 }}>Un perfil, tus tarjetas y tus próximos pasos, en un espacio hecho para ti.</Typography>
        </div>
        <div className="card-scene" aria-hidden="true">
          <div className="scene-orbit" /><div className="scene-orbit orbit-two" />
          <div className="decorative-card card-back"><span>tuya.</span><div className="card-lines" /></div>
          <div className="decorative-card card-front"><div className="card-top"><span>tuya.</span><NorthEastRounded /></div>
            <div className="card-chip"><i /><i /><i /></div><div className="card-number">•••• &nbsp; •••• &nbsp; •••• &nbsp; 2026</div>
            <div className="card-bottom"><span>HECHA PARA TI</span><span className="card-circles"><i /><i /></span></div>
          </div>
          <div className="floating-label"><span className="floating-check"><CheckRounded sx={{ fontSize: 18 }} /></span><div>Tu siguiente paso<span>Empieza contigo</span></div></div>
        </div>
        <div className="story-bottom"><LockOutlined sx={{ fontSize: 17 }} /><span>Tu información, en tu espacio.</span><span className="story-line" /></div>
      </aside>
      <section className="form-panel"><Box sx={{ width: '100%', maxWidth: 420 }}>{children}</Box></section>
    </main>
    <footer className="access-footer"><span>© {new Date().getFullYear()} Tuya Card Flow</span><span>Hecho para acompañarte, paso a paso.</span></footer>
  </div>;
}
