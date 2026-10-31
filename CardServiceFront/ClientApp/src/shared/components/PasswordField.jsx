import { useState } from 'react';
import { IconButton, InputAdornment, TextField } from '@mui/material';
import VisibilityOutlined from '@mui/icons-material/VisibilityOutlined';
import VisibilityOffOutlined from '@mui/icons-material/VisibilityOffOutlined';

export function PasswordField({ label = 'Contraseña', ...props }) {
  const [visible, setVisible] = useState(false);
  return <TextField {...props} label={label} type={visible ? 'text' : 'password'} fullWidth
    slotProps={{ input: { endAdornment: <InputAdornment position="end">
      <IconButton aria-label={visible ? 'Ocultar contraseña' : 'Mostrar contraseña'} edge="end" size="small"
        onClick={() => setVisible(!visible)} onMouseDown={event => event.preventDefault()}>
        {visible ? <VisibilityOffOutlined fontSize="small" /> : <VisibilityOutlined fontSize="small" />}
      </IconButton>
    </InputAdornment> } }} />;
}
