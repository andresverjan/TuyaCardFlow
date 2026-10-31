import { useCallback, useEffect, useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Alert, Box, Button, Card, CardContent, CircularProgress, Container, Divider, FormControl,
  InputLabel, MenuItem, Paper, Select, Stack, Tab, Tabs, TextField, Typography,
} from '@mui/material';
import ArrowBackRounded from '@mui/icons-material/ArrowBackRounded';
import CreditCardRounded from '@mui/icons-material/CreditCardRounded';
import ReceiptLongRounded from '@mui/icons-material/ReceiptLongRounded';
import { Brand } from '../../../shared/components/Brand';
import { useAuth } from '../../auth/context/AuthContext';
import { cardsApi } from '../../cards/services/cardsApi';
import { paymentsApi } from '../services/paymentsApi';
import { validatePayment } from '../validation/paymentValidation';

const PAGE_SIZE = 10;
const initialPayment = { cardId: '', amount: '', description: '', securityCode: '' };

function isExpired(card) {
  const now = new Date();
  return card.expirationYear < now.getFullYear()
    || (card.expirationYear === now.getFullYear() && card.expirationMonth < now.getMonth() + 1);
}

function formatDate(value) {
  return new Intl.DateTimeFormat('es-CO', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value));
}

function findCard(cards, cardId) { return cards.find(card => card.id === cardId); }

export function PaymentsPage() {
  const { user } = useAuth();
  const [tab, setTab] = useState(0);
  const [cards, setCards] = useState([]);
  const [cardsLoading, setCardsLoading] = useState(true);
  const [cardsError, setCardsError] = useState('');
  const [payment, setPayment] = useState(initialPayment);
  const [paymentError, setPaymentError] = useState('');
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState('');
  const [history, setHistory] = useState({ page: 1, pageSize: PAGE_SIZE, totalCount: 0, items: [] });
  const [historyLoading, setHistoryLoading] = useState(true);
  const [historyError, setHistoryError] = useState('');
  const [selectedTransaction, setSelectedTransaction] = useState(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [detailError, setDetailError] = useState('');

  const loadCards = useCallback(async () => {
    setCardsLoading(true); setCardsError('');
    try { setCards(await cardsApi.list(user.id)); }
    catch (error) { setCardsError(error.message); }
    finally { setCardsLoading(false); }
  }, [user.id]);

  const loadHistory = useCallback(async (page = history.page) => {
    setHistoryLoading(true); setHistoryError('');
    try { setHistory(await paymentsApi.history(user.id, page, PAGE_SIZE)); }
    catch (error) { setHistoryError(error.message); }
    finally { setHistoryLoading(false); }
  }, [history.page, user.id]);

  useEffect(() => { loadCards(); }, [loadCards]);
  useEffect(() => { loadHistory(history.page); }, [history.page, loadHistory]);

  const submitPayment = async event => {
    event.preventDefault(); setPaymentError(''); setSuccess('');
    const card = findCard(cards, payment.cardId);
    const validationError = validatePayment({ card, amount: payment.amount, securityCode: payment.securityCode });
    if (validationError) { setPaymentError(validationError.message); return; }
    if (isExpired(card)) { setPaymentError('La tarjeta seleccionada está vencida. Elige otra tarjeta.'); return; }

    setSaving(true);
    try {
      const transaction = await paymentsApi.register({
        userId: user.id,
        cardId: card.id,
        amount: Number(payment.amount),
        currency: 'COP',
        description: payment.description.trim() || null,
      });
      setPayment(initialPayment);
      setSuccess(`Pago registrado por ${new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP' }).format(transaction.amount)}. Referencia: ${transaction.id}.`);
      setTab(1);
      if (history.page === 1) await loadHistory(1);
      else setHistory(current => ({ ...current, page: 1 }));
    } catch (error) { setPaymentError(error.message); }
    finally { setSaving(false); }
  };

  const openTransaction = async id => {
    setSelectedTransaction(null); setDetailError(''); setDetailLoading(true);
    try { setSelectedTransaction(await paymentsApi.transaction(user.id, id)); }
    catch (error) { setDetailError(error.message); }
    finally { setDetailLoading(false); }
  };

  const availableCards = cards.filter(card => !isExpired(card));
  const totalPages = Math.max(1, Math.ceil(history.totalCount / PAGE_SIZE));

  return <Box sx={{ minHeight: '100vh', background: '#f7f8fa' }}>
    <header className="profile-header"><Brand /><Button component={RouterLink} to="/mi-perfil" color="inherit" startIcon={<ArrowBackRounded />}>Mi perfil</Button></header>
    <Container maxWidth="md" sx={{ py: { xs: 5, md: 9 } }}>
      <Typography className="eyebrow" sx={{ color: 'primary.main', mb: 2 }}>TU ACTIVIDAD</Typography>
      <Typography variant="h1" component="h1">Pagos y movimientos<span className="accent-text">.</span></Typography>
      <Typography color="text.secondary" sx={{ mt: 1.5, mb: 3 }}>Registra pagos y consulta el historial de transacciones de tu perfil.</Typography>
      <Paper elevation={0} sx={{ border: '1px solid #e8e8ee', borderRadius: 4, overflow: 'hidden' }}>
        <Tabs value={tab} onChange={(_, next) => setTab(next)} aria-label="Pagos y movimientos" sx={{ px: 2, borderBottom: '1px solid #ececf0' }}>
          <Tab label="Registrar pago" />
          <Tab label="Historial" />
        </Tabs>
        {tab === 0 && <Box component="form" onSubmit={submitPayment} sx={{ p: { xs: 2.5, sm: 4 } }}>
          <Typography variant="h3" component="h2" sx={{ mb: 1 }}>Nuevo pago</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>El código de seguridad se revisa por formato y se descarta. Nunca se guarda ni se envía al backend.</Typography>
          {paymentError && <Alert severity="error" sx={{ mb: 2 }}>{paymentError}</Alert>}
          {cardsError && <Alert severity="error" action={<Button color="inherit" size="small" onClick={loadCards}>Reintentar</Button>} sx={{ mb: 2 }}>{cardsError}</Alert>}
          {cardsLoading ? <Stack sx={{ alignItems: 'center', py: 4 }}><CircularProgress size={26} /></Stack>
            : cardsError ? null : cards.length === 0 ? <Alert severity="warning" action={<Button component={RouterLink} to="/tarjetas" color="inherit">Administrar tarjetas</Button>}>Primero agrega una tarjeta a tu perfil.</Alert>
              : availableCards.length === 0 ? <Alert severity="warning">No tienes tarjetas vigentes para registrar un pago.</Alert>
                : <Stack spacing={2.5}>
                  <FormControl fullWidth required>
                    <InputLabel id="payment-card-label">Tarjeta</InputLabel>
                    <Select labelId="payment-card-label" label="Tarjeta" value={payment.cardId}
                      onChange={event => setPayment(current => ({ ...current, cardId: event.target.value, securityCode: '' }))}>
                      {cards.map(card => <MenuItem key={card.id} value={card.id} disabled={isExpired(card)}>
                        {card.brand} ···· {card.last4} · Vence {String(card.expirationMonth).padStart(2, '0')}/{card.expirationYear}{isExpired(card) ? ' (vencida)' : ''}
                      </MenuItem>)}
                    </Select>
                  </FormControl>
                  <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                    <TextField label="Monto (COP)" name="amount" type="number" required fullWidth value={payment.amount}
                      onChange={event => setPayment(current => ({ ...current, amount: event.target.value }))}
                      slotProps={{ htmlInput: { min: 0.01, max: 9999999999999999.99, step: 0.01, inputMode: 'decimal' } }} />
                    <TextField label="Código de seguridad (CVV)" name="securityCode" type="password" required fullWidth
                      autoComplete="cc-csc" value={payment.securityCode}
                      onChange={event => setPayment(current => ({ ...current, securityCode: event.target.value.replace(/\D/g, '').slice(0, /american\s*express|amex/i.test(findCard(cards, payment.cardId)?.brand || '') ? 4 : 3) }))}
                      slotProps={{ htmlInput: { inputMode: 'numeric', maxLength: /american\s*express|amex/i.test(findCard(cards, payment.cardId)?.brand || '') ? 4 : 3 } }} />
                  </Stack>
                  <TextField label="Descripción (opcional)" name="description" fullWidth value={payment.description}
                    onChange={event => setPayment(current => ({ ...current, description: event.target.value }))}
                    slotProps={{ htmlInput: { maxLength: 250 } }} />
                  <Button type="submit" variant="contained" size="large" startIcon={<CreditCardRounded />}
                    disabled={saving || cardsLoading || !availableCards.length}>
                    {saving ? 'Registrando…' : 'Registrar pago'}
                  </Button>
                </Stack>}
        </Box>}

        {tab === 1 && <Box sx={{ p: { xs: 2.5, sm: 4 } }}>
          <Stack direction={{ xs: 'column', sm: 'row' }} sx={{ alignItems: { sm: 'center' }, justifyContent: 'space-between', mb: 2 }} spacing={1}>
            <Box><Typography variant="h3" component="h2">Historial de transacciones</Typography><Typography variant="body2" color="text.secondary">{history.totalCount} movimiento{history.totalCount === 1 ? '' : 's'}</Typography></Box>
            <Button color="inherit" onClick={() => loadHistory(history.page)} startIcon={<ReceiptLongRounded />}>Actualizar</Button>
          </Stack>
          {historyError && <Alert severity="error" sx={{ mb: 2 }}>{historyError}</Alert>}
          {detailError && <Alert severity="error" onClose={() => setDetailError('')} sx={{ mb: 2 }}>{detailError}</Alert>}
          {detailLoading && <Stack sx={{ alignItems: 'center', py: 3 }}><CircularProgress size={24} /></Stack>}
          {selectedTransaction && <Paper variant="outlined" sx={{ p: 2.5, mb: 2.5, borderRadius: 3 }}>
            <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}><Typography variant="h3">Detalle del movimiento</Typography><Button onClick={() => setSelectedTransaction(null)} color="inherit">Cerrar</Button></Stack>
            <Divider sx={{ my: 1.5 }} />
            <Typography variant="body2">Referencia: {selectedTransaction.id}</Typography>
            <Typography variant="body2">Tarjeta: {findCard(cards, selectedTransaction.cardId) ? `${findCard(cards, selectedTransaction.cardId).brand} ···· ${findCard(cards, selectedTransaction.cardId).last4}` : selectedTransaction.cardId}</Typography>
            <Typography variant="body2">Monto: {selectedTransaction.currency} {Number(selectedTransaction.amount).toLocaleString('es-CO')}</Typography>
            <Typography variant="body2">Fecha: {formatDate(selectedTransaction.occurredAtUtc)}</Typography>
            <Typography variant="body2">Descripción: {selectedTransaction.description || 'Sin descripción'}</Typography>
          </Paper>}
          {historyLoading ? <Stack sx={{ alignItems: 'center', py: 5 }}><CircularProgress size={26} /></Stack>
            : history.items.length === 0 ? <Paper elevation={0} sx={{ border: '1px dashed #dedee4', borderRadius: 3, p: 4, textAlign: 'center' }}>
              <ReceiptLongRounded sx={{ fontSize: 36, color: 'text.secondary', mb: 1 }} />
              <Typography>No hay transacciones registradas.</Typography>
            </Paper> : <Stack spacing={1.5}>{history.items.map(item => {
              const card = findCard(cards, item.cardId);
              return <Card key={item.id} elevation={0} sx={{ border: '1px solid #e8e8ee', borderRadius: 3 }}>
                <CardContent sx={{ p: { xs: 2, sm: 2.5 }, '&:last-child': { pb: { xs: 2, sm: 2.5 } } }}>
                  <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ alignItems: { sm: 'center' } }}>
                    <Box sx={{ flex: 1 }}><Typography variant="h3">{item.currency} {Number(item.amount).toLocaleString('es-CO', { minimumFractionDigits: 2 })}</Typography>
                      <Typography variant="body2" color="text.secondary">{card ? `${card.brand} ···· ${card.last4}` : 'Tarjeta'} · {formatDate(item.occurredAtUtc)}</Typography>
                      {item.description && <Typography variant="body2" sx={{ mt: .5 }}>{item.description}</Typography>}</Box>
                    <Button onClick={() => openTransaction(item.id)} color="inherit">Ver detalle</Button>
                  </Stack>
                </CardContent>
              </Card>;
            })}</Stack>}
          <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', mt: 2.5 }}>
            <Button onClick={() => setHistory(current => ({ ...current, page: current.page - 1 }))} disabled={history.page <= 1 || historyLoading}>Anterior</Button>
            <Typography variant="body2" color="text.secondary">Página {history.page} de {totalPages}</Typography>
            <Button onClick={() => setHistory(current => ({ ...current, page: current.page + 1 }))} disabled={history.page >= totalPages || historyLoading}>Siguiente</Button>
          </Stack>
        </Box>}
      </Paper>
    </Container>
  </Box>;
}
