import { useCallback, useEffect, useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Alert, Box, Button, Card, CardContent, CircularProgress, Container, IconButton, Paper, Stack, TextField, Typography,
} from '@mui/material';
import AddCardRounded from '@mui/icons-material/AddCardRounded';
import ArrowBackRounded from '@mui/icons-material/ArrowBackRounded';
import CreditCardRounded from '@mui/icons-material/CreditCardRounded';
import DeleteOutlineRounded from '@mui/icons-material/DeleteOutlineRounded';
import EditOutlined from '@mui/icons-material/EditOutlined';
import { Brand } from '../../../shared/components/Brand';
import { useAuth } from '../../auth/context/AuthContext';
import { cardsApi } from '../services/cardsApi';

const emptyForm = { holderName: '', brand: '', last4: '', expirationMonth: '', expirationYear: '' };
const fields = [
  { name: 'holderName', label: 'Nombre del titular', maxLength: 150, wide: true },
  { name: 'brand', label: 'Marca (Visa, Mastercard…)', maxLength: 50 },
  { name: 'last4', label: 'Últimos 4 dígitos', maxLength: 4, inputMode: 'numeric' },
  { name: 'expirationMonth', label: 'Mes de vencimiento', type: 'number', min: 1, max: 12 },
  { name: 'expirationYear', label: 'Año de vencimiento', type: 'number', min: 2000, max: 9999 },
];

function cardFormFrom(card) {
  return card ? {
    holderName: card.holderName, brand: card.brand, last4: card.last4,
    expirationMonth: String(card.expirationMonth), expirationYear: String(card.expirationYear),
  } : emptyForm;
}

export function CardsPage() {
  const { user } = useAuth();
  const [cards, setCards] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [formError, setFormError] = useState('');
  const [fieldError, setFieldError] = useState('');
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(null);
  const [notice, setNotice] = useState('');

  const loadCards = useCallback(async () => {
    setLoading(true); setLoadError('');
    try { setCards(await cardsApi.list(user.id)); }
    catch (error) { setLoadError(error.message); }
    finally { setLoading(false); }
  }, [user.id]);

  useEffect(() => { loadCards(); }, [loadCards]);

  const openCreate = () => { setEditing(null); setForm(emptyForm); setFieldError(''); setFormError(''); setDialogOpen(true); };
  const openEdit = card => { setEditing(card); setForm(cardFormFrom(card)); setFieldError(''); setFormError(''); setDialogOpen(true); };
  const closeDialog = () => { if (!saving) setDialogOpen(false); };

  const saveCard = async event => {
    event.preventDefault(); setFormError(''); setFieldError('');
    if (!/^\d{4}$/.test(form.last4)) { setFieldError('last4'); setFormError('Escribe exactamente los últimos cuatro dígitos.'); return; }
    const month = Number(form.expirationMonth); const year = Number(form.expirationYear);
    if (!Number.isInteger(month) || month < 1 || month > 12) { setFieldError('expirationMonth'); setFormError('El mes debe estar entre 1 y 12.'); return; }
    if (!Number.isInteger(year) || year < 2000 || year > 9999) { setFieldError('expirationYear'); setFormError('Escribe un año válido.'); return; }
    const payload = { ...form, expirationMonth: month, expirationYear: year };
    setSaving(true);
    try {
      if (editing) await cardsApi.update(editing.id, payload);
      else await cardsApi.create({ ...payload, userId: user.id });
      setDialogOpen(false); setNotice(editing ? 'Tarjeta actualizada.' : 'Tarjeta agregada.'); await loadCards();
    } catch (error) { setFormError(error.message); }
    finally { setSaving(false); }
  };

  const deleteCard = async () => {
    if (!deleting) return;
    setFormError('');
    try { await cardsApi.remove(deleting.id); setDeleting(null); setNotice('Tarjeta eliminada.'); await loadCards(); }
    catch (error) { setFormError(error.message); }
  };

  return <Box sx={{ minHeight: '100vh', background: '#f7f8fa' }}>
    <header className="profile-header"><Brand /><Button component={RouterLink} to="/mi-perfil" color="inherit" startIcon={<ArrowBackRounded />}>Mi perfil</Button></header>
    <Container maxWidth="md" sx={{ py: { xs: 5, md: 9 } }}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ alignItems: { sm: 'center' }, justifyContent: 'space-between', mb: 4 }}>
        <Box><Typography className="eyebrow" sx={{ color: 'primary.main', mb: 2 }}>TU BILLETERA</Typography>
          <Typography variant="h1" component="h1">Mis tarjetas<span className="accent-text">.</span></Typography>
          <Typography color="text.secondary" sx={{ mt: 1.5 }}>Administra las tarjetas asociadas a tu perfil.</Typography></Box>
        <Button variant="contained" startIcon={<AddCardRounded />} onClick={openCreate}>Agregar tarjeta</Button>
      </Stack>
      {notice && <Alert severity="success" onClose={() => setNotice('')} sx={{ mb: 2 }}>{notice}</Alert>}
      {loadError && <Alert severity="error" action={<Button color="inherit" size="small" onClick={loadCards}>Reintentar</Button>} sx={{ mb: 2 }}>{loadError}</Alert>}
      {loading ? <Stack alignItems="center" sx={{ py: 8 }}><CircularProgress size={28} /></Stack>
        : cards.length === 0 ? <Paper elevation={0} sx={{ border: '1px solid #e8e8ee', borderRadius: 4, p: { xs: 4, md: 6 }, textAlign: 'center' }}>
          <CreditCardRounded sx={{ fontSize: 42, color: 'primary.main', mb: 1 }} />
          <Typography variant="h3" component="h2">Aún no tienes tarjetas</Typography>
          <Typography color="text.secondary" sx={{ mt: 1, mb: 3 }}>Agrega una para verla y administrarla aquí.</Typography>
          <Button variant="outlined" startIcon={<AddCardRounded />} onClick={openCreate}>Agregar mi primera tarjeta</Button>
        </Paper> : <Stack spacing={2}>{cards.map(card => <Card key={card.id} elevation={0} sx={{ border: '1px solid #e8e8ee', borderRadius: 4 }}>
          <CardContent sx={{ p: { xs: 2.5, sm: 3 }, '&:last-child': { pb: { xs: 2.5, sm: 3 } } }}>
            <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
              <Box sx={{ width: 50, height: 50, borderRadius: 3, display: 'grid', placeItems: 'center', bgcolor: 'secondary.light', color: 'primary.main' }}><CreditCardRounded /></Box>
              <Box sx={{ flex: 1, minWidth: 0 }}><Typography variant="h3">{card.brand} ···· {card.last4}</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mt: .4 }}>{card.holderName} · Vence {String(card.expirationMonth).padStart(2, '0')}/{card.expirationYear}</Typography></Box>
              <IconButton aria-label={`Editar ${card.brand} terminada en ${card.last4}`} onClick={() => openEdit(card)}><EditOutlined /></IconButton>
              <IconButton aria-label={`Eliminar ${card.brand} terminada en ${card.last4}`} color="error" onClick={() => { setDeleting(card); setFormError(''); }}><DeleteOutlineRounded /></IconButton>
            </Stack>
          </CardContent></Card>)}</Stack>}
    </Container>

    {dialogOpen && <Paper component="form" onSubmit={saveCard} elevation={0} sx={{ maxWidth: 700, mx: 'auto', mb: 4, border: '1px solid #e8e8ee', borderRadius: 4, p: { xs: 2.5, sm: 3 } }}>
      <Typography variant="h3" component="h2" sx={{ mb: 1 }}>{editing ? 'Editar tarjeta' : 'Agregar tarjeta'}</Typography>
      <Typography color="text.secondary" variant="body2" sx={{ mb: 2 }}>Guarda solo los últimos cuatro dígitos. No ingreses el número completo de la tarjeta.</Typography>
      {formError && <Alert severity="error" sx={{ mb: 2 }}>{formError}</Alert>}
      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 2, pt: 1 }}>
        {fields.map(field => <TextField key={field.name} label={field.label} required fullWidth
          name={field.name} type={field.type || 'text'} value={form[field.name]}
          slotProps={{ htmlInput: { maxLength: field.maxLength, inputMode: field.inputMode, min: field.min, max: field.max } }}
          error={fieldError === field.name} onChange={event => setForm(current => ({ ...current, [field.name]: event.target.value }))}
          sx={field.wide ? { gridColumn: { sm: 'span 2' } } : undefined} />)}
      </Box>
      <Stack direction="row" spacing={1} sx={{ mt: 3, justifyContent: 'flex-end' }}>
        <Button onClick={closeDialog} disabled={saving} color="inherit">Cancelar</Button>
        <Button type="submit" variant="contained" disabled={saving}>{saving ? 'Guardando…' : editing ? 'Guardar cambios' : 'Agregar tarjeta'}</Button>
      </Stack>
    </Paper>}
    {deleting && <Alert severity="warning" sx={{ maxWidth: 700, mx: 'auto', mb: 3 }}
      action={<Stack direction="row" spacing={1}><Button color="inherit" onClick={() => setDeleting(null)}>Cancelar</Button><Button color="error" onClick={deleteCard}>Confirmar eliminación</Button></Stack>}>
      Se eliminará {deleting.brand} terminada en {deleting.last4}.
      {formError && <Typography component="span" sx={{ display: 'block', mt: 1 }}>{formError}</Typography>}
    </Alert>}
  </Box>;
}
